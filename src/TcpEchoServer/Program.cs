using System.Net;
using Microsoft.Extensions.Logging;
using TcpEchoServer;

var host = args.Length > 0 ? args[0] : "127.0.0.1";
var port = args.Length > 1 && int.TryParse(args[1], out var parsedPort) ? parsedPort : 5000;
var maxClients = args.Length > 2 && int.TryParse(args[2], out var parsedMaxClients) ? parsedMaxClients : 100;

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

if (maxClients < 1)
{
    Console.Error.WriteLine("Maximum concurrent clients must be at least 1.");
    return 2;
}

using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
}));
var logger = loggerFactory.CreateLogger<EchoServer>();

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

var server = new EchoServer(address, port, maxClients, logger);
try
{
    await server.RunAsync(shutdown.Token);
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    // Expected during graceful shutdown.
}

return 0;
