using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.Validation;

public class AllowedDynamicTabDataSourceAttribute : ValidationAttribute
{
    private static readonly string[] AllowedSources = new[]
    {
        "excel_upload", "database_query", "api", "manual", "form"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var source = value.ToString()?.ToLower();
        if (string.IsNullOrEmpty(source) || !AllowedSources.Contains(source))
        {
            return new ValidationResult(
                $"Invalid data source. Allowed sources are: {string.Join(", ", AllowedSources)}");
        }

        return ValidationResult.Success;
    }
}
