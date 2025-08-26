using System.ComponentModel.DataAnnotations;

namespace DocHub.Application.DTOs;

public class FileUploadDto
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? DocumentType { get; set; }
    public string? AuthorityName { get; set; }
    public string? AuthorityDesignation { get; set; }
    public string? Notes { get; set; }
    public string? Location { get; set; }
    public string? Department { get; set; }
    public string? TabId { get; set; }
    public string? LetterType { get; set; }
    public string? Version { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? ErrorMessage { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProcessedRows { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateFileUploadDto
{
    [Required]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string FileType { get; set; } = string.Empty;
    
    [Required]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    public string? Category { get; set; }
    
    public string? Description { get; set; }
    
    public string? DocumentType { get; set; }
    
    public string? AuthorityName { get; set; }
    
    public string? AuthorityDesignation { get; set; }
    
    public string? Notes { get; set; }
    
    public string? Location { get; set; }
    
    public string? Department { get; set; }
    
    public string? TabId { get; set; }
    
    public string? LetterType { get; set; }
    
    public string? Version { get; set; }
}

public class UpdateFileUploadDto
{
    public string? Category { get; set; }
    
    public string? Description { get; set; }
    
    public string? DocumentType { get; set; }
    
    public string? AuthorityName { get; set; }
    
    public string? AuthorityDesignation { get; set; }
    
    public string? Notes { get; set; }
    
    public string? Location { get; set; }
    
    public string? Department { get; set; }
    
    public string? LetterType { get; set; }
    
    public string? Version { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public bool IsProcessed { get; set; }
    
    public string? ProcessedBy { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public int ProcessedRows { get; set; }
    
    public List<string> Errors { get; set; } = new();
}
