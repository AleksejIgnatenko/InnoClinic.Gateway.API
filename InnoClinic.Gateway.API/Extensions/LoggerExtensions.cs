using Serilog;
using Serilog.Core;

namespace InnoClinic.Gateway.API.Extensions;

/// <summary>
/// Contains extension methods for creating and configuring Serilog logger.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Creates a Serilog logger with the specified configuration and integrates it with the host builder.
    /// </summary>
    /// <param name="loggerConfiguration">The Serilog logger configuration.</param>
    /// <param name="hostBuilder">The host builder to integrate the logger with.</param>
    /// <returns>The created Serilog logger.</returns>
    public static Logger CreateSerilog(this LoggerConfiguration loggerConfiguration, IHostBuilder hostBuilder)
    {
        Logger logger = loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        hostBuilder.UseSerilog(logger);

        return logger;
    }
}