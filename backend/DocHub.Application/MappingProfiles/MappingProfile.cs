using AutoMapper;
using DocHub.Application.DTOs;
using DocHub.Application.Models;
using DocHub.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Models = DocHub.Application.Models;
using DTOs = DocHub.Application.DTOs;

namespace DocHub.Application.MappingProfiles
{
    public class MappingProfile : Profile
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public MappingProfile()
        {
            // Digital Signature mappings
            CreateMap<DigitalSignatureEntity, DigitalSignatureDto>();
            CreateMap<DigitalSignatureDto, DigitalSignatureEntity>();
            CreateMap<CreateDigitalSignatureDto, DigitalSignatureEntity>();
            CreateMap<UpdateDigitalSignatureDto, DigitalSignatureEntity>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Dynamic Tab mappings
            CreateMap<DynamicTabEntity, DynamicTabDto>();
            CreateMap<DynamicTabDto, DynamicTabEntity>();

            // Letter Template mappings
            CreateMap<Core.Entities.LetterTemplateField, DTOs.LetterTemplateFieldDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src =>
                    src.FieldType != null ? Enum.Parse<DTOs.FieldType>(src.FieldType) : DTOs.FieldType.Text))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FieldName))
                .ForMember(dest => dest.Label, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.HelpText))
                .ForMember(dest => dest.ValidationRegex, opt => opt.MapFrom(src => src.ValidationRules));

            CreateMap<DTOs.LetterTemplateFieldDto, Core.Entities.LetterTemplateField>()
                .ForMember(dest => dest.FieldType, opt => opt.MapFrom(src =>
                    src.Type.ToString()))
                .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Label))
                .ForMember(dest => dest.HelpText, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ValidationRules, opt => opt.MapFrom(src => src.ValidationRegex));

            CreateMap<Core.Entities.LetterTemplate, DTOs.LetterTemplateDto>()
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.TemplateContent))
                .ForMember(dest => dest.Fields, opt => opt.Ignore())  // Handle fields separately
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.LastModifiedDate, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => src.UpdatedBy));

            CreateMap<DTOs.LetterTemplateDto, Core.Entities.LetterTemplate>()
                .ForMember(dest => dest.TemplateContent, opt => opt.MapFrom(src => src.Content))
                .ForMember(dest => dest.Fields, opt => opt.Ignore())  // Handle fields separately
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.LastModifiedDate))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.LastModifiedBy));

            // Letter Template Create/Update mappings
            CreateMap<CreateLetterTemplateDto, LetterTemplateEntity>();
            CreateMap<UpdateLetterTemplateDto, LetterTemplateEntity>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateLetterTemplateFieldDto, LetterTemplateFieldEntity>();
            CreateMap<UpdateLetterTemplateFieldDto, LetterTemplateFieldEntity>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Workflow mappings
            CreateMap<LetterWorkflowEntity, LetterWorkflowDto>()
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src =>
                    src.StepsJson != null ? JsonSerializer.Deserialize<List<WorkflowStepDto>>(src.StepsJson, JsonOptions) : null))
                .ForMember(dest => dest.Comments, opt => opt.MapFrom(src =>
                    src.CommentsJson != null ? JsonSerializer.Deserialize<List<WorkflowCommentDto>>(src.CommentsJson, JsonOptions) : null))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                    src.MetadataJson != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.MetadataJson, JsonOptions) : null));

            CreateMap<LetterWorkflowDto, LetterWorkflowEntity>()
                .ForMember(dest => dest.StepsJson, opt => opt.MapFrom(src =>
                    src.Steps != null ? JsonSerializer.Serialize(src.Steps, JsonOptions) : null))
                .ForMember(dest => dest.CommentsJson, opt => opt.MapFrom(src =>
                    src.Comments != null ? JsonSerializer.Serialize(src.Comments, JsonOptions) : null))
                .ForMember(dest => dest.MetadataJson, opt => opt.MapFrom(src =>
                    src.Metadata != null ? JsonSerializer.Serialize(src.Metadata, JsonOptions) : null));

            CreateMap<WorkflowStepDto, WorkflowStepEntity>();
            CreateMap<WorkflowStepEntity, WorkflowStepDto>();
            CreateMap<WorkflowCommentDto, WorkflowCommentEntity>();
            CreateMap<WorkflowCommentEntity, WorkflowCommentDto>();
            CreateMap<LetterWorkflowRequestDto, LetterWorkflowEntity>();

            // Email mappings
            CreateMap<EmailHistoryEntity, DTOs.EmailHistory>()
                .ForMember(dest => dest.Cc, opt => opt.MapFrom(src =>
                    src.CcJson != null ? JsonSerializer.Deserialize<List<string>>(src.CcJson, JsonOptions) : new List<string>()))
                .ForMember(dest => dest.Bcc, opt => opt.MapFrom(src =>
                    src.BccJson != null ? JsonSerializer.Deserialize<List<string>>(src.BccJson, JsonOptions) : new List<string>()))
                .ForMember(dest => dest.AttachmentNames, opt => opt.MapFrom(src =>
                    src.AttachmentNamesJson != null ? JsonSerializer.Deserialize<List<string>>(src.AttachmentNamesJson, JsonOptions) : new List<string>()))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                    src.MetadataJson != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(src.MetadataJson, JsonOptions) : new Dictionary<string, string>()));

            CreateMap<DTOs.EmailHistory, EmailHistoryEntity>()
                .ForMember(dest => dest.CcJson, opt => opt.MapFrom(src =>
                    JsonSerializer.Serialize(src.Cc, JsonOptions)))
                .ForMember(dest => dest.BccJson, opt => opt.MapFrom(src =>
                    JsonSerializer.Serialize(src.Bcc, JsonOptions)))
                .ForMember(dest => dest.AttachmentNamesJson, opt => opt.MapFrom(src =>
                    JsonSerializer.Serialize(src.AttachmentNames, JsonOptions)))
                .ForMember(dest => dest.MetadataJson, opt => opt.MapFrom(src =>
                    JsonSerializer.Serialize(src.Metadata, JsonOptions)));

            CreateMap<DTOs.EmailRequest, Models.EmailRequest>();
            CreateMap<Models.EmailRequest, EmailHistoryEntity>();

            // User mappings
            CreateMap<UserEntity, UserDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                    src.PermissionsJson != null ? JsonSerializer.Deserialize<List<string>>(src.PermissionsJson, JsonOptions) : null))
                .ForMember(dest => dest.Preferences, opt => opt.MapFrom(src =>
                    src.PreferencesJson != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.PreferencesJson, JsonOptions) : null));

            CreateMap<UserDto, UserEntity>()
                .ForMember(dest => dest.PermissionsJson, opt => opt.MapFrom(src =>
                    src.Permissions != null ? JsonSerializer.Serialize(src.Permissions, JsonOptions) : null))
                .ForMember(dest => dest.PreferencesJson, opt => opt.MapFrom(src =>
                    src.Preferences != null ? JsonSerializer.Serialize(src.Preferences, JsonOptions) : null));
        }
    }
}
