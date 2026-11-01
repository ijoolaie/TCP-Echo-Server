# TCP Echo Server

An asynchronous TCP echo server built with **C# and .NET 10**. It accepts concurrent clients and writes each client's received bytes back to that same client without changing their contents.

> **Status:** Phase 1 and Phase 2 implementation and integration-test coverage are committed. Build/test execution and CI confirmation remain pending.

## Features
- Asynchronous TCP accept and read/write operations.
- Multiple concurrent client sessions, bounded by a configurable connection limit.
- Byte-preserving echo for binary and arbitrary stream chunks.
- Client-local handling of disconnect and I/O failures.
- Cancellation-aware server shutdown.
- Structured console logging for server and client lifecycle events (payloads are never logged).
- Integration tests for binary payloads, simultaneous clients, large fragmented writes, connection limits, disconnects, and shutdown.
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
The defaults bind to loopback (127.0.0.1), port 5000, with at most 100 concurrent clients:
    dotnet run --project src/TcpEchoServer

Arguments are positional: bind IP, port, maximum concurrent clients.
    dotnet run --project src/TcpEchoServer -- 127.0.0.1 7000 32

Press Ctrl+C for graceful shutdown. The loopback default is intentional; do not expose the server publicly without explicit security and deployment review.

## Repository layout
- src/TcpEchoServer/ — server implementation
- tests/TcpEchoServer.Tests/ — xUnit loopback integration tests
- docs/HANDOFF.md and docs/ROADMAP.md
- .github/workflows/dotnet.yml — CI pipeline
- tcp-echo-server.sln

## Scope
Version 1.0 focuses on reliable concurrent echo, lifecycle management, connection limits, configuration, structured logging, automated tests, CI, and documentation. TLS, authentication, custom message framing, metrics exporters, container deployment, and public hosting remain out of scope unless separately approved.

## Project documents
- [Handoff and engineering decisions](docs/HANDOFF.md)
- [Roadmap](docs/ROADMAP.md)