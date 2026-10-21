using Tervia.Core;

namespace Tervia.Platforms.AndroidTV;

/// <summary>Fire OS shares the Android transport but has its own rules.</summary>
public static class FireTvPlatformProfile
{
    public static TvPlatform Platform => TvPlatform.FireTV;
    public static string PlatformName => "Fire OS";

    public static bool IsAmazonPackage(string packageId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        return packageId.StartsWith("com.amazon.", StringComparison.OrdinalIgnoreCase);
    }
}
