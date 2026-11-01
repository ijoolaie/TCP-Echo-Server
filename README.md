# TCP Echo Server

An asynchronous TCP echo server built with **C# and .NET 10**. It accepts concurrent clients and writes each client's received bytes back to that same client without changing their contents.

> **Status:** Phase 1–4 implementation and automated test coverage are committed. Build/test execution and CI confirmation remain pending.

## Features
- Asynchronous TCP accept and read/write operations.
- Multiple concurrent client sessions, bounded by a configurable connection limit.
- Byte-preserving echo for binary and arbitrary stream chunks.
- Client-local handling of disconnect and I/O failures.
- Cancellation-aware server shutdown.
- Structured console logging for server and client lifecycle events (payloads are never logged).
- Configuration through command-line arguments or environment variables, with validated values.
- Integration tests for binary payloads, simultaneous clients, large fragmented writes, connection limits, disconnects, shutdown, and configuration.
- GitHub Actions workflow for .NET 10 restore, build, and test on Ubuntu, Windows, and macOS.

## TCP stream semantics
TCP is a byte stream, not a message protocol. A read operation is not guaranteed to correspond to one application message or one client write. The server echoes arbitrary byte chunks unchanged. It does not assume UTF-8, newline framing, or message boundaries.

## Requirements
- .NET 10 SDK

## Build and test
    dotnet restore tcp-echo-server.sln
    dotnet build tcp-echo-server.sln --configuration Release
    dotnet test tcp-echo-server.sln --configuration Release

## Run locally
Defaults: bind to loopback (127.0.0.1), port 5000, maximum 100 concurrent clients.

    dotnet run --project src/TcpEchoServer

Positional arguments are: bind IP, port, maximum concurrent clients. Command-line arguments override environment variables.

    dotnet run --project src/TcpEchoServer -- 127.0.0.1 7000 32

Alternatively, configure with environment variables:

    TCP_ECHO_HOST=127.0.0.1
    TCP_ECHO_PORT=7000
    TCP_ECHO_MAX_CLIENTS=32

Environment variables are used when the corresponding positional argument is omitted. Invalid values are rejected with an error rather than silently replaced by defaults. Press Ctrl+C for graceful shutdown.

The loopback default is intentional; do not expose the server publicly without explicit security and deployment review.

## Repository layout
- src/TcpEchoServer/ — server implementation
- tests/TcpEchoServer.Tests/ — xUnit unit and loopback integration tests
- docs/HANDOFF.md and docs/ROADMAP.md
- .github/workflows/dotnet.yml — CI pipeline
- tcp-echo-server.sln

## Scope
Version 1.0 focuses on reliable concurrent echo, lifecycle management, connection limits, configuration, structured logging, automated tests, CI, and documentation. TLS, authentication, custom message framing, metrics exporters, container deployment, and public hosting remain out of scope unless separately approved.

## Project documents
- [Handoff and engineering decisions](docs/HANDOFF.md)
- [Roadmap](docs/ROADMAP.md)