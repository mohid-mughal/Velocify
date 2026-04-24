using System.Security.Claims;
using Velocify.Domain.Entities;

namespace Velocify.Application.Interfaces;

/// <summary>
/// Service interface for JWT token generation, validation, and claims extraction.
/// 
/// RESPONSIBILITIES:
/// - Generate access tokens (JWT) with 15-minute expiration
/// - Generate refresh tokens (cryptographically random) with 7-day expiration
/// - Validate JWT tokens and extract claims
/// - Provide token configuration and expiration information
/// 
/// SECURITY DESIGN:
/// - Access tokens are stateless JWTs signed with HMAC-SHA256
/// - Refresh tokens are cryptographically random 64-byte tokens
/// - Token validation includes signature verification, expiration check, and issuer/audience validation
/// - Claims extraction provides type-safe access to token data
/// 
/// USAGE:
/// - AuthService uses this to generate tokens during login/registration
/// - API middleware uses this to validate incoming tokens
/// - Controllers use this to extract user information from validated tokens
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// 
    /// TOKEN STRUCTURE:
    /// - Subject (sub): User ID
    /// - Email: User email address
    /// - Role: User role for authorization
    /// - Name: Full name for display purposes
    /// - Issued At (iat): Token creation timestamp
    /// - Expiration (exp): 15 minutes from creation
    /// - Issuer (iss): API identifier
    /// - Audience (aud): Intended token recipient
    /// 
    /// SECURITY:
    /// - Signed with HMAC-SHA256 using secret key from configuration
    /// - Short expiration (15 min) limits exposure window if compromised
    /// - Stateless: no database lookup required for validation
    /// </summary>
    /// <param name="user">User entity to generate token for</param>
    /// <returns>JWT access token as string</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// 
    /// SECURITY:
    /// - Uses RandomNumberGenerator (cryptographically secure RNG)
    /// - 64 bytes = 512 bits of entropy
    /// - Base64 encoded for safe transmission
    /// - Should be stored as SHA-256 hash in database
    /// 
    /// WHY NOT JWT:
    /// - Refresh tokens need to be revocable (JWTs are stateless)
    /// - Random tokens are simpler and more secure for long-lived credentials
    /// - No need for claims in refresh tokens
    /// </summary>
    /// <returns>Base64-encoded random refresh token</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT access token and returns the claims principal.
    /// 
    /// VALIDATION CHECKS:
    /// - Signature verification using secret key
    /// - Expiration check (token not expired)
    /// - Issuer validation (matches configured issuer)
    /// - Audience validation (matches configured audience)
    /// - Token format validation
    /// 
    /// THROWS:
    /// - SecurityTokenException if token is invalid, expired, or malformed
    /// </summary>
    /// <param name="token">JWT access token to validate</param>
    /// <returns>ClaimsPrincipal containing validated claims</returns>
    ClaimsPrincipal ValidateToken(string token);

    /// <summary>
    /// Extracts the user ID from a validated JWT token.
    /// </summary>
    /// <param name="principal">Claims principal from validated token</param>
    /// <returns>User ID as Guid</returns>
    Guid GetUserId(ClaimsPrincipal principal);

    /// <summary>
    /// Extracts the user email from a validated JWT token.
    /// </summary>
    /// <param name="principal">Claims principal from validated token</param>
    /// <returns>User email address</returns>
    string GetUserEmail(ClaimsPrincipal principal);

    /// <summary>
    /// Extracts the user role from a validated JWT token.
    /// </summary>
    /// <param name="principal">Claims principal from validated token</param>
    /// <returns>User role as string</returns>
    string GetUserRole(ClaimsPrincipal principal);

    /// <summary>
    /// Gets the configured access token expiration time in minutes.
    /// </summary>
    /// <returns>Access token expiration in minutes (default: 15)</returns>
    int GetAccessTokenExpirationMinutes();

    /// <summary>
    /// Gets the configured refresh token expiration time in days.
    /// </summary>
    /// <returns>Refresh token expiration in days (default: 7)</returns>
    int GetRefreshTokenExpirationDays();
}
