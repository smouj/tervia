using System.Text.Json;
using System.Text.Json.Serialization;
using Tervia.Application;
using Tervia.Capabilities;
using Tervia.Core;

namespace Tervia.Infrastructure;

/// <summary>
/// Versioned, local-only TV profile storage. Writes use a same-directory temporary
/// file followed by an atomic replacement so an interrupted save does not truncate
/// the last valid library.
/// </summary>
public sealed class JsonTvDeviceLibrary : ITvDeviceLibrary, IDisposable
{
    private const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonTvDeviceLibrary(TerviaPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        _filePath = Path.Combine(paths.Tvs, "library.json");
    }

    public void Dispose() => _gate.Dispose();

    public async Task<IReadOnlyList<TvDevice>> LoadAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await ReadDocumentAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        ValidateDevice(device);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var devices = (await ReadDocumentAsync(cancellationToken).ConfigureAwait(false)).ToList();
            var existingIndex = devices.FindIndex(item =>
                string.Equals(item.Id, device.Id, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                devices[existingIndex] = device;
            }
            else
            {
                devices.Add(device);
            }

            await WriteDocumentAsync(devices, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task RemoveAsync(string deviceId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var devices = (await ReadDocumentAsync(cancellationToken).ConfigureAwait(false))
                .Where(item => !string.Equals(item.Id, deviceId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            await WriteDocumentAsync(devices, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IReadOnlyList<TvDevice>> ReadDocumentAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 16 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var document = await JsonSerializer.DeserializeAsync<LibraryDocument>(
            stream,
            SerializerOptions,
            cancellationToken).ConfigureAwait(false);

        if (document is null || document.SchemaVersion != CurrentSchemaVersion || document.Devices is null)
        {
            throw new InvalidDataException("The local TV library is invalid or uses an unsupported schema version.");
        }

        return document.Devices.Select(ToDomain).ToList();
    }

    private async Task WriteDocumentAsync(IReadOnlyList<TvDevice> devices, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_filePath)
            ?? throw new InvalidOperationException("The TV library path has no parent directory.");
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(directory, $"library.{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16 * 1024,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                var document = new LibraryDocument
                {
                    SchemaVersion = CurrentSchemaVersion,
                    Devices = devices.Select(ToDocument).ToList(),
                };
                await JsonSerializer.SerializeAsync(stream, document, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static TvDevice ToDomain(DeviceDocument stored)
    {
        var capabilities = (stored.Capabilities ?? [])
            .Select(item => new CapabilityInfo
            {
                Id = new CapabilityId(item.Id),
                Status = item.Status,
                Reason = item.Reason,
                Requirements = item.Requirements ?? [],
                Risk = item.Risk,
                Source = item.Source,
                Confidence = item.Confidence,
            });

        return new TvDevice
        {
            Id = stored.Id,
            FriendlyName = stored.FriendlyName,
            NetworkAddress = stored.NetworkAddress,
            Manufacturer = stored.Manufacturer,
            Model = stored.Model,
            Platform = stored.Platform,
            PlatformVersion = stored.PlatformVersion,
            FirmwareVersion = stored.FirmwareVersion,
            SerialNumber = stored.SerialNumber,
            ScreenSizeInches = stored.ScreenSizeInches,
            Resolution = stored.Resolution,
            ConnectionMethod = stored.ConnectionMethod,
            LastSeen = stored.LastSeen,
            Room = stored.Room,
            Status = stored.Status,
            Capabilities = CapabilitySet.From(capabilities),
        };
    }

    private static DeviceDocument ToDocument(TvDevice device)
        => new()
        {
            Id = device.Id,
            FriendlyName = device.FriendlyName,
            NetworkAddress = device.NetworkAddress,
            Manufacturer = device.Manufacturer,
            Model = device.Model,
            Platform = device.Platform,
            PlatformVersion = device.PlatformVersion,
            FirmwareVersion = device.FirmwareVersion,
            SerialNumber = device.SerialNumber,
            ScreenSizeInches = device.ScreenSizeInches,
            Resolution = device.Resolution,
            ConnectionMethod = device.ConnectionMethod,
            LastSeen = device.LastSeen,
            Room = device.Room,
            Status = device.Status,
            Capabilities = device.Capabilities.All.Select(item => new CapabilityDocument
            {
                Id = item.Id.Value,
                Status = item.Status,
                Reason = item.Reason,
                Requirements = item.Requirements.ToArray(),
                Risk = item.Risk,
                Source = item.Source,
                Confidence = item.Confidence,
            }).ToList(),
        };

    private static void ValidateDevice(TvDevice device)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(device.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(device.FriendlyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(device.NetworkAddress);
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private sealed class LibraryDocument
    {
        public LibraryDocument() { }

        public int SchemaVersion { get; init; }
        public List<DeviceDocument>? Devices { get; init; } = [];
    }

    private sealed class DeviceDocument
    {
        public DeviceDocument() { }

        public string Id { get; init; } = string.Empty;
        public string FriendlyName { get; init; } = string.Empty;
        public string NetworkAddress { get; init; } = string.Empty;
        public string? Manufacturer { get; init; }
        public string? Model { get; init; }
        public TvPlatform Platform { get; init; }
        public string? PlatformVersion { get; init; }
        public string? FirmwareVersion { get; init; }
        public string? SerialNumber { get; init; }
        public int? ScreenSizeInches { get; init; }
        public string? Resolution { get; init; }
        public TvConnectionMethod ConnectionMethod { get; init; }
        public DateTimeOffset? LastSeen { get; init; }
        public string? Room { get; init; }
        public TvDeviceStatus Status { get; init; }
        public List<CapabilityDocument>? Capabilities { get; init; } = [];
    }

    private sealed class CapabilityDocument
    {
        public CapabilityDocument() { }

        public string Id { get; init; } = string.Empty;
        public CapabilityStatus Status { get; init; }
        public string? Reason { get; init; }
        public string[]? Requirements { get; init; } = [];
        public RiskLevel Risk { get; init; }
        public string? Source { get; init; }
        public CapabilityConfidence Confidence { get; init; }
    }
}
