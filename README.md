# TCP Echo Server

A small, asynchronous TCP echo server built with **C# and .NET 10**. It accepts multiple concurrent clients and writes each client's received bytes back to that same client without changing their contents.

> **Project status:** Early development. The repository currently contains project planning and initial scaffolding; features are marked complete only after implementation and verification.

## Goals

- Handle multiple TCP clients concurrently.
- Preserve the exact byte sequence received from each client.
- Isolate client disconnects and client-specific failures.
- Support cancellation and graceful server shutdown.
- Configure bind host, port, and maximum concurrent connections.
- Provide structured logging and automated unit/integration tests.
- Run build and test checks in GitHub Actions.

## Protocol notes

TCP is a byte stream, not a message protocol. A read operation is not guaranteed to correspond to one application message or one client write. The echo implementation therefore treats input as arbitrary byte chunks and echoes those bytes unchanged. It does not assume UTF-8, newline framing, or message boundaries.

## Requirements

- .NET 10 SDK

## Getting started

The runnable server and test projects are being added incrementally. Once the initial implementation is in place, the intended commands will be:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project src/TcpEchoServer
```

## Planned repository layout

```text
.
├── src/
│   └── TcpEchoServer/
├── tests/
│   └── TcpEchoServer.Tests/
├── docs/
│   ├── HANDOFF.md
│   └── ROADMAP.md
├── .github/
│   └── workflows/
└── README.md
```

## Development principles

- Keep the echo path byte-preserving and independent of text encodings.
- Use asynchronous socket/network APIs; avoid blocking per-client work.
- Make cancellation, resource ownership, and exception handling explicit.
- Prefer small, reviewable changes with tests.
- Do not expose the server publicly or deploy it without an explicit decision.

## Scope boundaries

Version 1.0 focuses on a reliable TCP echo service, lifecycle management, configuration, logging, tests, CI, and documentation. TLS, authentication, a custom message protocol, metrics exporters, container deployment, and public hosting are out of scope unless separately approved.

## Project documents

- [Handoff and engineering decisions](docs/HANDOFF.md)
- [Roadmap](docs/ROADMAP.md)
