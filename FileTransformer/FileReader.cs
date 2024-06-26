﻿namespace FileTransformer;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class FileReader : IFileReader
{
    internal static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        Converters = { new JsonStringEnumConverter() }
    };
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public FileReader(IFileSystem? fileSystem = null, ILogger<FileReader>? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger ?? NullLogger<FileReader>.Instance;
    }

    private async ValueTask<T?> ProcessAsync<T>(string filePath, Func<Stream, ValueTask<T>> task)
    {
        T? fileContent = default;
        try
        {
            using var fileStream = _fileSystem.FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            fileContent = await task(fileStream).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file: \"{0}\"", filePath);
        }
        return fileContent;
    }

    public async ValueTask<T?> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContent = await ProcessAsync(filePath, (stream) =>
            JsonSerializer.DeserializeAsync<T>(stream, JsonSerializerOptions, cancellationToken));
        return fileContent;
    }

    public async Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fileContent = await ProcessAsync(filePath, async (stream) =>
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync(cancellationToken);
        });
        return fileContent ?? string.Empty;
    }
}
