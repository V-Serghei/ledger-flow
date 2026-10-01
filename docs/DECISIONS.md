# Architecture decisions

## Purpose

Use lightweight Architecture Decision Records (ADRs) for choices that are expensive to reverse, affect several components/teams, introduce significant operational cost, establish a lasting constraint, or resolve a recurring disagreement. Do not create ADRs for routine implementation details or decisions that have not been made.

ADRs live in `docs/adr/` and are immutable history once accepted: supersede an ADR with a new one rather than rewriting the old context. Corrections of factual typos are acceptable.

## Status lifecycle

- **Proposed:** under discussion; not authority to implement.
- **Accepted:** current decision.
- **Deprecated:** retained for history but discouraged.
- **Superseded by ADR-NNNN:** replaced by a later decision.
- **Rejected:** considered and deliberately not selected.

## ADR template

```markdown
# NNNN: Short decision title

## Status

Proposed | Accepted | Rejected | Deprecated | Superseded by ADR-NNNN

## Context

What business/engineering situation exists? Include evidence, constraints, and forces.

## Problem

What precise decision is required now? What is explicitly out of scope?

## Considered alternatives

Include doing nothing/current design where viable. Compare relevant benefits, costs, failure modes, security, operability, migration, and reversibility.

## Decision

State the selected option and scope. Avoid vague aspirations.

## Consequences and trade-offs

List positive and negative consequences, new risks/obligations, and rejected benefits.

## Validation and follow-up

How will success be measured? What rollout, rollback, revisit trigger, or expiry applies?
```

## Decision process

1. Open a ticket with the concrete problem and evidence.
2. Write a proposed ADR before an irreversible implementation begins.
3. Review with the people responsible for product, data, security, and operations as applicable.
4. Time-box experiments and record results, not just opinions.
5. Accept/reject the ADR explicitly; merge it with or immediately before implementation.
6. Revisit when its stated assumptions or thresholds change.

Examples that may warrant future ADRs include RabbitMQ versus Kafka for an evidenced use case, modular monolith versus service extraction, EF Core versus Dapper for a measured query, cache consistency strategy, external identity provider, and deployment model. These examples are not pre-decisions.

## Existing ADR register

The fork already contains eight accepted ADRs:

| ADR | Decision | Current implementation note |
| --- | --- | --- |
| [0001](adr/0001-use-clean-architecture.md) | Clean Architecture | Dependency direction exists and has limited architecture tests. |
| [0002](adr/0002-use-event-sourcing-for-account-movements.md) | Event sourcing for account movements | Implemented for account streams only; event evolution/rebuild operations remain undeveloped. |
| [0003](adr/0003-use-cqrs-with-dapper-read-models.md) | CQRS with Dapper read models | Lightweight CQRS; Dapper currently serves only statement-entry reads, while other queries use EF Core. |
| [0004](adr/0004-use-transactional-outbox.md) | Transactional outbox | Atomic for deposit/transfer records; publisher recovery/competition and PIX-specific atomicity need work. |
| [0005](adr/0005-use-postgresql-as-source-of-truth.md) | PostgreSQL source of truth | Implemented. |
| [0006](adr/0006-use-redis-only-for-cache-idempotency-and-optimization.md) | Limit Redis to optimization roles | Constraint is sensible, but Redis is not currently used at all. |
| [0007](adr/0007-use-testcontainers-for-integration-tests.md) | Testcontainers integration tests | PostgreSQL behavior is exercised; Redis/RabbitMQ containers currently start without their behavior being tested. |
| [0008](adr/0008-use-optimistic-concurrency-for-financial-consistency.md) | Optimistic concurrency | Event version uniqueness is implemented; transfers also pessimistically lock the source read-model row. |

These ADRs record inherited decisions; this index does not claim their implementation is complete. When a current finding materially contradicts an accepted ADR's assumptions or scope, propose a superseding ADR instead of silently changing architecture.

