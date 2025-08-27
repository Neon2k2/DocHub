using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class EmailTemplate : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    public string? PlaceholderMappings { get; set; } // JSON mapping of placeholders

    [StringLength(100)]
    public string? Category { get; set; }

    public string? DefaultValues { get; set; } // JSON default values

    // Navigation properties
    public virtual ICollection<EmailHistory> EmailHistories { get; set; } = new List<EmailHistory>();
}

public class EmailHistory : BaseAuditableEntity
{
    [Required]
    public string GeneratedLetterId { get; set; } = string.Empty;

    public string? EmailTemplateId { get; set; }

    [Required]
    [StringLength(255)]
    public string ToEmail { get; set; } = string.Empty;

    [StringLength(255)]
    public string? CcEmail { get; set; }

    [StringLength(255)]
    public string? BccEmail { get; set; }

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public bool IsHtml { get; set; } = true;

    public string? SendGridMessageId { get; set; }

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual GeneratedLetter GeneratedLetter { get; set; } = null!;
    public virtual EmailTemplate? EmailTemplate { get; set; }
    public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}

public class EmailAttachment : BaseEntity
{
    [Required]
    public string EmailHistoryId { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    public long FileSize { get; set; }

    public bool IsInline { get; set; } = false;

    public string? ContentId { get; set; } // For inline attachments

    // Navigation property
    public virtual EmailHistory EmailHistory { get; set; } = null!;
}
