using System;
using System.Collections.Generic;

namespace DocHub.Application.DTOs
{
    public class LetterTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<LetterTemplateField> Fields { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class LetterTemplateField
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = "string";
        public bool IsRequired { get; set; }
        public string? DefaultValue { get; set; }
        public string? ValidationRegex { get; set; }
        public List<string>? AllowedValues { get; set; }
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
    }


    public class GeneratedLetter
    {
        public string Id { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsDigitallySigned { get; set; }
        public string? SignatureId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public Dictionary<string, object> FieldValues { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class LetterWorkflow
    {
        public string Id { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public List<WorkflowStep> Steps { get; set; } = new();
        public WorkflowStatus Status { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<WorkflowComment> Comments { get; set; } = new();
    }

    public class WorkflowStep
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Order { get; set; }
        public WorkflowStepStatus Status { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CompletedBy { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class WorkflowComment
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? StepName { get; set; }
    }

    public enum WorkflowStatus
    {
        Draft,
        InProgress,
        Completed,
        Rejected,
        Cancelled
    }

    public enum WorkflowStepStatus
    {
        Pending,
        InProgress,
        Completed,
        Rejected,
        Skipped
    }
}
