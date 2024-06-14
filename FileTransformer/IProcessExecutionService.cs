namespace FileTransformer;

public interface IProcessExecutionService
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}