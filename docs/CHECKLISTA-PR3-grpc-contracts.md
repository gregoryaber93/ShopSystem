# CHECKLIST PR3 - gRPC Contracts and Integration

## Goal
Enable real gRPC communication between services and align contracts.

## Implementation Tasks
- [ ] Add gRPC server endpoints in `ProductService`.
- [ ] Add gRPC server endpoints in `PromotionService`.
- [ ] Add gRPC server endpoints in `OrderService`.
- [ ] Align `products.proto` with domain model.
- [ ] Align `promotions.proto` with domain model.
- [ ] Align `orders.proto` with domain model.
- [ ] Add service-to-service JWT auth.
- [ ] Add timeout/retry/circuit policies.

## Tests
- [ ] Contract tests for proto vs DTO.
- [ ] Integration tests for gRPC call chain.
- [ ] Inter-service auth tests.

## Acceptance Criteria
- [ ] gRPC works end-to-end.
- [ ] No contract/domain mismatch.
