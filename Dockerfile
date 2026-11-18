FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Alerts.sln global.json ./
COPY src/Alerts.Domain/Alerts.Domain.csproj src/Alerts.Domain/
COPY src/Alerts.Application/Alerts.Application.csproj src/Alerts.Application/
COPY src/Alerts.Infrastructure/Alerts.Infrastructure.csproj src/Alerts.Infrastructure/
COPY src/Alerts.Api/Alerts.Api.csproj src/Alerts.Api/

RUN dotnet restore src/Alerts.Api/Alerts.Api.csproj

COPY src/ src/
RUN dotnet publish src/Alerts.Api/Alerts.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=5 \
    CMD curl -fsS http://localhost:8080/healthz || exit 1

USER app
ENTRYPOINT ["dotnet", "Alerts.Api.dll"]
