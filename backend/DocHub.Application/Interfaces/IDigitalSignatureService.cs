using DocHub.Core.Entities;

namespace DocHub.Application.Interfaces;

public interface IDigitalSignatureService
{
    Task<IEnumerable<DigitalSignature>> GetAllAsync();
    Task<DigitalSignature> GetByIdAsync(string id);
    Task<DigitalSignature> CreateAsync(DigitalSignature signature);
    Task<DigitalSignature> UpdateAsync(string id, DigitalSignature signature);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<DigitalSignature>> GetActiveSignaturesAsync();
    Task<DigitalSignature> GetByAuthorityNameAsync(string authorityName);
    Task<bool> ExistsAsync(string authorityName);
    Task<bool> ToggleActiveAsync(string id);
    Task<DigitalSignature> GenerateSignatureFromPROXKeyAsync(string authorityName, string authorityDesignation);
    Task<byte[]> GetSignatureImageAsync(string id);
    Task<string> GetLatestSignaturePathAsync();
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<DigitalSignature>> GetPagedAsync(int page, int pageSize);
}
