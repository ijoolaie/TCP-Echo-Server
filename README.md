# TCP Echo Server

An asynchronous TCP echo server built with C# and .NET 10. It accepts concurrent clients and writes each client's received bytes back to that same client without changing their contents.

> Status: Initial implementation and integration tests are committed. Local build/test verification is still pending; this is not a v1.0 release.

## Features
- Asynchronous TCP accept and read/write operations.
- Multiple concurrent client sessions.
- Byte-preserving echo, including binary payloads.
- Client-local handling of disconnect and I/O failures.
- Cancellation-aware server shutdown.
- Loopback integration tests for binary payloads and concurrent clients.
- GitHub Actions workflow for .NET 10 restore, build, and test.

## TCP stream semantics
TCP is a byte stream, not a message protocol. A read operation is not guaranteed to correspond to one application message or one client write. The server echoes arbitrary byte chunks unchanged. It does not assume UTF-8, newline framing, or message boundaries.

## Requirements
- .NET 10 SDK

## Build and test
    dotnet restore tcp-echo-server.sln
    dotnet build tcp-echo-server.sln --configuration Release
    dotnet test tcp-echo-server.sln --configuration Release

## Run locally
The server binds to loopback (127.0.0.1) on port 5000 by default:

    dotnet run --project src/TcpEchoServer

Optional positional arguments override the bind IP address and port:

    dotnet run --project src/TcpEchoServer -- 127.0.0.1 7000

Press Ctrl+C to request shutdown. The default loopback binding is intentional; do not expose the server publicly without an explicit security and deployment review.

## Repository layout
- src/TcpEchoServer/
- tests/TcpEchoServer.Tests/
- docs/HANDOFF.md and docs/ROADMAP.md
- .github/workflows/dotnet.yml
- tcp-echo-server.sln

## Scope
Version 1.0 aims to include reliable concurrent echo, graceful lifecycle management, configurable bind host/port and connection limits, structured logging, automated tests, CI, and complete documentation.

TLS, authentication, custom message framing, metrics exporters, container deployment, and public hosting are out of scope unless separately approved.

## Project documents
- [Handoff and engineering decisions](docs/HANDOFF.md)
- [Roadmap](docs/ROADMAP.md)