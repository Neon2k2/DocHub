using AutoMapper;
using DocHub.Core.Entities;
using DocHub.Application.DTOs;

namespace DocHub.Application.MappingProfiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Admin mappings
        CreateMap<Admin, UserDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => GetFirstName(src.FullName)))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => GetLastName(src.FullName)));
        
        CreateMap<Admin, AuthResponseDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src));

        // Employee mappings
        CreateMap<Employee, EmployeeDto>();
        CreateMap<EmployeeDto, Employee>();
        CreateMap<CreateEmployeeDto, Employee>();
        CreateMap<UpdateEmployeeDto, Employee>();

        // DynamicTab mappings
        CreateMap<DynamicTab, DynamicTabDto>();
        CreateMap<CreateDynamicTabDto, DynamicTab>();
        CreateMap<UpdateDynamicTabDto, DynamicTab>();

        // LetterTemplate mappings
        CreateMap<LetterTemplate, LetterTemplateDto>();
        CreateMap<CreateLetterTemplateDto, LetterTemplate>();
        CreateMap<UpdateLetterTemplateDto, LetterTemplate>();

        // LetterTemplateField mappings
        CreateMap<LetterTemplateField, LetterTemplateFieldDto>();
        CreateMap<CreateLetterTemplateFieldDto, LetterTemplateField>();
        CreateMap<UpdateLetterTemplateFieldDto, LetterTemplateField>();

        // GeneratedLetter mappings
        CreateMap<GeneratedLetter, GeneratedLetterDto>();
        CreateMap<CreateGeneratedLetterDto, GeneratedLetter>();
        CreateMap<UpdateGeneratedLetterDto, GeneratedLetter>();

        // LetterPreview mappings
        CreateMap<LetterPreview, LetterPreviewDto>();
        CreateMap<CreateLetterPreviewDto, LetterPreview>();
        CreateMap<UpdateLetterPreviewDto, LetterPreview>();

        // DigitalSignature mappings
        CreateMap<DigitalSignature, DigitalSignatureDto>();
        CreateMap<CreateDigitalSignatureDto, DigitalSignature>();
        CreateMap<UpdateDigitalSignatureDto, DigitalSignature>();

        // EmailHistory mappings
        CreateMap<EmailHistory, EmailHistoryDto>();
        CreateMap<CreateEmailHistoryDto, EmailHistory>();
        CreateMap<UpdateEmailHistoryDto, EmailHistory>();

        // LetterAttachment mappings
        CreateMap<LetterAttachment, LetterAttachmentDto>();
        CreateMap<CreateLetterAttachmentDto, LetterAttachment>();
        CreateMap<UpdateLetterAttachmentDto, LetterAttachment>();

        // FileUpload mappings
        CreateMap<FileUpload, FileUploadDto>();
        CreateMap<CreateFileUploadDto, FileUpload>();
        CreateMap<UpdateFileUploadDto, FileUpload>();

        // Note: PROXKeyInfo and DashboardStats are already DTOs, no additional mapping needed
    }

    private static string GetFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;
        
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : string.Empty;
    }

    private static string GetLastName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;
        
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
    }
}
