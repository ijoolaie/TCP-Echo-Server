# Project Handoff

## Objective
Build ijoolaie/tcp-echo-server: an asynchronous TCP echo server in C# targeting .NET 10. It accepts concurrent TCP clients and echoes each client's bytes back unchanged.

## Current status
- Phase 1 (minimal server) implementation committed.
- Phase 2 (concurrency, bounded sessions, shutdown, and logging) implementation committed.
- Phase 3 (validated CLI/environment configuration and precedence) implementation committed.
- Phase 4 edge-case coverage and source-level lifecycle review committed.
- .NET 10 solution, xUnit tests, and GitHub Actions workflow committed.
- **Verification is pending:** no successful local .NET build or test is claimed. The current execution environment does not have the .NET SDK installed. CI results must be checked before claiming runtime validation.

## Implemented behavior
- Asynchronous TCP accept loop and per-client echo tasks.
- Exact byte-preserving echo; no text decoding or application message framing.
- Concurrent sessions bounded by a configurable semaphore (default 100).
- Client-local I/O failures are logged and do not intentionally terminate other sessions.
- Cancellation-aware accept/read/write and shutdown.
- Structured console lifecycle logs; payload bytes are never logged.
- Defaults: 127.0.0.1:5000, maximum 100 concurrent clients.
- Positional CLI arguments: bind IP, port, maximum concurrent clients.
- Environment variables: TCP_ECHO_HOST, TCP_ECHO_PORT, TCP_ECHO_MAX_CLIENTS.
- Configuration precedence: positional CLI argument > corresponding environment variable > default.
- Invalid configuration and extra CLI arguments are rejected.
- Unit tests cover defaults, environment configuration, CLI precedence, and invalid values.
- Integration tests cover binary echo, concurrent clients, 1 MiB fragmented writes, max-client backpressure, disconnect resilience, shutdown, coalesced writes, and empty client streams.
- Client session cleanup releases the connection slot in a finally block; session setup and I/O are covered by the same exception handling scope.

## Commands
    dotnet restore tcp-echo-server.sln
    dotnet build tcp-echo-server.sln --configuration Release
    dotnet test tcp-echo-server.sln --configuration Release
    dotnet run --project src/TcpEchoServer -- 127.0.0.1 5000 100

## Remaining before v1.0
- Execute build/tests and fix any failures.
- Confirm GitHub Actions passes.
- Run tests on supported operating systems.
- Complete runtime security and operational review.
- Do not publish a v1.0 tag until verification is green.

## Safety constraints
- Keep loopback as the default bind address.
- Do not deploy or expose publicly without explicit approval.
- Never log client payloads.
- Keep unrelated repositories untouched.