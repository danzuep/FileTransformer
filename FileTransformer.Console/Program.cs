namespace FileTransformer.Console;

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
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
            .ConfigureServices(InitialiseServices)
            .ConfigureAppConfiguration(InitialiseConfiguration)
            .UseConsoleLifetime()
            .Build();

        void InitialiseConfiguration(IConfigurationBuilder builder) =>
            builder.AddCommandLineSwitchMappings((builder, switchMappings) =>
                builder.AddCommandLine(args, switchMappings), args);

        void InitialiseServices(HostBuilderContext context, IServiceCollection services)
        {
            //var configuration = context.Configuration.GetSection(WorkerOptions.SectionName);
            services.Configure<WorkerOptions>(context.Configuration);
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddFileTransformerServices(args);
            services.AddHostedService<Worker>();
        }
    }
}