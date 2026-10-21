using Tervia.Capabilities;
using Tervia.Core;
using Tervia.Infrastructure;
using Xunit;
using System;

namespace Tervia.Infrastructure.Tests;

public sealed class JsonTvDeviceLibraryTests
{
    [Fact]
    public async Task Library_round_trips_profile_and_capability_evidence()
    {
        var root = Path.Combine(Path.GetTempPath(), $"tervia-library-{Guid.NewGuid():N}");
        try
        {
            using var library = new JsonTvDeviceLibrary(new TerviaPaths(root));
            var device = new TvDevice
            {
                Id = "192.168.1.42",
                FriendlyName = "Living room",
                NetworkAddress = "192.168.1.42",
                Manufacturer = "Samsung",
                Model = "Model hint",
                Platform = TvPlatform.Tizen,
                LastSeen = new DateTimeOffset(2026, 10, 21, 9, 30, 0, TimeSpan.FromHours(2)),
                Room = "Living room",
                Status = TvDeviceStatus.Online,
                Capabilities = CapabilitySet.From(
                [
                    CapabilityInfo.Supported(CapabilityId.RemoteHome, RiskLevel.Safe, "probe:test"),
                    CapabilityInfo.Restricted(CapabilityId.AppsInstall, "Requires user pairing."),
                ]),
            };

            await library.SaveAsync(device, CancellationToken.None);

            var loaded = Assert.Single(await library.LoadAsync(CancellationToken.None));
            Assert.Equal(device.Id, loaded.Id);
            Assert.Equal(device.FriendlyName, loaded.FriendlyName);
            Assert.Equal(device.NetworkAddress, loaded.NetworkAddress);
            Assert.Equal(device.Manufacturer, loaded.Manufacturer);
            Assert.Equal(device.Model, loaded.Model);
            Assert.Equal(device.Platform, loaded.Platform);
            Assert.Equal(device.LastSeen, loaded.LastSeen);
            Assert.Equal(device.Room, loaded.Room);
            Assert.Equal(device.Status, loaded.Status);
            Assert.Equal("probe:test", loaded.Capabilities.Find(CapabilityId.RemoteHome)?.Source);
            Assert.Equal(CapabilityStatus.Restricted, loaded.Capabilities.Find(CapabilityId.AppsInstall)?.Status);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Saving_same_identity_updates_instead_of_duplicating()
    {
        var root = Path.Combine(Path.GetTempPath(), $"tervia-library-{Guid.NewGuid():N}");
        try
        {
            using var library = new JsonTvDeviceLibrary(new TerviaPaths(root));
            await library.SaveAsync(CreateDevice("192.168.1.42", "Old name"), CancellationToken.None);
            await library.SaveAsync(CreateDevice("192.168.1.42", "New name"), CancellationToken.None);

            var loaded = Assert.Single(await library.LoadAsync(CancellationToken.None));
            Assert.Equal("New name", loaded.FriendlyName);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Removing_device_keeps_other_profiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"tervia-library-{Guid.NewGuid():N}");
        try
        {
            using var library = new JsonTvDeviceLibrary(new TerviaPaths(root));
            await library.SaveAsync(CreateDevice("192.168.1.42", "Living room"), CancellationToken.None);
            await library.SaveAsync(CreateDevice("192.168.1.43", "Bedroom"), CancellationToken.None);

            await library.RemoveAsync("192.168.1.42", CancellationToken.None);

            var remaining = Assert.Single(await library.LoadAsync(CancellationToken.None));
            Assert.Equal("192.168.1.43", remaining.Id);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static TvDevice CreateDevice(string id, string name)
        => new()
        {
            Id = id,
            FriendlyName = name,
            NetworkAddress = id,
        };
}
