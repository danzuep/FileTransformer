using System.Text.Json;

namespace FileTransformer;

public interface IFileReader
{
    ValueTask<T?> DeserializeAsync<T>(string filePath, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
}