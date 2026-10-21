using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tervia.Capabilities;
using Tervia.Core;
using Tervia.Safety;

namespace Tervia.Application;

/// <summary>Coordinates the optional Android TV home app without exposing ADB to the UI.</summary>
public sealed class TvHomeService(
    TvSessionService sessions,
    TvOperationScheduler operations,
    ITvHomeProfileStore profiles)
{
    private static readonly JsonSerializerOptions ProfileJsonOptions = CreateProfileJsonOptions();

    public async Task<TvHomeProfile> LoadProfileAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        var existing = await profiles.LoadAsync(device.Id, cancellationToken).ConfigureAwait(false);
        return (existing ?? new TvHomeProfile()).ForDevice(device);
    }

    public async Task<IReadOnlyList<TvApplication>> ListApplicationsAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        return await WithHomeAdapterAsync(device, async (session, home, token) =>
        {
            if (session.Adapter is not ITvApplicationManager apps)
            {
                throw new UnsupportedCapabilityException("apps.list");
            }
            return await apps.ListApplicationsAsync(session, token).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task InstallAndApplyAsync(
        TvDevice device,
        TvHomeProfile profile,
        string packagePath,
        bool confirmedByUser,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        if (!confirmedByUser)
        {
            throw new OperationRejectedException("Instalar Tervia Home requiere confirmación explícita.");
        }

        var decision = sessions.Authorize(new SafetyRequest
        {
            OperationId = "home.install",
            Risk = RiskLevel.High,
            TvId = device.Id,
            Platform = device.Platform,
            Detail = "Instalar o actualizar únicamente el paquete de Tervia Home.",
        });
        if (decision.Outcome == SafetyOutcome.Denied)
        {
            throw new OperationRejectedException(decision.Reason);
        }

        if (!File.Exists(packagePath))
        {
            throw new FileNotFoundException("No se encontró el instalador de Tervia Home junto a Tervia.", packagePath);
        }

        var normalized = profile.ForDevice(device);
        await operations.ExecuteAsync(device.Id, async token =>
        {
            var session = await sessions.OpenAsync(device, token).ConfigureAwait(false);
            try
            {
                var home = RequireHomeAdapter(session);
                await home.InstallAsync(session, packagePath, token).ConfigureAwait(false);
                await home.ApplyProfileAsync(session, EncodeProfile(normalized), requestDefaultHomeRole: true, token)
                    .ConfigureAwait(false);
                await profiles.SaveAsync(device.Id, normalized, token).ConfigureAwait(false);
                return true;
            }
            finally
            {
                await DisconnectBestEffortAsync(session).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task ApplyAsync(TvDevice device, TvHomeProfile profile, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(profile);
        var normalized = profile.ForDevice(device);
        await WithHomeAdapterAsync(device, async (session, home, token) =>
        {
            await home.ApplyProfileAsync(session, EncodeProfile(normalized), requestDefaultHomeRole: false, token)
                .ConfigureAwait(false);
            await profiles.SaveAsync(device.Id, normalized, token).ConfigureAwait(false);
            return true;
        }, cancellationToken).ConfigureAwait(false);
    }

    public async Task OpenHomeSettingsAsync(TvDevice device, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(device);
        await WithHomeAdapterAsync(device, async (session, home, token) =>
        {
            await home.OpenHomeSettingsAsync(session, token).ConfigureAwait(false);
            return true;
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<T> WithHomeAdapterAsync<T>(
        TvDevice device,
        Func<TvSession, ITvHomeExperience, CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
        => await operations.ExecuteAsync(device.Id, async token =>
        {
            var session = await sessions.OpenAsync(device, token).ConfigureAwait(false);
            try
            {
                return await operation(session, RequireHomeAdapter(session), token).ConfigureAwait(false);
            }
            finally
            {
                await DisconnectBestEffortAsync(session).ConfigureAwait(false);
            }
        }, cancellationToken).ConfigureAwait(false);

    private static ITvHomeExperience RequireHomeAdapter(TvSession session)
        => session.Adapter is ITvHomeExperience home
            ? home
            : throw new UnsupportedCapabilityException("home.personalization");

    private static async Task DisconnectBestEffortAsync(TvSession session)
    {
        try
        {
            await session.Adapter.DisconnectAsync(session, CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Cleanup failures do not replace the actionable result.
        }
    }

    private static string EncodeProfile(TvHomeProfile profile)
    {
        var json = JsonSerializer.Serialize(profile, ProfileJsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        if (bytes.Length > 8_192)
        {
            throw new OperationRejectedException("El perfil de inicio supera el tamaño máximo permitido.");
        }
        return Convert.ToBase64String(bytes);
    }

    private static JsonSerializerOptions CreateProfileJsonOptions()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
