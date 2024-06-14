using FileTransformer.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileTransformer.Tests
{
    public class FileTransformerUnitTests : IDisposable
    {
        private readonly MockFileSystem _fileSystem;
        private readonly IHost _host;

        /// <summary>
        /// <see href="https://github.com/TestableIO/System.IO.Abstractions#readme"/>
        /// </summary>
        public FileTransformerUnitTests()
        {
            // Arrange
            _fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\Logs\log.txt", new MockFileData(string.Empty) }
            });
            _host = Program.CreateConsoleHost("-f", @"C:\Logs\log.txt");
        }

        [Fact]
        public void ReadFolder_WithValidContents_ReturnsValidModel()
        {
            // Arrange
            var fileReader = _host.Services.GetRequiredService<IFolderHandler>();
            // Act
            var outFile = fileReader.GetFilesFromFolder();
            // Assert
            Assert.NotNull(outFile);
        }

        [Theory]
        [InlineData(@"C:\Logs\log.txt", @"C:\Logs", "", "")]
        public async Task ReadFile_WithValidContents_ReturnsValidModelAsync(string filePath, string folderPathExport, string jsonFolderSuffix, string expected)
        {
            // Arrange
            _fileSystem.AddDirectory($"{folderPathExport}{jsonFolderSuffix}");
            var fileReader = _host.Services.GetRequiredService<IFileReader>();
            // Act
            var outFile = await fileReader.DeserializeAsync<Dictionary<string, string>>(filePath);
            // Assert
            var jsonOutFilePath = $"{_fileSystem.Path.Combine($"{folderPathExport}{jsonFolderSuffix}", _fileSystem.Path.GetFileNameWithoutExtension(filePath))}.json";
            var jsonOutFileData = _fileSystem.GetFile(jsonOutFilePath);
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}