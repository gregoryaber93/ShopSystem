# PR7 - Event Routing Matrix

## Jak korzystac z dokumentu
- Jedna linia = jeden event wersjonowany.
- `RabbitMQ` oznacza kanal workflow (szybka reakcja, retry, DLQ).
- `Kafka` oznacza kanal stream/replay/analityka.
- `PartitionKey` musi zachowac semantyke kolejnosci dla agregatu.

## Matryca

| Event | Producer | RabbitMQ | Kafka | PartitionKey | Consumerzy (workflow) | Consumerzy (stream) | Uwagi |
|---|---|---|---|---|---|---|---|
| UserCreated.v1 | AuthService | TAK | OPCJONALNIE | UserId | UserService | Dashboard/Logger (opcjonalnie) | Provisioning profilu |
| OrderPlaced.v1 | OrderService | TAK | TAK | OrderId lub UserId | PromotionService (opcjonalnie), Payment orchestration | DashboardService, LoggerService | Event hybrydowy |
| PaymentAuthorized.v1 | PaymantService | TAK | TAK | OrderId | OrderService | DashboardService, LoggerService | Aktualizacja statusu zamowienia |
| PaymentFailed.v1 | PaymantService | TAK | TAK | OrderId | OrderService | DashboardService, LoggerService | Workflow kompensacyjny |
| PointsEarned.v1 | PromotionService | OPCJONALNIE | TAK | UserId | (wg potrzeb) | DashboardService, LoggerService | Ledger loyalty |
| PointsSpent.v1 | PromotionService | OPCJONALNIE | TAK | UserId | (wg potrzeb) | DashboardService, LoggerService | Ledger loyalty |
| LoyaltyProfileUpdated.v1 | PromotionService | OPCJONALNIE | TAK | UserId | (wg potrzeb) | DashboardService, LoggerService | Read model + analityka |

## Konwencje techniczne
- Rabbit exchange: `<ServiceName>.exchange`
- Rabbit routing key: `<domain>.<event>.v1` albo utrzymane aktualne `<eventType>.ToLowerInvariant()` (wymagana spojnosc)
- Kafka topic: `<prefix>.<eventname>.v1`
- Headers minimum:
  - `eventType`
  - `eventVersion`
  - `occurredOnUtc`
  - `correlationId`
  - `causationId` (opcjonalnie)

## Decyzje do zatwierdzenia
- [x] `OrderPlaced.v1` zostaje eventem hybrydowym (Rabbit + Kafka).
- [x] `UserCreated.v1` nie trafia teraz do Kafka (tylko Rabbit workflow provisioning).
- [x] Loyalty events pozostaja Kafka-first; Rabbit tylko opcjonalnie przy wymaganiu natychmiastowego workflow.

## Zweryfikowane sciezki Stage 7
- RabbitMQ workflow:
  - `PaymantService` -> `OrderService` (`PaymentAuthorized.v1`, `PaymentFailed.v1`) z `ack/nack`, `requeue` i idempotencja po `EventId`.
  - `AuthService` -> `UserService` (`UserCreated.v1`) jako workflow provisioning.
- Kafka stream/analytics:
  - Publisherzy `OrderService` i `PaymantService` publikuja z kluczem `PartitionKey` i naglowkami (`eventType`, `eventVersion`, `occurredOnUtc`, `correlationId`).
  - `DashboardService` konsumuje do projekcji: `orders_projection`, `payments_projection`, `loyalty_projection`.
  - `LoggerService` konsumuje zdarzenia do centralnego audytu.
  - `group.id` rozdzielone: `dashboard-projections-v1`, `logger-audit-v1`.

## Review checklist
- [ ] Kazdy event ma jawnie wybrany kanal i uzasadnienie.
- [ ] Kazdy event ma okreslony partition key.
- [ ] Konsument workflow ma idempotencje po EventId.
- [ ] Konsument stream ma strategy replay (offset reset/checkpointing).
