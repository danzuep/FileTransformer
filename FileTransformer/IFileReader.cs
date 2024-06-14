namespace FileTransformer;

public interface IFileReader
{
    ValueTask<T> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken = default) where T : new();
}