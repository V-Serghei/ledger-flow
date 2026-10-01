# LedgerFlow problem-driven roadmap

## How to use this roadmap

This is a sequence of problems to earn the right to solve, not a calendar or technology installation list. Reorder stages when product evidence changes. Each stage should produce working behavior, tests, operational evidence, and learning—not merely a dependency in a project file.

## Stage 0: Understand and stabilize the inherited baseline

**Business problem:** The team cannot safely evolve a system whose actual behavior and documentation disagree.

**Engineering problem:** Known gaps include broken development-user normalization, unused dependencies, weak readiness signals, partial exception mapping, and incomplete verification of outbox/idempotency failure paths.

**Concepts learned:** Repository investigation, request tracing, ASP.NET Core pipeline, EF Core migrations, configuration precedence, regression testing, and technical-debt prioritization.

**Likely technologies:** Existing .NET 8, xUnit, EF Core, PostgreSQL, Docker Compose, GitHub Actions. No new platform is required.

**Expected outcome:** Reproducible local setup, truthful docs, a green baseline, prioritized correctness risks, and confidence that new work starts from understood behavior.

## Stage 1: Build realistic banking foundations

**Business problem:** The current immediate balance operations are too thin to represent credible banking behavior.

**Engineering problem:** Define transaction identity/lifecycle, account identifiers and currency rules, and eventually booked versus available balance or a double-entry ledger without breaking existing contracts accidentally.

**Concepts learned:** Requirements analysis, domain invariants, money/currency modeling, ledger/accounting fundamentals, API evolution, schema migrations, and backward compatibility.

**Likely technologies:** C#, ASP.NET Core, EF Core, PostgreSQL; possibly Dapper for a proven query need. Domain modeling is introduced only where behavior warrants it.

**Expected outcome:** A small but coherent banking model whose balances and transaction history are explainable and testable.

## Stage 2: Make correctness executable

**Business problem:** Financial and authorization regressions must be caught before customers or operators find them.

**Engineering problem:** Expand coverage around rollback, duplicate concurrent requests, resource ownership, token lifecycle, migrations, worker publishing, and failure recovery. Establish deterministic test data and a useful testing pyramid.

**Concepts learned:** Unit versus integration/API/contract testing, Testcontainers, fixtures, property-based thinking, test isolation, and CI diagnostics.

**Likely technologies:** xUnit, FluentAssertions, `WebApplicationFactory`, Testcontainers, PostgreSQL/RabbitMQ containers. Add a library only for a demonstrated testing capability.

**Expected outcome:** Tests protect externally meaningful behavior and critical consistency boundaries without over-mocking infrastructure.

## Stage 3: Concurrency, idempotency, and transactional integrity

**Business problem:** Retries and simultaneous money movement must never duplicate effects, lose accepted work, or create an invalid balance.

**Engineering problem:** Address idempotency races, non-atomic projection paths, PIX/outbox atomicity, lock ordering, deadlocks, retry semantics, and expired-record cleanup. Characterize isolation levels and PostgreSQL locking with evidence.

**Concepts learned:** ACID transactions, isolation, optimistic/pessimistic concurrency, unique constraints, deadlocks, idempotency state machines, and failure injection.

**Likely technologies:** PostgreSQL transactions/constraints/locks, EF Core execution strategies, SQL diagnostics. Redis is not the correctness fix.

**Expected outcome:** Documented consistency invariants with stress tests and predictable client-visible conflict/retry behavior.

## Stage 4: Query and runtime performance under measured load

**Business problem:** Transaction history and operational endpoints must remain responsive as data grows.

**Engineering problem:** Establish baselines, representative datasets, query plans, index effectiveness, keyset versus offset pagination, EF tracking/projections, allocation and round-trip costs, and safe load generation.

**Concepts learned:** `EXPLAIN (ANALYZE, BUFFERS)`, cardinality, indexes, N+1 detection, benchmarking, p95/p99 latency, throughput, saturation, and capacity reasoning.

**Likely technologies:** PostgreSQL tooling, EF Core logging, Dapper where measurements justify it, k6, BenchmarkDotNet for isolated code paths if needed.

**Expected outcome:** Measured service objectives and targeted optimizations with before/after evidence. Cache is considered only after query/data-shape fixes and with an invalidation plan.

## Stage 5: Decouple a real asynchronous side effect

**Business problem:** A non-critical side effect such as notification or audit export should not extend or endanger the synchronous transfer request.

**Engineering problem:** Define an integration event contract, reliable publication, an idempotent consumer, retries, poison handling, operational replay, and eventual-consistency expectations.

**Concepts learned:** Delivery semantics, consumer idempotency, message contracts, retry/backoff, dead-letter handling, correlation, and transactional outbox/inbox patterns.

**Likely technologies:** Existing RabbitMQ/MassTransit only if routing/work-queue requirements fit; PostgreSQL outbox/inbox. A database-backed worker may suffice for simpler jobs.

**Expected outcome:** One business-justified asynchronous workflow that survives duplicate delivery and process/broker failure and can be operated by a developer.

## Stage 6: Production observability and incident practice

**Business problem:** Operators need to detect, diagnose, and recover from failures before prolonged customer impact.

