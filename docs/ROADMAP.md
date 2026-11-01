# Roadmap

This roadmap distinguishes committed implementation from verified behavior. A feature is not considered verified until the relevant build/tests pass.

## Phase 0 — Repository foundations
- [x] Confirm repository access and default branch.
- [x] Replace placeholder README with project overview and commands.
- [x] Add handoff and roadmap documents.
- [x] Add .gitignore and .NET solution/project structure.

## Phase 1 — Minimal TCP echo server
- [x] Create .NET 10 server project.
- [x] Implement asynchronous accept loop.
- [x] Echo arbitrary byte chunks without decoding or framing.
- [x] Handle EOF and dispose client resources.
- [x] Author loopback integration tests for binary echo.

Verification pending: run restore, build, and tests with the .NET 10 SDK.

## Phase 2 — Concurrency and lifecycle
- [x] Support multiple simultaneous clients.
- [x] Isolate client-specific I/O exceptions.
- [x] Add cancellation-aware server shutdown.
- [ ] Enforce and test maximum concurrent connections.
- [x] Author integration coverage for two simultaneous clients.
- [ ] Verify shutdown behavior and client disconnect handling in tests.

## Phase 3 — Configuration and observability
- [x] Configure bind IP address and port through command-line arguments.
- [ ] Configure maximum connections.
- [ ] Add structured lifecycle logging.
- [x] Use loopback as the safe local default.

## Phase 4 — Quality and protocol correctness
- [x] Author binary payload preservation test.
- [ ] Test large payloads and fragmented/coalesced writes.
- [ ] Test empty payloads and EOF behavior.
- [ ] Review cancellation and resource ownership.
- [ ] Run tests on supported operating systems.

## Phase 5 — CI and release
- [x] Add GitHub Actions workflow for .NET 10 restore, build, and test.
- [x] Document build, run, and test commands.
- [ ] Confirm CI passes on GitHub.
- [ ] Complete security and operational review.
- [ ] Publish release notes and create a v1.0.0 tag only after acceptance criteria pass.

## Phase 6 — Optional future work
Consider TLS, container packaging, metrics, health endpoints, or protocol extensions only after v1.0 and with explicit scope approval.