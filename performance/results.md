# Performance Results

## Targets

- `GET /api/v1/accounts/{accountId}/statement` p95 under 300ms with 10k records.
- `POST /api/v1/transfers` p95 under 500ms under moderate load.
- 0 duplicated transfers under retry.
- 0 negative balances under concurrent transfer pressure.

## How to Run

Prepare a local environment with Docker Compose, create users and accounts, deposit enough balance into the source account, and export the variables used by the scripts.

```bash
k6 run -e BASE_URL=http://localhost:5000 -e TOKEN=$TOKEN -e SOURCE_ACCOUNT_ID=$SOURCE_ACCOUNT_ID -e DESTINATION_ACCOUNT_ID=$DESTINATION_ACCOUNT_ID performance/k6-transfer-test.js
k6 run -e BASE_URL=http://localhost:5000 -e TOKEN=$TOKEN -e ACCOUNT_ID=$ACCOUNT_ID performance/k6-statement-test.js
```

## Latest Measurement

No benchmark has been captured yet. The scripts are committed as repeatable scenarios so results can be added after the integration test environment and seed data are finalized.
