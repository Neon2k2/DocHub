using System.ComponentModel.DataAnnotations;

namespace DocHub.Core.Entities.Workflow;

public class WorkflowDefinition : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty; // Letter, Document, etc.

    public bool IsDefault { get; set; } = false;

    public string? ValidationRules { get; set; } // JSON rules for state transitions

    // Navigation properties
    public virtual ICollection<WorkflowState> States { get; set; } = new List<WorkflowState>();
    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}

public class WorkflowState : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty; // Draft, Review, Approved, etc.

    [StringLength(200)]
    public string? Description { get; set; }

    [Required]
    public string WorkflowDefinitionId { get; set; } = string.Empty;

    public bool IsInitial { get; set; } = false;
    public bool IsTerminal { get; set; } = false;

    public string? RequiredPermissions { get; set; } // JSON array of required permissions
    public string? AutomationRules { get; set; } // JSON rules for automated actions

    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public virtual ICollection<WorkflowTransition> OutgoingTransitions { get; set; } = new List<WorkflowTransition>();
    public virtual ICollection<WorkflowTransition> IncomingTransitions { get; set; } = new List<WorkflowTransition>();
}

public class WorkflowTransition : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string WorkflowDefinitionId { get; set; } = string.Empty;

    [Required]
    public string FromStateId { get; set; } = string.Empty;

    [Required]
    public string ToStateId { get; set; } = string.Empty;

    public string? RequiredPermissions { get; set; } // JSON array of required permissions
    public string? ValidationRules { get; set; } // JSON rules for transition validation
    public string? AutomationRules { get; set; } // JSON rules for automated actions

    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public virtual WorkflowState FromState { get; set; } = null!;
    public virtual WorkflowState ToState { get; set; } = null!;
}

public class WorkflowInstance : BaseAuditableEntity
{
    [Required]
    public string WorkflowDefinitionId { get; set; } = string.Empty;

    [Required]
    public string CurrentStateId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty; // Letter, Document, etc.

    [Required]
    public string EntityId { get; set; } = string.Empty;

    public string? StateData { get; set; } // JSON data specific to current state

    // Navigation properties
    public virtual WorkflowDefinition WorkflowDefinition { get; set; } = null!;
    public virtual WorkflowState CurrentState { get; set; } = null!;
    public virtual ICollection<WorkflowHistory> History { get; set; } = new List<WorkflowHistory>();
}

public class WorkflowHistory : BaseAuditableEntity
{
    [Required]
    public string WorkflowInstanceId { get; set; } = string.Empty;

    [Required]
    public string FromStateId { get; set; } = string.Empty;

    [Required]
    public string ToStateId { get; set; } = string.Empty;

    [Required]
    public string TransitionId { get; set; } = string.Empty;

    public string? Comments { get; set; }
    public string? MetaData { get; set; } // JSON additional data about the transition

    // Navigation properties
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
    public virtual WorkflowState FromState { get; set; } = null!;
    public virtual WorkflowState ToState { get; set; } = null!;
    public virtual WorkflowTransition Transition { get; set; } = null!;
}

public class WorkflowApproval : BaseAuditableEntity
{
    [Required]
    public string WorkflowInstanceId { get; set; } = string.Empty;

    [Required]
    public string TransitionId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ApproverType { get; set; } = string.Empty; // Role, User, Group

    [Required]
    public string ApproverId { get; set; } = string.Empty;

    public string? Comments { get; set; }
    public DateTime? DueDate { get; set; }
    public string? ReminderSettings { get; set; } // JSON reminder configuration

    // Navigation properties
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
    public virtual WorkflowTransition Transition { get; set; } = null!;
}
