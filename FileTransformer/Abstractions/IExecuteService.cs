namespace FileTransformer.Abstractions;

public interface IExecuteService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}