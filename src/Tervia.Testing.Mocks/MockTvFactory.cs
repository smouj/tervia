using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Testing.Mocks;

public static class MockTvFactory
{
    public static MockTv CreateSonyGoogleTv() => new(
        "Sony BRAVIA (mock)",
        TvPlatform.GoogleTV,
        CapabilitySet.From(
        [
            CapabilityInfo.Supported(CapabilityId.RemoteNavigation, RiskLevel.Safe, "mock"),
            CapabilityInfo.Supported(CapabilityId.AppsList, RiskLevel.Safe, "mock"),
            CapabilityInfo.Supported(CapabilityId.AppsLaunch, RiskLevel.Low, "mock"),
            CapabilityInfo.Supported(CapabilityId.DiagnosticsSystemInfo, RiskLevel.Safe, "mock"),
            CapabilityInfo.Supported(CapabilityId.DeveloperScreenshot, RiskLevel.Safe, "mock"),
            CapabilityInfo.Supported(CapabilityId.MaintenanceListPackages, RiskLevel.Low, "mock"),
        ]),
        [new TvApplication { Id = "com.netflix.ninja", Name = "Netflix" }]);

    public static MockTv CreateSamsungTizenTv() => new(
        "Samsung QN90 (mock)",
        TvPlatform.Tizen,
        CapabilitySet.From(
        [
            CapabilityInfo.Restricted(CapabilityId.RemoteNavigation, "pairing required", RiskLevel.Low),
            CapabilityInfo.Restricted(CapabilityId.AppsList, "developer mode required", RiskLevel.Medium),
        ]));

    public static MockTv CreateLgWebOsTv() => new(
        "LG OLED (mock)",
        TvPlatform.WebOS,
        CapabilitySet.From(
        [
            CapabilityInfo.Restricted(CapabilityId.RemoteNavigation, "pairing required", RiskLevel.Low),
        ]));

    public static MockTv CreateRokuTv() => new(
        "Roku TV (mock)",
        TvPlatform.RokuTV,
        CapabilitySet.From(
        [
            CapabilityInfo.Supported(CapabilityId.RemoteNavigation, RiskLevel.Safe, "mock"),
            CapabilityInfo.Supported(CapabilityId.AppsList, RiskLevel.Safe, "mock"),
            CapabilityInfo.Supported(CapabilityId.DiagnosticsSystemInfo, RiskLevel.Safe, "mock"),
        ]),
        [new TvApplication { Id = "12", Name = "Netflix" }]);

    public static MockTv CreateVidaaTv() => new(
        "Hisense VIDAA (mock)",
        TvPlatform.Vidaa,
        CapabilitySet.From(
        [
            CapabilityInfo.Unknown(CapabilityId.RemoteNavigation, "unverified"),
        ]));

    public static MockTv CreateUnknownTv() => new(
        "Unknown Smart TV (mock)",
        TvPlatform.Unknown,
        CapabilitySet.Empty);
}
