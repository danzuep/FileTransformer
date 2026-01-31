namespace FileTransformer.Abstractions;

public interface IExecute<T>
{
    Task ExecuteAsync(T value, CancellationToken cancellationToken = default);
}