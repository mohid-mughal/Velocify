using Velocify.Application.DTOs.Common;
using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Interfaces;

public interface IUserRepository
{
    Task<UserDto?> GetById(Guid id);
    Task<UserDto?> GetByEmail(string email);
    Task<PagedResult<UserDto>> GetList(int page = 1, int pageSize = 20);
    Task<UserDto> Create(UserDto userDto);
    Task<UserDto> Update(UserDto userDto);
    Task Delete(Guid id);
    Task<List<object>> GetProductivityHistory(Guid userId);
}
