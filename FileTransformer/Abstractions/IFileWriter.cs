using System.Text.Json;

namespace FileTransformer.Abstractions
{
    public interface IFileWriter
    {
        JsonSerializerOptions JsonOptions { get; set; }

        Task<bool> TryWriteAsync<T>(string path, T content, CancellationToken cancellationToken = default);
    }
}