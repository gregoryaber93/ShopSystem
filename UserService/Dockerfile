FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/API/UserService.Api.csproj", "src/API/"]
COPY ["src/Core/Application/UserService.Application.csproj", "src/Core/Application/"]
COPY ["src/Core/Contracts/UserService.Contracts.csproj", "src/Core/Contracts/"]
COPY ["src/Core/Domain/UserService.Domain.csproj", "src/Core/Domain/"]
COPY ["src/Infrastructure/UserService.Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/API/UserService.Api.csproj"

COPY . .
WORKDIR "/src/src/API"
RUN dotnet publish "UserService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
