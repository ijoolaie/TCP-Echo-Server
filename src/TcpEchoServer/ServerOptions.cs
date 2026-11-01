using System.Net;

namespace TcpEchoServer;

/// <summary>
/// Validated server configuration. Precedence: command-line arguments, environment variables, defaults.
/// </summary>
public sealed record ServerOptions(IPAddress BindAddress, int Port, int MaxConcurrentClients)
{
    public const string HostEnvironmentVariable = "TCP_ECHO_HOST";
    public const string PortEnvironmentVariable = "TCP_ECHO_PORT";
    public const string MaxClientsEnvironmentVariable = "TCP_ECHO_MAX_CLIENTS";

    public static ServerOptions Parse(string[] args, Func<string, string?>? getEnvironmentVariable = null)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length > 3)
            throw new ArgumentException("Too many arguments. Usage: [bind-ip] [port] [max-concurrent-clients].");

        getEnvironmentVariable ??= Environment.GetEnvironmentVariable;

        var host = GetValue(args, 0, HostEnvironmentVariable, "127.0.0.1", getEnvironmentVariable);
        var portText = GetValue(args, 1, PortEnvironmentVariable, "5000", getEnvironmentVariable);
        var maxClientsText = GetValue(args, 2, MaxClientsEnvironmentVariable, "100", getEnvironmentVariable);

        if (!IPAddress.TryParse(host, out var address))
            throw new ArgumentException($"Invalid IP address: {host}");

        if (!int.TryParse(portText, out var port) || port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            throw new ArgumentException($"Invalid TCP port: {portText}");

        if (!int.TryParse(maxClientsText, out var maxClients) || maxClients < 1)
            throw new ArgumentException($"Maximum concurrent clients must be a positive integer: {maxClientsText}");

        return new ServerOptions(address, port, maxClients);
    }

    private static string GetValue(
        string[] args,
        int index,
        string environmentVariable,
        string defaultValue,
        Func<string, string?> getEnvironmentVariable)
    {
        if (args.Length > index)
            return args[index];

        return getEnvironmentVariable(environmentVariable) ?? defaultValue;
    }
}
