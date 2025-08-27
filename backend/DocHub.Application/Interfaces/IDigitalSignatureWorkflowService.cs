using DocHub.Core.Entities;
using DocHub.Application.DTOs;
using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface IDigitalSignatureWorkflowService
{
    /// <summary>
    /// Generates a digital signature from pendrive content
    /// </summary>
    Task<DigitalSignature> GenerateSignatureFromPendriveAsync(
        string pendrivePath,
        string userId,
        string? signaturePurpose = null);

    /// <summary>
    /// Validates a digital signature against a document hash
    /// </summary>
    Task<bool> ValidateSignatureAsync(string signatureId, string documentHash);

    /// <summary>
    /// Gets the latest active signature for a user
    /// </summary>
    Task<DigitalSignature> GetLatestUserSignatureAsync(string userId);

    /// <summary>
    /// Renews an existing signature with extended expiry
    /// </summary>
    Task<DigitalSignature> RenewSignatureAsync(string signatureId, string userId);

    /// <summary>
    /// Revokes a digital signature
    /// </summary>
    Task<bool> RevokeSignatureAsync(string signatureId, string userId, string reason);

    /// <summary>
    /// Gets signature history for a user
    /// </summary>
    Task<IEnumerable<DigitalSignature>> GetSignatureHistoryAsync(string userId);

    /// <summary>
    /// Validates a document's signature
    /// </summary>
    Task<SignatureValidationResult> ValidateDocumentSignatureAsync(
        string documentPath,
        string signatureId);
}
