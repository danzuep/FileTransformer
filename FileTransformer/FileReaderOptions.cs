namespace FileTransformer;

public class FileReaderOptions
{
    public static readonly string SectionName = "FileReader";

    public string FilePath { get; set; } = null!;

    public string Output { get; set; } = null!;

    public string Extension { get; set; } = "*";

    public bool SearchAll { get; set; }

    public bool CreateDirectory { get; set; }
}
