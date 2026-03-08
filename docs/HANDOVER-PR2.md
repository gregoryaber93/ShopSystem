# HANDOVER-PR2

Work only on `docsEng/CHECKLISTA-PR2-order-and-promotions.md`.

## Complete Now
1. Backend-only order total calculation.
2. Promotion eligibility rules validation.
3. Tests for price manipulation and loyalty threshold.

## Allowed Files
- `OrderService/src/Core/Application/Features/Orders/Commands/PlaceOrder/PlaceOrderCommandHandler.cs`
- `PromotionService/src/Core/Contracts/Dtos/PromotionDto.cs`
- `PromotionService/src/Core/Application/Features/Promotions/Commands/AddPromotion/AddPromotionCommandHandler.cs`
- `PromotionService/src/Core/Application/Features/Promotions/Commands/UpdatePromotion/UpdatePromotionCommandHandler.cs`

## Out Of Scope
- gRPC server implementation.
- RabbitMQ/Kafka/Redis changes.

## Validation
- Build impacted solutions.
- Run unit/integration tests for Order/Promotion services.

## End Session
- Update `docsEng/CHECKLISTA-PR2-order-and-promotions.md`.
- Update `docsEng/STATUS-PR2.md`.
