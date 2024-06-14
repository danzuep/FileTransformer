namespace FileTransformer;

public class WorkerOptions
{
    public static readonly string SectionName = "Worker";

    public string FolderPath { get; set; } = null!;

    public string Output { get; set; } = null!;

    public string Extension { get; set; } = "*";

    public bool SearchAll { get; set; }

    public bool CreateDirectory { get; set; }
}
