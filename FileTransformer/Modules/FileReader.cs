namespace FileTransformer.Modules;

using System.IO;
using System.IO.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using FileTransformer.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class FileReader : IFileReader
{
    /// <inheritdoc cref="JsonSerializerOptions(JsonSerializerDefaults)"/>
    internal static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public FileReader(IFileSystem? fileSystem = null, ILogger<FileReader>? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger ?? NullLogger<FileReader>.Instance;
    }

    /// <inheritdoc cref="JsonSerializerOptions"/>
    public JsonSerializerOptions JsonOptions { get; set; } = DefaultJsonOptions;

    public async ValueTask<T?> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContent = await ProcessAsync(filePath, (stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)).ConfigureAwait(false);
        return fileContent;
    }

    /// <inheritdoc cref="File.ReadAllTextAsync(string, CancellationToken)"/>
    public async Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContent = await ProcessAsync(filePath, async (stream) =>
        {
            using var reader = new StreamReader(stream);
#if NET
            return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
            return await reader.ReadToEndAsync().ConfigureAwait(false);
#endif
        }, cancellationToken);
        return fileContent ?? string.Empty;
    }

    private async ValueTask<T?> ProcessAsync<T>(string filePath, Func<Stream, ValueTask<T>> task, CancellationToken cancellationToken = default)
    {
        T? fileContent = default;
        try
        {
            if (!_fileSystem.File.Exists(filePath))
            {
                _logger.LogInformation("File not found: \"{0}\"", filePath);
                return fileContent;
            }
            using var fileStream = _fileSystem.File.OpenRead(filePath);
            fileContent = await task(fileStream).ConfigureAwait(false);
            await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file: \"{0}\"", filePath);
        }
        return fileContent;
    }
}
