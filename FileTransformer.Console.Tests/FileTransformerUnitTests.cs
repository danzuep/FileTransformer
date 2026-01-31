using System.Diagnostics;
using System.IO.Abstractions;
using System.Text.Json;
using FileTransformer.Abstractions;
using FileTransformer.Extensions;
using FileTransformer.Models;
using FileTransformer.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileTransformer.Tests
{
    public class FileTransformerUnitTests : IDisposable
    {
        private static readonly string _folder = @"C:\Logs";
        private static readonly string _filePath = $"{_folder}\\log.txt";
        private static readonly string _fileContent = """{"key":"value"}""";

        private readonly MockFileSystem _mockFileSystem;
        private readonly FolderOptions _folderOptions;
        private readonly IHost _host;

        /// <summary>
        /// <see href="https://github.com/TestableIO/System.IO.Abstractions#readme"/>
        /// </summary>
        public FileTransformerUnitTests()
        {
            // Arrange
            _mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { _filePath, new MockFileData(_fileContent) }
            });
            _folderOptions = FolderOptions.Folder(_folder);
            _host = CreateTestHost("-r", _folder);
        }

        private IHost CreateTestHost(params string[] args)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices(InitialiseServices)
                .ConfigureAppConfiguration(InitialiseConfiguration)
                .Build();

            void InitialiseConfiguration(IConfigurationBuilder builder) =>
                builder.AddCommandLineSwitchMappings((builder, switchMappings) =>
                    builder.AddCommandLine(args, switchMappings), args);

            void InitialiseServices(HostBuilderContext context, IServiceCollection services)
            {
                services.Configure<FolderOptions>(context.Configuration);
                services.AddFileTransformerServices(args);
                services.AddSingleton<IFileSystem>(_mockFileSystem);
            }
        }

        [Fact]
        public void GetFilesFromFolder_WithOneArgument_ReturnsValidModel()
        {
            // Arrange
            var host = CreateTestHost(_folder);
            var folderHandler = host.Services.GetRequiredService<IFolderHandler>();
            // Act
            var filePaths = folderHandler.GetFilesFromFolder(_folderOptions);
            // Assert
            Assert.True(filePaths?.Count() > 0);
            Assert.Equal(_filePath, filePaths.First());
        }

        [Fact]
        public void GetFilesFromFolder_ReturnsValidModel()
        {
            // Arrange
            var folderHandler = _host.Services.GetRequiredService<IFolderHandler>();
            // Act
            var filePaths = folderHandler.GetFilesFromFolder(_folderOptions);
            // Assert
            Assert.True(filePaths?.Count() > 0);
            Assert.Equal(_filePath, filePaths.First());
        }

        [Theory]
        [InlineData("export.json")]
        [Conditional("DEBUG")]
        public void GetFilesFromFolder_WithNewFile_ReturnsValidModel(string newFileName)
        {
            // Arrange
            var newFilePath = _mockFileSystem.Path.Combine(_folder, newFileName);
            _mockFileSystem.AddFile(newFilePath, new MockFileData(_fileContent));
            var folderHandler = new FolderHandler(_mockFileSystem);
            // Act
            var filePaths = folderHandler.GetFilesFromFolder(_folderOptions);
            // Assert
            Assert.Contains(newFilePath, filePaths);
        }

        [Fact]
        public async Task ReadAllTextAsync_WithValidContents_ReturnsValidModel()
        {
            // Arrange
            var fileReader = new FileReader(_mockFileSystem);
            var cancellationToken = TestContext.Current.CancellationToken;
            // Act
            var fileContent = await fileReader.ReadAllTextAsync(_filePath, cancellationToken);
            // Assert
            Assert.Equal(_fileContent, fileContent);
        }

        [Fact]
        public async Task DeserializeAsync_WithValidContents_ReturnsValidModel()
        {
            // Arrange
            var fileReader = new FileReader(_mockFileSystem);
            var fileWriter = _host.Services.GetRequiredService<IFileWriter>();
            fileWriter.JsonOptions = new JsonSerializerOptions(FileReader.DefaultJsonOptions) { WriteIndented = false };
            var cancellationToken = TestContext.Current.CancellationToken;
            // Act
            var fileContents = await fileReader.DeserializeAsync<Dictionary<string, string>>(_filePath, cancellationToken);
            var isComplete = await fileWriter.TryWriteAsync(_filePath, fileContents, cancellationToken);
            var fileContent = await fileReader.ReadAllTextAsync(_filePath, cancellationToken);
            // Assert
            Assert.True(isComplete);
            Assert.Equal(_fileContent, fileContent);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidContents_ReturnsValidModel()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<IExecuteService>();
            var cancellationToken = TestContext.Current.CancellationToken;
            // Act
            await service.ExecuteAsync(cancellationToken);
        }

        [Fact(Explicit = true)]
        public async Task GetChildTypes_WithBaseType_ReturnsValidModel()
        {
            // Arrange
            var baseType = typeof(ExecuteNextFileHandler);
            // Act
            var childTypes = GetChildTypes(baseType);
            var tasks = childTypes.Select(ProcessAsync);
            await Task.WhenAll(tasks);
            // Assert
            Assert.True(childTypes.Count > 0);
            foreach (var type in childTypes)
            {
                Assert.True(baseType.IsAssignableFrom(type));
            }
        }

        private static IReadOnlyList<Type> GetChildTypes(Type baseType)
        {
            var types = baseType.Assembly
                .GetTypes()
                .Where(t => baseType.IsAssignableFrom(t) &&
                    t != baseType &&
                    !t.IsAbstract &&
                    !t.IsGenericTypeDefinition &&
                    t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.Name)
                .ToArray();
            return types;
        }

        private static async Task ProcessAsync(Type type, int index = 0)
        {
            var instance = Activator.CreateInstance(type) as IExecuteNextHandler;
            var cancellationToken = TestContext.Current.CancellationToken;
            if (instance is null) { return; }
            await instance.ExecuteAsync("appsettings.json", cancellationToken).ConfigureAwait(false);
            TestContext.Current.SendDiagnosticMessage("TestCase #{0:D2} run: {1}", index, type.Name);
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}