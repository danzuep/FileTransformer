namespace FileTransformer.Extensions;

using System.IO.Abstractions;
using FileTransformer.Abstractions;
using FileTransformer.Models;
using FileTransformer.Modules;
using FileTransformer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileTransformerServices(this IServiceCollection services, params string[] args)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }
        if (args is { Length: 1 })
        {
            services.AddOptions<FolderOptions>()
                .Configure(options => options.ReadDirectory = args[0]);
        }
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddScoped<IFileReader, FileReader>();
        services.AddScoped<IFileWriter, FileWriter>();
        services.AddScoped<IFolderHandler, FolderHandler>();
        services.AddScoped<IExecuteService, FolderHandlerService>();
        services.AddScoped<IFileHandler, FileHandlerFactory>();
        // Register file handlers in the order they should be executed
        services.AddScoped<IExecuteNextHandler, JsonValidationHandler>();
        services.AddScoped<IExecuteNextHandler, JsonFileReaderHandler>();
        return services;
    }

    public static IConfigurationBuilder AddCommandLineSwitchMappings(this IConfigurationBuilder builder, Action<IConfigurationBuilder, IDictionary<string, string>> action, params string[] args)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        if (args is { Length: > 1 })
        {
            var switchMappings = new Dictionary<string, string>()
            {
                // The folder to read from.
                { "-r", $"{FolderOptions.SectionName}:ReadDirectory" },
                // The folder to write to.
                { "-w", $"{FolderOptions.SectionName}:WriteDirectory" }
            };
            action(builder, switchMappings);
        }
        return builder;
    }
}
