using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces;

public interface ILetterStatusService
{
    Task<bool> UpdateLetterStatusAsync(string letterId, string newStatus, string? notes = null);
    Task<bool> UpdateLetterStatusBatchAsync(List<string> letterIds, string newStatus, string? notes = null);
    Task<List<LetterStatusHistory>> GetLetterStatusHistoryAsync(string letterId);
    Task<LetterStatusSummary> GetLetterStatusSummaryAsync();
    Task<bool> MarkLetterAsSentAsync(string letterId, string emailId);
    Task<bool> MarkLetterAsDeliveredAsync(string letterId);
    Task<bool> MarkLetterAsFailedAsync(string letterId, string errorMessage);
    Task<List<GeneratedLetter>> GetLettersByStatusAsync(string status, int page = 1, int pageSize = 20);
    Task<int> GetLetterCountByStatusAsync(string status);
}

public class LetterStatusSummary
{
    public int TotalLetters { get; set; }
    public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
