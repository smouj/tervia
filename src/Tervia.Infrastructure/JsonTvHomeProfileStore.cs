using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tervia.Application;
using Tervia.Core;

namespace Tervia.Infrastructure;

/// <summary>Stores one validated, versioned home profile per TV on this PC.</summary>
public sealed class JsonTvHomeProfileStore : ITvHomeProfileStore, IDisposable
{
    private const int SchemaVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    private readonly string _directory;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonTvHomeProfileStore(TerviaPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        _directory = Path.Combine(paths.Profiles, "home");
    }

    public void Dispose() => _gate.Dispose();

    public async Task<TvHomeProfile?> LoadAsync(string tvId, CancellationToken cancellationToken)
    {
        var path = PathFor(tvId);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 8 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
            var document = await JsonSerializer.DeserializeAsync<ProfileDocument>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
            if (document is null || document.SchemaVersion != SchemaVersion || document.Profile is null)
            {
                throw new InvalidDataException("El perfil local de inicio es inválido o usa otra versión.");
            }

            return document.Profile.Normalize();
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveAsync(string tvId, TvHomeProfile profile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var path = PathFor(tvId);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(_directory);
            var temporary = Path.Combine(_directory, $"{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                    bufferSize: 8 * 1024, FileOptions.Asynchronous | FileOptions.WriteThrough))
                {
                    await JsonSerializer.SerializeAsync(stream,
                        new ProfileDocument { SchemaVersion = SchemaVersion, Profile = profile.Normalize() },
                        SerializerOptions, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }

                File.Move(temporary, path, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporary))
                {
                    File.Delete(temporary);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private string PathFor(string tvId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tvId);
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tvId))).ToLowerInvariant();
        return Path.Combine(_directory, $"{digest}.json");
    }

    private sealed class ProfileDocument
    {
        public int SchemaVersion { get; init; }
        public TvHomeProfile? Profile { get; init; }
    }
}
