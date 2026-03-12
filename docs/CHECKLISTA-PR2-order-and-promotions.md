# CHECKLIST PR2 - Order and Promotions Correctness

## Goal
Harden order calculations and implement user-specific promotion rules.

## Implementation Tasks
- [x] `OrderService`: calculate final price on backend.
- [x] `OrderService`: fetch product data from `ProductService` (gRPC) instead of trusting request payload.
- [x] `PromotionService`: refine validation for `ProductDiscount` and `LoyaltyPoints`.
- [x] `PromotionService`: add/extend user-specific evaluation based on purchase history.
- [x] Add a read model for points/purchase history.

## Tests
- [ ] Changing request price does not change final order price.
- [ ] `ProductDiscount` applies only to selected products.
- [ ] `LoyaltyPoints` applies only after threshold is met.
- [ ] Promotion outside active time window is not applied.

## Acceptance Criteria
- [ ] Final price and discount are backend-only.
- [ ] Promotion rules are deterministic.
