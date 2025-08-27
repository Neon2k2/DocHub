namespace DocHub.Application.DTOs;

public class DocumentSignatureValidationResult
{
    public string DocumentPath { get; set; } = string.Empty;
    public string SignatureId { get; set; } = string.Empty;
    public string DocumentHash { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public DateTime ValidatedAt { get; set; }
    public SignatureDetails? SignatureDetails { get; set; }
}

public class SignatureDetails
{
    public string? UserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? SignatureType { get; set; }
}
