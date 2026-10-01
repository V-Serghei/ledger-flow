---
name: ledgerflow-code-review
description: Review LedgerFlow changes or a pull request for concrete correctness, financial consistency, security, database, messaging, testing, and operational risks. Use for review requests, not for ordinary implementation self-checks.
---

# LedgerFlow code review

Review as a production backend teammate. Read repository-root `AGENTS.md`, then inspect the diff plus enough surrounding source, tests, schema/configuration, and callers to validate behavior. Do not modify code unless the user separately asks for fixes.

Prioritize findings in this order:

1. Financial/domain invariant violations, data loss/corruption, non-atomic accepted operations, and unsafe retries.
2. Authentication, authorization/ownership, secret or sensitive-data exposure, and input/response security.
3. Concurrency, idempotency, transaction/isolation/locking, message delivery and recovery, migrations, and backward compatibility.
4. Incorrect HTTP contracts, error mapping, cancellation/resource lifetime, and dependency failure behavior.
5. Query shape, round trips, tracking, N+1, indexing/pagination, allocations, and performance only when evidence or a credible scale makes them material.
6. Missing tests or observability that would allow an important regression/failure to escape.
7. Maintainability issues with a concrete future cost; do not enforce personal pattern preferences.

For each actionable finding, state severity, exact file/line, the triggering scenario, user/system impact, and the smallest reasonable correction. Verify that a supposed bug is not prevented elsewhere. Keep questions and optional improvements separate from defects.

Finish with a concise review summary and verification gaps. If there are no actionable findings, say so and identify residual untested risks rather than inventing feedback.

