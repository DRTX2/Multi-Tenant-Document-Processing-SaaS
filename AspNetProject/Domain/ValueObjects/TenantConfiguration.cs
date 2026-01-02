namespace AspNetProject.Domain.ValueObjects;

public class TenantConfiguration
{
    public int MaxStorageMb { get; init; }
    public bool OcrEnabled { get; init; }
    public int RateLimitPerMinute { get; init; }

    private TenantConfiguration() { } // For EF Core

    public TenantConfiguration(int maxStorageMb, bool ocrEnabled, int rateLimitPerMinute)
    {
        if (maxStorageMb <= 0)
            throw new ArgumentException("Max storage must be positive", nameof(maxStorageMb));
        
        if (rateLimitPerMinute <= 0)
            throw new ArgumentException("Rate limit must be positive", nameof(rateLimitPerMinute));

        MaxStorageMb = maxStorageMb;
        OcrEnabled = ocrEnabled;
        RateLimitPerMinute = rateLimitPerMinute;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not TenantConfiguration other) return false;
        return MaxStorageMb == other.MaxStorageMb &&
               OcrEnabled == other.OcrEnabled &&
               RateLimitPerMinute == other.RateLimitPerMinute;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MaxStorageMb, OcrEnabled, RateLimitPerMinute);
    }
}