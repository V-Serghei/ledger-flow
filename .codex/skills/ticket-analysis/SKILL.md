---
name: ticket-analysis
description: Analyze and refine a LedgerFlow LF ticket before implementation, especially when requirements, current behavior, data consistency, or test scope are unclear. Do not use for a tiny mechanical edit with complete acceptance criteria.
---

# LedgerFlow ticket analysis

Produce an implementation-ready analysis without changing product code unless the user also asks for implementation.

1. Read repository-root `AGENTS.md` and the relevant parts of `docs/PRODUCT.md`, `docs/ARCHITECTURE.md`, and `docs/DEVELOPMENT.md`.
2. Restate the actor, business outcome, acceptance criteria, constraints, and out-of-scope behavior. Do not treat a proposed technology as the requirement.
3. Trace the relevant current path from entry point through domain/application/infrastructure to storage or external dependency. Verify documentation claims against source and tests.
4. Separate observed facts, inferences, unknowns, and assumptions. Ask only questions whose answers materially change behavior, security, data, compatibility, or architecture.
5. Identify authorization/ownership, validation, transaction boundaries, idempotency/retry, concurrency, migration, compatibility, observability, and failure modes as applicable.
6. Compare the smallest viable solution with meaningful alternatives, including keeping the current design when viable. Flag whether an ADR is warranted under `docs/DECISIONS.md`.
7. Propose a test/verification matrix at the cheapest boundaries that catch the risks. Use real infrastructure when its semantics are the subject.
8. Return: refined requirement, source-backed current behavior, risks/unknowns, proposed approach and trade-offs, acceptance criteria, test plan, operational/data impact, and clearly separated follow-up tickets.

For an educational ticket, invite the developer to offer their own diagnosis, investigation plan, failure analysis, or design before supplying the corresponding answer when this will build independence.

