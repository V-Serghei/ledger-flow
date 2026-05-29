# Use Optimistic Concurrency for Financial Consistency

## Status

Accepted

## Context

Concurrent transfers must never allow an account balance to become negative. Multiple requests may load the same starting balance and race to write events.

## Decision

Use expected aggregate versions with a unique `aggregate_id + aggregate_version` constraint in the Event Store. Conflicting writes return `409 Conflict`.

## Consequences

Only one writer can append a specific next event version for an account. Losing writers must retry or return a conflict.

## Alternatives Considered

Pessimistic locks and Redis locks were considered. Redis locks remain optional optimization, but they are not the core consistency mechanism.

## Trade-offs

Optimistic concurrency may reject requests under high contention, but it keeps the write path simple and auditable.
