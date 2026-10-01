# LedgerFlow product context

## Product intent

LedgerFlow is intended to become a credible digital-banking backend and a professional .NET learning environment. It is not currently a regulated banking product, does not connect to a real payment network, and should not be represented as production-ready.

This document separates verified present behavior from possible future scope. Source code and executable tests take precedence if documentation drifts.

## Current product

The current product is an educational, single-currency banking API with these user-visible capabilities:

- Customer registration, login, access tokens, rotating refresh tokens, and logout.
- Two roles: `Customer` and `Admin`.
- Customers can open accounts, list their accounts, and inspect one owned account.
- Admins can list/inspect all accounts and activate or deactivate accounts.
- Deposits into an owned account (admins are also accepted by the service).
- Transfers from a customer's owned source account to another account.
- A simulated PIX-like key registry and PIX payment implemented as an internal transfer. It has no Banco Central or external-network integration.
- Transfer lookup and paged account statements with optional date filtering.
- Idempotency-key handling for deposits, transfers, and PIX payments (PIX delegates to transfer idempotency using a prefixed key).
- HTTP health endpoints, Swagger/OpenAPI, request rate limiting, correlation response headers, structured console logs, and console-exported traces.

### Current business rules

- A newly opened account is active with a zero balance.
- Money is rounded to two decimal places using banker's rounding (`MidpointRounding.ToEven`). No currency is modeled.
- Operation amounts must be greater than zero.
- Inactive accounts cannot send or receive money.
- A debit cannot make an account balance negative.
- Source and destination accounts must differ.
- Customers may read and operate only resources allowed by explicit ownership checks; account status changes require the `AdminOnly` policy.
- Financial event streams in PostgreSQL are the balance history/source of truth; relational account, transfer, and statement tables are synchronous read models.

### Current boundaries and caveats

- A “customer” is currently a user with role `Customer`; there is no customer profile, KYC, address, legal identity, or relationship model.
- Accounts have no account number, currency, product type, overdraft, interest, fee, or ledger-entry accounting model.
- Deposits are direct balance credits without an external funding source or settlement lifecycle.
- Transfers are immediate and internal. There are no pending/reversed/failed settlement states.
- PIX is a simulation over internal accounts. Key formats are labels, not regulatory validation.
- There are no beneficiaries, configurable limits, scheduled payments, notifications, fraud decisions, audit UI/API, disputes, reconciliation, or administrative case management.
- There are no RabbitMQ consumers. The worker publishes integration events, but no current product behavior depends on consuming them.
- Redis is provisioned locally but unused by application code.

### API surface verified in source

| Area | Endpoints |
| --- | --- |
| Authentication | `POST /api/v1/auth/register`, `/login`, `/refresh`, `/logout` |
| Accounts | `POST /api/v1/accounts`; `GET /api/v1/accounts`; `GET /api/v1/accounts/{id}`; admin activate/deactivate patches |
| Money movement | `POST /api/v1/deposits`; `POST /api/v1/transfers`; `GET /api/v1/transfers/{id}` |
| PIX simulation | `POST /api/v1/pix/keys`; `GET /api/v1/pix/keys`; `POST /api/v1/pix/payments` |
| Statements | `GET /api/v1/accounts/{id}/statement` |
| Operations | `GET /health/live`; `GET /health/ready`; `/swagger` |

## Target product

The target is deliberately directional. Each capability must begin with a business requirement and acceptance criteria rather than a technology choice.

Potential domain evolution includes:

- Rich customer identity, onboarding, KYC/AML state, contact information, and customer status.
- Account products, stable identifiers, currencies, holds, available versus booked balance, blocking/restrictions, and lifecycle rules.
- Double-entry ledger concepts and auditable transaction lifecycle (pending, posted, rejected, reversed).
- Internal and external transfers, beneficiaries, approvals, schedules, limits, fees, and reconciliation.
- Searchable transaction history, statements, exports, and operational investigation views.
- Notifications with customer preferences and delivery outcomes.
- Risk/fraud checks with explainable decisions and manual review.
- Administrative and support operations with least privilege and durable audit trails.
- Production-grade security, observability, recovery, deployment, and incident response.

Target capabilities are not commitments to microservices, Kafka, Kubernetes, or any other platform. Those choices remain subordinate to demonstrated requirements.

## Product principles

- Financial correctness, authorization, and auditability take precedence over feature speed.
- Idempotency and retry behavior are part of the API contract.
- Customer-visible outcomes must be explainable from durable records.
- Security and privacy are product behavior, not deployment afterthoughts.
- Training scenarios should arise from realistic requirements or controlled exercises; do not insert deliberate faults into production paths.

