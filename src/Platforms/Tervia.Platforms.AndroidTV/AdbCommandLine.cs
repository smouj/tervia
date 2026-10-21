using System.Globalization;
using System.Net;

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

    public static string TargetForAddress(string networkAddress, int defaultPort = 5555)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(networkAddress);
        ArgumentOutOfRangeException.ThrowIfLessThan(defaultPort, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(defaultPort, 65535);

        var address = networkAddress.Trim();
        if (address[0] == '[')
        {
            var closingBracket = address.IndexOf(']');
            if (closingBracket > 1 &&
                IPAddress.TryParse(address[1..closingBracket], out var bracketAddress) &&
                closingBracket + 1 < address.Length && address[closingBracket + 1] == ':' &&
                int.TryParse(address.AsSpan(closingBracket + 2), NumberStyles.None, CultureInfo.InvariantCulture, out var bracketedPort) &&
                bracketedPort is >= 1 and <= 65535)
            {
                return FormatTarget(bracketAddress, bracketedPort);
            }

            throw InvalidAddress();
        }

        if (IPAddress.TryParse(address, out var ipAddress))
        {
            return FormatTarget(ipAddress, defaultPort);
        }

        var separator = address.LastIndexOf(':');
        if (separator > 0 &&
            IPAddress.TryParse(address[..separator], out var ipv4Address) &&
            ipv4Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
            int.TryParse(address.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var port) &&
            port is >= 1 and <= 65535)
        {
            return FormatTarget(ipv4Address, port);
        }

        throw InvalidAddress();
    }

    private static ArgumentException InvalidAddress()
        => new("The ADB address must be an IPv4 or IPv6 address, optionally with a port.", "networkAddress");

    private static string FormatTarget(IPAddress address, int port)
        => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6
            ? $"[{address}]:{port.ToString(CultureInfo.InvariantCulture)}"
            : $"{address}:{port.ToString(CultureInfo.InvariantCulture)}";
}
