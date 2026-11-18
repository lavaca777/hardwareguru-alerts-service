# hardwareguru-alerts-service

Microservicio de **Alertas** de HardwareGuru. Consume de forma asíncrona los eventos `BuildGuardada` publicados en Kafka, audita el consumo eléctrico de cada build y genera **alertas de riesgo persistentes** cuando la fuente de poder está al límite.

> Proyecto universitario · Curso de Microservicios · Repo 6 de 8

## Estado actual

Implementado en `001-alert-core` y `002-kafka-ingestion`:

- Regla estricta `> 85%` con comparación decimal.
- Entidad `Alert`, PostgreSQL con EF Core y migración inicial.
- Idempotencia por `eventId` con índice único.
- API `GET /alerts?userId=`, `PATCH /alerts/{id}/read` y `GET /healthz`.
- Consumidor Kafka con commit manual, retry y rewind ante fallos.
- 62 tests, 63.8% de cobertura de líneas y 79.2% de ramas.
- Flujo Kafka → PostgreSQL verificado con un broker real: creado, duplicado y no-alerta.

Pendiente: Docker/Compose/CI (`003-runtime-and-delivery`) e integración con el API Gateway.

La planificación completa está en [`spec/`](spec/) y las convenciones en [`agent.md`](agent.md).

## Tecnologías

- C# 12 / .NET 8 (Web API + Worker Service)
- Apache Kafka (modo KRaft, sin Zookeeper) + Confluent.Kafka
- PostgreSQL 16 + Entity Framework Core + Npgsql
- xUnit + Moq + `WebApplicationFactory`
- Docker + GitHub Actions — pendiente

## ¿Qué hace?

1. Escucha el tópico `builds_guardadas` cuando `Kafka__Enabled=true`.
2. Calcula `porcentajeUso = consumoEstimadoW / potenciaFuenteW * 100`.
3. Si `porcentajeUso > 85%`, guarda una **Alerta de Riesgo** sugiriendo *undervolt* o cambiar de fuente.
4. Expone una API REST para que el API Gateway (BFF) consulte y marque alertas como leídas.

## Arquitectura

```text
Kafka (builds_guardadas) ──► Consumer ──► Regla de negocio (>85%) ──► PostgreSQL (alerts)
                                                                          ▲
API Gateway (BFF) ──── GET /alerts, PATCH /alerts/{id}/read ─────────────┘
```

Los puertos locales previstos son:

| Servicio | Puerto |
|---|---|
| API de alertas | `5001` |
| PostgreSQL | `5436` |
| Kafka | `9092` |

## Contrato del evento

El esquema de referencia está en [`spec/constitution/contracts.md`](spec/constitution/contracts.md). El repo `hardwareguru-infra` todavía no existe; este es el contrato provisional:

```json
{
  "eventId": "4bb3c2e0-7a1e-4e6a-bc5b-1f86f8f7a7a2",
  "userId": "u1",
  "buildId": "b2",
  "buildName": "PC Gamer 1440p",
  "consumoEstimadoW": 504,
  "potenciaFuenteW": 500,
  "ocurridoEn": "2026-11-17T20:00:00Z"
}
```

## Idempotencia

El campo `eventId` tiene restricción **`UNIQUE`** en la tabla `alerts`. Si el mismo evento se entrega dos veces, la segunda inserción es ignorada y **no se duplica la alerta**.

## API REST

