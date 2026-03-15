# CHECKLISTA PR7 - RabbitMQ + Kafka (podzial odpowiedzialnosci)

## Cel
Wdrozyc oba brokery w jednym systemie, ale z jasnym podzialem odpowiedzialnosci:
- RabbitMQ: workflow/integration commands (krotki czas reakcji, retry, DLQ).
- Kafka: strumien zdarzen domenowych (retencja, replay, analityka, niezalezne consumer groups).

## Zakres
- Serwisy transakcyjne: AuthService, UserService, OrderService, PaymantService, PromotionService.
- Serwisy odczytowe/obserwowalnosc: DashboardService, LoggerService.

## Etap 0 - Baseline i konfiguracja
- [x] Potwierdz, ze brokers stack dziala: `docker compose -f docker-compose.brokers.yml up -d`.
- [ ] Ujednolic nazewnictwo kanalow:
  - Rabbit exchange per service, routing key per workflow.
  - Kafka topic: `<prefix>.<eventname>.v1`.
- [x] Dla OrderService i PaymantService dodaj brakujace envy brokerow w docker-compose:
  - `MessageBrokers__RabbitMq__Host/Port/Username/Password/Exchange/DeadLetterExchange`
  - `MessageBrokers__Kafka__BootstrapServers/TopicPrefix`
- [x] Dodaj dokument mapy zdarzen `docs/PR7-event-routing-matrix.md` (wg Etapu 1).

## Etap 1 - Event Routing Matrix (decyzja projektowa)
Dla kazdego eventu okresl docelowy kanal i klucz partycjonowania.

Minimalna macierz:
- `UserCreated.v1`
  - RabbitMQ: TAK (Auth -> User provisioning)
  - Kafka: OPCJONALNIE (audit stream)
  - PartitionKey: `UserId`
- `OrderPlaced.v1`
  - RabbitMQ: TAK (workflow: downstream reactions)
  - Kafka: TAK (analityka/replay)
  - PartitionKey: `UserId` lub `OrderId`
- `PaymentAuthorized.v1`
  - RabbitMQ: TAK (workflow update statusu zamowienia)
  - Kafka: TAK (audit, BI)
  - PartitionKey: `OrderId`
- `PaymentFailed.v1`
  - RabbitMQ: TAK (workflow kompensacyjny)
  - Kafka: TAK (audit, alerting)
  - PartitionKey: `OrderId`
- `PointsEarned.v1`, `PointsSpent.v1`, `LoyaltyProfileUpdated.v1`
  - RabbitMQ: opcjonalnie (jesli natychmiastowa akcja workflow)
  - Kafka: TAK (ledger/replay/analityka)
  - PartitionKey: `UserId`

## Etap 2 - RabbitMQ jako kanal workflow
### Auth + User (juz czesciowo gotowe)
- [x] Zachowaj obecny flow `UserCreated` z outbox Auth i consumerem w User.
- [ ] Doloz polityke retry i DLQ testowana scenariuszami awarii.

### Payment -> Order (nowa implementacja)
- [x] Dodaj worker konsumencki Rabbit w OrderService:
  - `OrderService/src/Infrastructure/Messaging/PaymentAuthorizedConsumerWorker.cs`
- [x] Obsluz eventy:
  - `PaymentAuthorized` -> status zamowienia `Paid`.
  - `PaymentFailed` -> status zamowienia `PaymentFailed` lub uruchom kompensacje.
- [x] Dodaj idempotent consumer table check po `EventId`.
- [x] Dodaj explicit ack/nack + requeue policy.

### Order -> Promotion (opcjonalny workflow)
- [ ] Jezeli chcesz natychmiastowej aktualizacji profilu lojalnosci:
  - dodaj Rabbit consumer w PromotionService dla `OrderPlaced`.
- [ ] Aktualizacja profilu ma byc idempotentna i oparta o `EventId`.

## Etap 3 - Kafka jako kanal event stream
### Publisher side
- [x] Zostaw publikacje Kafka z outboxu w OrderService i PaymantService.
- [x] Upewnij sie, ze klucz wiadomosci = `PartitionKey`.
- [x] Dopisz naglowki metadanych:
  - `eventType`, `eventVersion`, `occurredOnUtc`, `correlationId`.

### Consumer side (NOWE)
- [x] DashboardService: utworz consumer(y) Kafka do projekcji read modeli:
  - `orders_projection`
  - `payments_projection`
  - `loyalty_projection`
- [x] LoggerService: dodaj opcjonalny consumer Kafka do centralnego audytu zdarzen domenowych.
- [x] Uzyj osobnych `group.id` dla kazdego bounded contextu (dashboard, logger, analytics).
- [x] Dodaj checkpointing offsetow i mechanizm restart/recovery.

## Etap 4 - Uporzadkowanie granicy Rabbit vs Kafka
Wariant edukacyjny rekomendowany w 2 krokach:

1) Faza A (latwiejsza): dual publish dla wybranych eventow
- Rabbit do workflow.
- Kafka do streamingu i analityki.

2) Faza B (docelowa): swiadome rozdzielenie
- Eventy stricte workflow tylko Rabbit.
- Eventy stricte domenowe tylko Kafka.
- Eventy hybrydowe (np. `OrderPlaced`) moga zostac na obu, ale tylko z uzasadnieniem biznesowym.

## Etap 5 - Testy obowiazkowe
### RabbitMQ
- [ ] Retry i DLQ: wylacz konsumenta, wyslij event, potwierdz przejscie do DLQ.
- [x] Idempotencja: wyslij ten sam `EventId` 2x, efekt biznesowy tylko 1x.
- [ ] Recovery: po restarcie workera wiadomosci pending sa dalej obslugiwane.

### Kafka
- [ ] Ordering w partycji: eventy jednego `OrderId/UserId` przychodza we wlasciwej kolejnosci.
- [ ] Replay: odbuduj projekcje DashboardService od offsetu 0.
- [ ] Consumer group isolation: dashboard i logger konsumuje niezaleznie.

### End-to-end
- [ ] `OrderPlaced` -> `PaymentAuthorized` -> aktualizacja statusu zamowienia.
- [ ] Aktualizacja loyalty po zdarzeniach zamowien i/lub platnosci.
- [ ] Brak podwojnych skutkow mimo ponownej dostawy wiadomosci.

## Etap 6 - Obserwowalnosc i operacje
- [ ] Dodaj metryki:
  - outbox lag (czas od `CreatedAt` do publikacji),
  - consumer lag Kafka,
  - liczba retry Rabbit,
  - liczba wiadomosci DLQ.
- [ ] Dodaj logowanie strukturalne z `EventId`, `CorrelationId`, `EventType`.
- [ ] Przygotuj runbook awarii brokerow (restart, replay, opruznianie DLQ).

## Kryteria akceptacji PR7
- [x] Dla kazdego eventu istnieje jawna decyzja Rabbit/Kafka w macierzy.
- [ ] Workflow krytyczny dziala przez Rabbit z retry + DLQ + idempotencja.
- [ ] Strumien domenowy dziala przez Kafka i da sie go replayowac.
- [x] Dashboard/Logger potrafia konsumowac Kafka niezaleznie od workflow.
- [ ] Testy awaryjne i replay przechodza lokalnie i w CI.

## Sugerowana kolejnosc wdrozenia (2 sprinty)
Sprint 1:
- Etap 0, 1, 2 (Auth/User stabilizacja + Payment->Order workflow),
- testy Rabbit.

Sprint 2:
- Etap 3, 4, 6,
- consumerzy Kafka w Dashboard/Logger,
- replay i testy end-to-end.
