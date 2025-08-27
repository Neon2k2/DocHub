using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities;

public class Employee : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(50)]
    public string? Department { get; set; }

    [StringLength(50)]
    public string? Designation { get; set; }

    public DateTime? JoiningDate { get; set; }

    [StringLength(50)]
    public string EmployeeType { get; set; } = "permanent"; // permanent, consultant

    public string? AdditionalInfo { get; set; } // JSON additional info

    // Navigation properties
    public virtual ICollection<GeneratedLetter> GeneratedLetters { get; set; } = new List<GeneratedLetter>();
}

public class Admin : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(50)]
    public string Role { get; set; } = "Admin"; // SuperAdmin, Admin

    public string? Permissions { get; set; } // JSON permissions

    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<DynamicTab> CreatedTabs { get; set; } = new List<DynamicTab>();
    public virtual ICollection<LetterTemplate> CreatedTemplates { get; set; } = new List<LetterTemplate>();
}

public class NotificationPreference : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty; // Can be AdminId or EmployeeId

    [Required]
    [StringLength(50)]
    public string UserType { get; set; } = string.Empty; // admin, employee

    public bool EmailNotifications { get; set; } = true;

    public bool WebNotifications { get; set; } = true;

    public string? NotificationTypes { get; set; } // JSON array of notification types to receive
}

public class Notification : BaseAuditableEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserType { get; set; } = string.Empty; // admin, employee

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // letter, email, system

    public string? RelatedEntityId { get; set; }

    [StringLength(50)]
    public string? RelatedEntityType { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public string? ActionUrl { get; set; }
}
