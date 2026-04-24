using AutoMapper;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class AuditLogMappingProfile : Profile
{
    public AuditLogMappingProfile()
    {
        CreateMap<TaskAuditLog, TaskAuditLogDto>()
            .ForMember(dest => dest.ChangedBy, opt => opt.MapFrom(src => src.ChangedBy));
    }
}
