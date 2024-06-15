namespace FileTransformer;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

[ExcludeFromCodeCoverage]
public class Worker : IHostedService, IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource = null;
    private readonly IProcessExecutionService _processExecutionService;
    private readonly IHostApplicationLifetime? _hostApplicationLifetime;
    private readonly ILogger _logger;

    public Worker(IProcessExecutionService processExecutionService, IHostApplicationLifetime? hostApplicationLifetime = null, ILogger<Worker>? logger = null)
    {
        _processExecutionService = processExecutionService;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = logger ?? NullLogger<Worker>.Instance;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Worker started at {time:o}", DateTimeOffset.Now);
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await _processExecutionService.ExecuteAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Worker finished at {time:s}", DateTimeOffset.Now);
        _hostApplicationLifetime?.StopApplication();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}
