# LedgerFlow learning system

## Goal

LedgerFlow should develop an engineer who can independently investigate, design, implement, test, debug, review, optimize, deploy, and operate a serious .NET backend. Shipping working banking behavior is the vehicle for learning; collecting technologies is not the goal.

## Learning loop

For each substantial ticket:

1. **Frame:** Restate the business outcome, acceptance criteria, constraints, and unknowns.
2. **Investigate:** Trace the current path, reproduce the behavior, inspect tests/data/SQL/configuration, and separate facts from assumptions.
3. **Propose:** Compare at least the viable options, failure modes, security/data implications, and verification approach.
4. **Implement:** Make the smallest coherent change. The developer should write meaningful code rather than merely approve generated output.
5. **Verify:** Test behavior at the right boundary, inspect persistence/SQL/telemetry when relevant, and exercise failure paths.
6. **Review:** Self-review the diff as if it were someone else's; then receive realistic code-review feedback and respond with reasoning.
7. **Reflect:** Record what was learned, what remains uncertain, and one reusable principle. Update `docs/learning-journal.md` only when the entry has durable value.

The mentor should ask questions before providing a solution when the reasoning itself is the lesson: What invariant is at risk? Where is the transaction boundary? What can fail between these two writes? What will a retry do? Which test would disprove the hypothesis? Under incident pressure, act first as needed and teach from the evidence afterward.

## Learning tracks

### Language and application platform

- Modern C#: records/value types, nullability, exceptions, collections, async/await, cancellation, allocation awareness, and concurrency primitives.
- Modern .NET and ASP.NET Core: hosting, dependency injection, middleware ordering, model validation, configuration/options, HTTP semantics, Problem Details, background services, and diagnostics.
- REST and HTTP: resource/operation modeling, status codes, idempotency, caching headers, pagination, versioning, and backward compatibility.

Evidence of competence: explain the request pipeline, implement a vertical change without cargo-cult abstractions, preserve cancellation, and diagnose a failing endpoint from logs/tests/source.

### Data engineering

- EF Core: tracking, projections, includes, generated SQL, migrations, transactions, concurrency, batching, and execution strategies.
- LINQ and SQL: joins, aggregates, windows, indexes, cardinality, query plans, locks, isolation levels, deadlocks, and pagination.
- PostgreSQL as the current platform; learn SQL Server concepts by comparing isolation, indexing, execution plans, and operational tooling when useful.
- Dapper where control or measured read-path value justifies explicit SQL—not as an automatic “CQRS” requirement.

Evidence of competence: predict and inspect generated SQL, use `EXPLAIN`, justify an index from workload/cardinality, reproduce a race, and explain the transaction/lock behavior.

### Security and identity

- Authentication versus authorization, JWT validation, refresh-token lifecycle, password storage, OAuth 2.0 and OpenID Connect, resource ownership, least privilege, secret management, and threat modeling.
- Move toward an external identity provider only when product/deployment requirements justify the integration.

Evidence of competence: trace a principal from token to resource decision, identify confused-deputy/data-leak risks, test negative authorization paths, and distinguish local JWT mechanics from OAuth/OIDC protocols.

### Reliability and distributed systems

- Idempotency, transactions, optimistic/pessimistic concurrency, retries, timeouts, circuit breaking, eventual consistency, background workers, and transactional outbox/inbox.
- For every message path, model duplicate delivery, loss windows, producer/consumer crash, retry, poison messages, dead-letter handling, ordering, replay, and observability.
- RabbitMQ fits routing and work-queue requirements; Kafka fits durable ordered streams, replay, multiple independent consumers, analytics, and high event throughput. Requirements choose—not popularity.

Evidence of competence: draw the failure timeline, state delivery guarantees precisely, design an idempotent consumer, and operate recovery rather than merely publish a message.

### Architecture

- Learn DDD, CQRS, event-driven architecture, modular monoliths, and microservices as tools with costs.
- Begin with cohesive modules and explicit boundaries. Extract services only for demonstrated ownership/scaling/availability needs.
- Learn event sourcing from the current account stream, including evolution, replay, snapshots, and projection recovery gaps.

Evidence of competence: name the actual pattern implemented (including limitations), defend a boundary with business coupling, and reject unnecessary abstraction constructively.

### Testing and quality

- Unit, integration, API, architecture, contract, performance, resilience, and security testing.
- Prefer Testcontainers where database, broker, or cache semantics matter. Mocks are useful for pure collaboration seams but must not hide the behavior under investigation.
- Treat test design as risk design: what failure would be expensive, plausible, and otherwise invisible?

Evidence of competence: choose the cheapest test that catches the risk, write failure-first regression tests, avoid implementation coupling, and diagnose nondeterminism.

### Delivery and operations

- Docker/Compose, CI/CD, GitHub Actions, Azure DevOps concepts, Azure deployment primitives, configuration/secrets, migrations, artifact promotion, rollback, backup/restore, and supply-chain security.
- Kubernetes comes after concrete orchestration problems such as multi-service scheduling, autoscaling, rollout control, or platform standardization—not before.

Evidence of competence: build once/promote artifacts, explain deployment and migration failure modes, perform rollback/restore, and evaluate a simpler hosting model first.

### Observability and performance

- Structured logging, OpenTelemetry traces/metrics, correlation, Prometheus/Grafana concepts, health/readiness, SLOs, profiling, load tests, and capacity.
- Investigate slow endpoints through measurements: request trace, database time/plan, downstream calls, CPU/allocation, locks, and saturation.

Evidence of competence: form and test a performance hypothesis, create an actionable alert, correlate signals across a request/message, and demonstrate improvement with before/after results.

## Technology introduction rule

Technology is introduced because a real engineering requirement justifies its cost.

- Do not add RabbitMQ simply to learn RabbitMQ. Use asynchronous messaging when a concrete side effect should be decoupled or buffered, then learn the broker through that requirement.
- Do not add Kafka simply to learn Kafka. Require durable event streams, replay, independent consumers, ordering/throughput, or analytics needs.
- Do not add Redis as a reflex for a slow query or correctness race. First understand SQL, indexes, data shape, locks, and consistency; add cache only with ownership, expiry, invalidation, failure, and observability plans.
- Do not add Kubernetes while one or a few containers on a simpler managed runtime meet deployment needs.
- Do not create microservices to practice microservices. Earn a network boundary through team/domain/scale/availability constraints.

Small time-boxed experiments are allowed in a separate branch or scratch area when the learning question is explicit, success/failure criteria are written first, and experimental dependencies do not silently enter production code.

## Deliberate production exercises

Future exercises may simulate slow endpoints, database contention/deadlocks, duplicate transactions, stale cache, failed consumers, unavailable dependencies, authorization errors, broken rollout, memory/CPU pressure, excessive queries, and race conditions. Use tests, load harnesses, failure injection, disposable environments, or incident game days. Do not manufacture defects in the normal application path.

For each exercise capture: initial alert/symptom, customer impact, timeline, hypotheses, evidence, mitigation, root/contributing causes, permanent actions, and what observability was missing.

## Progression and autonomy

- **Guided:** mentor supplies investigation prompts and narrows choices.
- **Collaborative:** developer leads analysis/proposal; mentor challenges trade-offs and reviews.
- **Independent:** developer owns ticket through evidence and review; mentor focuses on hidden risks.
- **Leadership:** developer scopes incidents/architecture changes, mentors others, and improves the system of work.

Progress is demonstrated by decisions and outcomes, not by the number of tools used.

