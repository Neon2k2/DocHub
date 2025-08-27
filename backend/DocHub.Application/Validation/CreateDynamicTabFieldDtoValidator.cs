using FluentValidation;
using DocHub.Application.DTOs.DynamicTabs;

namespace DocHub.Application.Validation;

public class CreateDynamicTabFieldDtoValidator : AbstractValidator<CreateDynamicTabFieldDto>
{
    private static readonly string[] AllowedTypes = new[]
    {
        "string", "number", "date", "boolean", "email",
        "phone", "url", "currency", "percentage", "file",
        "select", "multiselect", "textarea", "richtext"
    };

    public CreateDynamicTabFieldDtoValidator()
    {
        RuleFor(x => x.FieldName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Field name can only contain letters, numbers, and underscores");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DataType)
            .NotEmpty()
            .Must(x => AllowedTypes.Contains(x.ToLower()))
            .WithMessage($"Invalid data type. Allowed types are: {string.Join(", ", AllowedTypes)}");

        RuleFor(x => x.ValidationRules)
            .Must(BeValidJson)
            .When(x => !string.IsNullOrEmpty(x.ValidationRules))
            .WithMessage("Validation rules must be valid JSON");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ExcelColumnName)
            .MaximumLength(100);

        RuleFor(x => x.DatabaseColumnName)
            .MaximumLength(100);
    }

    private bool BeValidJson(string? json)
    {
        if (string.IsNullOrEmpty(json)) return true;
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
