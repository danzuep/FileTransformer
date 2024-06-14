namespace FileTransformer;

public interface IFolderHandler
{
    IEnumerable<string> GetFilesFromFolder();
}