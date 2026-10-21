using Tervia.Core;
using System.Net;

namespace Tervia.Discovery;

/// <summary>Parses an SSDP response or NOTIFY payload into a candidate.</summary>
public static class SsdpResponseParser
{
    public static TvCandidate? Parse(string payload, IPAddress? sourceAddress = null)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = payload.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0 || !(lines[0].StartsWith("HTTP/", StringComparison.OrdinalIgnoreCase) ||
                                   lines[0].StartsWith("NOTIFY", StringComparison.OrdinalIgnoreCase) ||
                                   lines[0].StartsWith("M-SEARCH", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        foreach (var line in lines.Skip(1))
        {
            var separator = line.IndexOf(':', StringComparison.Ordinal);
            if (separator <= 0)
            {
                continue;
            }
            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (key.Length > 0)
            {
                headers[key] = value;
            }
        }

        var location = Get(headers, "LOCATION");
        // During a real scan, use the sender address from the local UDP packet.
        // LOCATION is untrusted input and must not redirect the user toward a
        // different host when they later choose to connect.
        var address = sourceAddress?.ToString() ?? (location is null ? null : ExtractHost(location));
        var server = Get(headers, "SERVER") ?? string.Empty;
        var usn = Get(headers, "USN") ?? string.Empty;
        var st = Get(headers, "ST") ?? string.Empty;
        var model = Get(headers, "MODELNAME") ?? Get(headers, "MODEL");
        var name = Get(headers, "FRIENDLYNAME") ?? Get(headers, "DEVICENAME") ?? model;

        if (address is null && name is null)
        {
            return null;
        }

        var platform = DetectPlatform(server, usn, st);

        return new TvCandidate
        {
            Name = name ?? address ?? "Unknown TV",
            Address = address ?? "unknown",
            DiscoveryProtocol = "ssdp",
            ManufacturerHint = DetectManufacturer(server, usn),
            ModelHint = model,
            PlatformHint = platform,
            Confidence = platform == TvPlatform.Unknown ? 0.4 : 0.8,
        };
    }

    private static string? Get(Dictionary<string, string> headers, string key)
        => headers.TryGetValue(key, out var value) && value.Length > 0 ? value : null;

    private static string? ExtractHost(string location)
    {
        if (!Uri.TryCreate(location, UriKind.Absolute, out var uri))
        {
            return null;
        }
        return uri.Host;
    }

    private static TvPlatform DetectPlatform(string server, string usn, string st)
    {
        var haystack = $"{server} {usn} {st}".ToLowerInvariant();

        if (haystack.Contains("roku", StringComparison.Ordinal))
        {
            return TvPlatform.RokuTV;
        }
        if (haystack.Contains("tizen", StringComparison.Ordinal) || haystack.Contains("samsung", StringComparison.Ordinal))
        {
            return TvPlatform.Tizen;
        }
        if (haystack.Contains("webos", StringComparison.Ordinal) || haystack.Contains("lge", StringComparison.Ordinal))
        {
            return TvPlatform.WebOS;
        }
        if (haystack.Contains("vidaa", StringComparison.Ordinal) || haystack.Contains("hisense", StringComparison.Ordinal))
        {
            return TvPlatform.Vidaa;
        }
        if (haystack.Contains("firetv", StringComparison.Ordinal) || haystack.Contains("fire tv", StringComparison.Ordinal))
        {
            return TvPlatform.FireTV;
        }
        if (haystack.Contains("google", StringComparison.Ordinal))
        {
            return TvPlatform.GoogleTV;
        }
        if (haystack.Contains("android", StringComparison.Ordinal))
        {
            return TvPlatform.AndroidTV;
        }

        return TvPlatform.Unknown;
    }

    private static string? DetectManufacturer(string server, string usn)
    {
        var haystack = $"{server} {usn}".ToLowerInvariant();
        if (haystack.Contains("samsung", StringComparison.Ordinal)) return "Samsung";
        if (haystack.Contains("lge", StringComparison.Ordinal) || haystack.Contains("lg ", StringComparison.Ordinal)) return "LG";
        if (haystack.Contains("roku", StringComparison.Ordinal)) return "Roku";
        if (haystack.Contains("hisense", StringComparison.Ordinal)) return "Hisense";
        if (haystack.Contains("sony", StringComparison.Ordinal)) return "Sony";
        return null;
    }
}
