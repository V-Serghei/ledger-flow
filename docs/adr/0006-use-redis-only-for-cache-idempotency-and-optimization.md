# Use Redis Only for Cache, Idempotency Lookup, and Optimization

## Status

Accepted

## Context

Redis is useful for fast lookup, cache, rate limiting, and optional lock optimization, but financial truth must be durable and transactional.

## Decision

Use Redis only as an optimization layer. PostgreSQL remains authoritative for account movements, balances, idempotency records, and audit trails.

## Consequences

Redis outages should degrade optional optimizations, not corrupt financial state.

## Alternatives Considered

Using Redis as the primary balance store was rejected because it would weaken transactional guarantees and auditability.

## Trade-offs

Some operations may be slower without Redis shortcuts, but correctness remains independent from cache availability.
