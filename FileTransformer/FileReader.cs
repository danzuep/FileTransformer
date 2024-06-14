namespace FileTransformer;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class FileReader : IFileReader
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

    private async ValueTask<T?> ProcessAsync<T>(string filePath, Func<Stream, ValueTask<T>> task)
    {
        T? fileContent = default;
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fileContent = await task(fileStream).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file: \"{0}\"", filePath);
        }
        return fileContent;
    }

    public async ValueTask<T> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken = default) where T : new()
    {
        var fileContent = await ProcessAsync(filePath, (stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions, cancellationToken));
        return fileContent ?? new();
    }

    public async Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContent = await ProcessAsync(filePath, async (stream) =>
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        });
        return fileContent ?? string.Empty;
    }
}
