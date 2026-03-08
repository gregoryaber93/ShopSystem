# STATUS-PR2

## Objective
Harden order calculations and implement correct promotion application logic.

## Active Checklist
- `docsEng/CHECKLISTA-PR2-order-and-promotions.md`

## Session Scope
- `OrderService`
- `PromotionService`

## Key Files
- `OrderService/src/Core/Application/Features/Orders/Commands/PlaceOrder/PlaceOrderCommandHandler.cs`
- `PromotionService/src/Core/Contracts/Dtos/PromotionDto.cs`
- `PromotionService/src/Core/Application/Features/Promotions/Commands/AddPromotion/AddPromotionCommandHandler.cs`
- `PromotionService/src/Core/Application/Features/Promotions/Commands/UpdatePromotion/UpdatePromotionCommandHandler.cs`

## Mandatory Outcomes
- Backend-only final order price.
- Valid promotion rules for product and loyalty paths.
- Tests covering manipulation and eligibility scenarios.

## Next Step
Implement backend pricing source-of-truth and add regression tests.
