# Use PostgreSQL as Source of Truth

## Status

Accepted

## Context

Balances and financial movements need durable transactional storage with uniqueness constraints and optimistic concurrency.

## Decision

Use PostgreSQL as the source of truth for the Event Store, idempotency records, read models, and Outbox.

## Consequences

The system can rely on transactions, indexes, JSONB event payloads, and unique constraints for consistency-critical operations.

## Alternatives Considered

Redis-only storage and broker-led state were rejected because neither should be the primary financial consistency boundary.

## Trade-offs

PostgreSQL centralizes consistency but requires careful schema design and migration discipline.
