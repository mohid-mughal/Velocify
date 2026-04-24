using AutoMapper;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class CommentMappingProfile : Profile
{
    public CommentMappingProfile()
    {
        CreateMap<TaskComment, CommentDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
    }
}
