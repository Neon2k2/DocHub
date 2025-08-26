using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task<Dictionary<string, object>> GetRealTimeMetricsAsync();
    Task<IEnumerable<ChartData>> GetChartDataAsync(string chartType, DateTime startDate, DateTime endDate);
    Task<CustomReportResult> GenerateCustomReportAsync(CustomReportRequest request);
    Task<byte[]> ExportDashboardDataAsync(string format, DateTime startDate, DateTime endDate);
    Task<IEnumerable<Alert>> GetSystemAlertsAsync();
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();
    Task<IEnumerable<TrendAnalysis>> GetTrendAnalysisAsync(string metric, int days);
}

public class ChartData
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class CustomReportRequest
{
    public string ReportName { get; set; } = string.Empty;
    public List<string> Metrics { get; set; } = new List<string>();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> Filters { get; set; } = new List<string>();
    public string GroupBy { get; set; } = string.Empty;
    public string SortBy { get; set; } = string.Empty;
    public bool Ascending { get; set; } = true;
}

public class CustomReportResult
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<ChartData> Data { get; set; } = new List<ChartData>();
    public Dictionary<string, object> Summary { get; set; } = new Dictionary<string, object>();
    public List<string> Warnings { get; set; } = new List<string>();
}

public class Alert
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class PerformanceMetrics
{
    public double AverageResponseTime { get; set; }
    public int RequestsPerSecond { get; set; }
    public double ErrorRate { get; set; }
    public int ActiveConnections { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DatabaseConnectionPool { get; set; }
}

public class TrendAnalysis
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public double Change { get; set; }
    public double ChangePercentage { get; set; }
    public string Trend { get; set; } = string.Empty;
}
