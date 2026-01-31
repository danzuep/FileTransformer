using System.Text.Json;

namespace FileTransformer.Abstractions;

public interface IFileReader
{
    JsonSerializerOptions JsonOptions { get; set; }

    ValueTask<T?> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken = default);
}