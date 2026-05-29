# Use Testcontainers for Integration Tests

## Status

Accepted

## Context

The API depends on PostgreSQL, Redis, and RabbitMQ. Mocking these dependencies would miss migration, transaction, networking, and serialization issues.

## Decision

Use Testcontainers to run real dependency containers for integration and concurrency tests.

## Consequences

Integration tests exercise realistic infrastructure and catch production-like failures earlier.

## Alternatives Considered

In-memory databases and hand-written fakes were rejected for consistency-critical tests because they do not behave like PostgreSQL under constraints and concurrency.

## Trade-offs

Tests are slower and require Docker, but their evidence is much stronger for this project.
