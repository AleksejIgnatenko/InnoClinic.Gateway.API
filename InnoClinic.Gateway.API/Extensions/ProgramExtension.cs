using Consul;
using InnoClinic.Gateway.API.Middlewares;
using InnoClinic.Gateway.Infrastructure.Options;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Serilog;

namespace InnoClinic.Gateway.API.Extensions;

/// <summary>
/// Contains extension methods for configuring the web application builder and application startup.
/// </summary>
public static class ProgramExtension
{
    /// <summary>
    /// Configures the web application builder with necessary services and configurations.
    /// </summary>
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .CreateSerilog(builder.Host);

        builder.Configuration
            .AddEnvironmentVariables()
            .LoadConfiguration();

        builder.Services
            .AddOptions(builder.Configuration)
            .AddSwaggerGen()
            .AddEndpointsApiExplorer()
            .AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(config =>
            {
                config.Address = new Uri("http://localhost:8500");
            }))
            .AddCors(options =>
            {
                var serviceProvider = builder.Services.BuildServiceProvider();
                var customCorsOptions = serviceProvider.GetRequiredService<IOptions<CustomCorsOptions>>();
                options.ConfigureAllowAllCors(customCorsOptions);
            })
            .AddControllers();


        builder.Services
            .AddOcelot(builder.Configuration)
            .AddConsul();

        return builder;
    }

    /// <summary>
    /// Configures the web application with necessary middleware and services during startup.
    /// </summary>
    public static async Task<WebApplication> ConfigureApplicationAsync(this WebApplication app)
    {
        app.UseCustomExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseCors("CorsPolicy");
        app.MapControllers();

        await app.UseOcelot();

        return app;
    }

    private static IConfiguration LoadConfiguration(this IConfigurationBuilder configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
            .Build();
    }

    private static IServiceCollection AddOptions(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.Configure<CustomCorsOptions>(configuration.GetSection(nameof(CustomCorsOptions)));

        return services;
    }

    private static CorsOptions ConfigureAllowAllCors(this CorsOptions options, IOptions<CustomCorsOptions> customCorsOptions)
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.WithHeaders().AllowAnyHeader();
            policy.WithOrigins(customCorsOptions.Value.AllowedOrigins);
            policy.WithMethods().AllowAnyMethod();
            policy.AllowCredentials();
        });

        return options;
    }

    private static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder webApplication)
    {
        webApplication.UseMiddleware<ExceptionHandlerMiddleware>();

        return webApplication;
    }
}