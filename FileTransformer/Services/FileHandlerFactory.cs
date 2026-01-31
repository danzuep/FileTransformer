namespace FileTransformer.Services;

using System.Threading;
using FileTransformer.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public sealed class FileHandlerFactory : IFileHandler
{
    private readonly IExecuteNextHandler _handler;
    private readonly ILogger<FileHandlerFactory>? _logger;

    public FileHandlerFactory(IEnumerable<IExecuteNextHandler> handlers, ILogger<FileHandlerFactory>? logger = null)
    {
        _logger = logger ?? NullLogger<FileHandlerFactory>.Instance;
        if (handlers == null)
        {
            throw new ArgumentNullException(nameof(handlers));
        }
        foreach (var handler in handlers)
        {
            if (_handler == null)
            {
                _handler = handler;
            }
            else
            {
                _handler.SetNext(handler);
            }
            _logger.LogDebug("Registered handler: {HandlerType}", handler.GetType().Name);
        }
        if (_handler == null)
        {
            throw new InvalidOperationException("No handlers registered.");
        }
    }

    public async Task ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await _handler.ExecuteAsync(filePath, cancellationToken).ConfigureAwait(false);
    }
}
