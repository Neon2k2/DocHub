using System;
using System.Collections.Generic;

namespace DocHub.Application.DTOs
{
    public class DigitalSignature
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public byte[] SignatureImage { get; set; } = Array.Empty<byte>();
        public string ImageType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class PROXKeyInfo
    {
        public string DeviceId { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string HolderName { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string IssuerName { get; set; } = string.Empty;
        public string CertificateThumbprint { get; set; } = string.Empty;
    }

    public class SignatureRequest
    {
        public string DocumentId { get; set; } = string.Empty;
        public string SignatureId { get; set; } = string.Empty;
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int PageNumber { get; set; } = 1;
        public Dictionary<string, object>? SignatureOptions { get; set; }
    }

    public class SignedDocument
    {
        public string Id { get; set; } = string.Empty;
        public string OriginalDocumentId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string SignedBy { get; set; } = string.Empty;
        public string SignatureId { get; set; } = string.Empty;
        public DateTime SignedAt { get; set; }
        public bool IsValid { get; set; }
        public string? ValidationMessage { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class SignatureValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public string? SignedBy { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? CertificateInfo { get; set; }
        public Dictionary<string, object> ValidationDetails { get; set; } = new();
    }
}
