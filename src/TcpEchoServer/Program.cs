using Microsoft.Extensions.Logging;
using TcpEchoServer;

ServerOptions options;
try
{
    options = ServerOptions.Parse(args);
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine("Usage: TcpEchoServer [bind-ip] [port] [max-concurrent-clients]");
    Console.Error.WriteLine("Environment: TCP_ECHO_HOST, TCP_ECHO_PORT, TCP_ECHO_MAX_CLIENTS");
    return 2;
}

using var loggerFactory = LoggerFactory.Create(builder => builder.AddSimpleConsole(logging =>
{
    logging.SingleLine = true;
    logging.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
}));
var logger = loggerFactory.CreateLogger<EchoServer>();

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

var server = new EchoServer(options.BindAddress, options.Port, options.MaxConcurrentClients, logger);
try
{
    await server.RunAsync(shutdown.Token);
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    // Expected during graceful shutdown.
}

return 0;
