# Use Clean Architecture

## Status

Accepted

## Context

The API must demonstrate clear ownership boundaries and avoid placing business rules in controllers or persistence code.

## Decision

Use Clean Architecture with separate Domain, Application, Infrastructure, Api, and Worker projects.

## Consequences

Domain code remains framework independent, application services express use cases, infrastructure implements external concerns, and controllers stay thin.

## Alternatives Considered

A single Web API project was considered but rejected because it hides architectural boundaries and encourages accidental coupling.

## Trade-offs

The solution has more projects and ceremony, but the separation makes testing, review, and future changes clearer.
