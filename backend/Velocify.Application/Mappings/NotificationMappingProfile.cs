using AutoMapper;
using Velocify.Application.DTOs.Notifications;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
