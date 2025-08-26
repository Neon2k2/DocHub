namespace DocHub.Application.Interfaces;

public interface IPerformanceMonitorService
{
    /// <summary>
    /// Starts monitoring an operation
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <returns>Operation context for tracking</returns>
    IOperationContext StartOperation(string operationName);

    /// <summary>
    /// Records a performance metric
    /// </summary>
    /// <param name="metricName">Name of the metric</param>
    /// <param name="value">Value of the metric</param>
    /// <param name="unit">Unit of measurement</param>
    void RecordMetric(string metricName, double value, string unit = "");

    /// <summary>
    /// Records a timing metric
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="duration">Duration of the operation</param>
    void RecordTiming(string operationName, TimeSpan duration);

    /// <summary>
    /// Records a counter metric
    /// </summary>
    /// <param name="counterName">Name of the counter</param>
    /// <param name="increment">Increment value (default 1)</param>
    void IncrementCounter(string counterName, long increment = 1);

    /// <summary>
    /// Gets performance statistics
    /// </summary>
    /// <returns>Performance statistics summary</returns>
    PerformanceStatistics GetPerformanceStatistics();
}

public interface IOperationContext : IDisposable
{
    /// <summary>
    /// Name of the operation being monitored
    /// </summary>
    string OperationName { get; }

    /// <summary>
    /// Start time of the operation
    /// </summary>
    DateTime StartTime { get; }

    /// <summary>
    /// Marks the operation as successful
    /// </summary>
    void MarkSuccess();

    /// <summary>
    /// Marks the operation as failed
    /// </summary>
    /// <param name="error">Error details</param>
    void MarkFailure(string error);
}

public class PerformanceStatistics
{
    public Dictionary<string, List<double>> Metrics { get; set; } = new();
    public Dictionary<string, List<TimeSpan>> Timings { get; set; } = new();
    public Dictionary<string, long> Counters { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
