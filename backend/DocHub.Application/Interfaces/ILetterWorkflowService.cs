using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface ILetterWorkflowService
{
    Task<LetterWorkflowDto> StartWorkflowAsync(string letterId, LetterWorkflowRequestDto request);
    Task<LetterWorkflowDto> UpdateWorkflowStatusAsync(string workflowId, WorkflowStatusDto newStatus);
    Task<LetterWorkflowDto> AddWorkflowCommentAsync(string workflowId, WorkflowCommentDto comment);
    Task<LetterWorkflowDto> GetWorkflowByIdAsync(string workflowId);
    Task<IEnumerable<LetterWorkflowDto>> GetWorkflowsByLetterIdAsync(string letterId);
    Task<LetterWorkflowDto> AssignWorkflowToUserAsync(string workflowId, string userId);
    Task<IEnumerable<LetterWorkflowDto>> GetWorkflowsByUserIdAsync(string userId);
    Task<IEnumerable<LetterWorkflowDto>> GetWorkflowsByStatusAsync(WorkflowStatusDto status);
    Task<bool> ValidateWorkflowTransitionAsync(string workflowId, WorkflowStatusDto newStatus);
    Task<BulkWorkflowResultDto> ProcessBulkWorkflowAsync(BulkWorkflowRequestDto request);
}
