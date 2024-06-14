namespace FileTransformer.Console;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class Program
{
    [ExcludeFromCodeCoverage]
    public static async Task Main(string[] args)
    {
        using var host = CreateConsoleHost(args);
        await host.RunAsync();
    }

    public static IHost CreateConsoleHost(params string[] args)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureLogging(log => log.SetMinimumLevel(LogLevel.Debug))
            .ConfigureAppConfiguration(InitialiseConfiguration)
            .ConfigureServices(InitialiseServices)
            .UseConsoleLifetime()
            .Build();

        void InitialiseConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            if (args is { Length: > 1 })
            {
                var switchMappings = new Dictionary<string, string>()
                {
                    // The file to read from.
                    { "-f", "FilePath" },
                    // The file to write to.
                    { "-o", "Output" }
                };
                builder.AddCommandLine(args, switchMappings);
            }
        }

        void InitialiseServices(HostBuilderContext context, IServiceCollection services)
        {
            var config = context.Configuration.GetSection(WorkerOptions.SectionName);
            services.Configure<WorkerOptions>(config);
            if (args is { Length: 1 })
            {
                services.AddOptions<WorkerOptions>()
                    .Configure(options => options.FolderPath = args[0]);
            }
            services.AddTransient<IFolderHandler, FolderHandler>();
            services.AddTransient<IFileReader, FileReader>();
            services.AddTransient<IProcessExecutionService, ProcessExecutionService>();
            services.AddHostedService<Worker>();
        }
    }
}