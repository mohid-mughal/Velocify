using AutoMapper;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class CommentMappingProfile : Profile
{
    public CommentMappingProfile()
    {
        CreateMap<TaskComment, CommentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TaskItemId, opt => opt.MapFrom(src => src.TaskItemId))
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.SentimentScore, opt => opt.MapFrom(src => src.SentimentScore))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}
