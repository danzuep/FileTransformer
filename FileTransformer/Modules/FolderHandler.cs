namespace FileTransformer.Modules;

using FileTransformer.Abstractions;
using FileTransformer.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions;
using System.Text;

public class FolderHandler : IFolderHandler
{
    private const int MaxFilePathLength = 260;

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger = NullLogger.Instance;

    public FolderHandler(IFileSystem? fileSystem = null, ILogger<FolderHandler>? logger = null) : base()
    {
        _fileSystem = fileSystem ?? new FileSystem();
        if (logger != null) _logger = logger;
    }

    public IEnumerable<string> GetFilesFromFolder(FolderOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ReadDirectory) ||
            !CheckDirectory(options.ReadDirectory, options.CreateDirectory))
            return Enumerable.Empty<string>();
        var split = options.SearchPattern?.Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(ConvertToFileExtensionFilter);
        var extension = string.Join(",", split ?? ["*"]);
        var option = options.SearchTopDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
        var filePaths = _fileSystem.Directory.EnumerateFiles(options.ReadDirectory, extension, option);
        return filePaths;
    }

    private static string ConvertToFileExtensionFilter(string fileExtension)
    {
        var sb = new StringBuilder();
        if (string.IsNullOrWhiteSpace(fileExtension))
            fileExtension = "*";
        else if (fileExtension.StartsWith("."))
            sb.Append("*");
        else if (!fileExtension.StartsWith("*.") &&
            !fileExtension.Contains("."))
            sb.Append("*.");
        sb.Append(fileExtension.Trim());
        return sb.ToString();
    }

    private void CreateDirectory(string uncPath)
    {
        try
        {
            _fileSystem.Directory.CreateDirectory(uncPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "CreateDirectory access is denied, folder not created: '{0}'", uncPath);
        }
    }

    private bool CheckDirectory(string uncPath, bool createDirectory = false)
    {
        bool exists = _fileSystem.Directory.Exists(uncPath);
        if (!exists && createDirectory)
            CreateDirectory(uncPath);
        else if (!exists)
            _logger.LogWarning("Folder not found: '{0}'.", uncPath);
        return exists;
    }

    /// <summary>
    /// Truncated hexadecimal string with up to 16^32 combinations.
    /// </summary>
    /// <param name="count">Base 16 exponent.</param>
    /// <returns>Truncated hexadecimal string</returns>
    internal static string GetHexId(int count = 32)
    {
        if (count < 1)
            count = 1;
        if (count > 32)
            count = 32;
        var uuid = Guid.NewGuid().ToString("n", null).Substring(0, count);
        return uuid;
    }

    private string GetOutputFileName(string? filePath, string subFolder = "Temp", string suffix = "", bool create = true)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;
        var outputPath = filePath!;
        var folder = _fileSystem.Path.GetDirectoryName(filePath);
        if (folder != null && (_fileSystem.File.Exists(outputPath) || !string.IsNullOrEmpty(suffix)))
        {
            var name = _fileSystem.Path.GetFileNameWithoutExtension(filePath)!;
            var extension = _fileSystem.Path.GetExtension(filePath);
            var hasSubFolder = !string.IsNullOrEmpty(subFolder);
            var fileName = $"{name}{suffix}{extension}";
            if (hasSubFolder && !folder.EndsWith(subFolder, StringComparison.OrdinalIgnoreCase))
                folder = _fileSystem.Path.Combine(folder, subFolder);
            if (create && hasSubFolder)
                _fileSystem.Directory.CreateDirectory(folder);
            outputPath = _fileSystem.Path.Combine(folder, fileName);
            if (_fileSystem.File.Exists(outputPath))
                outputPath = _fileSystem.Path.Combine(folder, $"{name}_{DateTime.Now.Ticks}{extension}");
            if (outputPath.Length > MaxFilePathLength)
                outputPath = _fileSystem.Path.Combine(folder, $"{GetHexId(name.Length)}{extension}");
        }
        return outputPath;
    }
}
