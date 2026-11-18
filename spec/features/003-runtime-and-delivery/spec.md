# 003 — Runtime and delivery

## Responsabilidad

Dejar el servicio ejecutable de forma reproducible en local y en CI: contenedores, variables de entorno, healthcheck, compose, pipeline y documentación. No agrega lógica de negocio nueva.

## Comportamiento

### Docker

- Imagen multi-stage:
  - build con `mcr.microsoft.com/dotnet/sdk:8.0`;
  - runtime con `mcr.microsoft.com/dotnet/aspnet:8.0`.
- La API escucha en `8080` dentro del contenedor y se publica en `5001`.
- Ejecutar como usuario no-root si la imagen base lo permite.
- Incluir healthcheck contra `/healthz`.

### Compose local

Servicios:

| Servicio | Puerto host | Uso |
|---|---|---|
| `postgres` | `5436` | Base `hardwareguru_alerts` |
| `kafka` | `9092` | Broker KRaft |
| `alerts-api` | `5001` | API + consumidor |

- PostgreSQL con volumen persistente y healthcheck.
- Kafka en modo KRaft, sin Zookeeper.
- Crear el tópico `builds_guardadas` al iniciar el entorno de desarrollo.
- La API espera PostgreSQL y Kafka saludables.
- Las variables se pasan por entorno; no se versionan secretos.
- `docker compose up --build` debe dejar el sistema listo.

### CI/CD

`.github/workflows/ci.yml` debe ejecutar:

1. `dotnet restore`
2. `dotnet format --verify-no-changes`
3. `dotnet build --no-restore`
4. `dotnet test --no-build --collect:"XPlat Code Coverage"`
5. Publicar el reporte de cobertura.
6. Build de la imagen Docker.

### Documentación

Actualizar `README.md` con:

- requisitos (.NET 8 SDK, Docker);
- comandos de arranque local;
- variables de entorno;
- ejemplo de evento `BuildGuardada`;
- producción de un evento de prueba;
- ejemplos de `GET /alerts` y `PATCH /alerts/{id}/read`;
- cómo correr tests y cobertura;
- nota de que la integración con gateway/frontend queda fuera de este repo.

## Criterios de aceptación

- [x] `docker compose up --build` levanta PostgreSQL, Kafka y la API.
- [x] `/healthz` responde `200`.
- [x] Un evento producido en Kafka genera una alerta consultable por la API.
- [x] Un evento duplicado no genera una segunda alerta.
- [x] `PATCH /alerts/{id}/read` funciona desde el contenedor.
- [x] `dotnet test` pasa y la cobertura es ≥ 60%.
- [x] El workflow de GitHub Actions valida build, tests y Docker.
- [x] La documentación permite arrancar sin conocimiento previo del repo.
