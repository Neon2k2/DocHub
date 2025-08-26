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
    
    [StringLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    [StringLength(100)]
    public string? Designation { get; set; }
    
    public DateTime? JoiningDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Computed property for full name
    public string Name => $"{FirstName} {MiddleName} {LastName}".Trim();
    
    // Navigation properties
    public virtual ICollection<GeneratedLetter> GeneratedLetters { get; set; } = new List<GeneratedLetter>();
}
