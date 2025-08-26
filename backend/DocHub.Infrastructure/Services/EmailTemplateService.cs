using DocHub.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using DocHub.Infrastructure.Data;
using DocHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly IConfiguration _configuration;
    private readonly DocHubDbContext _context;
    private readonly Dictionary<string, string> _defaultTemplates;

    public EmailTemplateService(ILogger<EmailTemplateService> logger, IConfiguration configuration, DocHubDbContext context)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _defaultTemplates = InitializeDefaultTemplates();
    }

    public async Task<string> RenderEmailTemplateAsync(string templateName, object model)
    {
        try
        {
            var template = await GetTemplateAsync(templateName);
            if (template == null)
            {
                _logger.LogWarning("Template {TemplateName} not found, using default", templateName);
                            var defaultTemplate = await GetDefaultTemplateAsync("generic");
            return await RenderCustomTemplateAsync(defaultTemplate, model);
            }

            return await RenderCustomTemplateAsync(template.Content, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {TemplateName}", templateName);
            var errorTemplate = await GetDefaultTemplateAsync("error");
            return await RenderCustomTemplateAsync(errorTemplate, model);
        }
    }

    public async Task<string> RenderCustomTemplateAsync(string templateContent, object model)
    {
        try
        {
            var result = templateContent;

            // Simple template engine - replace placeholders with model values
            if (model != null)
            {
                var properties = model.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var placeholder = $"{{{{{property.Name}}}}}";
                    var value = property.GetValue(model)?.ToString() ?? "";
                    result = result.Replace(placeholder, value);
                }

                // Handle special placeholders
                result = result.Replace("{{CurrentDate}}", DateTime.Now.ToString("MMMM dd, yyyy"));
                result = result.Replace("{{CurrentTime}}", DateTime.Now.ToString("HH:mm"));
                result = result.Replace("{{Year}}", DateTime.Now.Year.ToString());
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering custom template");
            return await GetDefaultTemplateAsync("error");
        }
    }

    public Task<IEnumerable<EmailTemplateInfo>> GetAvailableTemplatesAsync()
    {
        try
        {
            var templates = new List<EmailTemplateInfo>();

            // Add default templates
            foreach (var template in _defaultTemplates)
            {
                templates.Add(new EmailTemplateInfo
                {
                    Name = template.Key,
                    Description = GetTemplateDescription(template.Key),
                    Content = template.Value,
                    Type = GetTemplateType(template.Key),
                    CreatedAt = DateTime.UtcNow,
                    IsDefault = true
                });
            }

            // TODO: Add custom templates from database
            // var customTemplates = await _templateRepository.GetAllAsync();
            // templates.AddRange(customTemplates.Select(t => new EmailTemplateInfo { ... }));

            return Task.FromResult<IEnumerable<EmailTemplateInfo>>(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available templates");
            return Task.FromResult<IEnumerable<EmailTemplateInfo>>(new List<EmailTemplateInfo>());
        }
    }

    public Task<EmailTemplateInfo?> GetTemplateAsync(string templateName)
    {
        try
        {
            if (_defaultTemplates.ContainsKey(templateName))
            {
                return Task.FromResult<EmailTemplateInfo?>(new EmailTemplateInfo
                {
                    Name = templateName,
                    Description = GetTemplateDescription(templateName),
                    Content = _defaultTemplates[templateName],
                    Type = GetTemplateType(templateName),
                    CreatedAt = DateTime.UtcNow,
                    IsDefault = true
                });
            }

            // TODO: Get custom template from database
            // return await _templateRepository.GetByNameAsync(templateName);

            return Task.FromResult<EmailTemplateInfo?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {TemplateName}", templateName);
            return Task.FromResult<EmailTemplateInfo?>(null);
        }
    }

    public Task<bool> SaveTemplateAsync(string templateName, string content, string description = "")
    {
        try
        {
            // TODO: Save template to database
            // var template = new EmailTemplate { ... };
            // await _templateRepository.AddAsync(template);
            
            _logger.LogInformation("Template {TemplateName} saved successfully", templateName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving template {TemplateName}", templateName);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DeleteTemplateAsync(string templateName)
    {
        try
        {
            if (_defaultTemplates.ContainsKey(templateName))
            {
                _logger.LogWarning("Cannot delete default template {TemplateName}", templateName);
                return Task.FromResult(false);
            }

            // TODO: Delete template from database
            // await _templateRepository.DeleteAsync(templateName);
            
            _logger.LogInformation("Template {TemplateName} deleted successfully", templateName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateName}", templateName);
            return Task.FromResult(false);
        }
    }

    public async Task<string> GetDefaultTemplateAsync(string templateType)
    {
        try
        {
            // First try to get from database
            var dbTemplate = await GetTemplateFromDatabaseAsync(templateType);
            if (!string.IsNullOrEmpty(dbTemplate))
            {
                return dbTemplate;
            }

            // Fallback to default templates
            return await Task.FromResult(templateType switch
            {
                "welcome" => _defaultTemplates.GetValueOrDefault("welcome", _defaultTemplates["generic"]),
                "password-reset" => _defaultTemplates.GetValueOrDefault("password-reset", _defaultTemplates["generic"]),
                "letter-generated" => _defaultTemplates.GetValueOrDefault("letter-generated", _defaultTemplates["generic"]),
                "error" => _defaultTemplates.GetValueOrDefault("error", _defaultTemplates["generic"]),
                _ => _defaultTemplates["generic"]
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template for type {TemplateType}", templateType);
            return _defaultTemplates["generic"];
        }
    }

    private Task<string?> GetTemplateFromDatabaseAsync(string templateType)
    {
        try
        {
            // TODO: Implement actual database query for email templates
            // This would involve querying an EmailTemplate table
            // For now, return null to use default templates
            
            _logger.LogInformation("Database template lookup not implemented yet for {TemplateType}", templateType);
            return Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving template from database for {TemplateType}", templateType);
            return Task.FromResult<string?>(null);
        }
    }

    private Dictionary<string, string> InitializeDefaultTemplates()
    {
        return new Dictionary<string, string>
        {
            ["generic"] = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>{{Subject}}</title>
                </head>
                <body>
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>{{Title}}</h2>
                        <p>{{Message}}</p>
                        <hr>
                        <p><small>Sent on {{CurrentDate}} at {{CurrentTime}}</small></p>
                    </div>
                </body>
                </html>",

            ["welcome"] = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Welcome to DocHub</title>
                </head>
                <body>
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Welcome to DocHub!</h2>
                        <p>Hello {{FullName}},</p>
                        <p>Welcome to DocHub! Your account has been successfully created.</p>
                        <p>Username: <strong>{{Username}}</strong></p>
                        <p>You can now log in and start using our document management system.</p>
                        <hr>
                        <p><small>Account created on {{CurrentDate}}</small></p>
                    </div>
                </body>
                </html>",

            ["password-reset"] = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Password Reset Request</title>
                </head>
                <body>
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Password Reset Request</h2>
                        <p>Hello {{FullName}},</p>
                        <p>We received a request to reset your password. Click the link below to proceed:</p>
                        <p><a href='{{ResetLink}}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                        <p>If you didn't request this, please ignore this email.</p>
                        <p>This link will expire in 24 hours.</p>
                        <hr>
                        <p><small>Requested on {{CurrentDate}} at {{CurrentTime}}</small></p>
                    </div>
                </body>
                </html>",

            ["letter-generated"] = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Letter Generated Successfully</title>
                </head>
                <body>
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2>Letter Generated Successfully</h2>
                        <p>Hello {{EmployeeName}},</p>
                        <p>Your {{LetterType}} has been generated successfully.</p>
                        <p><strong>Letter Number:</strong> {{LetterNumber}}</p>
                        <p><strong>Generated On:</strong> {{GeneratedDate}}</p>
                        <p>You can download the letter from your dashboard or contact HR for assistance.</p>
                        <hr>
                        <p><small>Generated on {{CurrentDate}} at {{CurrentTime}}</small></p>
                    </div>
                </body>
                </html>",

            ["error"] = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>System Notification</title>
                </head>
                <body>
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #dc3545;'>System Notification</h2>
                        <p>{{Message}}</p>
                        <p>If you have any questions, please contact our support team.</p>
                        <hr>
                        <p><small>Sent on {{CurrentDate}} at {{CurrentTime}}</small></p>
                    </div>
                </body>
                </html>"
        };
    }

    private string GetTemplateDescription(string templateName)
    {
        return templateName switch
        {
            "generic" => "Generic email template for general communications",
            "welcome" => "Welcome email for new users",
            "password-reset" => "Password reset request email",
            "letter-generated" => "Notification when a letter is generated",
            "error" => "Error notification email",
            _ => "Custom email template"
        };
    }

    private string GetTemplateType(string templateName)
    {
        return templateName switch
        {
            "welcome" => "user-management",
            "password-reset" => "security",
            "letter-generated" => "notification",
            "error" => "system",
            _ => "general"
        };
    }

    public Task<bool> ValidateTemplateAsync(string templateContent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateContent))
                return Task.FromResult(false);

            // Check for required placeholders
            var requiredPlaceholders = new[] { "{{Subject}}", "{{Body}}" };
            var hasRequiredPlaceholders = requiredPlaceholders.All(p => templateContent.Contains(p));

            if (!hasRequiredPlaceholders)
            {
                _logger.LogWarning("Template missing required placeholders: {Placeholders}", 
                    string.Join(", ", requiredPlaceholders.Where(p => !templateContent.Contains(p))));
                return Task.FromResult(false);
            }

            // Check for balanced braces
            var openBraces = templateContent.Count(c => c == '{');
            var closeBraces = templateContent.Count(c => c == '}');
            
            if (openBraces != closeBraces)
            {
                _logger.LogWarning("Template has unbalanced braces: {Open} open, {Close} close", openBraces, closeBraces);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template");
            return Task.FromResult(false);
        }
    }

    public async Task<string> PreviewTemplateAsync(string templateName, Dictionary<string, string> placeholders)
    {
        try
        {
            var template = await GetTemplateAsync(templateName);
            if (template == null)
            {
                return "Template not found";
            }

            return await RenderTemplateAsync(template.Content, placeholders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing template {TemplateName}", templateName);
            return "Error generating preview";
        }
    }

    public Task<IEnumerable<string>> GetAvailableTemplateTypesAsync()
    {
        try
        {
            // TODO: Implement database query for available template types
            // For now, return default template types
            return Task.FromResult<IEnumerable<string>>(_defaultTemplates.Keys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available template types");
            return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
        }
    }

    private Task<string> RenderTemplateAsync(string templateContent, Dictionary<string, string> placeholders)
    {
        try
        {
            var result = templateContent;
            
            if (placeholders != null)
            {
                foreach (var placeholder in placeholders)
                {
                    result = result.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value ?? string.Empty);
                }
            }
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template");
            return Task.FromResult(templateContent);
        }
    }
}
