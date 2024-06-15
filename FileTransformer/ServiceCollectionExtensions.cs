namespace FileTransformer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileTransformerServices(this IServiceCollection services, params string[] args)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (args is { Length: 1 })
        {
            services.AddOptions<WorkerOptions>()
                .Configure(options => options.FolderPath = args[0]);
        }
        services.AddTransient<IFolderHandler, FolderHandler>();
        services.AddTransient<IFileReader, FileReader>();
        services.AddTransient<IProcessExecutionService, ProcessExecutionService>();
        return services;
    }

    public static IConfigurationBuilder AddCommandLineSwitchMappings(this IConfigurationBuilder builder, Action<IConfigurationBuilder, IDictionary<string, string>> action, params string[] args)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (args is { Length: > 1 })
        {
            var switchMappings = new Dictionary<string, string>()
                {
                    // The folder to read from.
                    { "-f", "FolderPath" },
                    // The file to write to.
                    { "-o", "Output" }
                };
            action(builder, switchMappings);
        }
        return builder;
    }
}
