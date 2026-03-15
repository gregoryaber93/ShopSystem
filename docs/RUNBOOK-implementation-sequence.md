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

## Stage 7
1. Complete `CHECKLISTA-PR7-rabbitmq-kafka-split.md`.
2. Finalize `PR7-event-routing-matrix.md` for all versioned events.
3. Validate RabbitMQ workflow paths and Kafka replay/analytics paths.

## Production Gate Criteria
- All checklists completed.
- No critical auth/role vulnerabilities.
- Event delivery and idempotency validated.
- Monitoring and alerting enabled.

## Internal User Provisioning Secrets
- Do not store internal API keys in repository files.
- Set `AuthService` secret in environment variable: `UserService__InternalApiKey`.
- Set `UserService` secret in environment variable: `InternalApi__ApiKey`.
- Keys must be identical for successful internal profile provisioning.

## Admin Seed Ownership
- `AuthService` is the single owner of admin bootstrap (`auth_users`, password, roles).
- `UserService` does not seed admin locally.
- Admin profile in `UserService` is synchronized from `AuthService` using `UserCreated` outbox event.
- This guarantees the same `UserId` for admin in both services.

## Docker Startup (Auth + User + Broker)
- Start brokers first from repository root: `docker compose -f docker-compose.brokers.yml up -d`.
- Start UserService stack: `cd UserService && docker compose up -d`.
- Start AuthService stack: `cd AuthService && docker compose up -d`.
- Verify API health manually on `http://localhost:5300` (UserService) and `http://localhost:5294` (AuthService).
- Compose files are preconfigured for gRPC sync and RabbitMQ outbox publish/consume.
