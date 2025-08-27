using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class DigitalSignature : BaseAuditableEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string SignatureImage { get; set; } = string.Empty; // Base64 encoded image

    public string? SignatureData { get; set; } // Any additional signature data

    [Required]
    [StringLength(50)]
    public string SignatureType { get; set; } = "proxkey"; // proxkey, other

    [StringLength(100)]
    public string? DeviceSerialNumber { get; set; }

    public string? CertificateData { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    // Navigation properties
    public virtual ICollection<GeneratedLetter> GeneratedLetters { get; set; } = new List<GeneratedLetter>();
}

public class SignatureHistory : BaseAuditableEntity
{
    [Required]
    public string DigitalSignatureId { get; set; } = string.Empty;

    [Required]
    public string DocumentId { get; set; } = string.Empty; // Can be GeneratedLetterId or other document ID

    [Required]
    [StringLength(50)]
    public string DocumentType { get; set; } = string.Empty; // letter, other

    public string? SignatureMetadata { get; set; } // JSON metadata about the signature

    public string? Location { get; set; }

    public string? Reason { get; set; }

    // Navigation property
    public virtual DigitalSignature DigitalSignature { get; set; } = null!;
}
