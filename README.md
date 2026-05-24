# ShopSystem

ShopSystem is a .NET 9 microservices-based e-commerce backend.  
It contains bounded-context services, an API gateway, message brokers, Redis, and PostgreSQL databases for local development.

## Architecture Overview

Main services in this repository:

- `ApiGateway` — public entry point for API traffic
- `AuthService` — authentication and identity workflows
- `UserService` — user management
- `ShopService` — orchestration across product, promotion, and order domains
- `ProductService` — product catalog domain
- `PromotionService` — promotions and discount rules
- `OrderService` — order lifecycle and checkout flow
- `PaymantService` — payment processing (name kept as-is in repository)
- `LoggerService` — audit/log ingestion from event streams
- `DashboardService` — read-model/projection updates for dashboards

Typical service layout:

- `src/API`
- `src/Core` (`Domain`, `Application`, `Contracts`)
- `src/Infrastructure`

## Repository Structure

```text
/ApiGateway
/AuthService
/DashboardService
/LoggerService
/OrderService
/PaymantService
/ProductService
/PromotionService
/ShopService
/UserService
/docker-compose.yml
/docker-compose.db.yml
/docker-compose.brokers.yml
```

## Prerequisites

- .NET SDK 9.0
- Docker and Docker Compose

## Local Run (Docker Compose)

From repository root, start everything:

```bash
docker compose up --build
```

This brings up all APIs + infrastructure on the shared `shopsystem-network`.

### Optional compose variants

- Infra only (RabbitMQ, Redpanda, Redis):  
  `docker compose -f docker-compose.brokers.yml up -d`
- Databases + brokers:  
  `docker compose -f docker-compose.db.yml up -d`

### Common endpoints

- API Gateway: `http://localhost:5280`
- AuthService: `http://localhost:5294`
- UserService: `http://localhost:5300`
- ShopService: `http://localhost:5292`
- PromotionService: `http://localhost:5293`
- ProductService: `http://localhost:5295`
- OrderService: `http://localhost:5297`
- PaymantService: `http://localhost:5298`
- LoggerService: `http://localhost:5299`
- DashboardService: `http://localhost:5301`
- RabbitMQ AMQP: `localhost:5672`
- RabbitMQ Management: `http://localhost:15672`
- Redpanda Kafka: `localhost:9092`
- Redis: `localhost:6379`

## Build and Test

Build all service solutions:

```bash
find . -name '*.sln' | sort | while read -r sln; do
  echo "=== BUILD $sln ==="
  dotnet build "$sln"
done
```

Run all test projects:

```bash
set -euo pipefail
find . -name '*Tests*.csproj' | sort | while read -r testproj; do
  echo "=== TEST $testproj ==="
  dotnet test "$testproj"
done
```

## Notes

- Services communicate via HTTP/gRPC and asynchronous broker messaging.
- PostgreSQL is provisioned per service in Docker Compose.
- RabbitMQ and Redpanda are both used for messaging/event flows.
