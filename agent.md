# HardwareGuru — Alerts Service

Microservicio interno que consume eventos de builds guardadas y genera alertas persistentes cuando el consumo supera el umbral seguro de la fuente. No autentica usuarios, no gestiona builds y no genera simulaciones.

## Stack

- **Lenguaje:** C# 12 / .NET 8
- **Framework / runtime:** ASP.NET Core Web API + `BackgroundService`
- **Base de datos:** PostgreSQL 16 + EF Core 8 + Npgsql
- **Mensajería:** Apache Kafka en modo KRaft + `Confluent.Kafka`
- **Tests:** xUnit, Moq, `WebApplicationFactory`, Testcontainers para PostgreSQL
- **Entrega:** Docker multi-stage + GitHub Actions

## Comandos

- `dotnet run --project src/Alerts.Api` — arranca API y consumidor en local
- `dotnet test` — ejecuta los tests
- `dotnet test --collect:"XPlat Code Coverage"` — tests con cobertura
- `dotnet format --verify-no-changes` — revisa el estilo
- `dotnet build` — compila la solución
- `docker compose up -d` — levanta PostgreSQL, Kafka y el servicio
- `docker compose down` — detiene el entorno local

## Estructura del proyecto

- `spec/` — especificaciones SDD; leer antes de codificar
- `spec/constitution/` — misión, stack, contratos y roadmap
- `spec/features/` — una carpeta por feature con `spec.md`, `plan.md` y `tasks.md`
- `src/Alerts.Domain/` — entidad, regla de negocio y contratos internos
- `src/Alerts.Application/` — casos de uso e interfaces
- `src/Alerts.Infrastructure/` — EF Core, migraciones, repositorio y consumidor Kafka
- `src/Alerts.Api/` — host ASP.NET Core, endpoints, healthcheck y DI
- `tests/Alerts.Tests/` — tests unitarios y de integración
- `docker-compose.yml` - PostgreSQL + Kafka + servicio para desarrollo local
- `Dockerfile` - imagen multi-stage de la API
- `.github/workflows/ci.yml` - build, tests, cobertura y smoke test Docker

## Convenciones

- Identificadores de C# en inglés; contratos JSON y documentación orientados al frontend en español.
- La regla de negocio vive en `Alerts.Application`/`Alerts.Domain`, nunca en controllers ni en el consumidor.
- Fechas siempre UTC y en ISO 8601.
- El servicio es interno: el gateway es su único cliente y ya validó el JWT.
- Los errores HTTP usan `ProblemDetails`.
- Los logs usan `ILogger`; nunca `Console.WriteLine`.
- La configuración se lee de variables de entorno; nunca se versionan secretos.
- `eventId` es la clave de idempotencia: un evento repetido no crea otra alerta.
- Los umbrales se comparan con decimales antes de redondear el porcentaje mostrado.
- No se expone el servicio a Internet ni se implementa autenticación aquí.

## No hagas

- No instales dependencias sin avisar primero.
- No implementes login, JWT, CRUD de builds, catálogo ni chat en este repo.
- No llames al gateway, al catálogo ni al simulador desde este servicio.
- No cambies el contrato del evento ni los campos JSON de la API sin coordinar.
- No uses `EnsureCreated` en producción; usa migraciones EF Core.
- No hagas commit de offsets de Kafka antes de persistir la alerta.
- El consumidor Kafka se activa con `Kafka__Enabled=true`; por defecto está apagado para no exigir un broker en tests o en la API sola.
- No loguees cadenas de conexión, tokens ni datos personales.
- No uses `DateTime.Now`; usa `DateTimeOffset.UtcNow` o el timestamp del evento.
- No rompas la idempotencia ni la restricción única de `eventId`.

## Flujo de trabajo

- Lee siempre `spec/features/[feature]/spec.md`, `plan.md` y `tasks.md` antes de escribir código.
- Sigue el orden de `spec/constitution/roadmap.md`; no empieces una feature si sus dependencias no están completas.
- Una feature a la vez. Antes de una tarea no trivial, propón el plan y espera confirmación.
- Marca las tareas completadas con `[x]` en el `tasks.md` correspondiente.
- Al terminar una feature: ejecuta tests, cobertura, build y lint; actualiza el README si cambió el uso.
- Si no estás seguro al 80%, pregunta; no inventes contratos ni comportamiento.

## Documentación

- `spec/constitution/mission.md` — qué hace el servicio y qué queda fuera.
- `spec/constitution/tech-stack.md` — tecnologías, puertos y restricciones.
- `spec/constitution/contracts.md` — evento Kafka y API REST compartida con el gateway.
- `spec/constitution/roadmap.md` — orden de features y criterio para avanzar.
- `spec/features/001-alert-core/` — regla, persistencia y API.
- `spec/features/002-kafka-ingestion/` — consumidor e idempotencia operativa.
- `spec/features/003-runtime-and-delivery/` — Docker, compose, CI y documentación.
