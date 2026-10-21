using System.Net.Http;

namespace Tervia.Platforms.Roku;

/// <summary>Roku External Control Protocol over the local network.</summary>
public sealed class RokuEcpClient : IDisposable
{
    private readonly HttpClient _http;

    public RokuEcpClient(string host, HttpClient? http = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        Host = host;
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        _http.BaseAddress = new Uri($"http://{host}:8060/");
    }

    public string Host { get; }

    public Task<string> GetDeviceInfoAsync(CancellationToken cancellationToken)
        => GetAsync("query/device-info", cancellationToken);

    public Task<string> GetApplicationsAsync(CancellationToken cancellationToken)
        => GetAsync("query/apps", cancellationToken);

    public Task KeyPressAsync(string key, CancellationToken cancellationToken)
        => PostAsync($"keypress/{key}", cancellationToken);

    public Task LaunchAsync(string applicationId, CancellationToken cancellationToken)
        => PostAsync($"launch/{applicationId}", cancellationToken);

    private async Task<string> GetAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(path, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task PostAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.PostAsync(path, content: null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose() => _http.Dispose();
}
