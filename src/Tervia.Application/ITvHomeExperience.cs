using Tervia.Core;

namespace Tervia.Application;

/// <summary>Optional platform feature for installing and customizing a TV-side home experience.</summary>
public interface ITvHomeExperience
{
    Task InstallAsync(TvSession session, string packagePath, CancellationToken cancellationToken);

    Task ApplyProfileAsync(TvSession session, string profileBase64, bool requestDefaultHomeRole, CancellationToken cancellationToken);

    Task OpenHomeSettingsAsync(TvSession session, CancellationToken cancellationToken);
}

public interface ITvHomeProfileStore
{
    Task<TvHomeProfile?> LoadAsync(string tvId, CancellationToken cancellationToken);

    Task SaveAsync(string tvId, TvHomeProfile profile, CancellationToken cancellationToken);
}
