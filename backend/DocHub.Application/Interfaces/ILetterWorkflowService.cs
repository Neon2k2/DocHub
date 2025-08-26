using DocHub.Application.DTOs;
using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces;

public interface ILetterWorkflowService
{
    Task<LetterWorkflowResult> ProcessLetterWorkflowAsync(LetterWorkflowRequest request);
    Task<BulkWorkflowResult> ProcessBulkWorkflowAsync(BulkWorkflowRequest request);
    Task<LetterWorkflowStatus> GetWorkflowStatusAsync(string letterId);
}
