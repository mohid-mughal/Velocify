using AutoMapper;
using Velocify.Application.DTOs.Users;
using Velocify.Domain.Entities;

namespace Velocify.Application.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<User, UserSummaryDto>();
    }
}
