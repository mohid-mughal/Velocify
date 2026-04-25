using AutoMapper;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class AuditLogMappingProfile : Profile
{
    public AuditLogMappingProfile()
    {
        CreateMap<TaskAuditLog, TaskAuditLogDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TaskItemId, opt => opt.MapFrom(src => src.TaskItemId))
            .ForMember(dest => dest.ChangedBy, opt => opt.MapFrom(src => src.ChangedBy))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.FieldName))
            .ForMember(dest => dest.OldValue, opt => opt.MapFrom(src => src.OldValue))
            .ForMember(dest => dest.NewValue, opt => opt.MapFrom(src => src.NewValue))
            .ForMember(dest => dest.ChangedAt, opt => opt.MapFrom(src => src.ChangedAt));
    }
}
