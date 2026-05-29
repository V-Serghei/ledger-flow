# Use Event Sourcing for Account Movements

## Status

Accepted

## Context

Financial operations need auditability, replayability, and a strong historical record of how a balance changed.

## Decision

Persist account movements as append-only events in `event_store` and rebuild account aggregates from event streams.

## Consequences

Every account movement has a durable audit trail. Read models can be rebuilt from events when projection logic changes.

## Alternatives Considered

Storing only mutable account balances was considered but rejected because it weakens auditability and makes historical reconstruction harder.

## Trade-offs

Event Sourcing increases implementation complexity and requires projection discipline, but it matches the financial consistency goals of the project.
