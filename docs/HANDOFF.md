# Project Handoff

## Objective
Build ijoolaie/tcp-echo-server: an asynchronous TCP echo server in C# targeting .NET 10. It accepts concurrent TCP clients and echoes each client's bytes back unchanged.

## Current status
- Phase 1 (minimal server) implementation committed.
- Phase 2 (concurrency, bounded sessions, shutdown, and logging) implementation committed.
- .NET 10 solution and xUnit integration tests committed.
- GitHub Actions workflow committed for restore/build/test.
- **Verification is pending:** no successful local .NET build or test is claimed. The current execution environment does not have the .NET SDK installed. CI must be checked after the latest commits.

## Implemented behavior
- Asynchronous TCP accept loop and per-client echo tasks.
- Exact byte-preserving echo; no text decoding or application message framing.
- Concurrent sessions bounded by a configurable semaphore (default 100).
- Client-local I/O failures are logged at Debug and do not intentionally terminate other sessions.
- Cancellation-aware accept/read/write and shutdown.
- Structured console lifecycle logs; payload bytes are never logged.
- Defaults: 127.0.0.1:5000, maximum 100 concurrent clients.
- CLI arguments: bind IP, port, max concurrent clients.
- Integration tests authored for binary echo, two clients, 1 MiB fragmented writes, max-client backpressure, disconnect resilience, and shutdown.

## Commands
    dotnet restore tcp-echo-server.sln
    dotnet build tcp-echo-server.sln --configuration Release
    dotnet test tcp-echo-server.sln --configuration Release
    dotnet run --project src/TcpEchoServer -- 127.0.0.1 5000 100

## Remaining before v1.0
- Execute build/tests and fix any failures.
- Confirm GitHub Actions passes.
- Expand edge-case tests if validation identifies gaps.
- Complete resource-lifecycle/security review.
- Do not publish a v1.0 tag until verification is green.

## Safety constraints
- Keep loopback as the default bind address.
- Do not deploy or expose publicly without explicit approval.
- Never log client payloads.
- Keep unrelated repositories untouched.