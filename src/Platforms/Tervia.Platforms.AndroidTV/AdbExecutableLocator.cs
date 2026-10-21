namespace Tervia.Platforms.AndroidTV;

/// <summary>Finds the user's installed Android Platform-Tools without changing global PATH.</summary>
public static class AdbExecutableLocator
{
    public static string Resolve()
    {
        var candidates = new List<string>();
        AddSdkCandidate(candidates, Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"));
        AddSdkCandidate(candidates, Environment.GetEnvironmentVariable("ANDROID_HOME"));

        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localApplicationData))
        {
            AddSdkCandidate(candidates, Path.Combine(localApplicationData, "Android", "Sdk"));
        }

        candidates.Add(Path.Combine(AppContext.BaseDirectory, "platform-tools", "adb.exe"));
        var installedSdk = candidates.FirstOrDefault(File.Exists);
        return installedSdk ?? "adb";
    }

    private static void AddSdkCandidate(List<string> candidates, string? sdkRoot)
    {
        if (!string.IsNullOrWhiteSpace(sdkRoot))
        {
            candidates.Add(Path.Combine(sdkRoot, "platform-tools", "adb.exe"));
        }
    }
}
