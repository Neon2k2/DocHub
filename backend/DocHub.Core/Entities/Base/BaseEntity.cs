namespace DocHub.Core.Entities;

public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;
}

public abstract class BaseAuditableEntity : BaseEntity
{
    public string? AuditLog { get; set; } // JSON log of changes
    public string? ProcessedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string Status { get; set; } = "pending"; // pending, processing, completed, failed
}
