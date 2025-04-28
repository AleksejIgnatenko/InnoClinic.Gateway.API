namespace InnoClinic.Gateway.Infrastructure.Options;

/// <summary>
/// Represents the CORS (Cross-Origin Resource Sharing) options for configuring allowed origins.
/// </summary>
public class CustomCorsOptions
{
    /// <summary>
    /// Gets or sets the list of allowed origins for CORS requests.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}