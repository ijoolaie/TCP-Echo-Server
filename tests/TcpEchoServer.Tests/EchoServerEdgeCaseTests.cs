using System.Net;
using System.Net.Sockets;
using TcpEchoServer;
using Xunit;

namespace TcpEchoServer.Tests;

public sealed class EchoServerEdgeCaseTests
{
    [Fact]
    public async Task EchoesCoalescedWritesAsOneUnframedByteStream()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0, maxConcurrentClients: 1);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Address, endpoint.Port);
            var stream = client.GetStream();

            byte[] first = [0x01, 0x02, 0x03];
            byte[] second = [0x00, 0xFE];
            byte[] third = [0x7F, 0x80, 0x81, 0x82];

            // Several writes are sent before reading. TCP does not preserve write boundaries.
            await stream.WriteAsync(first);
            await stream.WriteAsync(second);
            await stream.WriteAsync(third);

            var expected = first.Concat(second).Concat(third).ToArray();
            var echoed = new byte[expected.Length];
            await stream.ReadExactlyAsync(echoed).AsTask().WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal(expected, echoed);
        }
        finally
        {
            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task EmptyClientStreamClosesCleanlyAndServerContinuesServing()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0, maxConcurrentClients: 1);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        try
        {
            using (var emptyClient = new TcpClient())
            {
                await emptyClient.ConnectAsync(endpoint.Address, endpoint.Port);
                emptyClient.Client.Shutdown(SocketShutdown.Send);

                var stream = emptyClient.GetStream();
                var buffer = new byte[1];
                var bytesRead = await stream.ReadAsync(buffer).AsTask().WaitAsync(TimeSpan.FromSeconds(5));

                Assert.Equal(0, bytesRead);
            }

            using var healthyClient = new TcpClient();
            await healthyClient.ConnectAsync(endpoint.Address, endpoint.Port);
            var healthyStream = healthyClient.GetStream();
            byte[] payload = [0xAA, 0x00, 0x55];
            await healthyStream.WriteAsync(payload);

            var echoed = new byte[payload.Length];
            await healthyStream.ReadExactlyAsync(echoed).AsTask().WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal(payload, echoed);
        }
        finally
        {
            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task ShutdownCompletesWhenClientCapacityIsSaturated()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0, maxConcurrentClients: 1);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        try
        {
            using var activeClient = new TcpClient();
            using var waitingClient = new TcpClient();

            await activeClient.ConnectAsync(endpoint.Address, endpoint.Port);
            var activeStream = activeClient.GetStream();
            byte[] payload = [0xA5];
            await activeStream.WriteAsync(payload);
            var echoed = new byte[payload.Length];
            await activeStream.ReadExactlyAsync(echoed).AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(payload, echoed);

            // The first session remains open, so the second connection cannot obtain a slot.
            await waitingClient.ConnectAsync(endpoint.Address, endpoint.Port);

            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(serverTask.IsCompleted);
        }
        finally
        {
            if (!shutdown.IsCancellationRequested)
                await shutdown.CancelAsync();

            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }
}
