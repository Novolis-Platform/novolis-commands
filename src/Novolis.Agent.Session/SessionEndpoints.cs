using Novolis.Transports.LocalIpc;

namespace Novolis.Agent.Session;

public static class SessionEndpoints
{
    public const string DefaultPipeName = "novolis-game-session";
    public const string EnableEnvVar = "NOVOLIS_GAME_SESSION";
    public const string EndpointEnvVar = "NOVOLIS_GAME_SESSION_ENDPOINT";
    public const string HttpEnableEnvVar = "NOVOLIS_GAME_SESSION_HTTP";
    public const string HttpPortEnvVar = "NOVOLIS_GAME_SESSION_HTTP_PORT";
    public const string TcpEnableEnvVar = "NOVOLIS_GAME_SESSION_TCP";
    public const string TcpPortEnvVar = "NOVOLIS_GAME_SESSION_TCP_PORT";
    public const string HostMarkerFileName = "novolis-game-session.host";
    public const string HttpMarkerFileName = "novolis-game-session.http";
    public const string TcpMarkerFileName = "novolis-game-session.tcp";
    public const int DefaultHttpPort = 18765;
    public const int DefaultTcpPort = 18766;

    public static bool IsEnabledByEnvironment() => EnvTruthy(EnableEnvVar);

    /// <summary>
    /// HTTP is on when session agent is on, unless <c>NOVOLIS_GAME_SESSION_HTTP=0</c>.
    /// Explicit <c>HTTP=1</c> also enables HTTP without LocalIpc.
    /// </summary>
    public static bool IsHttpEnabledByEnvironment()
    {
        var http = Environment.GetEnvironmentVariable(HttpEnableEnvVar);
        if (EnvFalsy(http))
            return false;
        if (EnvTruthyValue(http))
            return true;
        return IsEnabledByEnvironment();
    }

    public static bool IsTcpEnabledByEnvironment() => EnvTruthy(TcpEnableEnvVar);

    public static int ResolveHttpPort()
    {
        var raw = Environment.GetEnvironmentVariable(HttpPortEnvVar);
        return int.TryParse(raw, out var port) && port is > 0 and < 65536 ? port : DefaultHttpPort;
    }

    public static int ResolveTcpPort()
    {
        var raw = Environment.GetEnvironmentVariable(TcpPortEnvVar);
        return int.TryParse(raw, out var port) && port is > 0 and < 65536 ? port : DefaultTcpPort;
    }

    public static string HostMarkerPath => Path.Combine(Path.GetTempPath(), HostMarkerFileName);

    public static string HttpMarkerPath => Path.Combine(Path.GetTempPath(), HttpMarkerFileName);

    public static string TcpMarkerPath => Path.Combine(Path.GetTempPath(), TcpMarkerFileName);

    public static string? TryReadHttpBaseUrl()
    {
        try
        {
            if (!File.Exists(HttpMarkerPath))
                return null;
            var lines = File.ReadAllLines(HttpMarkerPath);
            return lines.ElementAtOrDefault(1)?.Trim();
        }
        catch
        {
            return null;
        }
    }

    public static LocalIpcEndpoint CreateDefault(string? preferredAddress = null)
    {
        var overrideAddress = Environment.GetEnvironmentVariable(EndpointEnvVar);
        var address = !string.IsNullOrWhiteSpace(overrideAddress)
            ? overrideAddress
            : preferredAddress ?? DefaultPipeName;

        if (OperatingSystem.IsWindows())
            return new LocalIpcEndpoint(address, LocalIpcTransportKind.NamedPipe);

        var socketPath = Path.IsPathRooted(address)
            ? address
            : Path.Combine(Path.GetTempPath(), address + ".sock");
        return new LocalIpcEndpoint(socketPath, LocalIpcTransportKind.UnixDomainSocket);
    }

    private static bool EnvTruthy(string name) => EnvTruthyValue(Environment.GetEnvironmentVariable(name));

    private static bool EnvTruthyValue(string? value) =>
        string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);

    private static bool EnvFalsy(string? value) =>
        string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase)
        || string.Equals(value, "off", StringComparison.OrdinalIgnoreCase);
}
