namespace Tervia.Platforms.AndroidTV;

/// <summary>Builds adb argument lists. Never concatenates into a shell string.</summary>
public static class AdbCommandLine
{
    public static IReadOnlyList<string> ForDevice(string? serial, params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var list = new List<string>(arguments.Length + 2);
        if (!string.IsNullOrWhiteSpace(serial))
        {
            list.Add("-s");
            list.Add(serial);
        }
        list.AddRange(arguments);
        return list;
    }

    public static IReadOnlyList<string> Shell(string? serial, params string[] command)
        => ForDevice(serial, ["shell", .. command]);

    public static IReadOnlyList<string> GetProp(string? serial, string property)
        => Shell(serial, "getprop", property);
}
