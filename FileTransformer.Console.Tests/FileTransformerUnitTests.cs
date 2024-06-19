using System.IO.Abstractions;
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

        private readonly MockFileSystem _fileSystem;
        private readonly FolderOptions _folderOptions;
        private readonly IHost _host;

        /// <summary>
        /// <see href="https://github.com/TestableIO/System.IO.Abstractions#readme"/>
        /// </summary>
        public FileTransformerUnitTests()
        {
            // Arrange
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { _filePath, new MockFileData(_fileContent) }
            });
            _folderOptions = FolderOptions.Folder(_folder);
            _host = CreateTestHost("-f", _folder);
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
                services.Configure<WorkerOptions>(context.Configuration);
                services.AddSingleton<IFileSystem>(_fileSystem);
                services.AddTransient<FileWriter>();
                services.AddFileTransformerServices(args);
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
        public void GetFilesFromFolder_WithNewFile_ReturnsValidModel(string newFileName)
        {
            // Arrange
            var newFilePath = _fileSystem.Path.Combine(_folder, newFileName);
            _fileSystem.AddFile(newFilePath, new MockFileData(_fileContent));
            var folderHandler = new FolderHandler(_fileSystem);
            // Act
            var filePaths = folderHandler.GetFilesFromFolder(_folderOptions);
            // Assert
            Assert.Contains(newFilePath, filePaths);
        }

        [Fact]
        public async Task ReadAllTextAsync_WithValidContents_ReturnsValidModel()
        {
            // Arrange
            var fileReader = new FileReader(_fileSystem);
            // Act
            var fileContent = await fileReader.ReadAllTextAsync(_filePath, CancellationToken.None);
            // Assert
            Assert.Equal(_fileContent, fileContent);
        }

        [Fact]
        public async Task DeserializeAsync_WithValidContents_ReturnsValidModel()
        {
            // Arrange
            var fileReader = _host.Services.GetRequiredService<IFileReader>();
            var fileWriter = _host.Services.GetRequiredService<FileWriter>();
            // Act
            var fileContents = await fileReader.DeserializeAsync<Dictionary<string, string>>(_filePath, CancellationToken.None);
            var fileContent = fileWriter.Serialize(fileContents);
            // Assert
            Assert.NotNull(fileContent);
            Assert.Equal(_fileContent, fileContent);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidContents_ReturnsValidModel()
        {
            // Arrange
            var service = _host.Services.GetRequiredService<IProcessExecutionService>();
            // Act
            await service.ExecuteAsync(CancellationToken.None);
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}