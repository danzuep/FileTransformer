namespace FileTransformer.Models;

public class FolderOptions
{
    public static readonly string SectionName = "FileTransformer";

    public string ReadDirectory { get; set; } = "ToTransform";

    public string WriteDirectory { get; set; } = "Transformed";

    public string SearchPattern { get; set; } = "*";

    public bool SearchTopDirectoryOnly { get; set; }

    public bool CreateDirectory { get; set; }

    public static FolderOptions Folder(string path) =>
        new FolderOptions { ReadDirectory = path };
}
