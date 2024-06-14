namespace FileTransformer;

using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class ProcessExecutionService : IProcessExecutionService
{
    private readonly IFolderHandler _folderHandler;
    private readonly IFileReader _fileReader;
    private readonly ILogger _logger;

    public ProcessExecutionService(IFolderHandler folderHandler, IFileReader fileReader, ILogger<ProcessExecutionService>? logger = null)
    {
        _folderHandler = folderHandler;
        _fileReader = fileReader;
        _logger = logger ?? NullLogger<ProcessExecutionService>.Instance;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = new List<Task>();
            foreach (var filePath in _folderHandler.GetFilesFromFolder())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogTrace("Worker cancelled");
                    break;
                }
                tasks.Add(ExecuteAsync(filePath, cancellationToken));
            }
            await Task.Run(() => Task.WhenAll(tasks), cancellationToken);
        }
        catch (OperationCanceledException) // includes TaskCanceledException
        {
            _logger.LogDebug("Folder handler cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Folder handler failed to process all files.");
        }
    }

    private async Task ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = await _fileReader.DeserializeAsync<WorkerOptions>(filePath, cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("File deserialized: \"{0}\"", filePath);
        }
        catch (OperationCanceledException) // includes TaskCanceledException
        {
            _logger.LogDebug("File deserializer cancelled while processing file: \"{0}\"", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File deserializer failed to process file: \"{0}\"", filePath);
        }
    }
}
