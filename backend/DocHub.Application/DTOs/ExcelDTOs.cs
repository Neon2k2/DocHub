using System;
using System.Collections.Generic;

namespace DocHub.Application.DTOs
{
    public class ExcelValidationOptions
    {
        public int MinimumRows { get; set; } = 1;
        public int MaximumRows { get; set; } = 10000;
        public List<string> RequiredFields { get; set; } = new();
        public List<string> ValidDataTypes { get; set; } = new();
        public bool RequireHeaderRow { get; set; } = true;
        public Dictionary<string, string> FieldValidations { get; set; } = new();
    }

    public class ExcelValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class ExcelProcessingOptions
    {
        public int SheetIndex { get; set; }
        public bool HasHeaderRow { get; set; } = true;
        public List<string> RequiredColumns { get; set; } = new();
        public Dictionary<string, string> ColumnMappings { get; set; } = new();
    }

    public class ExcelProcessingResult
    {
        public string ProcessingId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public List<Dictionary<string, object>> ProcessedRows { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalRows { get; set; }
        public int ProcessedCount { get; set; }
        public int ErrorCount { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class ExcelTemplateInfo
    {
        public string TemplateId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public List<string> AvailableFields { get; set; } = new();
        public Dictionary<string, string> DefaultMappings { get; set; } = new();
        public Dictionary<string, string> FieldTypes { get; set; } = new();
        public List<string> RequiredFields { get; set; } = new();
        public string? Description { get; set; }
        public List<SheetInfo> Sheets { get; set; } = new();
        public DateTime ExtractedAt { get; set; }
    }

    public class SheetInfo
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public bool HasHeaderRow { get; set; } = true;
        public int DataRowCount { get; set; }
        public List<string> DataTypes { get; set; } = new();
        public List<string> SampleData { get; set; } = new();
    }

    public class DataMappingResult
    {
        public string TemplateId { get; set; } = string.Empty;
        public List<MappedRecord> MappedRecords { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int TotalRecords => MappedRecords.Count;
        public int SuccessfulRecords => MappedRecords.Count(r => r.IsValid);
        public int FailedRecords => MappedRecords.Count(r => !r.IsValid);
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }

    public class MappedRecord
    {
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> ValidationErrors { get; set; } = new();
        public bool IsValid => ValidationErrors.Count == 0;
    }

    public class ExcelUploadResult
    {
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
