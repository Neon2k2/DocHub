using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface IExcelDataProcessingService
{
    /// <summary>
    /// Processes an Excel file and extracts data
    /// </summary>
    Task<ExcelProcessingResult> ProcessExcelFileAsync(
        string filePath,
        ExcelProcessingOptions options);

    /// <summary>
    /// Validates an Excel file for correctness and structure
    /// </summary>
    Task<ExcelValidationResult> ValidateExcelFileAsync(
        string filePath,
        ExcelValidationOptions options);

    /// <summary>
    /// Maps Excel data to template fields
    /// </summary>
    Task<DataMappingResult> MapExcelDataToTemplateAsync(
        List<Dictionary<string, object>> excelData,
        string templateId,
        Dictionary<string, string> fieldMappings);

    /// <summary>
    /// Exports data to Excel format
    /// </summary>
    Task<string> ExportDataToExcelAsync(
        List<Dictionary<string, object>> data,
        string sheetName = "Data",
        Dictionary<string, string>? columnHeaders = null);

    /// <summary>
    /// Extracts template information from an Excel file
    /// </summary>
    Task<ExcelTemplateInfo> ExtractExcelTemplateInfoAsync(string filePath);
}
