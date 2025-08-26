namespace DocHub.Application.DTOs;

public class ExcelUploadResult
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SkippedRows { get; set; }
    public int ErrorRows { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<EmployeeDto> ProcessedEmployees { get; set; } = new();
    public string? FilePath { get; set; }
    public DateTime ProcessedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

public class ExcelValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, List<string>> ColumnErrors { get; set; } = new();
    public Dictionary<string, List<string>> RowErrors { get; set; } = new();
}
