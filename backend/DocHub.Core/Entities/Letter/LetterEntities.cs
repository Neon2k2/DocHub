using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class LetterTemplate : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string LetterType { get; set; } = string.Empty;

    [Required]
    public string TemplateContent { get; set; } = string.Empty; // Base64 encoded docx

    public string? TemplateFilePath { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Required]
    [StringLength(50)]
    public string DataSource { get; set; } = "excel"; // excel, database

    public string? DatabaseQuery { get; set; }

    public string? DefaultValues { get; set; } // JSON default values

    public string? ValidationRules { get; set; } // JSON validation rules

    // Navigation properties
    public virtual ICollection<LetterTemplateField> Fields { get; set; } = new List<LetterTemplateField>();
    public virtual ICollection<GeneratedLetter> GeneratedLetters { get; set; } = new List<GeneratedLetter>();
}

public class LetterTemplateField : BaseEntity
{
    [Required]
    public string LetterTemplateId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DataType { get; set; } = "string";

    public bool IsRequired { get; set; } = false;

    public string? DefaultValue { get; set; }

    public string? ValidationRules { get; set; } // JSON validation rules

    public string? PlaceholderTag { get; set; } // Content control tag

    public int SortOrder { get; set; } = 0;

    // Navigation property
    public virtual LetterTemplate LetterTemplate { get; set; } = null!;
}

public class GeneratedLetter : BaseAuditableEntity
{
    [Required]
    [StringLength(50)]
    public string LetterNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LetterType { get; set; } = string.Empty;

    [Required]
    public string LetterTemplateId { get; set; } = string.Empty;

    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    public string? DigitalSignatureId { get; set; }

    public string? LetterFilePath { get; set; }

    public string? LetterContent { get; set; } // Base64 encoded PDF

    public string? InputData { get; set; } // JSON data used to generate letter

    // Navigation properties
    public virtual LetterTemplate LetterTemplate { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
    public virtual DigitalSignature? DigitalSignature { get; set; }
    public virtual ICollection<LetterAttachment> Attachments { get; set; } = new List<LetterAttachment>();
    public virtual ICollection<EmailHistory> EmailHistories { get; set; } = new List<EmailHistory>();
    public virtual ICollection<LetterStatusHistory> StatusHistories { get; set; } = new List<LetterStatusHistory>();
}

public class LetterAttachment : BaseEntity
{
    [Required]
    public string GeneratedLetterId { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FileType { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    public long FileSize { get; set; }

    // Navigation property
    public virtual GeneratedLetter GeneratedLetter { get; set; } = null!;
}

public class LetterStatusHistory : BaseEntity
{
    [Required]
    public string GeneratedLetterId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Comments { get; set; }

    // Navigation property
    public virtual GeneratedLetter GeneratedLetter { get; set; } = null!;
}
