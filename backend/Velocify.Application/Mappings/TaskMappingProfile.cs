using AutoMapper;
using Velocify.Application.DTOs.Tasks;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<TaskItem, TaskDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.EstimatedHours, opt => opt.MapFrom(src => src.EstimatedHours))
            .ForMember(dest => dest.ActualHours, opt => opt.MapFrom(src => src.ActualHours))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags))
            .ForMember(dest => dest.AiPriorityScore, opt => opt.MapFrom(src => src.AiPriorityScore))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt))
            .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedTo))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.PredictedCompletionProbability, opt => opt.Ignore());

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
