using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class DigitalSignatureDto
{
    public string Id { get; set; } = string.Empty;
    public string SignatureName { get; set; } = string.Empty;
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityDesignation { get; set; } = string.Empty;
    public string SignatureImagePath { get; set; } = string.Empty;
    public string? SignatureData { get; set; }
    public DateTime SignatureDate { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

public class CreateDigitalSignatureDto
{
    public string SignatureName { get; set; } = string.Empty;
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityDesignation { get; set; } = string.Empty;
    public string SignatureImagePath { get; set; } = string.Empty;
    public string? SignatureData { get; set; }
    public DateTime SignatureDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateDigitalSignatureDto
{
    public string Id { get; set; } = string.Empty;
    public string SignatureName { get; set; } = string.Empty;
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityDesignation { get; set; } = string.Empty;
    public string SignatureImagePath { get; set; } = string.Empty;
    public string? SignatureData { get; set; }
    public DateTime SignatureDate { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
