using DocHub.Core.Entities.Workflow;
using DocHub.Core.Interfaces.Repositories;
using DocHub.Infrastructure.Services.Authorization;
using Newtonsoft.Json;
using System.Text.Json;

namespace DocHub.Infrastructure.Services.Workflow;

public interface IWorkflowService
{
    Task<WorkflowInstance> InitializeWorkflowAsync(string entityId, string entityType);
    Task<bool> TransitionToStateAsync(string instanceId, string toStateId, string userId, string? comments);
    Task<bool> CreateApprovalRequestAsync(string instanceId, string transitionId, string approverId, string approverType);
    Task<bool> ApproveTransitionAsync(string approvalId, string userId, string? comments);
    Task<bool> RejectTransitionAsync(string approvalId, string userId, string? comments);
    Task<IEnumerable<WorkflowHistory>> GetWorkflowHistoryAsync(string instanceId);
    Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(string approverId, string approverType);
    Task<bool> ValidateTransitionAsync(string instanceId, string toStateId, string userId);
}

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepo;
    private readonly IWorkflowInstanceRepository _workflowInstanceRepo;
    private readonly IWorkflowHistoryRepository _workflowHistoryRepo;
    private readonly IWorkflowApprovalRepository _workflowApprovalRepo;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILoggingService _loggingService;

    public WorkflowService(
        IWorkflowDefinitionRepository workflowDefinitionRepo,
        IWorkflowInstanceRepository workflowInstanceRepo,
        IWorkflowHistoryRepository workflowHistoryRepo,
        IWorkflowApprovalRepository workflowApprovalRepo,
        IAuthorizationService authorizationService,
        ILoggingService loggingService)
    {
        _workflowDefinitionRepo = workflowDefinitionRepo;
        _workflowInstanceRepo = workflowInstanceRepo;
        _workflowHistoryRepo = workflowHistoryRepo;
        _workflowApprovalRepo = workflowApprovalRepo;
        _authorizationService = authorizationService;
        _loggingService = loggingService;
    }

    public async Task<WorkflowInstance> InitializeWorkflowAsync(string entityId, string entityType)
    {
        try
        {
            var workflowDef = await _workflowDefinitionRepo.GetDefaultWorkflowForEntityTypeAsync(entityType);
            if (workflowDef == null)
                throw new InvalidOperationException($"No default workflow found for entity type: {entityType}");

            var initialState = workflowDef.States.FirstOrDefault(s => s.IsInitial);
            if (initialState == null)
                throw new InvalidOperationException($"No initial state found in workflow: {workflowDef.Id}");

            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = workflowDef.Id,
                CurrentStateId = initialState.Id,
                EntityId = entityId,
                EntityType = entityType
            };

            await _workflowInstanceRepo.AddAsync(instance);
            return instance;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "InitializeWorkflow", ex.Message, new { entityId, entityType });
            throw;
        }
    }

    public async Task<bool> TransitionToStateAsync(string instanceId, string toStateId, string userId, string? comments)
    {
        try
        {
            var instance = await _workflowInstanceRepo.GetWithRelatedDataAsync(instanceId);
            if (instance == null)
                return false;

            var workflowDef = await _workflowDefinitionRepo.GetWithRelatedDataAsync(instance.WorkflowDefinitionId);
            if (workflowDef == null)
                return false;

            var transition = workflowDef.Transitions.FirstOrDefault(t =>
                t.FromStateId == instance.CurrentStateId && t.ToStateId == toStateId);

            if (transition == null)
                return false;

            if (!await ValidateTransitionAsync(instanceId, toStateId, userId))
                return false;

            // Execute transition
            var oldStateId = instance.CurrentStateId;
            instance.CurrentStateId = toStateId;
            instance.UpdatedBy = userId;
            instance.UpdatedAt = DateTime.UtcNow;

            await _workflowInstanceRepo.UpdateAsync(instance);

            // Record history
            var history = new WorkflowHistory
            {
                WorkflowInstanceId = instanceId,
                FromStateId = oldStateId,
                ToStateId = toStateId,
                TransitionId = transition.Id,
                Comments = comments,
                CreatedBy = userId
            };

            await _workflowHistoryRepo.AddAsync(history);

            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "TransitionToState", ex.Message,
                new { instanceId, toStateId, userId });
            return false;
        }
    }

    public async Task<bool> CreateApprovalRequestAsync(string instanceId, string transitionId, string approverId, string approverType)
    {
        try
        {
            var instance = await _workflowInstanceRepo.GetByIdAsync(instanceId);
            if (instance == null)
                return false;

            var approval = new WorkflowApproval
            {
                WorkflowInstanceId = instanceId,
                TransitionId = transitionId,
                ApproverId = approverId,
                ApproverType = approverType,
                Status = "pending"
            };

            await _workflowApprovalRepo.AddAsync(approval);
            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "CreateApprovalRequest", ex.Message,
                new { instanceId, transitionId, approverId, approverType });
            return false;
        }
    }

    public async Task<bool> ApproveTransitionAsync(string approvalId, string userId, string? comments)
    {
        try
        {
            var approval = await _workflowApprovalRepo.GetByIdAsync(approvalId);
            if (approval == null || approval.Status != "pending")
                return false;

            approval.Status = "approved";
            approval.Comments = comments;
            approval.UpdatedBy = userId;
            approval.UpdatedAt = DateTime.UtcNow;

            await _workflowApprovalRepo.UpdateAsync(approval);

            var instance = await _workflowInstanceRepo.GetWithRelatedDataAsync(approval.WorkflowInstanceId);
            if (instance == null)
                return false;

            var transition = await GetTransitionAsync(approval.TransitionId);
            if (transition == null)
                return false;

            return await TransitionToStateAsync(instance.Id, transition.ToStateId, userId, comments);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "ApproveTransition", ex.Message,
                new { approvalId, userId });
            return false;
        }
    }

    public async Task<bool> RejectTransitionAsync(string approvalId, string userId, string? comments)
    {
        try
        {
            var approval = await _workflowApprovalRepo.GetByIdAsync(approvalId);
            if (approval == null || approval.Status != "pending")
                return false;

            approval.Status = "rejected";
            approval.Comments = comments;
            approval.UpdatedBy = userId;
            approval.UpdatedAt = DateTime.UtcNow;

            await _workflowApprovalRepo.UpdateAsync(approval);
            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "RejectTransition", ex.Message,
                new { approvalId, userId });
            return false;
        }
    }

    public async Task<IEnumerable<WorkflowHistory>> GetWorkflowHistoryAsync(string instanceId)
    {
        try
        {
            return await _workflowHistoryRepo.GetHistoryForInstanceAsync(instanceId);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "GetWorkflowHistory", ex.Message,
                new { instanceId });
            return Enumerable.Empty<WorkflowHistory>();
        }
    }

    public async Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(string approverId, string approverType)
    {
        try
        {
            return await _workflowApprovalRepo.GetPendingApprovalsAsync(approverId, approverType);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "GetPendingApprovals", ex.Message,
                new { approverId, approverType });
            return Enumerable.Empty<WorkflowApproval>();
        }
    }

    public async Task<bool> ValidateTransitionAsync(string instanceId, string toStateId, string userId)
    {
        try
        {
            var instance = await _workflowInstanceRepo.GetWithRelatedDataAsync(instanceId);
            if (instance == null)
                return false;

            var workflowDef = await _workflowDefinitionRepo.GetWithRelatedDataAsync(instance.WorkflowDefinitionId);
            if (workflowDef == null)
                return false;

            var transition = workflowDef.Transitions.FirstOrDefault(t =>
                t.FromStateId == instance.CurrentStateId && t.ToStateId == toStateId);

            if (transition == null)
                return false;

            // Check permissions
            if (!string.IsNullOrEmpty(transition.RequiredPermissions))
            {
                var requiredPermissions = JsonConvert.DeserializeObject<string[]>(transition.RequiredPermissions);
                if (requiredPermissions != null)
                {
                    foreach (var permission in requiredPermissions)
                    {
                        if (!await _authorizationService.HasPermissionAsync(userId, "employee", permission))
                            return false;
                    }
                }
            }

            // Check validation rules
            if (!string.IsNullOrEmpty(transition.ValidationRules))
            {
                // Implement custom validation rules here
                // This could involve checking document status, required fields, etc.
            }

            return true;
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync("Workflow", "ValidateTransition", ex.Message,
                new { instanceId, toStateId, userId });
            return false;
        }
    }

    private async Task<WorkflowTransition?> GetTransitionAsync(string transitionId)
    {
        var workflowDef = await _workflowDefinitionRepo.GetWithRelatedDataAsync(transitionId);
        return workflowDef?.Transitions.FirstOrDefault(t => t.Id == transitionId);
    }
}
