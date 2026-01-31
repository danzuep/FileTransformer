namespace FileTransformer.Modules;

using System.IO.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using FileTransformer.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class FileWriter : IFileWriter
{
    /// <inheritdoc cref="JsonSerializerOptions(JsonSerializerDefaults)"/>
    internal static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public FileWriter(IFileSystem? fileSystem = null, ILogger<FileWriter>? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger ?? NullLogger<FileWriter>.Instance;
    }

    /// <inheritdoc cref="JsonSerializerOptions"/>
    public JsonSerializerOptions JsonOptions { get; set; } = DefaultJsonOptions;

    /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, CancellationToken)"/>
    public async Task<bool> TryWriteAsync<T>(string path, T content, CancellationToken cancellationToken = default)
    {
        if (content is null || !_fileSystem.File.Exists(path))
        {
            return false;
        }
        try
        {
            using var fileStream = _fileSystem.File.OpenWrite(path);
            await JsonSerializer.SerializeAsync(fileStream, content, JsonOptions, cancellationToken).ConfigureAwait(false);
            await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write file: \"{0}\"", path);
            return false;
        }
    }
}
