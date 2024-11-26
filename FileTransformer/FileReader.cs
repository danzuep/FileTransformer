namespace FileTransformer;

using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class FileReader : IFileReader
{
    /// <inheritdoc cref="JsonSerializerOptions(JsonSerializerDefaults)"/>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public FileReader(IFileSystem? fileSystem = null, ILogger<FileReader>? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger ?? NullLogger<FileReader>.Instance;
    }

    private async ValueTask<T?> ProcessAsync<T>(string filePath, Func<Stream, ValueTask<T>> task)
    {
        T? fileContent = default;
        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                _logger.LogInformation("File not found: \"{0}\"", filePath);
                return fileContent;
            }
            using var fileStream = _fileSystem.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fileContent = await task(fileStream).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file: \"{0}\"", filePath);
        }
        return fileContent;
    }

    public async ValueTask<T?> DeserializeAsync<T>(string filePath, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        var fileContent = await ProcessAsync(filePath, (stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, options ?? JsonOptions, cancellationToken)).ConfigureAwait(false);
        return fileContent;
    }

    /// <inheritdoc cref="File.ReadAllTextAsync(string, CancellationToken)"/>
    public async Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContent = await ProcessAsync(filePath, async (stream) =>
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        });
        return fileContent ?? string.Empty;
    }
}
