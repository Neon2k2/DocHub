using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class ProcessExcelRequest
{
    public string FilePath { get; set; } = string.Empty;
    public ExcelProcessingOptions Options { get; set; } = new();
}

public class ExcelProcessingOptions
{
    public bool SkipHeaderRow { get; set; } = true;
    public bool ValidateData { get; set; } = true;
    public bool CreateMissingEmployees { get; set; } = true;
    public bool UpdateExistingEmployees { get; set; } = false;
    public string Department { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}

public class ExportEmployeeRequest
{
    public List<string> EmployeeIds { get; set; } = new();
    public List<string> Fields { get; set; } = new() { "EmployeeId", "FirstName", "LastName", "Email", "Department", "Designation" };
}

public class ExcelDataValidationResult
{
    public bool IsValid { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public List<ExcelValidationError> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public class ExcelValidationError
{
    public int RowNumber { get; set; }
    public string Column { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ExcelProcessingResult
{
    public string ProcessingId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int TotalProcessed { get; set; }
    public int SuccessfullyProcessed { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
}

public class ExcelProcessingHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public DateTime ProcessedAt { get; set; }
    public string ProcessedBy { get; set; } = string.Empty;
}

public class ExcelProcessingStatsDto
{
    public int TotalFilesProcessed { get; set; }
    public int TotalEmployeesImported { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastProcessedAt { get; set; }
}
