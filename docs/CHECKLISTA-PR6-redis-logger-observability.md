# CHECKLIST PR6 - Redis, Logger, Observability

## Goal
Add caching, centralized logging, and full traceability.

## Implementation Tasks
- [ ] Add Redis (`IDistributedCache`) in key services.
- [ ] Cache: products per shop.
- [ ] Cache: active promotions.
- [ ] Cache: user-specific eligibility.
- [ ] Add idempotency keys for `POST /orders` and `POST /payments`.
- [ ] Add `CorrelationId` middleware for all APIs.
- [ ] Add exception/problem logging middleware.
- [ ] Add HTTP client for `LoggerService` with fallback.
- [ ] Move `LoggerService` from in-memory to persistent storage.
- [ ] Add OpenTelemetry traces/metrics/log correlation.

## Tests
- [ ] `CorrelationId` propagates through service call chain.
- [ ] Endpoint and error logs reach `LoggerService`.
- [ ] Cache invalidation works after product/promotion updates.
- [ ] Repeated request with same idempotency key does not create duplicates.

## Acceptance Criteria
- [ ] 100% endpoints have observability baseline.
- [ ] Cache provides measurable latency improvement.
