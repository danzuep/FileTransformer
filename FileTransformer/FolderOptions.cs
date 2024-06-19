namespace FileTransformer;

public class FolderOptions
{
    public string FolderPath { get; set; } = null!;

    public string Extension { get; set; } = "*";

    public bool SearchAll { get; set; } = true;

    public bool CreateDirectory { get; set; }

    public static FolderOptions Folder(string path) =>
        new FolderOptions { FolderPath = path };
}
