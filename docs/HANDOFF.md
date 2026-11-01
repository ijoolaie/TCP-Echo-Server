# Project Handoff

## Objective
Build ijoolaie/tcp-echo-server: an asynchronous TCP echo server in C# targeting .NET 10.

The server accepts multiple concurrent TCP clients and echoes the exact bytes received from each client back to that same client.

## Repository status
- Repository: ijoolaie/tcp-echo-server.
- Default branch: main; repository is private.
- README, handoff, roadmap, .gitignore, solution, server project, integration test project, and GitHub Actions workflow are committed.
- Initial server implementation is present.
- Build/test status: not yet verified. The current execution environment does not have the .NET SDK installed, so no successful local build or test run is claimed.
- GitHub Actions is configured to perform restore, build, and test on pushes and pull requests to main.

## Implemented behavior
- C# project targeting net10.0.
- Asynchronous TCP accept loop.
- Concurrent per-client echo tasks.
- Byte-preserving reads and writes; no text decoding or message framing.
- EOF ends a client session.
- Client-local I/O exceptions do not intentionally stop other sessions.
- Cancellation token stops accepting and cancels active client operations.
- Default bind address is loopback and default port is 5000.
- Command-line arguments can override bind address and port.
- Integration tests have been authored for binary payload preservation and two simultaneous clients.

## Fixed technical decisions
1. Language: C#.
2. Target framework: .NET 10.
3. Networking model: asynchronous TCP.
4. Echo semantics: byte-preserving; no implicit UTF-8 decoding, newline framing, or assumption that one read equals one message.
5. Client isolation: a client failure must not terminate unrelated client sessions.
6. Lifecycle: cancellation-aware shutdown and deterministic client disposal.
7. Configuration: bind IP and port currently; maximum connection count remains to be implemented.
8. Verification: unit/integration tests and GitHub Actions CI.

## Known gaps before v1.0
- Maximum concurrent connection limit.
- Structured logging.
- More exhaustive tests for large payloads, stream fragmentation/coalescing, EOF, and shutdown.
- Build/test and CI confirmation.
- Final security and resource-lifecycle review.

## Engineering rules
- Use asynchronous APIs and cancellation tokens where appropriate.
- Handle EOF as a normal client disconnect.
- Distinguish client-local failures from server-wide failures.
- Bound concurrent client handling before v1.0.
- Avoid logging payload contents.
- Keep defaults safe for local development.
- Do not deploy or bind to public interfaces without explicit approval.
- Make small changes and verify each phase before claiming completion.

## Definition of Done for v1.0
- [ ] Server starts with documented configuration.
- [x] Multiple clients can connect concurrently (implementation and test authored; test execution pending).
- [x] Each client receives the bytes it sends (implementation and test authored; test execution pending).
- [ ] Binary, large, and arbitrary chunk boundaries are verified.
- [x] Client I/O errors are isolated in implementation.
- [ ] Graceful shutdown is verified by tests.
- [ ] Maximum concurrent clients is enforced.
- [ ] Structured lifecycle logging is present.
- [ ] Unit and loopback integration tests pass.
- [ ] GitHub Actions passes on .NET 10.
- [x] README and release roadmap describe current scope and status.

## Next steps
1. Run dotnet restore, dotnet build, and dotnet test using the .NET 10 SDK.
2. Fix any compiler/test failures found by those commands or CI.
3. Add a maximum connection limit and structured logging.
4. Expand lifecycle and stream-boundary tests.
5. Complete review before tagging v1.0.0.