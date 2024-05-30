using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileTransformer
{
    internal class FileReader : IFileReader
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly ILogger _logger;

        public FileReader(ILogger<FileReader>? logger = null)
        {
            _logger = logger ?? NullLogger<FileReader>.Instance;
        }

        private async Task<T?> ProcessAsync<T>(string filePath, Func<Stream, ValueTask<T>> task)
        {
            T? fileContent = default;
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                fileContent = await task(fileStream);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to read file: \"{filePath}\"");
            }
            return fileContent;
        }

        public async Task<T> DeserializeAsync<T>(string filePath) where T : new()
        {
            var fileContent = await ProcessAsync(filePath, (stream) =>
                JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions));
            return fileContent ?? new();
        }

        public async Task<string> ReadAllTextAsync(string filePath)
        {
            var fileContent = await ProcessAsync(filePath, async (stream) =>
            {
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            });
            return fileContent ?? string.Empty;
        }
    }
}
