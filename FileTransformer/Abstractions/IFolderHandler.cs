using FileTransformer.Models;

namespace FileTransformer.Abstractions;

public interface IFolderHandler
{
    IEnumerable<string> GetFilesFromFolder(FolderOptions options);
}