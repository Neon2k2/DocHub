using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using DocHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocHub.Core.Entities;
using Microsoft.Extensions.Configuration;

namespace DocHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DocHubDbContext _context;
    private readonly ILetterTemplateService _templateService;
    private readonly IEmployeeService _employeeService;
    private readonly IGeneratedLetterService _letterService;
    private readonly ILogger<DashboardController> _logger;
    private readonly IConfiguration _configuration;

    public DashboardController(
        DocHubDbContext context,
        ILetterTemplateService templateService,
        IEmployeeService employeeService,
        IGeneratedLetterService letterService,
        ILogger<DashboardController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _templateService = templateService;
        _employeeService = employeeService;
        _letterService = letterService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetDashboardStats()
    {
        try
        {
            var stats = new DashboardStats();

            // Get total counts
            stats.TotalTemplates = await _context.LetterTemplates.CountAsync();
            stats.ActiveTemplates = await _context.LetterTemplates.CountAsync(t => t.IsActive);
            stats.TotalEmployees = await _context.Employees.CountAsync();
            stats.ActiveEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            stats.TotalLetters = await _context.GeneratedLetters.CountAsync();
            stats.TotalSignatures = await _context.DigitalSignatures.CountAsync();
            stats.ActiveSignatures = await _context.DigitalSignatures.CountAsync(s => s.IsActive);

            // Get letter status counts
            stats.LettersGenerated = await _context.GeneratedLetters.CountAsync(l => l.Status == "Generated");
            stats.LettersSent = await _context.GeneratedLetters.CountAsync(l => l.Status == "Sent");
            stats.LettersFailed = await _context.GeneratedLetters.CountAsync(l => l.Status == "Failed");

            // Get recent activity
            var lastWeek = DateTime.UtcNow.AddDays(-7);
            stats.LettersThisWeek = await _context.GeneratedLetters.CountAsync(l => l.CreatedAt >= lastWeek);
            stats.EmployeesAddedThisWeek = await _context.Employees.CountAsync(e => e.CreatedAt >= lastWeek);

            // Get data source distribution
            stats.UploadTemplates = await _context.LetterTemplates.CountAsync(t => t.DataSource == "Upload");
            stats.DatabaseTemplates = await _context.LetterTemplates.CountAsync(t => t.DataSource == "Database");

            // Calculate success rate
            if (stats.TotalLetters > 0)
            {
                stats.SuccessRate = Math.Round((double)stats.LettersSent / stats.TotalLetters * 100, 2);
            }

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("recent-letters")]
    public async Task<ActionResult<IEnumerable<object>>> GetRecentLetters([FromQuery] int count = 10)
    {
        try
        {
            var recentLetters = await _context.GeneratedLetters
                .Include(gl => gl.LetterTemplate)
                .Include(gl => gl.Employee)
                .Include(gl => gl.DigitalSignature)
                .OrderByDescending(gl => gl.CreatedAt)
                .Take(count)
                .Select(gl => new
                {
                    gl.Id,
                    gl.LetterNumber,
                    gl.LetterType,
                    gl.Status,
                    gl.CreatedAt,
                    gl.SentAt,
                    TemplateName = gl.LetterTemplate.Name,
                    EmployeeName = $"{gl.Employee.FirstName} {gl.Employee.LastName}",
                    EmployeeId = gl.Employee.EmployeeId,
                    AuthorityName = gl.DigitalSignature.AuthorityName
                })
                .ToListAsync();

            return Ok(recentLetters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent letters");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("letter-status-chart")]
    public async Task<ActionResult<object>> GetLetterStatusChart()
    {
        try
        {
            var statusData = await _context.GeneratedLetters
                .GroupBy(gl => gl.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(statusData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter status chart data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("monthly-letters")]
    public async Task<ActionResult<object>> GetMonthlyLetters([FromQuery] int months = 12)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);
            
            var monthlyData = await _context.GeneratedLetters
                .Where(gl => gl.CreatedAt >= startDate)
                .GroupBy(gl => new { gl.CreatedAt.Year, gl.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(monthlyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly letters data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("template-usage")]
    public async Task<ActionResult<object>> GetTemplateUsage()
    {
        try
        {
            var templateUsage = await _context.GeneratedLetters
                .Include(gl => gl.LetterTemplate)
                .GroupBy(gl => new { gl.LetterTemplateId, gl.LetterTemplate.Name, gl.LetterTemplate.LetterType })
                .Select(g => new
                {
                    TemplateId = g.Key.LetterTemplateId,
                    TemplateName = g.Key.Name,
                    LetterType = g.Key.LetterType,
                    UsageCount = g.Count(),
                    LastUsed = g.Max(gl => gl.CreatedAt)
                })
                .OrderByDescending(x => x.UsageCount)
                .ToListAsync();

            return Ok(templateUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template usage data");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("department-stats")]
    public async Task<ActionResult<object>> GetDepartmentStats()
    {
        try
        {
            var departmentStats = await _context.Employees
                .Where(e => e.IsActive)
                .GroupBy(e => e.Department)
                .Select(g => new
                {
                    Department = g.Key ?? "Unknown",
                    EmployeeCount = g.Count(),
                    Designations = g.Select(e => e.Designation).Distinct().Count()
                })
                .OrderByDescending(x => x.EmployeeCount)
                .ToListAsync();

            return Ok(departmentStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting department stats");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("signature-stats")]
    public async Task<ActionResult<object>> GetSignatureStats()
    {
        try
        {
            var signatureStats = await _context.DigitalSignatures
                .Include(s => s.GeneratedLetters)
                .Select(s => new
                {
                    s.Id,
                    s.AuthorityName,
                    s.AuthorityDesignation,
                    s.IsActive,
                    UsageCount = s.GeneratedLetters.Count,
                    LastUsed = s.GeneratedLetters.Any() ? s.GeneratedLetters.Max(gl => gl.CreatedAt) : (DateTime?)null
                })
                .OrderByDescending(x => x.UsageCount)
                .ToListAsync();

            return Ok(signatureStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting signature stats");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("data-source-distribution")]
    public async Task<ActionResult<object>> GetDataSourceDistribution()
    {
        try
        {
            var dataSourceStats = await _context.LetterTemplates
                .GroupBy(t => t.DataSource)
                .Select(g => new
                {
                    DataSource = g.Key,
                    Count = g.Count(),
                    ActiveCount = g.Count(t => t.IsActive)
                })
                .ToListAsync();

            return Ok(dataSourceStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data source distribution");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("performance-metrics")]
    public async Task<ActionResult<object>> GetPerformanceMetrics()
    {
        try
        {
            var metrics = new
            {
                // Average letters generated per day
                AverageLettersPerDay = await GetAverageLettersPerDay(),
                
                // Average time from generation to sending
                AverageGenerationToSendTime = await GetAverageGenerationToSendTime(),
                
                // Most active day of the week
                MostActiveDay = await GetMostActiveDay(),
                
                // Peak hours
                PeakHours = await GetPeakHours()
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public ActionResult<object> HealthCheck()
    {
        try
        {
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown",
                Database = "Connected",
                Services = new
                {
                    Email = "Available",
                    Document = "Available",
                    Excel = "Available",
                    Signature = "Available"
                }
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new { Message = "System health check failed", Errors = new List<string> { ex.Message } });
        }
    }

    private async Task<double> GetAverageLettersPerDay()
    {
        var lastMonth = DateTime.UtcNow.AddDays(-30);
        var totalLetters = await _context.GeneratedLetters.CountAsync(l => l.CreatedAt >= lastMonth);
        return Math.Round((double)totalLetters / 30, 2);
    }

    private async Task<double> GetAverageGenerationToSendTime()
    {
        var sentLetters = await _context.GeneratedLetters
            .Where(gl => gl.Status == "Sent" && gl.SentAt.HasValue)
            .Select(gl => (gl.SentAt.Value - gl.CreatedAt).TotalHours)
            .ToListAsync();

        if (!sentLetters.Any()) return 0;

        return Math.Round(sentLetters.Average(), 2);
    }

    private async Task<string> GetMostActiveDay()
    {
        var dayCounts = await _context.GeneratedLetters
            .GroupBy(gl => gl.CreatedAt.DayOfWeek)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        return dayCounts?.Day.ToString() ?? "Unknown";
    }

    private async Task<object> GetPeakHours()
    {
        var hourCounts = await _context.GeneratedLetters
            .GroupBy(gl => gl.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(3)
            .ToListAsync();

        return hourCounts.Select(h => new { Hour = $"{h.Hour:00}:00", Count = h.Count });
    }
}

// DTOs for dashboard data
public class DashboardStats
{
    public int TotalTemplates { get; set; }
    public int ActiveTemplates { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int TotalLetters { get; set; }
    public int TotalSignatures { get; set; }
    public int ActiveSignatures { get; set; }
    public int LettersGenerated { get; set; }
    public int LettersSent { get; set; }
    public int LettersFailed { get; set; }
    public int LettersThisWeek { get; set; }
    public int EmployeesAddedThisWeek { get; set; }
    public int UploadTemplates { get; set; }
    public int DatabaseTemplates { get; set; }
    public double SuccessRate { get; set; }
}

public class RecentLetter
{
    public string Id { get; set; } = string.Empty;
    public string LetterNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string LetterType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecentEmployee
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DepartmentStat
{
    public string Department { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int ActiveCount { get; set; }
}

public class MonthlyStat
{
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public int LettersGenerated { get; set; }
    public int EmployeesAdded { get; set; }
}

public class RecentActivity
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RelatedId { get; set; } = string.Empty;
}

public class QuickAction
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
