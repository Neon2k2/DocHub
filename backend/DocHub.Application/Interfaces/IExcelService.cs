using DocHub.Core.Entities;
using DocHub.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace DocHub.Application.Interfaces
{
    public interface IExcelService
    {
        Task<List<Employee>> ProcessExcelFileAsync(Stream fileStream);
        Task<List<Employee>> ProcessExcelFileAsync(string filePath);
        Task<byte[]> GenerateExcelTemplateAsync(List<string> requiredFields);
        Task<byte[]> ExportEmployeesToExcelAsync(List<Employee> employees);
        Task<Dictionary<string, List<string>>> ValidateExcelDataAsync(List<Employee> employees);
        Task<ExcelValidationResult> ValidateEmployeeDataFromStreamAsync(Stream fileStream);
        Task<List<Employee>> ProcessEmployeeDataAsync(Stream fileStream);
        Task<byte[]> GenerateEmployeeTemplateAsync();
        Task<List<Employee>> ProcessEmployeeExcelFileAsync(string filePath);
        Task<List<string>> GetExcelHeadersAsync(Stream fileStream);
        Task<bool> IsValidExcelFileAsync(Stream fileStream);
        Task<byte[]> CreateSampleExcelAsync();
        Task<List<Employee>> GetEmployeesFromExcelAsync(Stream fileStream, string? sheetName = null);
        Task<Dictionary<string, object>> GetExcelMetadataAsync(Stream fileStream);
        Task<bool> ValidateEmployeeDataAsync(Employee employee);
        Task<List<string>> GetRequiredFieldsAsync();
        Task<byte[]> GenerateBulkUploadTemplateAsync();
        
        // New methods for ExcelDataController
        Task<ExcelDataValidationResult> ValidateExcelDataAsync(IFormFile file);
        Task<ExcelProcessingResult> ProcessExcelDataAsync(string filePath, ExcelProcessingOptions options);
        Task<IEnumerable<ExcelProcessingHistoryDto>> GetProcessingHistoryAsync();
        Task<byte[]> ExportEmployeeDataAsync(List<string> employeeIds, List<string> fields);
        Task<ExcelProcessingStatsDto> GetProcessingStatsAsync();
        Task<ExcelProcessingResult?> RetryProcessingAsync(string processingId);
    }
}
