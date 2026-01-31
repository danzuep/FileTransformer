using Microsoft.Extensions.Logging;

namespace FileTransformer.Services;

public class JsonValidationHandler : ExecuteNextFileHandler
{
    private static readonly string _validPattern = "json";

    public JsonValidationHandler() : base(null)
    {
    }

    public JsonValidationHandler(ILogger<JsonValidationHandler> logger) : base(logger)
    {
    }

    public override async Task ExecuteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            _logger.LogDebug("\"{Pattern}\" suffix deserializer skipped due to missing file path: \"{FilePath}\"", _validPattern, filePath);
            return;
        }
        if (!filePath.EndsWith(_validPattern))
        {
            _logger.LogDebug("\"{Pattern}\" suffix deserializer does not apply for: \"{FilePath}\"", _validPattern, filePath);
            return;
        }
        await base.ExecuteAsync(filePath, cancellationToken).ConfigureAwait(false);
    }
}