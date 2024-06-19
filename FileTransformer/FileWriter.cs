namespace FileTransformer;

using System.IO.Abstractions;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class FileWriter
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public FileWriter(IFileSystem? fileSystem = null, ILogger<FileWriter>? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger ?? NullLogger<FileWriter>.Instance;
    }

    public string Serialize<T>(T fileObject) =>
        JsonSerializer.Serialize(fileObject, FileReader.JsonSerializerOptions);

    public async Task WriteAllTextAsync<T>(T fileObject, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = Serialize(fileObject);
            await _fileSystem.File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write file: \"{0}\"", filePath);
        }
    }
}
