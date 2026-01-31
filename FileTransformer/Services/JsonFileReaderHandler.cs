namespace FileTransformer.Services;

using System.Threading;
using FileTransformer.Abstractions;
using Microsoft.Extensions.Logging;

public sealed class JsonFileReaderHandler : ExecuteNextFileHandler
{
    private readonly IFileReader _fileReader;

    public JsonFileReaderHandler(IFileReader fileReader, ILogger<JsonFileReaderHandler>? logger = null) : base(logger)
    {
        _fileReader = fileReader;
    }

    public override async Task ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deserializing file: \"{FilePath}\"", filePath);
            var content = await _fileReader.DeserializeAsync<Dictionary<string, object>>(filePath!, cancellationToken).ConfigureAwait(false);
            _logger.LogTrace("{Content}", content);
        }
        catch (OperationCanceledException) // includes TaskCanceledException
        {
            _logger.LogDebug("File deserializer cancelled while processing file: \"{FilePath}\"", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File deserializer failed to process file: \"{FilePath}\"", filePath);
        }
    }
}
