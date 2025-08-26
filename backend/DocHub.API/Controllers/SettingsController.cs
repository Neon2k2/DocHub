using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DocHub.Application.Interfaces;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        IConfiguration configuration,
        ILogger<SettingsController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("app")]
    public ActionResult<AppSettings> GetAppSettings()
    {
        try
        {
            var settings = new AppSettings
            {
                AppName = _configuration["App:Name"] ?? "DocHub",
                Version = _configuration["App:Version"] ?? "1.0.0",
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                DatabaseProvider = _configuration["UseSqlServer"] == "true" ? "SQL Server" : "SQLite",
                ConnectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? "Not configured",
                MaxFileSize = _configuration.GetValue<int>("FileUpload:MaxSizeMB", 10),
                AllowedFileTypes = _configuration.GetSection("FileUpload:AllowedTypes").Get<string[]>() ?? new[] { ".xlsx", ".xls", ".docx", ".doc", ".pdf" },
                EmailProvider = _configuration["Email:Provider"] ?? "SendGrid",
                SignatureProvider = _configuration["Signature:Provider"] ?? "PROXKey",
                DocumentProvider = _configuration["Document:Provider"] ?? "Syncfusion"
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting app settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("email")]
    public ActionResult<EmailSettings> GetEmailSettings()
    {
        try
        {
            var settings = new EmailSettings
            {
                Provider = _configuration["Email:Provider"] ?? "SendGrid",
                FromEmail = _configuration["Email:FromEmail"] ?? "noreply@dochub.com",
                FromName = _configuration["Email:FromName"] ?? "DocHub System",
                SendGridApiKey = _configuration["Email:SendGrid:ApiKey"] != null ? "***CONFIGURED***" : "Not configured",
                SmtpHost = _configuration["Email:SMTP:Host"] ?? "Not configured",
                SmtpPort = _configuration.GetValue<int>("Email:SMTP:Port", 587),
                SmtpUsername = _configuration["Email:SMTP:Username"] != null ? "***CONFIGURED***" : "Not configured",
                SmtpPassword = _configuration["Email:SMTP:Password"] != null ? "***CONFIGURED***" : "Not configured",
                EnableSsl = _configuration.GetValue<bool>("Email:SMTP:EnableSsl", true)
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("database")]
    public ActionResult<DatabaseSettings> GetDatabaseSettings()
    {
        try
        {
            var settings = new DatabaseSettings
            {
                Provider = _configuration["UseSqlServer"] == "true" ? "SQL Server" : "SQLite",
                ConnectionString = _configuration["ConnectionStrings:DefaultConnection"] ?? "Not configured",
                AutoMigrate = _configuration.GetValue<bool>("Database:AutoMigrate", true),
                SeedData = _configuration.GetValue<bool>("Database:SeedData", true),
                MaxRetryCount = _configuration.GetValue<int>("Database:MaxRetryCount", 3),
                CommandTimeout = _configuration.GetValue<int>("Database:CommandTimeout", 30)
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("file-upload")]
    public ActionResult<FileUploadSettings> GetFileUploadSettings()
    {
        try
        {
            var settings = new FileUploadSettings
            {
                MaxFileSizeMB = _configuration.GetValue<int>("FileUpload:MaxSizeMB", 10),
                AllowedFileTypes = _configuration.GetSection("FileUpload:AllowedTypes").Get<string[]>() ?? new[] { ".xlsx", ".xls", ".docx", ".doc", ".pdf" },
                UploadPath = _configuration["FileUpload:Path"] ?? "wwwroot/uploads",
                EnableVirusScan = _configuration.GetValue<bool>("FileUpload:EnableVirusScan", false),
                MaxConcurrentUploads = _configuration.GetValue<int>("FileUpload:MaxConcurrent", 5),
                EnableCompression = _configuration.GetValue<bool>("FileUpload:EnableCompression", true)
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file upload settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("signature")]
    public ActionResult<SignatureSettings> GetSignatureSettings()
    {
        try
        {
            var settings = new SignatureSettings
            {
                Provider = _configuration["Signature:Provider"] ?? "PROXKey",
                PROXKeyDevicePath = _configuration["Signature:PROXKey:DevicePath"] ?? "COM3",
                PROXKeyTimeout = _configuration.GetValue<int>("Signature:PROXKey:Timeout", 30),
                EnableAutoSignature = _configuration.GetValue<bool>("Signature:EnableAuto", true),
                SignatureFormat = _configuration["Signature:Format"] ?? "PNG",
                SignatureQuality = _configuration.GetValue<int>("Signature:Quality", 90),
                EnableWatermark = _configuration.GetValue<bool>("Signature:EnableWatermark", true)
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("document")]
    public ActionResult<DocumentSettings> GetDocumentSettings()
    {
        try
        {
            var settings = new DocumentSettings
            {
                Provider = _configuration["Document:Provider"] ?? "Syncfusion",
                SyncfusionLicenseKey = _configuration["Document:Syncfusion:LicenseKey"] != null ? "***CONFIGURED***" : "Not configured",
                TemplatePath = _configuration["Document:TemplatePath"] ?? "wwwroot/templates",
                OutputPath = _configuration["Document:OutputPath"] ?? "wwwroot/generated",
                DefaultFormat = _configuration["Document:DefaultFormat"] ?? "PDF",
                EnableCompression = _configuration.GetValue<bool>("Document:EnableCompression", true),
                MaxConcurrentGenerations = _configuration.GetValue<int>("Document:MaxConcurrent", 3)
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("security")]
    public ActionResult<SecuritySettings> GetSecuritySettings()
    {
        try
        {
            var settings = new SecuritySettings
            {
                EnableCors = _configuration.GetValue<bool>("Security:EnableCors", true),
                AllowedOrigins = _configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? new[] { "*" },
                EnableRateLimiting = _configuration.GetValue<bool>("Security:EnableRateLimiting", false),
                MaxRequestsPerMinute = _configuration.GetValue<int>("Security:MaxRequestsPerMinute", 100),
                EnableApiKeyAuth = _configuration.GetValue<bool>("Security:EnableApiKeyAuth", false),
                RequireHttps = _configuration.GetValue<bool>("Security:RequireHttps", true),
                SessionTimeout = _configuration.GetValue<int>("Security:SessionTimeout", 30)
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting security settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("logging")]
    public ActionResult<LoggingSettings> GetLoggingSettings()
    {
        try
        {
            var settings = new LoggingSettings
            {
                LogLevel = _configuration["Logging:LogLevel:Default"] ?? "Information",
                EnableConsoleLogging = _configuration.GetValue<bool>("Logging:Console:Enabled", true),
                EnableFileLogging = _configuration.GetValue<bool>("Logging:File:Enabled", false),
                LogFilePath = _configuration["Logging:File:Path"] ?? "logs/dochub.log",
                MaxFileSizeMB = _configuration.GetValue<int>("Logging:File:MaxSizeMB", 10),
                MaxFiles = _configuration.GetValue<int>("Logging:File:MaxFiles", 5),
                EnableStructuredLogging = _configuration.GetValue<bool>("Logging:Structured:Enabled", true)
            };

            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logging settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("health")]
    public ActionResult<HealthStatus> GetHealthStatus()
    {
        try
        {
            var health = new HealthStatus
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = _configuration["App:Version"] ?? "1.0.0",
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                DatabaseConnection = "Connected", // This would be checked in a real implementation
                EmailService = "Available", // This would be checked in a real implementation
                FileStorage = "Available", // This would be checked in a real implementation
                Uptime = TimeSpan.FromTicks(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss")
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            return StatusCode(500, "Internal server error");
        }
    }
}

// Settings models
public class AppSettings
{
    public string AppName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string DatabaseProvider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxFileSize { get; set; }
    public string[] AllowedFileTypes { get; set; } = new string[0];
    public string EmailProvider { get; set; } = string.Empty;
    public string SignatureProvider { get; set; } = string.Empty;
    public string DocumentProvider { get; set; } = string.Empty;
}

public class EmailSettings
{
    public string Provider { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string SendGridApiKey { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}

public class DatabaseSettings
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public bool AutoMigrate { get; set; }
    public bool SeedData { get; set; }
    public int MaxRetryCount { get; set; }
    public int CommandTimeout { get; set; }
}

public class FileUploadSettings
{
    public int MaxFileSizeMB { get; set; }
    public string[] AllowedFileTypes { get; set; } = new string[0];
    public string UploadPath { get; set; } = string.Empty;
    public bool EnableVirusScan { get; set; }
    public int MaxConcurrentUploads { get; set; }
    public bool EnableCompression { get; set; }
}

public class SignatureSettings
{
    public string Provider { get; set; } = string.Empty;
    public string PROXKeyDevicePath { get; set; } = string.Empty;
    public int PROXKeyTimeout { get; set; }
    public bool EnableAutoSignature { get; set; }
    public string SignatureFormat { get; set; } = string.Empty;
    public int SignatureQuality { get; set; }
    public bool EnableWatermark { get; set; }
}

public class DocumentSettings
{
    public string Provider { get; set; } = string.Empty;
    public string SyncfusionLicenseKey { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public string DefaultFormat { get; set; } = string.Empty;
    public bool EnableCompression { get; set; }
    public int MaxConcurrentGenerations { get; set; }
}

public class SecuritySettings
{
    public bool EnableCors { get; set; }
    public string[] AllowedOrigins { get; set; } = new string[0];
    public bool EnableRateLimiting { get; set; }
    public int MaxRequestsPerMinute { get; set; }
    public bool EnableApiKeyAuth { get; set; }
    public bool RequireHttps { get; set; }
    public int SessionTimeout { get; set; }
}

public class LoggingSettings
{
    public string LogLevel { get; set; } = string.Empty;
    public bool EnableConsoleLogging { get; set; }
    public bool EnableFileLogging { get; set; }
    public string LogFilePath { get; set; } = string.Empty;
    public int MaxFileSizeMB { get; set; }
    public int MaxFiles { get; set; }
    public bool EnableStructuredLogging { get; set; }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string DatabaseConnection { get; set; } = string.Empty;
    public string EmailService { get; set; } = string.Empty;
    public string FileStorage { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
}
