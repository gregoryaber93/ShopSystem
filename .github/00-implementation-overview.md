# ShopSystem - Implementation Plan

## Goal
Implement a model where:
- Admin creates users with the Manager role and manages shops.
- Manager manages products and promotions.
- Promotions are split into:
  - product-based promotions,
  - user-based promotions depending on purchase history.
- Architecture is based on event-driven design + selective event sourcing.

## Current State (Summary)
- `UserService` has an admin-only endpoint: `POST /api/users/create`.
- `AuthenticationService` has a public register endpoint that allows roles (`Admin`, `Manager`, `User`) - this must be fixed.
- `PromotionService` already has promotion types: `ProductDiscount`, `LoyaltyPoints`.
- `ShopService` has gRPC clients and Rabbit/Kafka "publishers", but publishers are currently placeholders only.
- `LoggerService` and `DashboardService` are in-memory.

## Priorities
1. Security and roles.
2. Correctness of order/payment/promotions.
3. gRPC contracts + real server endpoints.
4. Outbox + broker + idempotency.
5. Redis cache + read models.
6. Event sourcing for transactional domains.
7. Central logging and observability.

## Global Definition of Done
- Unit and integration tests pass.
- Endpoints return consistent `ProblemDetails`.
- `CorrelationId` works everywhere.
- Events are idempotent and versioned.
- Monitoring and logs are centrally available.
