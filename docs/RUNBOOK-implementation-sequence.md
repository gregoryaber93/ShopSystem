# RUNBOOK - Implementation Sequence

## Stage 1
1. Complete `CHECKLISTA-PR1-security-and-roles.md`.
2. Merge after green auth and role-based tests.

## Stage 2
1. Complete `CHECKLISTA-PR2-order-and-promotions.md`.
2. Confirm correct backend-only pricing and discounts.

## Stage 3
1. Complete `CHECKLISTA-PR3-grpc-contracts.md`.
2. Verify proto contracts are stable.

## Stage 4
1. Complete `CHECKLISTA-PR4-outbox-brokers.md`.
2. Confirm reliable event publishing.

## Stage 5
1. Complete `CHECKLISTA-PR5-event-sourcing.md`.
2. Validate replay and snapshots.

## Stage 6
1. Complete `CHECKLISTA-PR6-redis-logger-observability.md`.
2. Verify tracing, logging, and cache metrics.

## Production Gate Criteria
- All checklists completed.
- No critical auth/role vulnerabilities.
- Event delivery and idempotency validated.
- Monitoring and alerting enabled.
