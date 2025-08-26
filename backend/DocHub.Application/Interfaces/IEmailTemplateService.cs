namespace DocHub.Application.Interfaces;

public interface IEmailTemplateService
{
    Task<string> RenderEmailTemplateAsync(string templateName, object model);
    Task<string> RenderCustomTemplateAsync(string templateContent, object model);
    Task<IEnumerable<EmailTemplateInfo>> GetAvailableTemplatesAsync();
    Task<EmailTemplateInfo?> GetTemplateAsync(string templateName);
    Task<bool> SaveTemplateAsync(string templateName, string content, string description = "");
    Task<bool> DeleteTemplateAsync(string templateName);
    Task<string> GetDefaultTemplateAsync(string templateType);
    Task<bool> ValidateTemplateAsync(string templateContent);
    Task<string> PreviewTemplateAsync(string templateName, Dictionary<string, string> placeholders);
    Task<IEnumerable<string>> GetAvailableTemplateTypesAsync();
}

public class EmailTemplateInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDefault { get; set; }
}
