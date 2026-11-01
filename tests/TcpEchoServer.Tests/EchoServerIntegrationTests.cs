using System.Net;
using System.Net.Sockets;
using TcpEchoServer;

namespace TcpEchoServer.Tests;

public sealed class EchoServerIntegrationTests
{
    [Fact]
    public async Task EchoesBinaryPayloadWithoutChangingBytes()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Address, endpoint.Port);
            var stream = client.GetStream();
            byte[] payload = [0x00, 0xFF, 0x10, 0x0A, 0x80, 0x7F, 0x00];

            await stream.WriteAsync(payload);
            var echoed = new byte[payload.Length];
            await stream.ReadExactlyAsync(echoed);

            Assert.Equal(payload, echoed);
        }
        finally
        {
            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task ServesTwoClientsConcurrently()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        try
        {
            using var first = new TcpClient();
            using var second = new TcpClient();
            await Task.WhenAll(
                first.ConnectAsync(endpoint.Address, endpoint.Port),
                second.ConnectAsync(endpoint.Address, endpoint.Port));

            var firstPayload = System.Text.Encoding.UTF8.GetBytes("first-client");
            var secondPayload = System.Text.Encoding.UTF8.GetBytes("second-client");
            var firstStream = first.GetStream();
            var secondStream = second.GetStream();

            await Task.WhenAll(
                firstStream.WriteAsync(firstPayload).AsTask(),
                secondStream.WriteAsync(secondPayload).AsTask());

            var firstEcho = new byte[firstPayload.Length];
            var secondEcho = new byte[secondPayload.Length];
            await Task.WhenAll(
                firstStream.ReadExactlyAsync(firstEcho).AsTask(),
                secondStream.ReadExactlyAsync(secondEcho).AsTask());

            Assert.Equal(firstPayload, firstEcho);
            Assert.Equal(secondPayload, secondEcho);
        }
        finally
        {
            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task EchoesLargePayloadWrittenInFragments()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(endpoint.Address, endpoint.Port);
            var stream = client.GetStream();
            var payload = Enumerable.Range(0, 1024 * 1024)
                .Select(index => (byte)(index % 251))
                .ToArray();

            for (var offset = 0; offset < payload.Length; offset += 4093)
            {
                var count = Math.Min(4093, payload.Length - offset);
                await stream.WriteAsync(payload.AsMemory(offset, count));
            }

            var echoed = new byte[payload.Length];
            await stream.ReadExactlyAsync(echoed);
            Assert.Equal(payload, echoed);
        }
        finally
        {
            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task EnforcesMaximumConcurrentClientCount()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0, maxConcurrentClients: 1);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        try
        {
            using var first = new TcpClient();
            using var second = new TcpClient();
            await first.ConnectAsync(endpoint.Address, endpoint.Port);
            await Task.Delay(100);
            await second.ConnectAsync(endpoint.Address, endpoint.Port);

            var secondStream = second.GetStream();
            var payload = System.Text.Encoding.UTF8.GetBytes("wait-for-slot");
            await secondStream.WriteAsync(payload);
            var echoed = new byte[payload.Length];
            var readTask = secondStream.ReadExactlyAsync(echoed).AsTask();

            await Task.Delay(200);
            Assert.False(readTask.IsCompleted);

            first.Dispose();
            await readTask.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(payload, echoed);
        }
        finally
        {
            await shutdown.CancelAsync();
            await serverTask.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    [Fact]
    public async Task ClientDisconnectDoesNotStopServerAndShutdownCompletes()
    {
        using var shutdown = new CancellationTokenSource();
        var server = new EchoServer(IPAddress.Loopback, 0);
        var serverTask = server.RunAsync(shutdown.Token);
        var endpoint = Assert.IsType<IPEndPoint>(server.LocalEndpoint);

        using (var disconnectedClient = new TcpClient())
        {
            await disconnectedClient.ConnectAsync(endpoint.Address, endpoint.Port);
        }

        try
        {
            using var healthyClient = new TcpClient();
            await healthyClient.ConnectAsync(endpoint.Address, endpoint.Port);
            var stream = healthyClient.GetStream();
            byte[] payload = [9, 8, 7, 6, 5];
            await stream.WriteAsync(payload);
            var echoed = new byte[payload.Length];
            await stream.ReadExactlyAsync(echoed);
            Assert.Equal(payload, echoed);

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
