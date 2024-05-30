namespace FileTransformer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

public sealed class Program
{
    public static async Task<int> Main(string[] args)
    {
        var provider = InitialiseServiceProvider(args);

        var fileHandler = provider.GetRequiredService<FileHandler>();
        var tasks = fileHandler.GetFilesFromFolder()
            .Select(f => ProcessFile(provider, f));
        await Task.WhenAll(tasks);

        return 0;
    }

    private static async Task ProcessFile(IServiceProvider provider, string file)
    {
        var fileReader = provider.GetRequiredService<FileReader>();
        var content = await fileReader.DeserializeAsync<FileModel>(file);
    }

    private static IConfigurationRoot InitialiseConfiguration(string[] args)
    {
        var switchMappings = new Dictionary<string, string>()
        {
            // The file to read from.
            { "-f", "FilePath" },
            // The file to write to.
            { "-o", "Output" }
        };
        var builder = new ConfigurationBuilder()
            .AddCommandLine(args, switchMappings);
        return builder.Build();
    }

    public static IServiceProvider InitialiseServiceProvider(params string[] args)
    {
        var config = InitialiseConfiguration(args);
        var services = new ServiceCollection();
        services.AddLogging(o => o.SetMinimumLevel(LogLevel.Debug).AddDebug().AddConsole());
        services.Configure<FileReaderOptions>(config);
        services.AddTransient<IFileReader, FileReader>();
        services.AddTransient<IFileHandler, FileHandler>();
        return services.BuildServiceProvider();
    }
}