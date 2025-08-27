using AutoMapper;
using DocHub.Application.DTOs.DynamicTabs;
using DocHub.Core.Entities;
using System.Text.Json;

namespace DocHub.Application.MappingProfiles;

public class DynamicTabProfile : Profile
{
    public DynamicTabProfile()
    {
        CreateMap<DynamicTab, DynamicTabDto>()
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields));

        CreateMap<CreateDynamicTabDto, DynamicTab>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<UpdateDynamicTabDto, DynamicTab>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<DynamicTabField, DynamicTabFieldDto>();
        CreateMap<CreateDynamicTabFieldDto, DynamicTabField>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()));

        CreateMap<DynamicTabData, DynamicTabDataDto>()
            .ForMember(dest => dest.Data, opt => opt.MapFrom(src =>
                JsonSerializer.Deserialize<Dictionary<string, object>>(src.DataContent ?? "{}")));

        CreateMap<AddTabDataDto, DynamicTabData>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid().ToString()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.DataContent, opt => opt.MapFrom(src =>
                JsonSerializer.Serialize(src.Data)));
    }
}
