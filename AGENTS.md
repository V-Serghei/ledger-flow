# LedgerFlow agent guide

## Purpose

LedgerFlow is a long-term, production-oriented digital-banking backend and a practical learning environment for modern .NET engineering. The current codebase is a fork of an educational Banking API; treat what exists as a starting point, not as a production-ready system or a mandate to add every fashionable technology.

The primary outcome is the developer's engineering independence: investigating unfamiliar code, forming hypotheses, designing trade-offs, implementing deliberately, testing realistically, reviewing critically, and operating what they build.

## Start every task with evidence

- Read the ticket and the relevant files before proposing a solution. Do not assume `README.md` or an ADR perfectly matches runtime behavior.
- For unfamiliar work, trace the request or data path end to end and identify the consistency, authorization, and failure boundaries.
- Preserve the current architecture unless the task establishes a concrete reason to change it.
- Separate observed facts, inferences, and proposals. If a requirement is ambiguous and the choice changes business behavior, ask.
- Do not implement unrelated cleanup while completing a ticket. Record worthwhile follow-ups separately.

Use `docs/PRODUCT.md` for current versus target scope, `docs/ARCHITECTURE.md` for the verified system map, `docs/ROADMAP.md` for problem-driven evolution, and `docs/DEVELOPMENT.md` for detailed workflow and standards.

## Mentoring behavior

- Optimize for learning, not maximum generated code.
- On educational tickets, first invite the developer to explain the problem, likely investigation points, proposed solution, failure modes, and test strategy when doing so adds learning value.
- Expose important concepts and trade-offs. Challenge unsupported conclusions and review code as a professional teammate would.
- Let the developer write meaningful portions of code. Give less scaffolding as competence grows.
- For urgent incidents or explicitly requested implementation, act directly, then explain the reasoning and evidence.

## Engineering expectations

- Use modern, idiomatic C# and follow repository conventions unless the ticket justifies changing them.
- Prefer the simplest design that satisfies known requirements. Do not add an interface, layer, pattern, or dependency without a concrete seam or problem.
- Use async I/O correctly and propagate `CancellationToken` through supported call paths.
- Keep domain rules explicit. Do not bury banking behavior in generic infrastructure abstractions.
- Consider database round trips, generated SQL, tracking, projections, pagination, indexes, locks, transactions, and query plans when data behavior matters.
- Treat PostgreSQL as the current source of truth. EF Core does not replace SQL knowledge; Redis must not become the financial consistency boundary.
- Never log credentials, access/refresh tokens, signing keys, or sensitive customer data. Treat local secrets as development-only.
- Treat compiler/analyzer warnings as work to understand, not noise to suppress casually.

## Architecture and distributed systems

- Current direction: layered modular monolith with `Domain <- Application <- Infrastructure`, HTTP API and a separate outbox publisher worker. See `docs/ARCHITECTURE.md` for important qualifications.
- Do not introduce microservices, Kubernetes, DDD patterns, brokers, caches, or new libraries merely for practice.
- Introduce technology only when an observed requirement warrants its operational and cognitive cost; record significant decisions using `docs/DECISIONS.md`.
- For messaging work, reason explicitly about producer/consumer crashes, duplicates, loss, retry, poison messages, ordering, idempotency, transactional boundaries, eventual consistency, and observability.
- RabbitMQ and Kafka are not interchangeable defaults: select based on delivery/routing versus durable-stream/replay requirements.

## Testing expectations

- Protect business behavior and contracts, not private implementation details.
- Use domain unit tests for pure rules; integration/API tests for persistence, transactions, auth, serialization, and infrastructure behavior; architecture tests only for boundaries worth preserving.
- Prefer real PostgreSQL/broker/cache behavior through Testcontainers when that behavior is material. Do not mock away the risk under test.
- Every bug fix should normally include a regression test. Financial changes require happy-path, authorization, idempotency, concurrency, rollback, and retry analysis as applicable.
- Run the smallest relevant checks during development, then the proportionate broader suite before completion. Never claim checks ran when tooling or infrastructure was unavailable.

## Git workflow

- Work from a ticket such as `LF-101`; keep changes focused and reviewable.
- Inspect `git status` before and after work. Preserve unrelated user changes.
- Use a short-lived branch when requested or when team workflow requires it. Suggested names: `feature/LF-123-short-name`, `fix/LF-123-short-name`, or `chore/LF-123-short-name`.
- Prefer small conventional commits that explain intent. Do not commit, push, rewrite history, or open a pull request unless explicitly asked.
- A pull request should link the ticket, explain behavior and trade-offs, identify database/operational impact, and provide test evidence.

## Definition of done

A ticket is done when its requirement and acceptance criteria are satisfied; relevant tests pass; authorization, validation, failure, concurrency, data migration, and observability implications were considered; documentation/ADR/configuration changed when needed; no unrelated scope slipped in; the diff was self-reviewed; and verification evidence plus residual risks are reported.

## Technology introduction gate

Before adding infrastructure or a dependency, document: the concrete problem, evidence it exists, requirements and constraints, alternatives (including doing nothing), operational/testing/security costs, failure modes, migration and rollback plan, and measurable success criteria. Prefer a reversible experiment before a platform commitment.

