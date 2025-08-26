using DocHub.Application.Interfaces;
using DocHub.Core.Entities;
using DocHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DocHub.Infrastructure.Services;

public class LetterStatusService : ILetterStatusService
{
    private readonly DocHubDbContext _context;
    private readonly ILogger<LetterStatusService> _logger;

    public LetterStatusService(DocHubDbContext context, ILogger<LetterStatusService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> UpdateLetterStatusAsync(string letterId, string newStatus, string? notes = null)
    {
        try
        {
            var letter = await _context.GeneratedLetters
                .FirstOrDefaultAsync(l => l.Id == letterId);

            if (letter == null)
            {
                _logger.LogWarning("Letter not found for status update: {LetterId}", letterId);
                return false;
            }

            var oldStatus = letter.Status;
            letter.Status = newStatus;
            letter.UpdatedAt = DateTime.UtcNow;

            // Add status history
            var statusHistory = new LetterStatusHistory
            {
                LetterId = letterId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = "System", // TODO: Get from user context
                Notes = notes
            };

            _context.LetterStatusHistories.Add(statusHistory);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter status updated: {LetterId} {OldStatus} -> {NewStatus}", 
                letterId, oldStatus, newStatus);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating letter status for {LetterId}", letterId);
            return false;
        }
    }

    public async Task<bool> UpdateLetterStatusBatchAsync(List<string> letterIds, string newStatus, string? notes = null)
    {
        try
        {
            var letters = await _context.GeneratedLetters
                .Where(l => letterIds.Contains(l.Id))
                .ToListAsync();

            if (!letters.Any())
            {
                _logger.LogWarning("No letters found for batch status update");
                return false;
            }

            var statusHistories = new List<LetterStatusHistory>();
            var now = DateTime.UtcNow;

            foreach (var letter in letters)
            {
                var oldStatus = letter.Status;
                letter.Status = newStatus;
                letter.UpdatedAt = now;

                statusHistories.Add(new LetterStatusHistory
                {
                    LetterId = letter.Id,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    ChangedAt = now,
                    ChangedBy = "System", // TODO: Get from user context
                    Notes = notes
                });
            }

            _context.LetterStatusHistories.AddRange(statusHistories);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Batch status update completed: {Count} letters updated to {Status}", 
                letters.Count, newStatus);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating letter statuses in batch");
            return false;
        }
    }

    public async Task<List<LetterStatusHistory>> GetLetterStatusHistoryAsync(string letterId)
    {
        try
        {
            return await _context.LetterStatusHistories
                .Where(h => h.LetterId == letterId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status history for letter {LetterId}", letterId);
            return new List<LetterStatusHistory>();
        }
    }

    public async Task<LetterStatusSummary> GetLetterStatusSummaryAsync()
    {
        try
        {
            var summary = await _context.GeneratedLetters
                .GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalLetters = summary.Sum(s => s.Count);
            var statusBreakdown = summary.ToDictionary(s => s.Status, s => s.Count);

            return new LetterStatusSummary
            {
                TotalLetters = totalLetters,
                StatusBreakdown = statusBreakdown,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter status summary");
            return new LetterStatusSummary();
        }
    }

    public async Task<bool> MarkLetterAsSentAsync(string letterId, string emailId)
    {
        try
        {
            var letter = await _context.GeneratedLetters
                .FirstOrDefaultAsync(l => l.Id == letterId);

            if (letter == null)
            {
                _logger.LogWarning("Letter not found for marking as sent: {LetterId}", letterId);
                return false;
            }

            letter.Status = "Sent";
            letter.SentAt = DateTime.UtcNow;
            letter.EmailId = emailId;
            letter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter marked as sent: {LetterId}", letterId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking letter as sent: {LetterId}", letterId);
            return false;
        }
    }

    public async Task<bool> MarkLetterAsDeliveredAsync(string letterId)
    {
        try
        {
            var letter = await _context.GeneratedLetters
                .FirstOrDefaultAsync(l => l.Id == letterId);

            if (letter == null)
            {
                _logger.LogWarning("Letter not found for marking as delivered: {LetterId}", letterId);
                return false;
            }

            letter.Status = "Delivered";
            letter.DeliveredAt = DateTime.UtcNow;
            letter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter marked as delivered: {LetterId}", letterId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking letter as delivered: {LetterId}", letterId);
            return false;
        }
    }

    public async Task<bool> MarkLetterAsFailedAsync(string letterId, string errorMessage)
    {
        try
        {
            var letter = await _context.GeneratedLetters
                .FirstOrDefaultAsync(l => l.Id == letterId);

            if (letter == null)
            {
                _logger.LogWarning("Letter not found for marking as failed: {LetterId}", letterId);
                return false;
            }

            letter.Status = "Failed";
            letter.ErrorMessage = errorMessage;
            letter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Letter marked as failed: {LetterId} - {Error}", letterId, errorMessage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking letter as failed: {LetterId}", letterId);
            return false;
        }
    }

    public async Task<List<GeneratedLetter>> GetLettersByStatusAsync(string status, int page = 1, int pageSize = 20)
    {
        try
        {
            return await _context.GeneratedLetters
                .Where(l => l.Status == status)
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letters by status: {Status}", status);
            return new List<GeneratedLetter>();
        }
    }

    public async Task<int> GetLetterCountByStatusAsync(string status)
    {
        try
        {
            return await _context.GeneratedLetters
                .CountAsync(l => l.Status == status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting letter count by status: {Status}", status);
            return 0;
        }
    }


}


