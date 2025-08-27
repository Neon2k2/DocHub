using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces
{
    public interface ILetterService
    {
        Task<LetterTemplate> GetTemplateByIdAsync(int id);
        Task<List<LetterTemplate>> GetAllTemplatesAsync();
        Task<LetterTemplate> CreateTemplateAsync(LetterTemplate template);
        Task<LetterTemplate> UpdateTemplateAsync(int id, LetterTemplate template);
        Task<bool> DeleteTemplateAsync(int id);

        Task<GeneratedLetter> GenerateLetterAsync(int templateId, Dictionary<string, object> fieldValues);
        Task<GeneratedLetter> GetGeneratedLetterByIdAsync(int id);
        Task<List<GeneratedLetter>> GetGeneratedLettersByTemplateIdAsync(int templateId);

        Task<LetterWorkflow> StartWorkflowAsync(int generatedLetterId, List<WorkflowStep> steps);
        Task<LetterWorkflow> UpdateWorkflowStatusAsync(int workflowId, WorkflowStatus status);
        Task<LetterWorkflow> AddWorkflowCommentAsync(int workflowId, WorkflowComment comment);
        Task<LetterWorkflow> GetWorkflowByIdAsync(int workflowId);
        Task<List<LetterWorkflow>> GetWorkflowsByLetterIdAsync(int generatedLetterId);
    }
}
