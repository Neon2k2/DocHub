using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocHub.Application.DTOs;
using DocHub.Application.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace DocHub.Infrastructure.Services.ExcelDataProcessing
{
    public class ExcelDataProcessingService : IExcelDataProcessingService
    {
        private readonly ILogger<ExcelDataProcessingService> _logger;

        public ExcelDataProcessingService(ILogger<ExcelDataProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<ExcelValidationResult> ValidateExcelFileAsync(string filePath, ExcelValidationOptions options)
        {
            try
            {
                _logger.LogInformation("Validating Excel file: {FilePath}", filePath);

                var result = new ExcelValidationResult
                {
                    Errors = new List<string>(),
                    Warnings = new List<string>(),
                    FieldErrors = new Dictionary<string, List<string>>()
                };

                await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook(filePath);
                    var worksheet = workbook.Worksheet(1);

                    // Check minimum rows
                    var rowCount = worksheet.RowsUsed().Count();
                    if (rowCount < options.MinimumRows)
                    {
                        result.Errors.Add($"Worksheet has fewer rows ({rowCount}) than required ({options.MinimumRows})");
                    }

                    // Check required fields
                    var headers = GetHeaderRow(worksheet);
                    var missingFields = options.RequiredFields
                        .Where(field => !headers.Contains(field, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var field in missingFields)
                    {
                        result.Errors.Add($"Required field '{field}' is missing");
                    }
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Excel file: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<ExcelTemplateInfo> ExtractExcelTemplateInfoAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Extracting template info from Excel file: {FilePath}", filePath);

                return await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook(filePath);

                    var templateInfo = new ExcelTemplateInfo
                    {
                        FilePath = filePath,
                        TemplateName = Path.GetFileNameWithoutExtension(filePath),
                        ExtractedAt = DateTime.UtcNow,
                        Sheets = new List<SheetInfo>()
                    };

                    foreach (var worksheet in workbook.Worksheets)
                    {
                        var headers = GetHeaderRow(worksheet);
                        var sheetInfo = new SheetInfo
                        {
                            Name = worksheet.Name ?? string.Empty,
                            Headers = headers,
                            DataRowCount = Math.Max(0, worksheet.RowsUsed().Count() - 1),
                            HasHeaderRow = true,
                            DataTypes = new List<string>(),
                            SampleData = new List<string>()
                        };

                        // Get sample data and infer types
                        if (worksheet.RowsUsed().Count() > 1)
                        {
                            var firstDataRow = worksheet.Row(2);
                            foreach (var header in headers)
                            {
                                var columnNumber = headers.IndexOf(header) + 1;
                                var cell = firstDataRow.Cell(columnNumber);
                                sheetInfo.SampleData.Add(cell.GetString());
                                sheetInfo.DataTypes.Add(InferDataType(cell));
                            }
                        }

                        templateInfo.Sheets.Add(sheetInfo);

                        // Add all headers as available fields
                        if (headers.Any())
                        {
                            templateInfo.AvailableFields.AddRange(headers);

                            // Create default mappings (field -> field)
                            foreach (var header in headers)
                            {
                                templateInfo.DefaultMappings[header] = header;
                                templateInfo.FieldTypes[header] = "string"; // Default type
                            }
                        }
                    }

                    // Remove duplicates from available fields
                    templateInfo.AvailableFields = templateInfo.AvailableFields.Distinct().ToList();

                    return templateInfo;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting template info: {FilePath}", filePath);
                throw;
            }
        }

        public async Task<string> ExportDataToExcelAsync(
            List<Dictionary<string, object>> data,
            string sheetName = "Data",
            Dictionary<string, string>? columnHeaders = null)
        {
            try
            {
                _logger.LogInformation("Exporting data to Excel: {RecordCount} records", data.Count);

                var outputFileName = $"Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.xlsx";
                var outputPath = Path.Combine(Path.GetTempPath(), outputFileName);

                await Task.Run(() =>
                {
                    using var workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    if (data.Any())
                    {
                        var firstRow = data.First();
                        var columns = columnHeaders?.Keys.ToList() ?? firstRow.Keys.ToList();

                        // Add headers
                        for (int i = 0; i < columns.Count; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = columnHeaders?[columns[i]] ?? columns[i];
                        }

                        // Add data
                        for (int row = 0; row < data.Count; row++)
                        {
                            var rowData = data[row];
                            for (int col = 0; col < columns.Count; col++)
                            {
                                var cell = worksheet.Cell(row + 2, col + 1);
                                var value = rowData.GetValueOrDefault(columns[col]);
                                if (value != null)
                                {
                                    cell.Value = value switch
                                    {
                                        DateTime dt => dt,
                                        bool b => b,
                                        int i => i,
                                        double d => d,
                                        decimal dec => (double)dec,
                                        _ => value.ToString()
                                    };
                                }
                            }
                        }
                    }

                    workbook.SaveAs(outputPath);
                });

                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data to Excel");
                throw;
            }
        }

        public async Task<DataMappingResult> MapExcelDataToTemplateAsync(
            List<Dictionary<string, object>> excelData,
            string templateId,
            Dictionary<string, string> fieldMappings)
        {
            try
            {
                _logger.LogInformation("Mapping {RecordCount} records to template {TemplateId}",
                    excelData.Count, templateId);

                var result = new DataMappingResult
                {
                    TemplateId = templateId,
                    MappedRecords = new List<MappedRecord>(),
                    Errors = new List<string>(),
                    Warnings = new List<string>(),
                    UnmappedFields = new Dictionary<string, string>()
                };

                await Task.Run(() =>
                {
                    for (int rowIndex = 0; rowIndex < excelData.Count; rowIndex++)
                    {
                        var row = excelData[rowIndex];
                        var mappedRecord = new MappedRecord
                        {
                            RowIndex = rowIndex + 1,
                            MappedData = new Dictionary<string, object>(),
                            UnmappedFields = new List<string>()
                        };

                        foreach (var mapping in fieldMappings)
                        {
                            var sourceField = mapping.Key;
                            var targetField = mapping.Value;

                            if (row.TryGetValue(sourceField, out var value))
                            {
                                mappedRecord.MappedData[targetField] = value;
                            }
                            else
                            {
                                mappedRecord.UnmappedFields.Add(sourceField);
                            }
                        }

                        mappedRecord.IsValid = !mappedRecord.UnmappedFields.Any();
                        result.MappedRecords.Add(mappedRecord);
                    }

                    // Update success status based on mapping results
                    result.Success = result.MappedRecords.All(r => r.IsValid);
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping data to template {TemplateId}", templateId);
                throw;
            }
        }

        public async Task<ExcelProcessingResult> ProcessExcelFileAsync(
            string filePath,
            ExcelProcessingOptions options)
        {
            try
            {
                _logger.LogInformation("Processing Excel file: {FilePath}", filePath);

                var fileExtension = Path.GetExtension(filePath).ToLower();
                return fileExtension switch
                {
                    ".xlsx" => await ProcessXlsxFileAsync(filePath, options),
                    ".xls" => await ProcessXlsFileAsync(filePath, options),
                    _ => throw new NotSupportedException($"File extension {fileExtension} is not supported")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Excel file: {FilePath}", filePath);
                throw;
            }
        }

        private string InferDataType(IXLCell cell)
        {
            if (cell.Value.IsNumber) return "number";
            if (cell.Value.IsDateTime) return "date";
            if (cell.Value.IsBoolean) return "boolean";
            return "string";
        }

        private List<string> GetHeaderRow(IXLWorksheet worksheet)
        {
            var headers = new List<string>();
            var firstRow = worksheet.FirstRowUsed();
            if (firstRow != null)
            {
                foreach (var cell in firstRow.CellsUsed())
                {
                    var value = cell.GetString().Trim();
                    headers.Add(value);
                }
            }
            return headers;
        }

        private async Task<ExcelProcessingResult> ProcessXlsxFileAsync(
            string filePath,
            ExcelProcessingOptions options)
        {
            var result = new ExcelProcessingResult
            {
                ProcessingId = Guid.NewGuid().ToString(),
                FilePath = filePath,
                ProcessedRows = new List<Dictionary<string, object>>(),
                Errors = new List<string>()
            };

            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(options.SheetIndex + 1);

                // Get headers
                var headers = GetHeaderRow(worksheet);
                if (!headers.Any())
                {
                    throw new InvalidOperationException("No headers found in the worksheet");
                }

                // Filter headers if specific columns are selected
                if (options.SelectedColumns.Any())
                {
                    headers = headers.Where(h => options.SelectedColumns.Contains(h)).ToList();
                }

                // Process data rows
                var startRow = 2; // Always assume header row for consistency
                var endRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;

                if (options.MaxRows.HasValue)
                {
                    endRow = Math.Min(endRow, options.MaxRows.Value + 1);
                }

                for (int row = startRow; row <= endRow; row++)
                {
                    var rowData = new Dictionary<string, object>();
                    var hasData = false;

                    foreach (var header in headers)
                    {
                        var columnIndex = worksheet.FirstRowUsed().CellsUsed()
                            .First(c => c.GetString().Trim().Equals(header, StringComparison.OrdinalIgnoreCase))
                            .WorksheetColumn().ColumnNumber();

                        var cell = worksheet.Cell(row, columnIndex);

                        if (!cell.Value.IsBlank)
                        {
                            object value = cell.Value.IsNumber ? cell.Value.GetNumber() :
                                         cell.Value.IsDateTime ? cell.Value.GetDateTime() :
                                         cell.Value.IsBoolean ? cell.Value.GetBoolean() :
                                         cell.GetString();

                            // Apply column mapping if specified
                            var targetField = options.ColumnMappings.GetValueOrDefault(header, header);
                            rowData[targetField] = value;
                            hasData = true;
                        }
                        else
                        {
                            var targetField = options.ColumnMappings.GetValueOrDefault(header, header);
                            rowData[targetField] = string.Empty;
                        }
                    }

                    if (hasData)
                    {
                        result.ProcessedRows.Add(rowData);
                    }
                }
            });

            return result;
        }

        private async Task<ExcelProcessingResult> ProcessXlsFileAsync(
            string filePath,
            ExcelProcessingOptions options)
        {
            // Reuse XLSX processing since ClosedXML handles both formats
            return await ProcessXlsxFileAsync(filePath, options);
        }
    }
}


