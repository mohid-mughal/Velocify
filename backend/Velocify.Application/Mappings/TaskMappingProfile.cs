using AutoMapper;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskDto>()
            .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedTo))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));

        CreateMap<TaskItem, TaskDetailDto>()
            .IncludeBase<TaskItem, TaskDto>()
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments.Where(c => !c.IsDeleted)))
            .ForMember(dest => dest.AuditLog, opt => opt.MapFrom(src => src.AuditLogs))
            .ForMember(dest => dest.Subtasks, opt => opt.MapFrom(src => src.Subtasks.Where(s => !s.IsDeleted)))
            .ForMember(dest => dest.AverageSentiment, opt => opt.MapFrom(src => 
                src.Comments.Where(c => !c.IsDeleted && c.SentimentScore.HasValue).Any()
                    ? src.Comments.Where(c => !c.IsDeleted && c.SentimentScore.HasValue).Average(c => c.SentimentScore!.Value)
                    : (decimal?)null));
    }
}
