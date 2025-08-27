using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.Validation;

public class AllowedDynamicTabDataTypeAttribute : ValidationAttribute
{
    private static readonly string[] AllowedTypes = new[]
    {
        "string", "number", "date", "boolean", "email",
        "phone", "url", "currency", "percentage", "file",
        "select", "multiselect", "textarea", "richtext"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var dataType = value.ToString()?.ToLower();
        if (string.IsNullOrEmpty(dataType) || !AllowedTypes.Contains(dataType))
        {
            return new ValidationResult(
                $"Invalid data type. Allowed types are: {string.Join(", ", AllowedTypes)}");
        }

        return ValidationResult.Success;
    }
}
