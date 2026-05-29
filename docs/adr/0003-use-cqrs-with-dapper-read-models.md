# Use CQRS with Dapper Read Models

## Status

Accepted

## Context

Financial writes need invariant protection, while account and statement queries need optimized data shapes and pagination.

## Decision

Separate commands from queries. Commands use domain behavior and Event Store persistence. Queries read optimized models with Dapper where appropriate.

## Consequences

Read paths stay simple and fast. Write paths remain focused on consistency and business rules.

## Alternatives Considered

Using the same EF Core entities for reads and writes was considered but rejected because it blends persistence models with application contracts.

## Trade-offs

CQRS introduces duplication between events and read models, but it keeps the model honest about different read and write needs.
