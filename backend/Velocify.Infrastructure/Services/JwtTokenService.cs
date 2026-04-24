using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Velocify.Application.Interfaces;
using Velocify.Domain.Entities;

namespace Velocify.Infrastructure.Services;

/// <summary>
/// JWT token service implementation for generating, validating, and extracting claims from tokens.
/// 
/// TOKEN ARCHITECTURE:
/// - Access Tokens: JWT with 15-minute expiration, signed with HMAC-SHA256
/// - Refresh Tokens: Cryptographically random 64-byte tokens with 7-day expiration
/// - Token Validation: Signature verification, expiration check, issuer/audience validation
/// 
/// SECURITY CONSIDERATIONS:
/// 1. Access tokens are short-lived (15 min) to limit exposure window if compromised
/// 2. Refresh tokens are cryptographically random (512 bits entropy) and stored as SHA-256 hashes
/// 3. JWT secret key must be at least 32 characters (256 bits) for HMAC-SHA256 security
/// 4. Token validation is stateless (no database lookup) for performance
/// 5. Claims are extracted type-safely to prevent injection attacks
/// 
/// CONFIGURATION REQUIREMENTS:
/// - JwtSettings:SecretKey: At least 32 characters, stored securely (Azure Key Vault in production)
/// - JwtSettings:Issuer: API identifier (e.g., https://api.velocify.com)
/// - JwtSettings:Audience: Intended token recipient (e.g., https://app.velocify.com)
/// - JwtSettings:AccessTokenExpirationMinutes: Default 15 minutes
/// - JwtSettings:RefreshTokenExpirationDays: Default 7 days
/// 
/// PERFORMANCE:
/// - JWT generation: ~1-2ms (CPU-bound, no I/O)
/// - JWT validation: ~0.5-1ms (signature verification)
/// - Refresh token generation: ~0.1ms (RNG operation)
/// - All operations are stateless (no database queries)
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    // JWT configuration keys
    private const string JWT_SECRET_KEY = "JwtSettings:SecretKey";
    private const string JWT_ISSUER = "JwtSettings:Issuer";
    private const string JWT_AUDIENCE = "JwtSettings:Audience";
    private const string JWT_ACCESS_TOKEN_EXPIRATION = "JwtSettings:AccessTokenExpirationMinutes";
    private const string JWT_REFRESH_TOKEN_EXPIRATION = "JwtSettings:RefreshTokenExpirationDays";

    // Default values if configuration is missing
    private const int DEFAULT_ACCESS_TOKEN_EXPIRATION_MINUTES = 15;
    private const int DEFAULT_REFRESH_TOKEN_EXPIRATION_DAYS = 7;
    private const int MINIMUM_SECRET_KEY_LENGTH = 32; // 256 bits for HMAC-SHA256

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _tokenHandler = new JwtSecurityTokenHandler();
        
        // Validate configuration on startup
        ValidateConfiguration();
    }

    /// <summary>
    /// Generates a JWT access token with user claims.
    /// 
    /// TOKEN STRUCTURE:
    /// - Subject (sub): User ID for identifying the authenticated user
    /// - Email: User email address for display and audit purposes
    /// - Role: User role for authorization decisions (SuperAdmin, Admin, Member)
    /// - Name: Full name for UI display
    /// - JTI (JWT ID): Unique token identifier for tracking and potential revocation
    /// - IAT (Issued At): Token creation timestamp for audit trail
    /// - EXP (Expiration): Token expiration timestamp (15 minutes from creation)
    /// - ISS (Issuer): API identifier for token source validation
    /// - AUD (Audience): Intended recipient for token destination validation
    /// 
    /// SECURITY:
    /// - Signed with HMAC-SHA256 using secret key from configuration
    /// - Secret key must be at least 32 characters (256 bits)
    /// - Token is stateless: no database lookup required for validation
    /// - Short expiration (15 min) limits damage if token is compromised
    /// - JTI enables token tracking and potential blacklisting if needed
    /// 
    /// CLAIMS USAGE:
    /// - Subject: Used by [Authorize] attribute to identify current user
    /// - Role: Used by [Authorize(Roles = "Admin")] for role-based access control
    /// - Email: Used for audit logging and user display
    /// - Name: Used in UI to display current user's name
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        var secretKey = GetSecretKey();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(ClaimTypes.Name, user.CalculateFullName()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration[JWT_ISSUER],
            audience: _configuration[JWT_AUDIENCE],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// 
    /// SECURITY:
    /// - Uses RandomNumberGenerator (cryptographically secure RNG)
    /// - 64 bytes = 512 bits of entropy (extremely difficult to guess)
    /// - Base64 encoded for safe transmission in HTTP headers/cookies
    /// - Token should be stored as SHA-256 hash in database (never stored in plain text)
    /// 
    /// WHY NOT JWT FOR REFRESH TOKENS:
    /// - Refresh tokens need to be revocable (JWTs are stateless)
    /// - Random tokens are simpler and more secure for long-lived credentials
    /// - No need for claims in refresh tokens (only used to obtain access tokens)
    /// - Easier to implement token rotation (invalidate old, issue new)
    /// 
    /// ENTROPY CALCULATION:
    /// - 64 bytes = 512 bits
    /// - 2^512 possible values ≈ 10^154 combinations
    /// - Brute force attack is computationally infeasible
    /// - Even at 1 trillion guesses per second, would take longer than age of universe
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Validates a JWT access token and returns the claims principal.
    /// 
    /// VALIDATION CHECKS:
    /// 1. Token format validation (valid JWT structure)
    /// 2. Signature verification using secret key (ensures token wasn't tampered with)
    /// 3. Expiration check (token not expired)
    /// 4. Issuer validation (matches configured issuer)
    /// 5. Audience validation (matches configured audience)
    /// 6. Clock skew tolerance (5 minutes to account for server time differences)
    /// 
    /// SECURITY:
    /// - Signature verification prevents token tampering
    /// - Expiration check prevents replay attacks with old tokens
    /// - Issuer/audience validation prevents token misuse across different systems
    /// - Clock skew tolerance prevents false rejections due to time sync issues
    /// 
    /// PERFORMANCE:
    /// - Validation is CPU-bound (signature verification)
    /// - No database queries (stateless validation)
    /// - Typical validation time: 0.5-1ms
    /// 
    /// THROWS:
    /// - SecurityTokenException: Token is invalid, expired, or malformed
    /// - ArgumentException: Token is null or empty
    /// </summary>
    public ClaimsPrincipal ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        }

        var secretKey = GetSecretKey();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = _configuration[JWT_ISSUER],
            ValidateAudience = true,
            ValidAudience = _configuration[JWT_AUDIENCE],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
        };

        try
        {
            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Additional validation: ensure token is JWT and uses correct algorithm
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token algorithm");
            }

            return principal;
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            // Re-throw security exceptions as-is
            throw;
        }
        catch (Exception ex)
        {
            // Wrap unexpected exceptions
            throw new SecurityTokenException("Token validation failed", ex);
        }
    }

    /// <summary>
    /// Extracts the user ID from a validated JWT token.
    /// 
    /// CLAIM SOURCE:
    /// - Uses JwtRegisteredClaimNames.Sub (Subject) claim
    /// - Subject claim contains the user's unique identifier (Guid)
    /// 
    /// USAGE:
    /// - Controllers use this to identify the current authenticated user
    /// - Authorization logic uses this to check resource ownership
    /// - Audit logging uses this to track user actions
    /// </summary>
    public Guid GetUserId(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            throw new InvalidOperationException("User ID claim not found in token");
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new InvalidOperationException("User ID claim is not a valid GUID");
        }

        return userId;
    }

    /// <summary>
    /// Extracts the user email from a validated JWT token.
    /// 
    /// CLAIM SOURCE:
    /// - Uses JwtRegisteredClaimNames.Email claim
    /// 
    /// USAGE:
    /// - UI displays current user's email
    /// - Audit logs include user email for traceability
    /// - Email-based lookups in admin interfaces
    /// </summary>
    public string GetUserEmail(ClaimsPrincipal principal)
    {
        var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? principal.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            throw new InvalidOperationException("Email claim not found in token");
        }

        return email;
    }

    /// <summary>
    /// Extracts the user role from a validated JWT token.
    /// 
    /// CLAIM SOURCE:
    /// - Uses ClaimTypes.Role claim
    /// - Role values: SuperAdmin, Admin, Member
    /// 
    /// USAGE:
    /// - [Authorize(Roles = "Admin")] attribute uses this for authorization
    /// - Controllers check role for conditional logic
    /// - UI shows/hides features based on role
    /// </summary>
    public string GetUserRole(ClaimsPrincipal principal)
    {
        var role = principal.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(role))
        {
            throw new InvalidOperationException("Role claim not found in token");
        }

        return role;
    }

    /// <summary>
    /// Gets the configured access token expiration time in minutes.
    /// Defaults to 15 minutes if not configured.
    /// 
    /// SECURITY RATIONALE:
    /// - 15 minutes balances security and user experience
    /// - Short enough to limit exposure if token is compromised
    /// - Long enough to avoid frequent re-authentication
    /// - Refresh tokens handle long-term sessions
    /// </summary>
    public int GetAccessTokenExpirationMinutes()
    {
        return _configuration.GetValue<int?>(JWT_ACCESS_TOKEN_EXPIRATION)
            ?? DEFAULT_ACCESS_TOKEN_EXPIRATION_MINUTES;
    }

    /// <summary>
    /// Gets the configured refresh token expiration time in days.
    /// Defaults to 7 days if not configured.
    /// 
    /// SECURITY RATIONALE:
    /// - 7 days balances security and convenience
    /// - Long enough for "remember me" functionality
    /// - Short enough to require periodic re-authentication
    /// - Can be revoked immediately if compromise is detected
    /// </summary>
    public int GetRefreshTokenExpirationDays()
    {
        return _configuration.GetValue<int?>(JWT_REFRESH_TOKEN_EXPIRATION)
            ?? DEFAULT_REFRESH_TOKEN_EXPIRATION_DAYS;
    }

    /// <summary>
    /// Gets the JWT secret key from configuration and validates its length.
    /// 
    /// SECURITY:
    /// - Secret key must be at least 32 characters (256 bits) for HMAC-SHA256
    /// - Shorter keys are cryptographically weak and vulnerable to brute force
    /// - Key should be stored securely (Azure Key Vault, environment variables)
    /// - Never commit secret keys to source control
    /// 
    /// THROWS:
    /// - InvalidOperationException if key is not configured or too short
    /// </summary>
    private string GetSecretKey()
    {
        var secretKey = _configuration[JWT_SECRET_KEY];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "JWT secret key is not configured. Set JwtSettings:SecretKey in appsettings.json or environment variables.");
        }

        if (secretKey.Length < MINIMUM_SECRET_KEY_LENGTH)
        {
            throw new InvalidOperationException(
                $"JWT secret key must be at least {MINIMUM_SECRET_KEY_LENGTH} characters long for security. " +
                $"Current length: {secretKey.Length}");
        }

        return secretKey;
    }

    /// <summary>
    /// Validates JWT configuration on service initialization.
    /// Fails fast if configuration is invalid to prevent runtime errors.
    /// 
    /// VALIDATION CHECKS:
    /// - Secret key exists and meets minimum length requirement
    /// - Issuer is configured
    /// - Audience is configured
    /// 
    /// RATIONALE:
    /// - Fail fast principle: detect configuration errors at startup
    /// - Prevents runtime failures during authentication
    /// - Provides clear error messages for misconfiguration
    /// </summary>
    private void ValidateConfiguration()
    {
        // Validate secret key (will throw if invalid)
        _ = GetSecretKey();

        // Validate issuer
        if (string.IsNullOrWhiteSpace(_configuration[JWT_ISSUER]))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured. Set JwtSettings:Issuer in appsettings.json.");
        }

        // Validate audience
        if (string.IsNullOrWhiteSpace(_configuration[JWT_AUDIENCE]))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured. Set JwtSettings:Audience in appsettings.json.");
        }
    }
}
