namespace DocHub.Application.DTOs;

public class GenerateLetterRequest
{
    public string LetterTemplateId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? DigitalSignatureId { get; set; }
    public Dictionary<string, string> FieldValues { get; set; } = new();
    public List<string> AttachmentPaths { get; set; } = new();
}
