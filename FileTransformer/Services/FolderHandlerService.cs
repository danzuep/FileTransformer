namespace FileTransformer.Services;

using System.Threading;
using FileTransformer.Abstractions;
using FileTransformer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

public sealed class FolderHandlerService : IExecuteService
{
    private readonly FolderOptions _options;
    private readonly IFolderHandler _folderHandler;
    private readonly IFileHandler _fileHandler;
    private readonly ILogger<FolderHandlerService>? _logger;

    public FolderHandlerService(IOptions<FolderOptions> options, IFolderHandler folderHandler, IFileHandler fileHandler, ILogger<FolderHandlerService>? logger = null)
    {
        _options = options.Value;
        _folderHandler = folderHandler;
        _fileHandler = fileHandler;
        _logger = logger ?? NullLogger<FolderHandlerService>.Instance;
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
                    _logger?.LogTrace("Folder handler cancelled");
                    break;
                }
                tasks.Add(_fileHandler.ExecuteAsync(filePath, cancellationToken));
            }
            await Task.Run(() => Task.WhenAll(tasks), cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) // includes TaskCanceledException
        {
            _logger?.LogDebug("Folder handler cancelled");
        }
        catch (AggregateException ex)
        {
            _logger?.LogError(ex, "Folder handler failed to process all files.");
        }
    }
}