Base local: `http://localhost:5001`

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/alerts?userId={id}` | Lista las alertas del usuario, más recientes primero |
| `PATCH` | `/alerts/{id}/read` | Marca una alerta como leída; es idempotente |
| `GET` | `/healthz` | Healthcheck |

Ejemplo de alerta:

```json
{
  "id": "8f14e45f-ceea-4e8f-9d42-9c1f4b2a7c10",
  "buildId": "b2",
  "buildName": "PC Gamer 1440p",
  "porcentajeUso": 101,
  "mensaje": "Tu fuente de 500 W está al 101% de uso con esta build \"PC Gamer 1440p\".",
  "sugerencia": "Haz undervolt o cambia a una fuente de mayor potencia.",
  "leida": false,
  "creadaEn": "2026-11-17T20:00:01Z"
}
```

El servicio es interno: el gateway es el único cliente y ya validó el JWT. No implementa autenticación propia.

## Variables de entorno

| Variable | Ejemplo | Estado |
|---|---|---|
| `ConnectionStrings__Postgres` | `Host=localhost;Port=5436;Database=hardwareguru_alerts;Username=postgres;Password=postgres` | Implementada |
| `ASPNETCORE_URLS` | `http://localhost:5001` | Implementada por ASP.NET Core |
| `Kafka__Enabled` | `true` | Deshabilitado por defecto; activa el consumidor |
| `Kafka__BootstrapServers` | `127.0.0.1:9092` | Implementada; en Docker usar `kafka:9092` |
| `Kafka__Topic` | `builds_guardadas` | Implementada |
| `Kafka__GroupId` | `alerts-service` | Implementada |
| `Kafka__AllowAutoCreateTopics` | `false` | Implementada; usar `true` solo en desarrollo |
| `Kafka__MaxProcessingRetries` | `3` | Implementada |
| `Kafka__RetryDelayMs` | `1000` | Implementada |
| `Kafka__ReconnectDelayMs` | `5000` | Implementada |

## Cómo ejecutarlo

**Requisitos:** .NET 8 SDK y un PostgreSQL en el puerto `5436` con la base `hardwareguru_alerts`.

```bash
# 1. Aplicar la migración
dotnet ef database update \
  --project src/Alerts.Infrastructure \
  --startup-project src/Alerts.Api

# 2. Ejecutar la API
dotnet run --project src/Alerts.Api
```

La conexión por defecto está en `src/Alerts.Api/appsettings.json`; se puede sobrescribir con `ConnectionStrings__Postgres`.

### Activar Kafka

El consumidor está deshabilitado por defecto para que la API y los tests puedan correr sin un broker. Con Kafka en `127.0.0.1:9092`:

```powershell
$env:Kafka__Enabled = "true"
$env:Kafka__AllowAutoCreateTopics = "true"  # solo desarrollo
dotnet run --project src/Alerts.Api
```

En producción o en Docker se debe crear el tópico `builds_guardadas` explícitamente y dejar `AllowAutoCreateTopics` en `false`.

Ejemplo de evento para probar el flujo:

```powershell
$event = @'
{"eventId":"4bb3c2e0-7a1e-4e6a-bc5b-1f86f8f7a7a2","userId":"u1","buildId":"b2","buildName":"PC Gamer 1440p","consumoEstimadoW":504,"potenciaFuenteW":500,"ocurridoEn":"2026-11-17T20:00:00Z"}
'@
$event | docker exec -i <contenedor-kafka> /opt/kafka/bin/kafka-console-producer.sh --bootstrap-server localhost:9092 --topic builds_guardadas
```

Docker y Compose se agregan en `003-runtime-and-delivery`.

## Tests

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Resultado actual: **62 tests**, 63.8% de líneas y 79.2% de ramas. La meta del proyecto es **≥ 60%**.

## Estructura

```text
hardwareguru-alerts-service/
├── agent.md
├── spec/                       # SDD: constitución, contratos y features
├── src/
│   ├── Alerts.Api/             # Controllers, DTOs y host
│   ├── Alerts.Application/     # Casos de uso e interfaces
│   ├── Alerts.Infrastructure/  # EF Core, repositorio, migraciones y Kafka
│   └── Alerts.Domain/          # Entidad y regla de negocio
├── tests/
│   └── Alerts.Tests/
├── global.json
└── Alerts.sln
```

## Checklist de avance

- [x] Regla de negocio (> 85%)
- [x] Idempotencia con `eventId` UNIQUE
- [x] API REST (`GET /alerts`, `PATCH /alerts/{id}/read`, `GET /healthz`)
- [x] Tests con cobertura ≥ 60%
- [x] Consumidor Kafka (modo KRaft), commit manual y retry
- [ ] Dockerfile y compose local
- [ ] CI/CD (GitHub Actions)

## Equipo

Responsable: Integrante 3
