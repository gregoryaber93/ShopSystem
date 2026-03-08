# PR2 - Order and Promotion Business Correctness

## Scope
- Harden order calculation.
- Implement user-specific promotion evaluation based on history.

## Required Changes

### OrderService
- File: `OrderService/src/Core/Application/Features/Orders/Commands/PlaceOrder/PlaceOrderCommandHandler.cs`
- Change:
  - Do not trust final price from client request.
  - Fetch current product data (price, existence, shop) from `ProductService` via gRPC.
  - Compute total price on backend only.

### PromotionService
- File: `PromotionService/src/Core/Contracts/Dtos/PromotionDto.cs`
- Change:
  - Keep and extend model for user-specific promotions.
- Handler files:
  - `PromotionService/src/Core/Application/Features/Promotions/Commands/AddPromotion/AddPromotionCommandHandler.cs`
  - `PromotionService/src/Core/Application/Features/Promotions/Commands/UpdatePromotion/UpdatePromotionCommandHandler.cs`
- Change:
  - Time window validation.
  - `RequiredPoints` validation.
  - Clear separation of promotion activation policies.

### Loyalty / purchase history
- Add a read model for user points or purchase history.
- Promotion evaluation should accept `userId` and cart context.

## Tests
- Client-side price manipulation does not change final order price.
- `ProductDiscount` applies only to selected products.
- `LoyaltyPoints` applies only when threshold is met.
- Inactive (time-window) promotion is not applied.

## DoD
- Final price and discount are backend-only.
- Promotion rules are deterministic and testable.
