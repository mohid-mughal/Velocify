using Velocify.Domain.Enums;

namespace Velocify.Domain.Entities;

public class AiInteractionLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AiFeatureType FeatureType { get; set; }
    public string InputSummary { get; set; } = string.Empty;
    public string OutputSummary { get; set; } = string.Empty;
    public int? TokensUsed { get; set; }
    public int LatencyMs { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
}
