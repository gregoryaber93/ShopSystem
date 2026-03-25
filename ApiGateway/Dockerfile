FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/API/ApiGateway.Api.csproj", "src/API/"]

RUN dotnet restore "src/API/ApiGateway.Api.csproj"

COPY . .
WORKDIR "/src/src/API"
RUN dotnet publish "ApiGateway.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.Api.dll"]