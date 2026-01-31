namespace FileTransformer.Abstractions;

public interface IExecuteNextHandler : IExecute<string>
{
    IExecuteNextHandler SetNext(IExecuteNextHandler handler);
}