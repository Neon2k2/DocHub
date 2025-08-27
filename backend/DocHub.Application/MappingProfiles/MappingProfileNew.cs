using AutoMapper;
using DocHub.Application.DTOs;
using DocHub.Application.Models;
using DocHub.Application.Utilities;
using DocHub.Core.Entities;
using System;
using System.Collections.Generic;

namespace DocHub.Application.MappingProfiles
{
    public class MappingProfileNew : Profile
    {
        public MappingProfileNew()
        {
            // Digital Signature mappings
            CreateMap<DigitalSignatureEntity, DigitalSignatureDto>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, string>>(src.MetadataJson)));

            CreateMap<DigitalSignatureDto, DigitalSignatureEntity>()
                .ForMember(dest => dest.MetadataJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Metadata)));

            // Dynamic Tab mappings
            CreateMap<DynamicTabEntity, DynamicTabDto>()
                .ForMember(dest => dest.Fields, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<DynamicTabFieldDto>>(src.FieldsJson)))
                .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, object>>(src.ConfigurationJson)));

            CreateMap<DynamicTabDto, DynamicTabEntity>()
                .ForMember(dest => dest.FieldsJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Fields)))
                .ForMember(dest => dest.ConfigurationJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Configuration)));

            // Letter Template mappings
            CreateMap<LetterTemplateFieldEntity, LetterTemplateFieldDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src =>
                    Enum.Parse<DTOs.FieldType>(src.Type)))
                .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, object>>(src.ConfigurationJson)));

            CreateMap<LetterTemplateFieldDto, LetterTemplateFieldEntity>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src =>
                    src.Type.ToString()))
                .ForMember(dest => dest.ConfigurationJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Configuration)));

            CreateMap<LetterTemplateEntity, LetterTemplateDto>()
                .ForMember(d => d.Fields, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<LetterTemplateFieldDto>>(src.FieldsJson)))
                .ForMember(d => d.Metadata, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, object>>(src.MetadataJson)));

            CreateMap<LetterTemplateDto, LetterTemplateEntity>()
                .ForMember(d => d.FieldsJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Fields)))
                .ForMember(d => d.MetadataJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Metadata)));

            // Letter Template CRUD mappings
            CreateMap<CreateLetterTemplateDto, LetterTemplateEntity>();
            CreateMap<UpdateLetterTemplateDto, LetterTemplateEntity>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateLetterTemplateFieldDto, LetterTemplateFieldEntity>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src =>
                    src.Type.ToString()))
                .ForMember(dest => dest.ConfigurationJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Configuration)));

            CreateMap<UpdateLetterTemplateFieldDto, LetterTemplateFieldEntity>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null))
                .ForMember(dest => dest.ConfigurationJson, opt => opt.MapFrom(src =>
                    src.Configuration != null ? JsonHelper.Serialize(src.Configuration) : null));

            // Workflow mappings
            CreateMap<LetterWorkflowEntity, LetterWorkflowDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    Enum.Parse<WorkflowStatusDto>(src.Status)))
                .ForMember(d => d.Steps, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<WorkflowStepDto>>(src.StepsJson)))
                .ForMember(d => d.Comments, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<WorkflowCommentDto>>(src.CommentsJson)))
                .ForMember(d => d.Metadata, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, object>>(src.MetadataJson)));

            CreateMap<LetterWorkflowDto, LetterWorkflowEntity>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Status.ToString()))
                .ForMember(d => d.StepsJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Steps)))
                .ForMember(d => d.CommentsJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Comments)))
                .ForMember(d => d.MetadataJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Metadata)));

            CreateMap<WorkflowStepDto, WorkflowStepEntity>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Status.ToString()))
                .ForMember(dest => dest.RequiredActionsJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.RequiredActions)))
                .ForMember(dest => dest.ConfigurationJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Configuration)));

            CreateMap<WorkflowStepEntity, WorkflowStepDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    Enum.Parse<WorkflowStepStatusDto>(src.Status)))
                .ForMember(dest => dest.RequiredActions, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<string>>(src.RequiredActionsJson)))
                .ForMember(dest => dest.Configuration, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, object>>(src.ConfigurationJson)));

            CreateMap<WorkflowCommentDto, WorkflowCommentEntity>()
                .ForMember(dest => dest.AttachmentsJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Attachments)));

            CreateMap<WorkflowCommentEntity, WorkflowCommentDto>()
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<string>>(src.AttachmentsJson)));

            CreateMap<LetterWorkflowRequestDto, LetterWorkflowEntity>();

            // Email mappings
            CreateMap<EmailHistoryEntity, EmailHistoryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    Enum.Parse<EmailStatusDto>(src.Status)))
                .ForMember(dest => dest.Cc, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<string>>(src.CcJson)))
                .ForMember(dest => dest.Bcc, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<string>>(src.BccJson)))
                .ForMember(dest => dest.AttachmentNames, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<string>>(src.AttachmentNamesJson)))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, string>>(src.MetadataJson)));

            CreateMap<EmailHistoryDto, EmailHistoryEntity>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Status.ToString()))
                .ForMember(dest => dest.CcJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Cc)))
                .ForMember(dest => dest.BccJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Bcc)))
                .ForMember(dest => dest.AttachmentNamesJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.AttachmentNames)))
                .ForMember(dest => dest.MetadataJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Metadata)));

            CreateMap<EmailRequestDto, Models.EmailRequest>();
            CreateMap<Models.EmailRequest, EmailHistoryEntity>();

            // User mappings
            CreateMap<UserEntity, UserDto>()
                .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<List<string>>(src.PermissionsJson)))
                .ForMember(dest => dest.Preferences, opt => opt.MapFrom(src =>
                    JsonHelper.Deserialize<Dictionary<string, object>>(src.PreferencesJson)));

            CreateMap<UserDto, UserEntity>()
                .ForMember(dest => dest.PermissionsJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Permissions)))
                .ForMember(dest => dest.PreferencesJson, opt => opt.MapFrom(src =>
                    JsonHelper.Serialize(src.Preferences)));
        }
    }
}
