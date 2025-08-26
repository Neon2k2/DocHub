using Microsoft.Extensions.Configuration;

namespace DocHub.API.Configuration;

public class AppConfiguration
{
    private readonly IConfiguration _configuration;

    public AppConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string SendGridApiKey => 
        _configuration["SendGrid:ApiKey"] ?? 
        Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? 
        throw new InvalidOperationException("SendGrid API key not configured");

    public string SyncfusionLicenseKey => 
        _configuration["Syncfusion:LicenseKey"] ?? 
        Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY") ?? 
        throw new InvalidOperationException("Syncfusion license key not configured");

    public string ConnectionString => 
        _configuration.GetConnectionString("DefaultConnection") ?? 
        Environment.GetEnvironmentVariable("DOCS_DB_CONNECTION") ?? 
        "Data Source=DocHub.db";

    public bool UseSqlServer => 
        _configuration.GetValue<bool>("UseSqlServer") || 
        Environment.GetEnvironmentVariable("USE_SQL_SERVER")?.ToLower() == "true";

    public string ProxKeyDevicePath => 
        _configuration["Signature:PROXKey:DevicePath"] ?? 
        Environment.GetEnvironmentVariable("PROXKEY_DEVICE_PATH") ?? 
        "COM3";

    public int ProxKeyTimeout => 
        _configuration.GetValue<int?>("Signature:PROXKey:Timeout") ?? 
        (int.TryParse(Environment.GetEnvironmentVariable("PROXKEY_TIMEOUT"), out var timeout) ? timeout : 30);

    public string[] AllowedOrigins => 
        _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? 
        new[] { "*" };
}
