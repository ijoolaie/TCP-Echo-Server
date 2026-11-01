# Roadmap

This roadmap is a plan, not evidence that a feature has shipped. Check source code and CI results before marking work complete.

## Phase 0 — Repository foundations

- [x] Confirm repository access and default branch.
- [x] Replace placeholder README with project scope and engineering notes.
- [x] Add handoff and roadmap documents.
- [ ] Add .gitignore and .NET solution/project structure.

## Phase 1 — Minimal TCP echo server

- [ ] Create .NET 10 server project.
- [ ] Implement asynchronous accept loop.
- [ ] Echo arbitrary byte chunks without decoding or framing.
- [ ] Handle EOF and dispose client resources.
- [ ] Add a loopback smoke test.

## Phase 2 — Concurrency and lifecycle

- [ ] Support multiple simultaneous clients.
- [ ] Isolate client-specific exceptions.
- [ ] Add cancellation-aware graceful shutdown.
- [ ] Enforce maximum concurrent connections.
- [ ] Test simultaneous clients, abrupt disconnects, and shutdown.

## Phase 3 — Configuration and observability

- [ ] Configure bind host and port.
- [ ] Configure maximum connections.
- [ ] Add structured logs for server and client lifecycle.
- [ ] Avoid logging raw payloads.
- [ ] Document safe local defaults.

## Phase 4 — Quality and protocol correctness

- [ ] Test binary payload preservation.
- [ ] Test large payloads and fragmented/coalesced writes.
- [ ] Test empty payloads and EOF behavior.
- [ ] Review cancellation and resource ownership.
- [ ] Run tests on supported operating systems.

## Phase 5 — CI and release

- [ ] Add GitHub Actions for restore, build, and test on .NET 10.
- [ ] Document build, run, and test commands.
- [ ] Review security and operational defaults.
- [ ] Publish release notes and create a v1.0.0 tag only after all acceptance criteria pass.

## Phase 6 — Optional future work

Consider TLS, container packaging, metrics, health endpoints, or protocol extensions only after v1.0 and with explicit scope approval.
