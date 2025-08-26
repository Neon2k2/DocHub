using DocHub.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Services;

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly ILogger<LoggingService> _performanceLogger;
    private readonly ILogger<LoggingService> _securityLogger;
    private readonly ILogger<LoggingService> _auditLogger;
    private readonly ILogger<LoggingService> _businessLogger;

    public LoggingService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LoggingService>();
        _performanceLogger = loggerFactory.CreateLogger<LoggingService>();
        _securityLogger = loggerFactory.CreateLogger<LoggingService>();
        _auditLogger = loggerFactory.CreateLogger<LoggingService>();
        _businessLogger = loggerFactory.CreateLogger<LoggingService>();
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogError(string message, params object[] args)
    {
        _logger.LogError(message, args);
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void LogTrace(string message, params object[] args)
    {
        _logger.LogTrace(message, args);
    }

    public void LogPerformance(string operation, TimeSpan duration, params object[] args)
    {
        var argsList = new List<object> { operation, duration.TotalMilliseconds };
        argsList.AddRange(args);
        
        _performanceLogger.LogInformation(
            "Performance: {Operation} completed in {Duration}ms", 
            argsList.ToArray());
    }

    public void LogSecurityEvent(string eventType, string userId, string details, params object[] args)
    {
        var argsList = new List<object> { eventType, userId, details };
        argsList.AddRange(args);
        
        _securityLogger.LogWarning(
            "Security Event: {EventType} by user {UserId} - {Details}", 
            argsList.ToArray());
    }

    public void LogAuditTrail(string action, string userId, string resource, string details, params object[] args)
    {
        var argsList = new List<object> { action, userId, resource, details };
        argsList.AddRange(args);
        
        _auditLogger.LogInformation(
            "Audit Trail: {Action} on {Resource} by {UserId} - {Details}", 
            argsList.ToArray());
    }

    public void LogBusinessEvent(string eventType, string operation, string userId, object data, params object[] args)
    {
        var argsList = new List<object> { eventType, operation, userId, data };
        argsList.AddRange(args);
        
        _businessLogger.LogInformation(
            "Business Event: {EventType} - {Operation} by {UserId} with data {Data}", 
            argsList.ToArray());
    }
}
