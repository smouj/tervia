using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace Tervia.Core;

/// <summary>Local customization shared by Tervia and the optional Android TV home app.</summary>
public sealed record TvHomeProfile
{
    public const string DefaultSubtitle = "Todo tu entretenimiento, en un mismo lugar.";
    public static readonly IReadOnlySet<string> SupportedThemes = new[]
    {
        "ocean", "violet", "sunset", "forest", "rose", "gold", "ice", "graphite",
    }.ToFrozenSet(StringComparer.Ordinal);
    public static readonly IReadOnlySet<string> SupportedCardLayouts = new[]
    {
        "comfortable", "compact",
    }.ToFrozenSet(StringComparer.Ordinal);
    private static readonly Regex PackageIdPattern = new(
        @"\A[A-Za-z][A-Za-z0-9_]*(?:\.[A-Za-z0-9_]+)+\z",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public string Title { get; init; } = "Mi televisión";
    public string Subtitle { get; init; } = DefaultSubtitle;
    public string ApplicationsTitle { get; init; } = "Tus aplicaciones";
    public string Theme { get; init; } = "ocean";
    public string CardLayout { get; init; } = "comfortable";
    public bool ShowClock { get; init; } = true;
    public bool ShowBranding { get; init; } = true;
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
            ApplicationsTitle = NormalizeText(ApplicationsTitle, "Tus aplicaciones", 36),
            Theme = SupportedThemes.Contains(Theme) ? Theme : "ocean",
            CardLayout = SupportedCardLayouts.Contains(CardLayout) ? CardLayout : "comfortable",
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
