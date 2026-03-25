# Sprint Backlog and DoD

## Sprint 1
1. Role security hardening.
2. Tighten user/manager creation flow.
3. Price and discount correctness.
4. ProblemDetails + CorrelationId.

## Sprint 2
1. gRPC server-side + contracts.
2. LoggerService integration in all services.
3. Redis cache + idempotency keys.

## Sprint 3
1. Outbox + real Rabbit/Kafka publishing.
2. Event sourcing for order and loyalty.
3. Read-side projections and lag monitoring.

## Global Acceptance Criteria
1. No role escalation through public API.
2. Order flow does not trust client price.
3. User-specific promotions are based on purchase history.
4. Event delivery is reliable and idempotent.
5. Logging and tracing are centralized and consistent.
