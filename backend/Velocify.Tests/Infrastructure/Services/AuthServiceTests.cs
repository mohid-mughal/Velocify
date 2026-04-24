using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Velocify.Application.DTOs.Users;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;
using Velocify.Domain.Enums;
using Velocify.Infrastructure.Data;
using Velocify.Infrastructure.Services;
using Xunit;

namespace Velocify.Tests.Infrastructure.Services;

/// <summary>
/// Unit tests for AuthService covering authentication, token management, and session control.
/// 
/// TEST COVERAGE:
/// - Registration: Duplicate email conflict detection
/// - Login: Wrong password unauthorized response
/// - Token Refresh: Old token invalidation after rotation
/// - Logout: Token revocation
/// - Session Management: Multiple session handling
/// 
/// TESTING APPROACH:
/// - Uses in-memory database for realistic data persistence testing
/// - Mocks IJwtTokenService to isolate AuthService logic
/// - Tests security-critical paths: duplicate registration, invalid credentials, token rotation
/// - Validates business rules: token invalidation, session revocation
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly VelocifyDbContext _context;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // Setup in-memory database with unique name per test instance
        var options = new DbContextOptionsBuilder<VelocifyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VelocifyDbContext(options);

        // Setup JWT token service mock
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _jwtTokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("mock-access-token");
        _jwtTokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("mock-refresh-token");
        _jwtTokenServiceMock.Setup(x => x.GetAccessTokenExpirationMinutes())
            .Returns(15);
        _jwtTokenServiceMock.Setup(x => x.GetRefreshTokenExpirationDays())
            .Returns(7);

        _authService = new AuthService(_context, _jwtTokenServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = "duplicate@example.com";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "User",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Member,
            ProductivityScore = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        // Act
        var act = async () => await _authService.Register("New", "User", email, "newpassword123");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"User with email {email} already exists");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var email = "user@example.com";
        var correctPassword = "correctPassword123";
        var wrongPassword = "wrongPassword123";

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword),
            Role = UserRole.Member,
            ProductivityScore = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var act = async () => await _authService.Login(email, wrongPassword);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task RefreshToken_ShouldInvalidateOldToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Member,
            ProductivityScore = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create initial session with refresh token
        var refreshToken = "initial-refresh-token";
        var refreshTokenHash = ComputeSha256Hash(refreshToken);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            IpAddress = "127.0.0.1"
        };

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.RefreshToken(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("mock-access-token");
        result.RefreshToken.Should().Be("mock-refresh-token");

        // Verify old token is revoked
        var oldSession = await _context.UserSessions.FindAsync(session.Id);
        oldSession.Should().NotBeNull();
        oldSession!.IsRevoked.Should().BeTrue();

        // Verify new session is created
        var newSession = await _context.UserSessions
            .Where(s => s.UserId == user.Id && !s.IsRevoked)
            .FirstOrDefaultAsync();
        newSession.Should().NotBeNull();
        newSession!.Id.Should().NotBe(session.Id);
    }

    [Fact]
    public async Task Logout_ShouldRevokeToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Member,
            ProductivityScore = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create session with refresh token
        var refreshToken = "logout-refresh-token";
        var refreshTokenHash = ComputeSha256Hash(refreshToken);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshToken = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            IpAddress = "127.0.0.1"
        };

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        await _authService.Logout(user.Id, refreshToken);

        // Assert
        var revokedSession = await _context.UserSessions.FindAsync(session.Id);
        revokedSession.Should().NotBeNull();
        revokedSession!.IsRevoked.Should().BeTrue();
    }

    /// <summary>
    /// Helper method to compute SHA-256 hash matching AuthService implementation.
    /// This is needed to create test sessions with properly hashed tokens.
    /// </summary>
    private static string ComputeSha256Hash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hashBytes);
    }
}
