using System.Net;
using TcpEchoServer;
using Xunit;

namespace TcpEchoServer.Tests;

public sealed class ServerOptionsTests
{
    [Fact]
    public void UsesLoopbackDefaultsWhenNoConfigurationIsProvided()
    {
        var options = ServerOptions.Parse([], _ => null);

        Assert.Equal(IPAddress.Loopback, options.BindAddress);
        Assert.Equal(5000, options.Port);
        Assert.Equal(100, options.MaxConcurrentClients);
    }

    [Fact]
    public void ReadsConfigurationFromEnvironmentVariables()
    {
        var values = new Dictionary<string, string?>
        {
            [ServerOptions.HostEnvironmentVariable] = "127.0.0.2",
            [ServerOptions.PortEnvironmentVariable] = "7001",
            [ServerOptions.MaxClientsEnvironmentVariable] = "25"
        };

        var options = ServerOptions.Parse([], name => values.GetValueOrDefault(name));

        Assert.Equal(IPAddress.Parse("127.0.0.2"), options.BindAddress);
        Assert.Equal(7001, options.Port);
        Assert.Equal(25, options.MaxConcurrentClients);
    }

    [Fact]
    public void CommandLineArgumentsOverrideEnvironmentVariables()
    {
        var options = ServerOptions.Parse(
            ["127.0.0.3", "8002", "12"],
            name => name switch
            {
                ServerOptions.HostEnvironmentVariable => "127.0.0.2",
                ServerOptions.PortEnvironmentVariable => "7001",
                ServerOptions.MaxClientsEnvironmentVariable => "25",
                _ => null
            });

        Assert.Equal(IPAddress.Parse("127.0.0.3"), options.BindAddress);
        Assert.Equal(8002, options.Port);
        Assert.Equal(12, options.MaxConcurrentClients);
    }

    [Theory]
    [InlineData("not-an-ip", "5000", "100")]
    [InlineData("127.0.0.1", "not-a-port", "100")]
    [InlineData("127.0.0.1", "65536", "100")]
    [InlineData("127.0.0.1", "5000", "0")]
    [InlineData("127.0.0.1", "5000", "many")]
    public void RejectsInvalidConfiguration(string host, string port, string maxClients)
    {
        var values = new Dictionary<string, string?>
        {
            [ServerOptions.HostEnvironmentVariable] = host,
            [ServerOptions.PortEnvironmentVariable] = port,
            [ServerOptions.MaxClientsEnvironmentVariable] = maxClients
        };

        Assert.Throws<ArgumentException>(() =>
            ServerOptions.Parse([], name => values.GetValueOrDefault(name)));
    }

    [Fact]
    public void RejectsMoreThanThreeArguments()
    {
        Assert.Throws<ArgumentException>(() =>
            ServerOptions.Parse(["127.0.0.1", "5000", "100", "unexpected"], _ => null));
    }
}
