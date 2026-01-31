using FileTransformer.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

// Handler that implements the Chain of Responsibility design pattern
public abstract class ExecuteNextFileHandler : IExecuteNextHandler
{
    private IExecuteNextHandler? _next;
    protected readonly ILogger _logger;

    protected ExecuteNextFileHandler(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    public IExecuteNextHandler SetNext(IExecuteNextHandler handler)
    {
        _next = handler ?? throw new ArgumentNullException(nameof(handler));
        return handler;
    }

    public virtual async Task ExecuteAsync(string value, CancellationToken cancellationToken = default)
    {
        if (_next is null) { return; }
        await _next.ExecuteAsync(value, cancellationToken).ConfigureAwait(false);
    }
}