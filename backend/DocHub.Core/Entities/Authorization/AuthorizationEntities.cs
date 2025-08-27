using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities.Authorization;

public class Role : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    public bool IsSystem { get; set; } = false; // True for built-in roles like Admin, SuperAdmin

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Permission : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "letters.create", "templates.edit"

    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty; // e.g., "Letters", "Templates", "Email"

    [Required]
    [StringLength(50)]
    public string Action { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"

    [StringLength(200)]
    public string? Description { get; set; }

    public bool IsSystem { get; set; } = false;

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : BaseEntity
{
    [Required]
    public string RoleId { get; set; } = string.Empty;

    [Required]
    public string PermissionId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

public class UserRole : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RoleId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UserType { get; set; } = string.Empty; // "Admin" or "Employee"

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
}
