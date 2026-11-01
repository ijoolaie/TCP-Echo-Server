using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace TcpEchoServer;

/// <summary>
/// A byte-preserving TCP echo server. Reads are arbitrary stream chunks, not messages.
/// </summary>
public sealed class EchoServer
{
    private readonly TcpListener _listener;
    private readonly SemaphoreSlim _connectionSlots;
    private readonly ILogger<EchoServer> _logger;
    private readonly int _maxConcurrentClients;

    public EchoServer(
        IPAddress address,
        int port,
        int maxConcurrentClients = 100,
        ILogger<EchoServer>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            throw new ArgumentOutOfRangeException(nameof(port));
        if (maxConcurrentClients < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrentClients));

        _listener = new TcpListener(address, port);
        _maxConcurrentClients = maxConcurrentClients;
        _connectionSlots = new SemaphoreSlim(maxConcurrentClients, maxConcurrentClients);
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EchoServer>.Instance;
    }

    public IPEndPoint? LocalEndpoint => _listener.LocalEndpoint as IPEndPoint;

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var clients = new ConcurrentDictionary<int, Task>();
        var clientId = 0;
        _listener.Start();
        _logger.LogInformation(
            "TCP echo server started on {Endpoint}; maximum concurrent clients: {MaxClients}",
            LocalEndpoint, _maxConcurrentClients);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (SocketException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await _connectionSlots.WaitAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    client.Dispose();
                    break;
                }

                var id = Interlocked.Increment(ref clientId);
                var task = HandleClientAsync(client, id, cancellationToken);
                clients[id] = task;
                _ = task.ContinueWith(
                    completed =>
                    {
                        clients.TryRemove(id, out var _);
                        _connectionSlots.Release();
                        _ = completed.Exception;
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }
        finally
        {
            _listener.Stop();
            try
            {
                await Task.WhenAll(clients.Values);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Active client reads are canceled as part of graceful shutdown.
            }

            _logger.LogInformation("TCP echo server stopped");
        }
    }

    private async Task HandleClientAsync(TcpClient client, int clientId, CancellationToken cancellationToken)
    {
        using (client)
        {
            var remoteEndpoint = client.Client.RemoteEndPoint;
            _logger.LogInformation("Client {ClientId} connected from {RemoteEndpoint}", clientId, remoteEndpoint);
            var stream = client.GetStream();
            var buffer = new byte[81920];

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                    if (bytesRead == 0)
                        break;

                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("Client {ClientId} operation canceled during server shutdown", clientId);
                throw;
            }
            catch (IOException exception)
            {
                _logger.LogDebug(exception, "I/O ended for client {ClientId}", clientId);
            }
            catch (SocketException exception)
            {
                _logger.LogDebug(exception, "Socket ended for client {ClientId}", clientId);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected failure in client session {ClientId}", clientId);
            }
            finally
            {
                _logger.LogInformation("Client {ClientId} disconnected", clientId);
            }
        }
    }
}
