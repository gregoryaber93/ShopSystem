# ShopSystem

ShopSystem is a .NET 9 microservices-based e-commerce backend. The repository contains multiple bounded services, an API Gateway, messaging infrastructure, and Docker Compose files for local development.

## Architecture overview

The system follows a modular microservice approach with separate services for core business areas:

- `ApiGateway` – entry gateway
- `AuthService` – authentication and identity workflows
- `UserService` – user management
- `ShopService` – shop orchestration and cross-service coordination
- `ProductService` – product domain
- `PromotionService` – promotions and discount rules
- `OrderService` – order lifecycle
- `PaymantService` – payment processing
- `LoggerService` – logging/audit ingestion
- `DashboardService` – projection/read-model style dashboard data

Each service is organized in layered projects (`API`, `Core` with `Domain/Application/Contracts`, and `Infrastructure`).

## Repository structure

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
- Docker + Docker Compose

## Run with Docker Compose

From repository root:

```bash
docker compose up --build
```

This starts all service APIs, PostgreSQL instances, RabbitMQ, and Redpanda on the shared `shopsystem-network`.

### Useful exposed ports

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
- RabbitMQ Management: `http://localhost:15672`
- Redpanda Kafka: `localhost:9092`

## Build and test

Build all solutions:

```bash
find . -name '*.sln' | sort | while read -r sln; do dotnet build "$sln"; done
```

Run test projects:

```bash
find . -name '*Tests*.csproj' | sort | while read -r testproj; do dotnet test "$testproj"; done
```

## Notes

- Service-to-service communication includes HTTP/gRPC and broker-based messaging.
- PostgreSQL databases are provisioned per service in Docker Compose.
