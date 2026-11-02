# TCP Echo Server v1.0.0

**Release status:** Release candidate notes prepared. The GitHub release and `v1.0.0` tag have not yet been published.

## Overview

The first release of TCP Echo Server provides a small, asynchronous TCP echo server built with C# and .NET 10. It supports concurrent clients and echoes each client's received bytes back unchanged.

## Included

- Asynchronous TCP accept, read, and write operations.
- Multiple simultaneous client sessions with a configurable maximum (default: 100).
- Byte-preserving echo for binary and arbitrary stream chunks; no UTF-8 decoding, newline framing, or message-boundary assumptions.
- Client disconnect and I/O failure isolation.
- Cancellation-aware graceful shutdown, including listener stop and tracked client-task cleanup.
- Structured lifecycle logging without logging client payloads.
- Validated configuration using command-line arguments and environment variables.
- Safe defaults: bind address `127.0.0.1`, port `5000`, maximum clients `100`.
- Unit and loopback integration tests covering binary data, concurrent clients, large fragmented payloads, connection limits, disconnects, shutdown, stream edge cases, and configuration.
- GitHub Actions CI for .NET 10 on Ubuntu, Windows, and macOS.

## Configuration

Command-line arguments are positional: bind IP, port, maximum concurrent clients.

```bash
dotnet run --project src/TcpEchoServer -- 127.0.0.1 5000 100
```

Environment variables: `TCP_ECHO_HOST`, `TCP_ECHO_PORT`, and `TCP_ECHO_MAX_CLIENTS`. A supplied command-line argument takes precedence over its corresponding environment variable, which takes precedence over the default.

## Verification

The latest pre-release CI run (#52) passed on Ubuntu, macOS, and Windows. Each platform completed restore, build, and all 17 tests successfully (51 successful test executions total), with zero build warnings and zero errors.

- [GitHub Actions run #52](https://github.com/ijoolaie/TCP-Echo-Server/actions/runs/41387945426)
- Verified commit: `9544d7bf30d6d92ff043e62ac853262c50e9e93b`

## Not included

TLS, authentication, application-level message framing, metrics exporters, container packaging, deployment, and public hosting are not part of this release scope.

## Security note

The server binds to loopback by default. Binding to a non-loopback interface requires explicit configuration. Review network exposure and deployment security before using it outside a local development environment.
