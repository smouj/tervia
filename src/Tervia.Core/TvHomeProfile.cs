using System.Text.RegularExpressions;

namespace Tervia.Core;

/// <summary>Local customization shared by Tervia and the optional Android TV home app.</summary>
public sealed record TvHomeProfile
{
    public const string DefaultSubtitle = "Todo tu entretenimiento, en un mismo lugar.";
    private static readonly HashSet<string> Themes = new(StringComparer.Ordinal)
    {
        "ocean", "violet", "sunset", "forest",
    };
    private static readonly Regex PackageIdPattern = new(
        @"\A[A-Za-z][A-Za-z0-9_]*(?:\.[A-Za-z0-9_]+)+\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public string Title { get; init; } = "Mi televisión";
    public string Subtitle { get; init; } = DefaultSubtitle;
    public string Theme { get; init; } = "ocean";
    public bool ShowClock { get; init; } = true;
    public IReadOnlyList<string> PinnedApplicationIds { get; init; } = [];
    public IReadOnlyList<string> HiddenApplicationIds { get; init; } = [];

    public TvHomeProfile ForDevice(TvDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        return (this with
        {
            Title = string.IsNullOrWhiteSpace(Title) || Title == "Mi televisión" ? device.FriendlyName : Title,
        }).Normalize();
    }

    public TvHomeProfile Normalize()
        => this with
        {
            Title = NormalizeText(Title, "Mi televisión", 42),
            Subtitle = NormalizeText(Subtitle, DefaultSubtitle, 88),
            Theme = Themes.Contains(Theme) ? Theme : "ocean",
            PinnedApplicationIds = NormalizePackageIds(PinnedApplicationIds),
            HiddenApplicationIds = NormalizePackageIds(HiddenApplicationIds),
        };

    private static string NormalizeText(string? value, string fallback, int limit)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = new string(value
            .Select(character => char.IsControl(character) ? ' ' : character)
            .Take(limit)
            .ToArray())
            .Trim();
        normalized = string.Join(' ', normalized.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return normalized.Length == 0 ? fallback : normalized;
    }

    private static string[] NormalizePackageIds(IEnumerable<string>? values)
        => (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value) && PackageIdPattern.IsMatch(value))
            .Distinct(StringComparer.Ordinal)
            .Take(100)
            .ToArray();
}
