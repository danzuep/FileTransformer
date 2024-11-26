namespace FileTransformer;

using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

public sealed class ProcessExecutionService : IProcessExecutionService
{
    private readonly FolderOptions _options;
    private readonly IFolderHandler _folderHandler;
    private readonly IFileReader _fileReader;
    private readonly ILogger _logger;

    public ProcessExecutionService(IOptions<FolderOptions> options, IFolderHandler folderHandler, IFileReader fileReader, ILogger<ProcessExecutionService>? logger = null)
    {
        _options = options.Value;
        _folderHandler = folderHandler;
        _fileReader = fileReader;
        _logger = logger ?? NullLogger<ProcessExecutionService>.Instance;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = new List<Task>();
            foreach (var filePath in _folderHandler.GetFilesFromFolder(_options))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogTrace("Folder handler cancelled");
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
        catch (AggregateException ex)
        {
            _logger.LogError(ex, "Folder handler failed to process all files.");
        }
    }

    private async Task ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deserializing file: \"{0}\"", filePath);
            var content = await _fileReader.DeserializeAsync<Dictionary<string, string>>(filePath, null, cancellationToken).ConfigureAwait(false);
            if (content != null)
            {
                foreach (var pair in content)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogTrace("File deserializer cancelled while processing file: \"{0}\"", filePath);
                        break;
                    }
                    _logger.LogTrace("{0}", pair);
                }
            }
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
