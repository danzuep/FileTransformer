namespace FileTransformer;

using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class FileWriter : IFileWriter
{
    /// <inheritdoc cref="JsonSerializerOptions"/>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public FileWriter(IFileSystem? fileSystem = null, ILogger<FileWriter>? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger ?? NullLogger<FileWriter>.Instance;
    }

    /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, CancellationToken)"/>
    public async Task<bool> WriteAsync<T>(string path, T content, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var fileStream = _fileSystem.File.OpenWrite(path);
            await JsonSerializer.SerializeAsync(fileStream, content, options ?? JsonOptions, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write file: \"{0}\"", path);
            return false;
        }
    }
}
