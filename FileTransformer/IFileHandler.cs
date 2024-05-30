namespace FileTransformer;

public interface IFileHandler
{
    IEnumerable<string> GetFilesFromFolder();
}