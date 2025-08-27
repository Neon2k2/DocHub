using DocHub.Core.Entities.Workflow;
using DocHub.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DocHub.Infrastructure.Repositories;

public class WorkflowDefinitionRepository : GenericRepository<WorkflowDefinition>, IWorkflowDefinitionRepository
{
    private readonly ApplicationDbContext _context;

    public WorkflowDefinitionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<WorkflowDefinition?> GetWithRelatedDataAsync(string id)
    {
        return await _context.Set<WorkflowDefinition>()
            .Include(w => w.States)
            .Include(w => w.Transitions)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WorkflowDefinition?> GetDefaultWorkflowForEntityTypeAsync(string entityType)
    {
        return await _context.Set<WorkflowDefinition>()
            .Include(w => w.States)
            .Include(w => w.Transitions)
            .FirstOrDefaultAsync(w => w.EntityType == entityType && w.IsDefault);
    }
}

public class WorkflowInstanceRepository : GenericRepository<WorkflowInstance>, IWorkflowInstanceRepository
{
    private readonly ApplicationDbContext _context;

    public WorkflowInstanceRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<WorkflowInstance?> GetWithRelatedDataAsync(string id)
    {
        return await _context.Set<WorkflowInstance>()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .Include(w => w.History)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WorkflowInstance?> GetByEntityAsync(string entityId, string entityType)
    {
        return await _context.Set<WorkflowInstance>()
            .Include(w => w.WorkflowDefinition)
            .Include(w => w.CurrentState)
            .FirstOrDefaultAsync(w => w.EntityId == entityId && w.EntityType == entityType);
    }
}

public class WorkflowHistoryRepository : GenericRepository<WorkflowHistory>, IWorkflowHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public WorkflowHistoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowHistory>> GetHistoryForInstanceAsync(string workflowInstanceId)
    {
        return await _context.Set<WorkflowHistory>()
            .Include(h => h.FromState)
            .Include(h => h.ToState)
            .Include(h => h.Transition)
            .Where(h => h.WorkflowInstanceId == workflowInstanceId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}

public class WorkflowApprovalRepository : GenericRepository<WorkflowApproval>, IWorkflowApprovalRepository
{
    private readonly ApplicationDbContext _context;

    public WorkflowApprovalRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkflowApproval>> GetPendingApprovalsAsync(string approverId, string approverType)
    {
        return await _context.Set<WorkflowApproval>()
            .Include(a => a.WorkflowInstance)
                .ThenInclude(w => w.CurrentState)
            .Include(a => a.Transition)
            .Where(a => a.ApproverId == approverId &&
                       a.ApproverType == approverType &&
                       a.Status == "pending")
            .OrderBy(a => a.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkflowApproval>> GetInstanceApprovalsAsync(string workflowInstanceId)
    {
        return await _context.Set<WorkflowApproval>()
            .Include(a => a.Transition)
            .Where(a => a.WorkflowInstanceId == workflowInstanceId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}
