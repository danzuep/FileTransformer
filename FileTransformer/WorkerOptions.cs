namespace FileTransformer;

public class WorkerOptions : FolderOptions
{
    public static readonly string SectionName = "Worker";

    public string? Output { get; set; }
}
