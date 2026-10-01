# LedgerFlow development workflow

## Repository baseline

- Solution: `Banking.sln`
- Runtime target: .NET 8 (SDK is not pinned by `global.json`).
- Runtime dependencies: PostgreSQL; RabbitMQ only for the worker's publisher. Redis is currently provisioned but unused.
- Local orchestration: `docker-compose.yml` and Bash scripts under `scripts/`.
- CI: `.github/workflows/ci.yml`.

Do not assume local tools exist. Check `dotnet --info` and `docker version` before promising verification. Keep secrets outside committed production configuration; values currently committed are explicitly local-development credentials.

## Company-like ticket flow

Tickets use `LF-###` and may originate from Product Owner, Business Analyst, Senior Developer, QA, DevOps, Security, Support, or a Production Incident.

### 1. Requirement

Record the actor, desired outcome, business rules, acceptance criteria, constraints, and explicitly out-of-scope behavior. Distinguish a product request from a proposed implementation.

### 2. Investigation

- Reproduce or trace current behavior.
- Locate relevant API, application contract, domain behavior, persistence, schema/migrations, configuration, worker, and tests.
- Identify authorization and data ownership.
- Identify transaction and consistency boundaries, retries/idempotency, concurrency, and operational signals.
- For database work, inspect SQL and plans rather than reasoning only from LINQ.
- Write facts, assumptions, questions, and risks separately.

### 3. Clarification

Ask only questions whose answers materially change business behavior, data migration, security, API compatibility, or architecture. Offer concrete options and trade-offs. Do not hide an important ambiguity behind a default.

### 4. Proposed solution

Describe the smallest coherent change, alternatives considered, data/API/operational impact, rollout/rollback if relevant, and test plan. Significant architectural choices use `DECISIONS.md` and an ADR.

For learning-oriented work, the developer should first explain what they think the problem is, where they would investigate, their proposed solution, what might fail, and how they would test it.

### 5. Implementation

- Keep the diff focused on the ticket.
- Follow dependency direction and existing naming/layout.
- Make domain rules explicit; avoid speculative abstractions.
- Propagate cancellation through I/O APIs.
- Use transactions that match the business atomicity boundary.
- Add migrations deliberately; never edit an applied migration to change production history.
- Add logs/metrics/traces only when they answer an operational question and avoid sensitive data.

### 6. Tests and verification

Start with the smallest test that captures the requirement or regression, then expand in proportion to risk.

Suggested commands (when the SDK/runtime is available):

```powershell
dotnet restore Banking.sln
dotnet build Banking.sln --configuration Release --no-restore
dotnet format Banking.sln --verify-no-changes --no-restore
dotnet test tests/Banking.UnitTests/Banking.UnitTests.csproj --configuration Release --no-build
dotnet test tests/Banking.ArchitectureTests/Banking.ArchitectureTests.csproj --configuration Release --no-build
dotnet test tests/Banking.IntegrationTests/Banking.IntegrationTests.csproj --configuration Release --no-build
docker compose build
```

Integration tests require a working Docker daemon for Testcontainers. Report the exact commands and outcomes; if a check cannot run, report the missing prerequisite and do not convert prior evidence into a current pass.

### 7. Self-review

Review the diff, not just the final files:

- Does it meet each acceptance criterion without unrelated scope?
- Are authentication, authorization, validation, and information disclosure correct?
- Can retries, concurrency, partial failures, or cancellation leave inconsistent state?
- Are SQL shape, round trips, tracking, indexes, and pagination appropriate?
- Are event/message contracts compatible and consumers assumed idempotent?
- Do tests fail for the right reason before the fix and cover behavior rather than internals?
- Are configuration, migrations, docs, and operational signals complete?

### 8. Code review

Reviewer prioritizes correctness, financial/data integrity, security, reliability, compatibility, performance evidence, and maintainability. Findings should cite a concrete scenario and affected line/path. “I prefer another pattern” is not sufficient.

