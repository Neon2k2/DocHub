using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace DocHub.Infrastructure.Services.Excel
{
    public class EPPlusExcelService : IExcelService
    {
        private readonly ILogger<EPPlusExcelService> _logger;

        public EPPlusExcelService(ILogger<EPPlusExcelService> logger)
        {
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<List<Employee>> ProcessExcelFileAsync(Stream fileStream)
        {
            try
            {
                var employees = new List<Employee>();
                
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                
                if (worksheet == null)
                {
                    throw new InvalidOperationException("No worksheet found in Excel file");
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2) // Header + at least one data row
                {
                    throw new InvalidOperationException("Excel file must contain at least one data row");
                }

                // Get headers from first row
                var headers = GetHeaders(worksheet, 1);
                
                // Process data rows starting from row 2
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var employee = CreateEmployeeFromRow(worksheet, row, headers);
                        if (employee != null)
                        {
                            employees.Add(employee);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error processing row {Row} in Excel file", row);
                        // Continue processing other rows
                    }
                }

                _logger.LogInformation("Successfully processed {Count} employees from Excel file", employees.Count);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Excel file");
                throw;
            }
        }

        public async Task<List<Employee>> ProcessExcelFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Excel file not found: {filePath}");
            }

            using var fileStream = File.OpenRead(filePath);
            return await ProcessExcelFileAsync(fileStream);
        }

        public async Task<byte[]> GenerateExcelTemplateAsync(List<string> requiredFields)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Employee Template");

                // Add headers
                for (int i = 0; i < requiredFields.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = requiredFields[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Add sample data row
                var sampleData = GetSampleData(requiredFields);
                for (int i = 0; i < requiredFields.Count; i++)
                {
                    worksheet.Cells[2, i + 1].Value = sampleData[i];
                    worksheet.Cells[2, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel template");
                throw;
            }
        }

        public async Task<byte[]> ExportEmployeesToExcelAsync(List<Employee> employees)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Employees");

                // Add headers
                var headers = GetRequiredFields();
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Add data rows
                for (int row = 0; row < employees.Count; row++)
                {
                    var employee = employees[row];
                    var col = 1;

                    worksheet.Cells[row + 2, col++].Value = employee.EmployeeId;
                    worksheet.Cells[row + 2, col++].Value = employee.Name;
                    worksheet.Cells[row + 2, col++].Value = employee.Email;
                    worksheet.Cells[row + 2, col++].Value = employee.PhoneNumber;
                    worksheet.Cells[row + 2, col++].Value = employee.Department;
                    worksheet.Cells[row + 2, col++].Value = employee.Designation;
                    worksheet.Cells[row + 2, col++].Value = employee.JoiningDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[row + 2, col++].Value = employee.IsActive ? "Active" : "Inactive";
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting employees to Excel");
                throw;
            }
        }

        public async Task<Dictionary<string, List<string>>> ValidateExcelDataAsync(List<Employee> employees)
        {
            var validationErrors = new Dictionary<string, List<string>>();

            foreach (var employee in employees)
            {
                var errors = new List<string>();

                // Validate required fields
                if (string.IsNullOrWhiteSpace(employee.EmployeeId))
                    errors.Add("Employee ID is required");
                
                if (string.IsNullOrWhiteSpace(employee.Name))
                    errors.Add("Name is required");
                
                if (string.IsNullOrWhiteSpace(employee.Email))
                    errors.Add("Email is required");

                // Validate email format
                if (!string.IsNullOrWhiteSpace(employee.Email) && !IsValidEmail(employee.Email))
                    errors.Add("Invalid email format");

                // Validate phone number format
                if (!string.IsNullOrWhiteSpace(employee.PhoneNumber) && !IsValidPhoneNumber(employee.PhoneNumber))
                    errors.Add("Invalid phone number format");

                if (errors.Any())
                {
                    validationErrors[employee.EmployeeId ?? "Unknown"] = errors;
                }
            }

            return await Task.FromResult(validationErrors);
        }

        public async Task<List<string>> GetExcelHeadersAsync(Stream fileStream)
        {
            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                
                if (worksheet == null)
                {
                    return new List<string>();
                }

                var headers = GetHeaders(worksheet, 1);
                return await Task.FromResult(headers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Excel headers");
                return new List<string>();
            }
        }

        private List<string> GetRequiredFields()
        {
            return new List<string>
            {
                "Employee ID",
                "Name",
                "Email",
                "Phone Number",
                "Department",
                "Designation",
                "Joining Date",
                "Status"
            };
        }

        public async Task<bool> IsValidExcelFileAsync(Stream fileStream)
        {
            try
            {
                using var package = new ExcelPackage(fileStream);
                return package.Workbook.Worksheets.Any();
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> CreateSampleExcelAsync()
        {
            var requiredFields = GetRequiredFields();
            return await GenerateExcelTemplateAsync(requiredFields);
        }

        public async Task<List<Employee>> GetEmployeesFromExcelAsync(Stream fileStream, string? sheetName = null)
        {
            return await ProcessExcelFileAsync(fileStream);
        }

        public async Task<Dictionary<string, object>> GetExcelMetadataAsync(Stream fileStream)
        {
            try
            {
                using var package = new ExcelPackage(fileStream);
                var metadata = new Dictionary<string, object>
                {
                    ["WorksheetCount"] = package.Workbook.Worksheets.Count,
                    ["WorksheetNames"] = package.Workbook.Worksheets.Select(w => w.Name).ToList(),
                    ["FileSize"] = fileStream.Length,
                    ["LastModified"] = DateTime.Now
                };

                if (package.Workbook.Worksheets.Any())
                {
                    var firstSheet = package.Workbook.Worksheets.First();
                    metadata["FirstSheetName"] = firstSheet.Name;
                    metadata["RowCount"] = firstSheet.Dimension?.Rows ?? 0;
                    metadata["ColumnCount"] = firstSheet.Dimension?.Columns ?? 0;
                }

                return await Task.FromResult(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Excel metadata");
                return await Task.FromResult(new Dictionary<string, object>());
            }
        }

        public async Task<bool> ValidateEmployeeDataAsync(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.EmployeeId) ||
                string.IsNullOrWhiteSpace(employee.Name) ||
                string.IsNullOrWhiteSpace(employee.Email))
            {
                return false;
            }

            return IsValidEmail(employee.Email);
        }

        public async Task<ExcelValidationResult> ValidateEmployeeDataFromStreamAsync(Stream fileStream)
        {
            try
            {
                var employees = await ProcessExcelFileAsync(fileStream);
                var validationErrors = await ValidateExcelDataAsync(employees);
                
                var result = new ExcelValidationResult
                {
                    IsValid = !validationErrors.Any(),
                    Errors = validationErrors.Values.SelectMany(x => x).ToList(),
                    Warnings = new List<string>(),
                    ColumnErrors = validationErrors,
                    RowErrors = new Dictionary<string, List<string>>()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Excel data from stream");
                return new ExcelValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { ex.Message },
                    Warnings = new List<string>(),
                    ColumnErrors = new Dictionary<string, List<string>>(),
                    RowErrors = new Dictionary<string, List<string>>()
                };
            }
        }

        public async Task<List<Employee>> ProcessEmployeeDataAsync(Stream fileStream)
        {
            // This is an alias for ProcessExcelFileAsync
            return await ProcessExcelFileAsync(fileStream);
        }

        public async Task<byte[]> GenerateEmployeeTemplateAsync()
        {
            // This is an alias for GenerateBulkUploadTemplateAsync
            return await GenerateBulkUploadTemplateAsync();
        }

        public async Task<List<Employee>> ProcessEmployeeExcelFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"Excel file not found: {filePath}");
                }

                _logger.LogInformation("Processing employee Excel file: {FilePath}", filePath);
                
                using var fileStream = File.OpenRead(filePath);
                var employees = await ProcessExcelFileAsync(fileStream);
                
                _logger.LogInformation("Successfully processed {Count} employees from Excel file", employees.Count);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing employee Excel file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<List<string>> GetRequiredFieldsAsync()
        {
            return await Task.FromResult(new List<string>
            {
                "EmployeeId",
                "Name",
                "Email",
                "PhoneNumber",
                "Department",
                "Designation",
                "JoiningDate",
                "IsActive"
            });
        }

        public async Task<byte[]> GenerateBulkUploadTemplateAsync()
        {
            var requiredFields = GetRequiredFieldsAsync().Result;
            return await GenerateExcelTemplateAsync(requiredFields);
        }

        private List<string> GetHeaders(ExcelWorksheet worksheet, int row)
        {
            var headers = new List<string>();
            var colCount = worksheet.Dimension?.Columns ?? 0;

            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[row, col].Value?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(header))
                {
                    headers.Add(header);
                }
            }

            return headers;
        }

        private Employee CreateEmployeeFromRow(ExcelWorksheet worksheet, int row, List<string> headers)
        {
            try
            {
                var employee = new Employee
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                for (int col = 0; col < headers.Count; col++)
                {
                    var header = headers[col];
                    var value = worksheet.Cells[row, col + 1].Value?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(value)) continue;

                    switch (header.ToLower())
                    {
                        case "employeeid":
                        case "employee id":
                        case "id":
                        case "emp id":
                            employee.EmployeeId = value;
                            break;
                        case "name":
                        case "fullname":
                        case "full name":
                        case "employee name":
                            // Split the name into components
                            var nameParts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (nameParts.Length >= 1)
                            {
                                employee.FirstName = nameParts[0];
                                if (nameParts.Length >= 2)
                                {
                                    employee.LastName = nameParts[1];
                                    if (nameParts.Length >= 3)
                                    {
                                        employee.MiddleName = string.Join(" ", nameParts.Skip(2));
                                    }
                                }
                            }
                            break;
                        case "firstname":
                        case "first name":
                        case "fname":
                            employee.FirstName = value;
                            break;
                        case "lastname":
                        case "last name":
                        case "lname":
                            employee.LastName = value;
                            break;
                        case "email":
                        case "email address":
                            employee.Email = value;
                            break;
                        case "phonenumber":
                        case "phone number":
                        case "phone":
                        case "mobile":
                        case "contact":
                            employee.PhoneNumber = value;
                            break;
                        case "department":
                        case "dept":
                        case "division":
                            employee.Department = value;
                            break;
                        case "designation":
                        case "title":
                        case "position":
                        case "role":
                            employee.Designation = value;
                            break;
                        case "joiningdate":
                        case "joining date":
                        case "start date":
                        case "hire date":
                            if (DateTime.TryParse(value, out var joiningDate))
                            {
                                // Handle joining date if needed
                            }
                            break;
                        case "isactive":
                        case "active":
                        case "status":
                            if (bool.TryParse(value, out var isActive))
                            {
                                employee.IsActive = isActive;
                            }
                            else if (value.ToLower() == "yes" || value.ToLower() == "y")
                            {
                                employee.IsActive = true;
                            }
                            else if (value.ToLower() == "no" || value.ToLower() == "n")
                            {
                                employee.IsActive = false;
                            }
                            break;
                    }
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(employee.EmployeeId))
                {
                    employee.EmployeeId = $"EMP_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                }

                if (string.IsNullOrWhiteSpace(employee.FirstName))
                {
                    employee.FirstName = "Unknown";
                }

                if (string.IsNullOrWhiteSpace(employee.LastName))
                {
                    employee.LastName = "Employee";
                }

                if (string.IsNullOrWhiteSpace(employee.Email))
                {
                    employee.Email = $"{employee.FirstName.ToLower()}.{employee.LastName.ToLower()}@company.com";
                }

                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee from row {Row}", row);
                return null;
            }
        }

        private List<string> GetSampleData(List<string> fields)
        {
            var sampleData = new List<string>();
            
            foreach (var field in fields)
            {
                switch (field.ToLower())
                {
                    case "employeeid":
                        sampleData.Add("EMP001");
                        break;
                    case "name":
                        sampleData.Add("John Doe");
                        break;
                    case "email":
                        sampleData.Add("john.doe@company.com");
                        break;
                    case "phonenumber":
                        sampleData.Add("+1-555-0123");
                        break;
                    case "department":
                        sampleData.Add("IT");
                        break;
                    case "designation":
                        sampleData.Add("Software Developer");
                        break;
                    case "joiningdate":
                        sampleData.Add("01/01/2024");
                        break;
                    case "isactive":
                        sampleData.Add("True");
                        break;
                    default:
                        sampleData.Add("Sample Data");
                        break;
                }
            }

            return sampleData;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            try
            {
                var regex = new Regex(@"^[\+]?[0-9\s\-\(\)]+$");
                return regex.IsMatch(phoneNumber);
            }
            catch
            {
                return false;
            }
        }

        // New methods for ExcelDataController
        public async Task<ExcelDataValidationResult> ValidateExcelDataAsync(IFormFile file)
        {
            try
            {
                var result = new ExcelDataValidationResult
                {
                    IsValid = true,
                    TotalRows = 0,
                    ValidRows = 0,
                    InvalidRows = 0,
                    Errors = new List<ExcelValidationError>(),
                    Warnings = new List<string>()
                };

                using var stream = file.OpenReadStream();
                var employees = await ProcessExcelFileAsync(stream);
                
                result.TotalRows = employees.Count;
                result.ValidRows = employees.Count(e => !string.IsNullOrWhiteSpace(e.FirstName) && !string.IsNullOrWhiteSpace(e.Email));
                result.InvalidRows = result.TotalRows - result.ValidRows;

                // Add validation errors for invalid rows
                for (int i = 0; i < employees.Count; i++)
                {
                    var employee = employees[i];
                    if (string.IsNullOrWhiteSpace(employee.FirstName) || string.IsNullOrWhiteSpace(employee.Email))
                    {
                        result.Errors.Add(new ExcelValidationError
                        {
                            RowNumber = i + 2, // +2 because Excel is 1-based and we skip header
                            Column = string.IsNullOrWhiteSpace(employee.FirstName) ? "FirstName" : "Email",
                            Error = "Required field is missing",
                            Value = string.IsNullOrWhiteSpace(employee.FirstName) ? employee.FirstName : employee.Email
                        });
                    }
                }

                result.IsValid = result.InvalidRows == 0;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Excel data");
                throw;
            }
        }

        public async Task<ExcelProcessingResult> ProcessExcelDataAsync(string filePath, ExcelProcessingOptions options)
        {
            try
            {
                var processingId = Guid.NewGuid().ToString();
                var result = new ExcelProcessingResult
                {
                    ProcessingId = processingId,
                    Success = true,
                    TotalProcessed = 0,
                    SuccessfullyProcessed = 0,
                    Failed = 0,
                    Errors = new List<string>(),
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = "System" // TODO: Get from current user context
                };

                var employees = await ProcessExcelFileAsync(filePath);
                result.TotalProcessed = employees.Count;
                result.SuccessfullyProcessed = employees.Count;
                
                // TODO: Implement actual employee creation/update logic
                // This would involve calling the EmployeeService

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Excel data");
                throw;
            }
        }

        public async Task<IEnumerable<ExcelProcessingHistoryDto>> GetProcessingHistoryAsync()
        {
            // TODO: Implement actual database query
            // This would involve querying a processing history table
            return new List<ExcelProcessingHistoryDto>
            {
                new ExcelProcessingHistoryDto
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = "Sample_File.xlsx",
                    Status = "Completed",
                    TotalRows = 100,
                    ProcessedRows = 95,
                    ProcessedAt = DateTime.UtcNow.AddDays(-1),
                    ProcessedBy = "System"
                }
            };
        }

        public async Task<byte[]> ExportEmployeeDataAsync(List<string> employeeIds, List<string> fields)
        {
            try
            {
                // TODO: Implement actual employee data retrieval
                // This would involve calling the EmployeeService to get employee data
                
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Employee Export");

                // Add headers
                for (int i = 0; i < fields.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = fields[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // TODO: Add actual employee data rows
                // For now, just add sample data
                worksheet.Cells[2, 1].Value = "Sample Employee";

                return await package.GetAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting employee data");
                throw;
            }
        }

        public async Task<ExcelProcessingStatsDto> GetProcessingStatsAsync()
        {
            // TODO: Implement actual database query
            return new ExcelProcessingStatsDto
            {
                TotalFilesProcessed = 10,
                TotalEmployeesImported = 500,
                SuccessfulImports = 480,
                FailedImports = 20,
                SuccessRate = 96.0,
                LastProcessedAt = DateTime.UtcNow.AddHours(-2)
            };
        }

        public async Task<ExcelProcessingResult?> RetryProcessingAsync(string processingId)
        {
            try
            {
                // TODO: Implement actual retry logic
                // This would involve retrieving the failed processing record and retrying
                
                var result = new ExcelProcessingResult
                {
                    ProcessingId = processingId,
                    Success = true,
                    TotalProcessed = 0,
                    SuccessfullyProcessed = 0,
                    Failed = 0,
                    Errors = new List<string>(),
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = "System"
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying processing {ProcessingId}", processingId);
                return null;
            }
        }
    }
}
