using DocHub.Core.Entities.Workflow;

namespace DocHub.Core.Interfaces.Repositories;

public interface IWorkflowDefinitionRepository : IGenericRepository<WorkflowDefinition>
{
    Task<WorkflowDefinition?> GetWithRelatedDataAsync(string id);
    Task<WorkflowDefinition?> GetDefaultWorkflowForEntityTypeAsync(string entityType);
}

public interface IWorkflowInstanceRepository : IGenericRepository<WorkflowInstance>
{
    Task<WorkflowInstance?> GetWithRelatedDataAsync(string id);
    Task<WorkflowInstance?> GetByEntityAsync(string entityId, string entityType);
}

public interface IWorkflowHistoryRepository : IGenericRepository<WorkflowHistory>
{
    Task<IEnumerable<WorkflowHistory>> GetHistoryForInstanceAsync(string workflowInstanceId);
}

public interface IWorkflowApprovalRepository : IGenericRepository<WorkflowApproval>
{
    Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(string approverId, string approverType);
    Task<IEnumerable<WorkflowApproval>> GetInstanceApprovalsAsync(string workflowInstanceId);
}
