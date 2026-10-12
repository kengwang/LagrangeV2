using Lagrange.Milky.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Json;

namespace Lagrange.Milky.Extensions;

/// <summary>Configures Milky's console and optional JSON file logging.</summary>
public static class LoggingBuilderExtension
{
    /// <summary>Registers Serilog while preserving Microsoft logging category filters.</summary>
    /// <param name="builder">Application builder supplying configuration and content root.</param>
    /// <returns>The root logger, which the caller must dispose after disposing the host.</returns>
    public static Logger ConfigureMilkyLogging(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var file = builder.Configuration.GetSection("Logging:JsonFile").Get<JsonFileLoggingConfiguration>() ?? new();
        var configuration = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");

        if (file.Enabled)
        {
            if (string.IsNullOrWhiteSpace(file.Path))
                throw new ArgumentException("Logging:JsonFile:Path must not be blank.");
            if (file.RetainedFileCountLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(file.RetainedFileCountLimit), "Logging:JsonFile:RetainedFileCountLimit must be positive.");
            if (file.FileSizeLimitBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(file.FileSizeLimitBytes), "Logging:JsonFile:FileSizeLimitBytes must be positive.");

            configuration.WriteTo.File(new JsonFormatter(renderMessage: true),
                Path.GetFullPath(file.Path, builder.Environment.ContentRootPath),
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: file.RetainedFileCountLimit,
                fileSizeLimitBytes: file.FileSizeLimitBytes);
        }

        var logger = configuration.CreateLogger();
        builder.Logging.ClearProviders();
        // AddSerilog() installs a provider-level Trace filter that overrides Logging:LogLevel.
        // Register directly so the existing LoggerFactory rules remain authoritative.
        builder.Services.AddSingleton<ILoggerProvider>(_ => new SerilogLoggerProvider(logger, dispose: false));
        return logger;
    }
}
