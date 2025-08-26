using AutoMapper;
using DocHub.Core.Entities;
using DocHub.Application.DTOs;

namespace DocHub.Infrastructure.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Debug: Log that this profile is being loaded
        System.Diagnostics.Debug.WriteLine("AutoMapperProfile constructor called - profile is being loaded");
        Console.WriteLine("AutoMapperProfile constructor called - profile is being loaded");
        
        // Letter Template mappings
        CreateMap<LetterTemplate, LetterTemplateDto>()
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields));
        CreateMap<LetterTemplateDto, LetterTemplate>();
        CreateMap<CreateLetterTemplateDto, LetterTemplate>();
        CreateMap<UpdateLetterTemplateDto, LetterTemplate>();

        // Letter Template Field mappings
        CreateMap<LetterTemplateField, LetterTemplateFieldDto>();
        CreateMap<LetterTemplateFieldDto, LetterTemplateField>();
        CreateMap<CreateLetterTemplateFieldDto, LetterTemplateField>();
        CreateMap<UpdateLetterTemplateFieldDto, LetterTemplateField>();

        // Employee mappings
        CreateMap<Employee, EmployeeDto>();
        CreateMap<EmployeeDto, Employee>();
        CreateMap<CreateEmployeeDto, Employee>();
        CreateMap<UpdateEmployeeDto, Employee>();

        // Digital Signature mappings
        CreateMap<DigitalSignature, DigitalSignatureDto>();
        CreateMap<DigitalSignatureDto, DigitalSignature>();
        CreateMap<CreateDigitalSignatureDto, DigitalSignature>();
        CreateMap<UpdateDigitalSignatureDto, DigitalSignature>();

        // Generated Letter mappings
        CreateMap<GeneratedLetter, GeneratedLetterDto>()
            .ForMember(dest => dest.LetterTemplate, opt => opt.MapFrom(src => src.LetterTemplate))
            .ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src.Employee))
            .ForMember(dest => dest.DigitalSignature, opt => opt.MapFrom(src => src.DigitalSignature))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));
        CreateMap<GeneratedLetterDto, GeneratedLetter>();
        CreateMap<CreateGeneratedLetterDto, GeneratedLetter>();
        CreateMap<UpdateGeneratedLetterDto, GeneratedLetter>();

        // Letter Attachment mappings
        CreateMap<LetterAttachment, LetterAttachmentDto>();
        CreateMap<LetterAttachmentDto, LetterAttachment>();

        // EmailHistory mappings
        CreateMap<EmailHistory, EmailHistoryDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}".Trim() : null))
            .ForMember(dest => dest.LetterType, opt => opt.MapFrom(src => src.GeneratedLetter != null ? src.GeneratedLetter.LetterType : null))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));
        CreateMap<EmailHistoryDto, EmailHistory>();
        CreateMap<CreateEmailHistoryRequest, EmailHistory>();
        CreateMap<EmailAttachment, EmailAttachmentDto>();
        CreateMap<EmailAttachmentDto, EmailAttachment>();
        CreateMap<AddEmailAttachmentRequest, EmailAttachment>();

        // LetterPreview mappings
        CreateMap<LetterPreview, LetterPreviewDto>()
            .ForMember(dest => dest.LetterTemplateName, opt => opt.MapFrom(src => src.LetterTemplate != null ? src.LetterTemplate.Name : null))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}".Trim() : null))
            .ForMember(dest => dest.AuthorityName, opt => opt.MapFrom(src => src.DigitalSignature != null ? src.DigitalSignature.AuthorityName : null))
            .ForMember(dest => dest.AuthorityDesignation, opt => opt.MapFrom(src => src.DigitalSignature != null ? src.DigitalSignature.AuthorityDesignation : null))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));
        CreateMap<LetterPreviewDto, LetterPreview>();
        CreateMap<UpdatePreviewRequest, LetterPreview>();

        // DynamicTab mappings
        CreateMap<DynamicTab, DynamicTabDto>();
        CreateMap<DynamicTabDto, DynamicTab>();
        CreateMap<CreateDynamicTabDto, DynamicTab>();
        CreateMap<UpdateDynamicTabDto, DynamicTab>();
    }
}
