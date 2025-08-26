using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface ISignatureService
{
    Task<DigitalSignatureDto> GenerateSignatureAsync(string authorityName, string authorityDesignation, string signatureData);
    Task<DigitalSignatureDto> GetLatestSignatureAsync();
    Task<DigitalSignatureDto> GetSignatureByAuthorityAsync(string authorityName);
    Task<bool> ValidateSignatureAsync(string signatureData);
    Task<string> ApplySignatureToDocumentAsync(byte[] documentBytes, DigitalSignatureDto signature);
}