**Engineering problem:** Define useful logs, metrics, traces, dependency readiness, dashboards, alerts, runbooks, SLOs, and controlled incident exercises.

**Concepts learned:** OpenTelemetry signals, RED/USE methods, correlation, sampling, alert quality, incident command, postmortems, and capacity signals.

**Likely technologies:** OpenTelemetry SDK/exporter, Prometheus/Grafana or a cloud observability service when a deployment environment exists, structured logging, health checks.

**Expected outcome:** Evidence-led diagnosis of scenarios such as slow SQL, duplicate messages, dependency outage, high CPU/memory, and authorization failures—without planting defects in normal code.

## Stage 7: Durable event streams when requirements demand them

**Business problem:** Multiple independent consumers, long-term replay, analytics, or high-throughput ordered streams may outgrow queue-style integration.

**Engineering problem:** Prove stream requirements, partitioning keys, ordering scope, schema evolution, retention, replay safety, and consumer lag operations.

**Concepts learned:** Logs versus queues, partitions, consumer groups, schema compatibility, replay, backpressure, and event governance.

**Likely technologies:** Kafka only if these requirements are real; RabbitMQ remains suitable for routing/work queues. A PostgreSQL outbox still bridges the local transaction.

**Expected outcome:** A justified, operated stream use case—not Kafka installed for résumé coverage.

## Stage 8: Deployment, security hardening, and scaling

**Business problem:** The service must be deployable, recoverable, secure, and scalable for a defined environment.

**Engineering problem:** Artifact promotion, secret management, migrations, rollback, backups/restores, vulnerability management, least privilege, horizontal behavior, and disaster recovery.

**Concepts learned:** CI/CD, supply-chain security, infrastructure configuration, cloud primitives, blue/green or rolling deployment, RPO/RTO, and threat modeling.

**Likely technologies:** Containers and a simple managed runtime first; Azure/GitHub Actions or Azure DevOps concepts as context demands. Kubernetes only when orchestration requirements exceed the simpler model.

**Expected outcome:** Repeatable deployment with observable rollout, verified rollback/restore, and documented operational ownership.

## Possible later decomposition

A modular monolith is the default while one team and one deployment boundary are sufficient. Consider service extraction only when independent ownership, scaling, availability, regulatory isolation, or release cadence provides more value than network failures, eventual consistency, duplicated data, and operational overhead.

## Initial backlog candidates

These are proposals, not started work. Refine each through the workflow in `DEVELOPMENT.md` before implementation.

| ID | Type / origin | Problem and intended outcome |
| --- | --- | --- |
| **LF-101** | Bug · Support | Development seed users cannot authenticate because seed and login normalize email differently. Reproduce it, align the invariant, and add a regression test for documented credentials. |
| **LF-102** | Testing · QA | Establish a repeatable baseline command/evidence record for all 46 tests, formatting, migrations, and Compose build; remove unnecessary container startup from API tests only after proving it is unused. |
| **LF-103** | Reliability · Senior Developer | Characterize account-open and status-change partial-commit risk, then make event append and synchronous projection atomic with failure-path integration tests. |
| **LF-104** | Bug/Security · QA | Audit exception-to-HTTP mapping, beginning with concurrent deposits and admin account status operations, so expected conflicts do not become opaque 500s. |
| **LF-105** | Database · Senior Developer | Reproduce simultaneous reuse of one idempotency key, document the race, and implement deterministic replay/conflict behavior plus expiry cleanup criteria. |
| **LF-106** | Reliability · DevOps | Add dependency-aware readiness while keeping liveness process-local; document startup migration behavior and expected responses during PostgreSQL outage. |
| **LF-107** | Messaging · Senior Developer | Test the outbox worker against PostgreSQL and RabbitMQ, including publish success, retry exhaustion, crash-after-claim, and two-worker competition; propose a recoverable claim protocol. |
| **LF-108** | Security · Security | Threat-model current JWT/refresh-token flows and development secrets; prioritize token reuse detection, lockout/rate-limit behavior, key management, and sensitive logging without choosing an identity provider prematurely. |
| **LF-109** | Performance · Product/Dev | Define a representative statement dataset, capture generated SQL and query plans, measure current offset pagination, and recommend an index/pagination change only if evidence supports it. |
| **LF-110** | Feature · Product Owner | Specify account blocking separately from inactive lifecycle: actors, reasons, debit/credit effects, audit trail, and unblock policy. Produce acceptance criteria before code. |
| **LF-111** | Refactoring · Senior Developer | Assess duplicated controller claim extraction and exception mapping; propose the smallest centralization that improves consistency without hiding endpoint-specific contracts. |
| **LF-112** | DevOps · DevOps | Produce a deployment-readiness gap assessment covering non-root containers, secrets, artifact scanning, migration ownership, backup/restore, and rollback; do not add Kubernetes. |

### Recommended first assignment: LF-101

LF-101 is narrow enough for onboarding but crosses several useful boundaries: development environment, configuration, EF persistence, normalization invariants, authentication, integration testing, and documentation trust. The developer can independently reproduce the symptom, trace seed-to-query behavior, articulate where normalization should live, write a failing test, implement a minimal fix, and self-review for existing-data implications. It delivers visible value without forcing an architectural redesign.

