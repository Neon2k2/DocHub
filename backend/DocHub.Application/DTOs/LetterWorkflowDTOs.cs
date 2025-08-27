using System.Text.Json.Serialization;

namespace DocHub.Application.DTOs
{
    public class LetterWorkflowDto
    {
        public string Id { get; set; }
        public string LetterId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<WorkflowStepDto> Steps { get; set; } = new();
        public WorkflowStatusDto Status { get; set; }
        public string AssignedUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public List<WorkflowCommentDto> Comments { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class WorkflowStepDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string AssignedRoleId { get; set; }
        public WorkflowStepStatusDto Status { get; set; }
        public List<string> RequiredActions { get; set; } = new();
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class WorkflowCommentDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<string> Attachments { get; set; } = new();
        public string RelatedStepId { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkflowStatusDto
    {
        Draft,
        InProgress,
        PendingApproval,
        Approved,
        Rejected,
        Completed,
        Cancelled
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkflowStepStatusDto
    {
        Pending,
        InProgress,
        Completed,
        Skipped,
        Failed
    }

    public class LetterWorkflowRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<WorkflowStepDto> Steps { get; set; } = new();
        public string AssignedUserId { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class BulkWorkflowRequestDto
    {
        public List<string> LetterIds { get; set; } = new();
        public LetterWorkflowRequestDto WorkflowTemplate { get; set; }
        public bool ContinueOnError { get; set; }
    }

    public class BulkWorkflowResultDto
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<string> SuccessfulLetterIds { get; set; } = new();
        public Dictionary<string, string> Errors { get; set; } = new();
    }
}
