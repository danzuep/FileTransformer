
namespace FileTransformer
{
    internal interface IFileReader
    {
        Task<T> DeserializeAsync<T>(string filePath) where T : new();

        Task<string> ReadAllTextAsync(string filePath);
    }
}