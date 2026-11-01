using System.Net;
using TcpEchoServer;

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 && int.TryParse(args[1], out var parsedPort) ? parsedPort : 5000;

if (!IPAddress.TryParse(host, out var address))
{
    Console.Error.WriteLine($"Invalid IP address: {host}");
    return 2;
}

if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
{
    Console.Error.WriteLine($"Invalid TCP port: {port}");
    return 2;
}

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

var server = new EchoServer(address, port);
Console.WriteLine($"TCP echo server listening on {address}:{port}");
Console.WriteLine("Press Ctrl+C to stop.");

try
{
    await server.RunAsync(shutdown.Token);
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    // Expected during graceful shutdown.
}

Console.WriteLine("TCP echo server stopped.");
return 0;
