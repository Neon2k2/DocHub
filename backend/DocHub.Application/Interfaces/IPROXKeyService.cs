using DocHub.Core.Entities;
using DocHub.Application.DTOs;

namespace DocHub.Application.Interfaces;

public interface IPROXKeyService
{
    Task<DigitalSignature> GenerateSignatureAsync(GenerateSignatureRequest request);
    Task<bool> TestDeviceConnectionAsync();
    Task<DeviceStatus> GetDeviceStatusAsync();
    Task<PROXKeyInfoDto> GetDeviceInfoAsync();
    Task<bool> IsPROXKeyConnectedAsync();
    Task<bool> ValidatePROXKeyAsync();
    Task<bool> ValidateSignatureAsync(string signatureData);
    Task<byte[]> GetSignatureImageAsync(string signatureId);
    Task<DigitalSignature> UpdateSignatureAsync(string signatureId, DigitalSignature signature);
    Task<bool> DeleteSignatureAsync(string signatureId);
    Task<List<DigitalSignature>> GetSignaturesByAuthorityAsync(string authorityName);
    Task<DigitalSignature> GetLatestSignatureAsync();
    Task<PROXKeyInfoDto> GetPROXKeyInfoAsync();
}

public class DeviceStatus
{
    public bool IsConnected { get; set; }
    public string DevicePath { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
