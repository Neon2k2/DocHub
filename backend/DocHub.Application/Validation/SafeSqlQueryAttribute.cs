using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DocHub.Application.Validation;

public class SafeSqlQueryAttribute : ValidationAttribute
{
    private static readonly string[] DangerousKeywords = new[]
    {
        "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "CREATE", "TRUNCATE",
        "EXEC", "EXECUTE", "sp_", "xp_", "--", ";", "/*", "*/"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success;

        var query = value.ToString()?.ToUpper() ?? string.Empty;

        // Check for dangerous SQL keywords
        foreach (var keyword in DangerousKeywords)
        {
            if (Regex.IsMatch(query, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
            {
                return new ValidationResult(
                    "The query contains potentially dangerous operations. Only SELECT statements are allowed.");
            }
        }

        // Ensure query starts with SELECT
        if (!query.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult("Only SELECT statements are allowed.");
        }

        return ValidationResult.Success;
    }
}
