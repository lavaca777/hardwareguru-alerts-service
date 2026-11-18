# 001 — Plan de implementación

## Pasos

1. Crear la solución y los proyectos `Alerts.Domain`, `Alerts.Application`, `Alerts.Infrastructure`, `Alerts.Api` y `tests/Alerts.Tests`.
2. Implementar `Alert`, `BuildSavedEvent`, la regla del umbral y la generación de mensaje/sugerencia.
3. Definir `IAlertRepository` y los casos de uso `ProcessBuildEvent`, `ListAlerts` y `MarkAsRead`.
4. Configurar EF Core con PostgreSQL, la entidad `Alert`, el índice único de `EventId` y la migración inicial.
5. Implementar el repositorio y el manejo del duplicado por restricción única.
6. Crear los endpoints REST, `ProblemDetails`, healthcheck y registro de dependencias.
7. Escribir tests unitarios de regla, casos de uso, repositorio y API.
8. Completar cobertura y actualizar el README con la API y la migración.

## Notas técnicas

- La regla debe ser una función pura y testeable sin EF ni HTTP.
- Usar `decimal` para la comparación y redondear solo el valor que se guarda/muestra.
- La restricción única es la última defensa de idempotencia; no alcanza con un `Any` previo.
- Usar `DateTimeOffset.UtcNow` para `CreadaEn`; no usar `DateTime.Now`.
- Mapear el JSON con los nombres exactos del frontend (`buildId`, `buildName`, `porcentajeUso`, `leida`, `creadaEn`).
- Los errores HTTP deben ser `ProblemDetails`; no devolver strings sueltos.
- La API no implementa JWT: el gateway es el único cliente y ya validó la sesión.
- La conexión se toma de `ConnectionStrings__Postgres`.
- No añadir paquetes fuera de EF Core/Npgsql, ASP.NET Core, xUnit/Moq y Testcontainers sin avisar. Testcontainers es opcional y requiere aprobación; si no se aprueba, usar un PostgreSQL local para el test de idempotencia.
