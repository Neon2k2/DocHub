namespace DocHub.Application.Interfaces;

public interface ILoggingService
{
    /// <summary>
    /// Logs an information message
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs an error message
    /// </summary>
    void LogError(string message, params object[] args);

    /// <summary>
    /// Logs an error with exception details
    /// </summary>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// Logs a debug message
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs a trace message
    /// </summary>
    void LogTrace(string message, params object[] args);

    /// <summary>
    /// Logs performance metrics
    /// </summary>
    void LogPerformance(string operation, TimeSpan duration, params object[] args);

    /// <summary>
    /// Logs security events
    /// </summary>
    void LogSecurityEvent(string eventType, string userId, string details, params object[] args);

    /// <summary>
    /// Logs audit trail information
    /// </summary>
    void LogAuditTrail(string action, string userId, string resource, string details, params object[] args);

    /// <summary>
    /// Logs business operation events
    /// </summary>
    void LogBusinessEvent(string eventType, string operation, string userId, object data, params object[] args);
}
