using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace TcpEchoServer;

/// <summary>
/// A byte-preserving TCP echo server. Each read is echoed as an arbitrary
/// stream chunk; no message or text encoding boundaries are assumed.
/// </summary>
public sealed class EchoServer
{
    private readonly TcpListener _listener;

    public EchoServer(IPAddress address, int port)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (port is < IPEndPoint.MinPort or > IPEndPoint.MaxPort)
            throw new ArgumentOutOfRangeException(nameof(port));

        _listener = new TcpListener(address, port);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var clients = new ConcurrentDictionary<int, Task>();
        var clientId = 0;
        _listener.Start();

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

                var id = Interlocked.Increment(ref clientId);
                var task = HandleClientAsync(client, cancellationToken);
                clients[id] = task;
                _ = task.ContinueWith(
                    _ => clients.TryRemove(id, out _),
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
                // Active client reads are canceled as part of server shutdown.
            }
        }
    }

    private static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
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
            catch (IOException)
            {
                // A client disconnect or I/O failure is local to this session.
            }
            catch (SocketException)
            {
                // A client disconnect or socket failure is local to this session.
            }
        }
    }
}
