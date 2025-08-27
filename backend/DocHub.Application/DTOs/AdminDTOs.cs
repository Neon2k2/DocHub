using System;
using System.Collections.Generic;

namespace DocHub.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Permissions { get; set; } = new();
        public Dictionary<string, object> Preferences { get; set; } = new();
    }

    public class AdminManagementDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> AssignedTabs { get; set; } = new();
        public List<string> AssignedTemplates { get; set; } = new();
        public Dictionary<string, object> Permissions { get; set; } = new();
        public bool IsSuperAdmin { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    public class NotificationPreferences
    {
        public bool EmailNotifications { get; set; } = true;
        public bool SignatureExpiration { get; set; } = true;
        public bool LetterGeneration { get; set; } = true;
        public bool WorkflowUpdates { get; set; } = true;
        public Dictionary<string, bool> CustomPreferences { get; set; } = new();
    }

    public class DashboardStats
    {
        public int TotalLettersGenerated { get; set; }
        public int PendingSignatures { get; set; }
        public int EmailsSent { get; set; }
        public int ActiveWorkflows { get; set; }
        public Dictionary<string, int> LettersByCategory { get; set; } = new();
        public Dictionary<string, int> EmailStatusCounts { get; set; } = new();
        public Dictionary<string, object> CustomStats { get; set; } = new();
    }
}
