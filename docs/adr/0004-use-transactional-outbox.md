# Use Transactional Outbox

## Status

Accepted

## Context

Publishing directly to RabbitMQ during a financial transaction can create inconsistent outcomes if the database commit succeeds and broker publish fails.

## Decision

Store integration messages in `outbox_messages` in the same PostgreSQL transaction as business changes. A worker publishes pending messages later.

## Consequences

Broker failures no longer break database consistency. Messages can be retried and audited through status fields.

## Alternatives Considered

Direct broker publishing from command handlers was rejected because it couples the transaction boundary to an external network dependency.

## Trade-offs

The Outbox adds delayed publication and worker complexity, but it gives predictable consistency under broker failures.
