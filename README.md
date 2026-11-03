# hardwareguru-alerts-service

Microservicio de **Alertas** de HardwareGuru. Consume de forma asíncrona los eventos `BuildGuardada` publicados en Kafka, audita el consumo eléctrico de cada build y genera **alertas de riesgo persistentes** cuando la fuente de poder está al límite.

> Proyecto universitario · Curso de Microservicios · Repo 5 de 7

## Tecnologías

- C# / .NET 8 (Worker Service + Web API)
- Apache Kafka (modo KRaft, sin Zookeeper)
- PostgreSQL (base de datos propia)
- Entity Framework Core + Npgsql
- xUnit + Moq
- Docker + GitHub Actions

## ¿Qué hace?

1. Escucha el tópico `builds_guardadas`.
2. Calcula `porcentajeUso = consumoEstimadoW / potenciaFuenteW * 100`.
3. Si `porcentajeUso > 85%`, guarda una **Alerta de Riesgo** sugiriendo *undervolt* o cambiar de fuente.
4. Expone una API REST para que el API Gateway (BFF) consulte y marque alertas como leídas.

## Arquitectura

```
Kafka (builds_guardadas) ──► Consumer ──► Regla de negocio (>85%) ──► PostgreSQL (alerts)
                                                                          ▲
API Gateway (BFF) ──── GET /alerts, PATCH /alerts/{id}/read ──────────────┘
```

## Contrato del evento

El esquema oficial vive en `hardwareguru-infra/events/`. Campos mínimos esperados:

```json
{
  "eventId": "uuid",
  "userId": "string",
  "buildId": "string",
  "buildName": "string",
  "consumoEstimadoW": 420,
  "potenciaFuenteW": 500,
  "ocurridoEn": "2026-01-01T12:00:00Z"
}
```

> Ajustar a lo que se acuerde en el repo de infraestructura.

## Idempotencia

El campo `eventId` de Kafka tiene restricción **`UNIQUE`** en la tabla de alertas. Si el mismo evento se entrega dos veces, la segunda inserción es ignorada y **no se duplica la alerta**.

## API REST

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/alerts?userId={id}` | Lista las alertas del usuario |
| `PATCH` | `/alerts/{id}/read` | Marca una alerta como leída |

## Variables de entorno

| Variable | Ejemplo | Descripción |
|---|---|---|
| `ConnectionStrings__Postgres` | `Host=localhost;Port=5432;Database=alerts;Username=...;Password=...` | Conexión a PostgreSQL |
| `Kafka__BootstrapServers` | `localhost:9092` | Broker Kafka |
| `Kafka__Topic` | `builds_guardadas` | Tópico a consumir |
| `Kafka__GroupId` | `alerts-service` | Consumer group |
| `Alerts__UmbralUsoPorcentaje` | `85` | Umbral de riesgo |
| `ASPNETCORE_URLS` | `http://+:<PUERTO>` | Puerto de la API |

## Cómo ejecutarlo

**Requisitos:** .NET 8 SDK, Docker.

```bash
# 1. Levantar Kafka y PostgreSQL desde el repo de infraestructura
cd ../hardwareguru-infra/compose
docker compose up -d

# 2. Aplicar migraciones
dotnet ef database update

# 3. Ejecutar el servicio
dotnet run
```

Con Docker:

```bash
docker build -t hardwareguru-alerts-service .
docker run --env-file .env -p <PUERTO>:<PUERTO> hardwareguru-alerts-service
```

## Tests

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Meta: **≥ 60% de cobertura** (xUnit + Moq).

## Estructura sugerida

```
hardwareguru-alerts-service/
├── src/
│   ├── Alerts.Api/            # Controllers REST
│   ├── Alerts.Application/    # Reglas de negocio
│   ├── Alerts.Infrastructure/ # EF Core, Kafka consumer
│   └── Alerts.Domain/         # Entidades
├── tests/
│   └── Alerts.Tests/
├── Dockerfile
└── .github/workflows/ci.yml
```

## Checklist de avance

- [ ] Consumidor Kafka (modo KRaft)
- [ ] Regla de negocio (> 85%)
- [ ] Idempotencia con `eventId` UNIQUE
- [ ] API REST (`GET /alerts`, `PATCH /alerts/{id}/read`)
- [ ] Tests con 60% de cobertura
- [ ] Dockerfile
- [ ] CI/CD (GitHub Actions)

## Equipo

Responsable: Integrante 3