The author should respond with evidence or update the code; neither blind acceptance nor defensive dismissal is useful. Resolve disagreements by restating requirements and risks.

### 9. QA

QA validates acceptance criteria plus negative, boundary, authorization, retry, and concurrency paths. Record environment/configuration and relevant data. A discovered defect becomes a linked ticket or blocks completion according to severity.

### 10. Completion

Summarize behavior, files/data/config changed, verification evidence, deployment/migration notes, residual risks, and follow-ups. Meet the `AGENTS.md` definition of done.

## Coding standards

- Enable and respect nullable reference types and implicit usings as configured.
- Prefer domain-specific names and small cohesive methods over generic managers/helpers.
- Do not create interfaces solely to mirror every class. Interfaces should represent a boundary, alternate implementation, or meaningful test seam.
- Prefer immutable request/response records where appropriate; keep persistence entities explicit.
- Use exceptions for exceptional/domain failure according to existing contracts, but avoid duplicating endpoint mappings indefinitely; centralization requires a scoped ticket and contract tests.
- Avoid sync-over-async, fire-and-forget request work, and swallowed cancellation.
- Measure before micro-optimizing. Consider allocations where profiling or a hot path makes them material.
- Comments should explain intent, invariant, or non-obvious constraint—not restate code.

## Database standards

For changes that affect persistence:

1. State the source of truth and atomicity invariant.
2. Inspect generated SQL or write/inspect explicit SQL.
3. Count likely round trips and consider tracking/projection needs.
4. Evaluate cardinality and existing indexes; capture `EXPLAIN (ANALYZE, BUFFERS)` for material query work using safe representative data.
5. Consider N+1 behavior, pagination stability, isolation, row/table locks, optimistic conflicts, deadlocks, and retry safety.
6. Write a forward migration and, when applicable, a backfill/rollback or roll-forward plan.
7. Test against real PostgreSQL behavior when the risk depends on PostgreSQL.

Do not solve database correctness with cache or a distributed lock. Do not add an index without explaining the query and write/storage trade-off.

## API and security standards

- Preserve HTTP semantics and stable Problem Details contracts where clients may rely on them.
- Validate syntax at the edge and business invariants in the domain/application boundary.
- Authentication proves identity; authorization checks action and resource ownership. Test both horizontal and vertical privilege boundaries.
- Use secure randomness, established cryptography, secret providers for non-local environments, and least privilege.
- Never put tokens/passwords/keys/PII in logs, test output, URLs, or exceptions.
- Threat-model new money movement, admin capability, integration, and externally supplied identifier.

## Messaging standards

Before adding a producer or consumer, write the message purpose, owner, schema/versioning policy, ordering key, duplicate behavior, retry/backoff, poison/dead-letter path, data consistency boundary, and operational recovery process.

Assume at-least-once delivery unless proven otherwise. A broker acknowledgment and local database commit are separate facts. Consumers must normally be idempotent and observable.

## Git and pull requests

- Begin from a clean understanding of `git status`; do not discard unrelated changes.
- Branch/commit conventions are defined in `AGENTS.md`. Include the ticket ID in branch/PR context.
- Suggested commit: `fix(auth): LF-101 align seeded email normalization`.
- PR description should include: outcome, approach/trade-offs, data/API/config impact, test evidence, screenshots/log snippets only when useful, rollout/rollback, and follow-ups.
- Do not commit generated secrets, local logs/PIDs, build outputs, or test-container state.

## Current local-development notes

- `docker compose up --build` starts API, worker, PostgreSQL, Redis, and RabbitMQ. API currently depends on Redis/RabbitMQ health in Compose even though it does not use them.
- API applies migrations on startup. Coordinate this before horizontal deployment.
- `scripts/start-local.sh` and `scripts/stop-local.sh` are Bash scripts, not native PowerShell scripts.
- Development seed credentials are documented in `README.md`, but a normalization mismatch is a known LF-101 candidate; do not assume they work until fixed.
- `/health/live` and `/health/ready` currently have identical shallow behavior.

