# 001 — Tareas

- [x] Crear solución y proyectos de dominio, aplicación, infraestructura, API y tests.
- [x] Crear `Alert` y `BuildSavedEvent` con fechas UTC.
- [x] Implementar la regla estricta `> 85%` y el cálculo del porcentaje redondeado.
- [x] Implementar la generación de `mensaje` y `sugerencia`.
- [x] Definir `IAlertRepository` y los casos de uso de procesar, listar y marcar como leída.
- [x] Configurar EF Core, DbContext, entidad e índice único de `eventId`.
- [x] Crear la migración inicial y documentar cómo aplicarla.
- [x] Implementar el repositorio con manejo de duplicados por restricción única.
- [x] Crear `GET /alerts?userId=`, `PATCH /alerts/{id}/read` y `GET /healthz`.
- [x] Configurar `ProblemDetails` para 400 y 404.
- [x] Escribir tests de la regla: 85.0%, 85.1%, fuente 0 y redondeo.
- [x] Escribir tests de idempotencia, filtrado por usuario y orden.
- [x] Escribir tests HTTP de la API y del healthcheck.
- [x] Verificar cobertura ≥ 60% del módulo.
