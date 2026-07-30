using System.Net.Http.Json;
using System.Text.Json;

namespace Novolis.Agent.Session;

/// <summary>HTTP client for <see cref="SessionHttpHost"/> (agent / MCP sidecar).</summary>
public sealed class SessionHttpClient : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsClient;

    public SessionHttpClient(string baseUrl, HttpClient? httpClient = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        BaseUrl = baseUrl.TrimEnd('/');
        if (httpClient is null)
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl + "/") };
            _ownsClient = true;
        }
        else
        {
            _http = httpClient;
            _ownsClient = false;
        }
    }

    public string BaseUrl { get; }

    public static SessionHttpClient? TryFromEnvironmentOrMarker()
    {
        var env = Environment.GetEnvironmentVariable("NOVOLIS_GAME_SESSION_HTTP_URL");
        var url = !string.IsNullOrWhiteSpace(env) ? env.Trim() : SessionEndpoints.TryReadHttpBaseUrl();
        return string.IsNullOrWhiteSpace(url) ? null : new SessionHttpClient(url);
    }

    public Task<SessionHelloResponseDto> HelloAsync(CancellationToken cancellationToken = default) =>
        GetResultAsync<SessionHelloResponseDto>("session/hello", cancellationToken);

    public Task<SessionSnapshotDto> SnapshotAsync(CancellationToken cancellationToken = default) =>
        GetResultAsync<SessionSnapshotDto>("session/snapshot", cancellationToken);

    public Task<SessionActionsResponseDto> ActionsAsync(CancellationToken cancellationToken = default) =>
        GetResultAsync<SessionActionsResponseDto>("session/actions", cancellationToken);

    public Task<SessionCommandResultDto> ContinueAsync(CancellationToken cancellationToken = default) =>
        PostResultAsync<SessionCommandResultDto>("session/continue", null, cancellationToken);

    public Task<SessionSubscribeResponseDto> SubscribeAsync(CancellationToken cancellationToken = default) =>
        PostResultAsync<SessionSubscribeResponseDto>("session/subscribe", null, cancellationToken);

    public Task<SessionCommandResultDto> CommandAsync(
        SessionCommandDto command,
        CancellationToken cancellationToken = default) =>
        PostResultAsync<SessionCommandResultDto>("session/command", command, cancellationToken);

    private async Task<T> GetResultAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(path, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await UnwrapAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<T> PostResultAsync<T>(string path, object? body, CancellationToken cancellationToken)
    {
        using var response = body is null
            ? await _http.PostAsync(path, content: null, cancellationToken).ConfigureAwait(false)
            : await _http.PostAsJsonAsync(path, body, SessionJsonDispatcher.JsonOptions, cancellationToken)
                .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await UnwrapAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<T> UnwrapAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (doc.RootElement.TryGetProperty("ok", out var ok) && ok.ValueKind == JsonValueKind.False)
        {
            var err = doc.RootElement.TryGetProperty("error", out var e) ? e.GetString() : "request failed";
            throw new InvalidOperationException(err);
        }

        if (!doc.RootElement.TryGetProperty("result", out var result))
            throw new InvalidOperationException("Response missing result.");

        return result.Deserialize<T>(SessionJsonDispatcher.JsonOptions)
               ?? throw new InvalidOperationException("Failed to deserialize result.");
    }

    public ValueTask DisposeAsync()
    {
        if (_ownsClient)
            _http.Dispose();
        return ValueTask.CompletedTask;
    }
}
