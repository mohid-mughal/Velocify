namespace Velocify.Domain.Entities;

public class UserSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
    public string IpAddress { get; set; } = string.Empty;

    // Navigation Properties
    public User User { get; set; } = null!;

    // Business Methods
    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}
