using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class PROXKeyInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public DateTime LastConnected { get; set; }
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, string> DeviceProperties { get; set; } = new();
    public string? SerialNumber { get; set; }
    public List<string> AvailableSignatures { get; set; } = new();
}

public class GenerateSignatureRequestDto
{
    public string AuthorityName { get; set; } = string.Empty;
    public string AuthorityDesignation { get; set; } = string.Empty;
    public string DocumentHash { get; set; } = string.Empty;
    public string? SignaturePurpose { get; set; }
    public DateTime SignatureDate { get; set; }
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}

public class GenerateSignatureResponseDto
{
    public bool Success { get; set; }
    public string? SignatureData { get; set; }
    public string? CertificateData { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? TransactionId { get; set; }
}

public class SignDocumentRequest
{
    [Required]
    public byte[] DocumentBytes { get; set; } = Array.Empty<byte>();
    
    [Required]
    [StringLength(100)]
    public string AuthorityName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(8)]
    public string Pin { get; set; } = string.Empty;
}

public class ValidateSignatureRequest
{
    [Required]
    public byte[] DocumentBytes { get; set; } = Array.Empty<byte>();
    
    [Required]
    public byte[] Signature { get; set; } = Array.Empty<byte>();
}

public class ChangePinRequest
{
    [Required]
    [StringLength(8)]
    public string OldPin { get; set; } = string.Empty;
    
    [Required]
    [StringLength(8)]
    public string NewPin { get; set; } = string.Empty;
}
