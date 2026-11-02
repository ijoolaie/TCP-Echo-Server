# Roadmap

A checked item means implementation or test coverage is committed. Passing build/test/CI is tracked separately and must not be inferred from source files.

## Phase 0 — Repository foundations
- [x] Confirm repository access and default branch.
- [x] Add README, handoff, and roadmap.
- [x] Add .gitignore and .NET solution/project structure.

## Phase 1 — Minimal TCP echo server
- [x] Create .NET 10 server project.
- [x] Implement asynchronous accept loop.
- [x] Echo arbitrary byte chunks without decoding or framing.
- [x] Handle EOF and dispose client resources.
- [x] Add loopback tests for binary echo and concurrent clients.
- [x] Execute restore, build, and tests with .NET 10 on GitHub Actions (17 tests passed per OS).

## Phase 2 — Concurrency and lifecycle
- [x] Support multiple simultaneous clients.
- [x] Isolate client-specific I/O failures.
- [x] Add cancellation-aware shutdown.
- [x] Enforce maximum concurrent clients with a semaphore.
- [x] Add structured lifecycle logs without payload contents.
- [x] Add integration coverage for two clients, max-client backpressure, large fragmented writes, disconnect resilience, and shutdown.
- [x] Execute tests and verify lifecycle behavior in CI (17/17 passed on Ubuntu, Windows, and macOS).

## Phase 3 — Configuration and observability
- [x] Configure bind IP, port, and maximum concurrent clients through command-line arguments.
- [x] Support TCP_ECHO_HOST, TCP_ECHO_PORT, and TCP_ECHO_MAX_CLIENTS environment variables.
- [x] Define configuration precedence: command-line argument, environment variable, then safe default.
- [x] Reject malformed or out-of-range configuration instead of silently falling back.
- [x] Structured console logging for server/client lifecycle.
- [x] Safe loopback default.
- [x] Execute configuration tests and verify startup behavior in CI (17/17 passed on Ubuntu, Windows, and macOS).

## Phase 4 — Quality and protocol correctness
- [x] Author binary payload preservation test.
- [x] Author large payload / fragmented write test.
- [x] Author coalesced-write test confirming byte-stream semantics and no framing assumptions.
- [x] Author empty client-stream (half-close) test and verify subsequent clients are served.
- [x] Review cancellation flow and client resource ownership; ensure every acquired connection slot is released in the client-session finally block.
- [x] Author shutdown-under-capacity-pressure test: an active session and a second connected client while the sole client slot is occupied.
- [x] Execute all tests and inspect runtime behavior in CI.
- [x] Run tests on supported operating systems (Ubuntu, Windows, macOS).

## Phase 5 — CI and release
- [x] Add GitHub Actions workflow for .NET 10 restore, build, and test.
- [x] Configure CI matrix for Ubuntu, Windows, and macOS.
- [x] Document build, run, and test commands.
- [x] Confirm latest CI passes on GitHub (workflow run #49, commit 8903c91; 17/17 tests per OS).
- [x] Complete source-level security and operational review: loopback default, validated configuration, bounded sessions, payload-free logging, resource cleanup, and cancellation-aware shutdown.
- [ ] Confirm desired GitHub repository visibility (currently public).
- [ ] Publish release notes and tag v1.0.0 after the visibility/release decision is confirmed.

## Phase 6 — Optional future work
Consider TLS, container packaging, metrics, health endpoints, or protocol extensions only after v1.0 and with explicit scope approval.
