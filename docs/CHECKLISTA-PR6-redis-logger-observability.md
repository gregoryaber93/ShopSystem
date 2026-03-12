# CHECKLIST PR6 - Redis, Logger, Observability

## Goal
Add caching, centralized logging, and full traceability.

## Implementation Tasks
- [x] Add Redis (`IDistributedCache`) in key services.
- [x] Cache: products per shop.
- [x] Cache: active promotions.
- [x] Cache: user-specific eligibility.
- [x] Add idempotency keys for `POST /orders` and `POST /payments`.
- [x] Add `CorrelationId` middleware for all APIs.
- [x] Add exception/problem logging middleware.
- [x] Add HTTP client for `LoggerService` with fallback.
- [x] Move `LoggerService` from in-memory to persistent storage.
- [x] Add OpenTelemetry traces/metrics/log correlation.

## Tests
- [ ] `CorrelationId` propagates through service call chain.
- [ ] Endpoint and error logs reach `LoggerService`.
- [ ] Cache invalidation works after product/promotion updates.
- [ ] Repeated request with same idempotency key does not create duplicates.

## Acceptance Criteria
- [ ] 100% endpoints have observability baseline.
- [ ] Cache provides measurable latency improvement.
