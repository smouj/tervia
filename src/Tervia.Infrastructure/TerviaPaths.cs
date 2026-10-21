namespace Tervia.Infrastructure;

/// <summary>Local-first storage layout. Nothing here leaves the machine.</summary>
public sealed class TerviaPaths
{
    public TerviaPaths(string? root = null)
    {
        Root = root ?? DefaultRoot();
    }

    public string Root { get; }
    public string Config => Path.Combine(Root, "config");
    public string Tvs => Path.Combine(Root, "tvs");
    public string Profiles => Path.Combine(Root, "profiles");
    public string Logs => Path.Combine(Root, "logs");
    public string Snapshots => Path.Combine(Root, "snapshots");
    public string Diagnostics => Path.Combine(Root, "diagnostics");
    public string Cache => Path.Combine(Root, "cache");

    public void EnsureCreated()
    {
        foreach (var directory in new[] { Root, Config, Tvs, Profiles, Logs, Snapshots, Diagnostics, Cache })
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string DefaultRoot()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local))
        {
            local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }
        return Path.Combine(local, "Tervia");
    }
}
