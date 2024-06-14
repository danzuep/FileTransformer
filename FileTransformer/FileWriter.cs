namespace FileTransformer;

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class FileWriter
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ILogger _logger;

    public FileWriter(ILogger<FileWriter>? logger = null)
    {
        _logger = logger ?? NullLogger<FileWriter>.Instance;
    }

    public async Task ProcessAsync<T>(T fileObject, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(fileObject, _jsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, json, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write file: \"{0}\"", filePath);
        }
    }
}
