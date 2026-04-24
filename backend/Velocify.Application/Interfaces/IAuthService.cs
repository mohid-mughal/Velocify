using Velocify.Application.DTOs.Users;

namespace Velocify.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> Register(string firstName, string lastName, string email, string password);
    Task<AuthResponseDto> Login(string email, string password);
    Task<AuthResponseDto> RefreshToken(string refreshToken);
    Task Logout(Guid userId, string refreshToken);
    Task RevokeAllSessions(Guid userId);
}
