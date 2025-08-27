using FluentValidation;
using DocHub.Application.DTOs.DynamicTabs;

namespace DocHub.Application.Validation;

public class UpdateDynamicTabDtoValidator : AbstractValidator<UpdateDynamicTabDto>
{
    public UpdateDynamicTabDtoValidator()
    {
        RuleFor(x => x.DisplayName)
            .MaximumLength(200)
            .Matches(@"^[a-zA-Z0-9\s_-]+$")
            .When(x => !string.IsNullOrEmpty(x.DisplayName))
            .WithMessage("Display name can only contain letters, numbers, spaces, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.TabType)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "letter", "database", "upload" }.Contains(x.ToLower()))
            .WithMessage("TabType must be either 'letter', 'database', or 'upload'");

        RuleFor(x => x.DataSource)
            .Must(x => string.IsNullOrEmpty(x) || new[] { "excel_upload", "database_query", "api", "manual", "form" }.Contains(x.ToLower()))
            .WithMessage("Invalid data source type");

        When(x => x.DataSource == "database_query", () =>
        {
            RuleFor(x => x.DatabaseQuery)
                .NotEmpty()
                .Must(ContainsSafeQuery)
                .WithMessage("The query contains potentially dangerous operations. Only SELECT statements are allowed.");
        });

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue);

        RuleFor(x => x.Icon)
            .Matches(@"^[a-zA-Z0-9_-]*$")
            .When(x => !string.IsNullOrEmpty(x.Icon))
            .WithMessage("Icon can only contain letters, numbers, underscores, and hyphens");

        RuleFor(x => x.Color)
            .Matches(@"^#?[a-fA-F0-9]{6}$")
            .When(x => !string.IsNullOrEmpty(x.Color))
            .WithMessage("Color must be a valid hex color code");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty();

        RuleForEach(x => x.Fields)
            .SetValidator(new CreateDynamicTabFieldDtoValidator())
            .When(x => x.Fields != null);
    }

    private bool ContainsSafeQuery(string? query)
    {
        if (string.IsNullOrEmpty(query)) return true;

        var dangerousKeywords = new[]
        {
            "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "CREATE", "TRUNCATE",
            "EXEC", "EXECUTE", "sp_", "xp_", "--", ";", "/*", "*/"
        };

        var upperQuery = query.ToUpper();
        return !dangerousKeywords.Any(keyword => upperQuery.Contains(keyword)) &&
               upperQuery.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
    }
}
