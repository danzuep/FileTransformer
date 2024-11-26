
using System.Text.Json;

namespace FileTransformer
{
    public interface IFileWriter
    {
        Task<bool> WriteAsync<T>(string path, T content, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);
    }
}