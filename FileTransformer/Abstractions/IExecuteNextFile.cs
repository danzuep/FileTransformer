namespace FileTransformer.Abstractions;

public interface IExecuteNextFile : IExecute<string>
{
    IExecuteNextFile SetNext(IExecuteNextFile handler);
}