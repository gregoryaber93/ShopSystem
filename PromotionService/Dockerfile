FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/API/PromotionService.Api.csproj", "src/API/"]
COPY ["src/Core/Application/PromotionService.Application.csproj", "src/Core/Application/"]
COPY ["src/Core/Contracts/PromotionService.Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Domain/PromotionService.Domain.csproj", "src/Core/Domain/"]
COPY ["src/Infrastructure/PromotionService.Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/API/PromotionService.Api.csproj"

COPY . .
WORKDIR "/src/src/API"
RUN dotnet publish "PromotionService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PromotionService.Api.dll"]
