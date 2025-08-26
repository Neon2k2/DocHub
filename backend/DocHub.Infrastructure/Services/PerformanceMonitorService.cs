using DocHub.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Services;

public class PerformanceMonitorService : IPerformanceMonitorService
{
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly Dictionary<string, List<double>> _metrics = new();
    private readonly Dictionary<string, List<TimeSpan>> _timings = new();
    private readonly Dictionary<string, long> _counters = new();
    private readonly object _lock = new object();

    public PerformanceMonitorService(ILogger<PerformanceMonitorService> logger)
    {
        _logger = logger;
    }

    public IOperationContext StartOperation(string operationName)
    {
        return new OperationContext(this, operationName);
    }

    public void RecordMetric(string metricName, double value, string unit = "")
    {
        lock (_lock)
        {
            if (!_metrics.ContainsKey(metricName))
            {
                _metrics[metricName] = new List<double>();
            }
            _metrics[metricName].Add(value);
        }

        _logger.LogDebug("Performance Metric: {MetricName} = {Value} {Unit}", metricName, value, unit);
    }

    public void RecordTiming(string operationName, TimeSpan duration)
    {
        lock (_lock)
        {
            if (!_timings.ContainsKey(operationName))
            {
                _timings[operationName] = new List<TimeSpan>();
            }
            _timings[operationName].Add(duration);
        }

        _logger.LogDebug("Performance Timing: {OperationName} took {Duration}ms", operationName, duration.TotalMilliseconds);
    }

    public void IncrementCounter(string counterName, long increment = 1)
    {
        lock (_lock)
        {
            if (!_counters.ContainsKey(counterName))
            {
                _counters[counterName] = 0;
            }
            _counters[counterName] += increment;
        }

        _logger.LogDebug("Performance Counter: {CounterName} incremented by {Increment}", counterName, increment);
    }

    public PerformanceStatistics GetPerformanceStatistics()
    {
        lock (_lock)
        {
            return new PerformanceStatistics
            {
                Metrics = new Dictionary<string, List<double>>(_metrics),
                Timings = new Dictionary<string, List<TimeSpan>>(_timings),
                Counters = new Dictionary<string, long>(_counters)
            };
        }
    }

    private class OperationContext : IOperationContext
    {
        private readonly PerformanceMonitorService _monitor;
        private readonly DateTime _startTime;
        private bool _disposed = false;

        public string OperationName { get; }
        public DateTime StartTime => _startTime;

        public OperationContext(PerformanceMonitorService monitor, string operationName)
        {
            _monitor = monitor;
            OperationName = operationName;
            _startTime = DateTime.UtcNow;
        }

        public void MarkSuccess()
        {
            if (!_disposed)
            {
                var duration = DateTime.UtcNow - _startTime;
                _monitor.RecordTiming(OperationName, duration);
                _monitor.IncrementCounter($"{OperationName}_success");
            }
        }

        public void MarkFailure(string error)
        {
            if (!_disposed)
            {
                var duration = DateTime.UtcNow - _startTime;
                _monitor.RecordTiming(OperationName, duration);
                _monitor.IncrementCounter($"{OperationName}_failure");
                _monitor._logger.LogWarning("Operation {OperationName} failed after {Duration}ms: {Error}", 
                    OperationName, duration.TotalMilliseconds, error);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                // Auto-mark as success if not explicitly marked
                MarkSuccess();
            }
        }
    }
}
