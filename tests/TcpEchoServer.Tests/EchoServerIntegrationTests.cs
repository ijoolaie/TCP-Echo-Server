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
}
