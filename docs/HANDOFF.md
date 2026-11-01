# Project Handoff

## Objective

Build `ijoolaie/tcp-echo-server`: an asynchronous TCP echo server in C# targeting .NET 10.

The server must accept multiple concurrent TCP clients and echo the exact bytes received from each client back to that same client.

## Repository status

- Repository is available and connected: `ijoolaie/tcp-echo-server`.
- Default branch: `main`.
- Visibility: private.
- Initial README placeholder was present and has been replaced with project documentation.
- Implementation status must be assessed from committed source and test results, not from this document.

## Fixed technical decisions

1. Language: C#.
2. Target framework: .NET 10.
3. Networking model: asynchronous TCP.
4. Echo semantics: byte-preserving; no implicit UTF-8 decoding, newline framing, or assumption that one read equals one message.
5. Client isolation: a client failure must not terminate unrelated client sessions.
6. Lifecycle: cancellation-aware graceful shutdown and deterministic disposal.
7. Configuration: bind host, port, and maximum concurrent clients.
8. Verification: unit tests, loopback integration tests, and GitHub Actions CI.

## Engineering rules

- Use asynchronous APIs and cancellation tokens where appropriate.
- Handle EOF as a normal client disconnect.
- Do not swallow server-wide failures; distinguish them from client-local failures.
- Bound concurrent client handling.
- Avoid logging payload contents.
- Keep defaults safe for local development.
- Do not deploy or bind to public interfaces without an explicit approval.
- Make small changes and verify each phase before claiming completion.

## Definition of Done for v1.0

- [ ] Server starts with documented configuration.
- [ ] Multiple clients can connect concurrently.
- [ ] Each client receives exactly the bytes it sends.
- [ ] Binary payloads and arbitrary chunk boundaries are supported.
- [ ] Disconnects and client errors are isolated.
- [ ] Graceful shutdown stops accepting clients and releases resources.
- [ ] Maximum concurrent clients is enforced.
- [ ] Structured lifecycle logging is present without payload logging.
- [ ] Unit and loopback integration tests pass.
- [ ] GitHub Actions restores, builds, and tests on .NET 10.
- [ ] README and release notes describe actual behavior.

## Next implementation sequence

1. Add solution and source/test projects targeting .NET 10.
2. Implement the minimal asynchronous echo loop.
3. Add concurrent-client and disconnect integration tests.
4. Add configuration, connection limits, and graceful shutdown.
5. Add logging, CI, and final documentation.
6. Verify build/test results before creating a v1.0 tag.
