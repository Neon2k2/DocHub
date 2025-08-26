namespace DocHub.Application.DTOs;

public class SendEmailRequest
{
    public string GeneratedLetterId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string> AttachmentPaths { get; set; } = new();
    public bool UseLatestSignature { get; set; } = true;
    public string? EmailId { get; set; }
}
