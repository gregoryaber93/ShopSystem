FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/API/DashboardService.Api.csproj", "src/API/"]
COPY ["src/Core/Application/DashboardService.Application.csproj", "src/Core/Application/"]
COPY ["src/Core/Contracts/DashboardService.Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Domain/DashboardService.Domain.csproj", "src/Core/Domain/"]
COPY ["src/Infrastructure/DashboardService.Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/API/DashboardService.Api.csproj"

COPY . .
WORKDIR "/src/src/API"
RUN dotnet publish "DashboardService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DashboardService.Api.dll"]

