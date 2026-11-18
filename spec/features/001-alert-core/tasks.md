# 001 — Tareas

- [ ] Crear solución y proyectos de dominio, aplicación, infraestructura, API y tests.
- [ ] Crear `Alert` y `BuildSavedEvent` con fechas UTC.
- [ ] Implementar la regla estricta `> 85%` y el cálculo del porcentaje redondeado.
- [ ] Implementar la generación de `mensaje` y `sugerencia`.
- [ ] Definir `IAlertRepository` y los casos de uso de procesar, listar y marcar como leída.
- [ ] Configurar EF Core, DbContext, entidad e índice único de `eventId`.
- [ ] Crear la migración inicial y documentar cómo aplicarla.
- [ ] Implementar el repositorio con manejo de duplicados por restricción única.
- [ ] Crear `GET /alerts?userId=`, `PATCH /alerts/{id}/read` y `GET /healthz`.
- [ ] Configurar `ProblemDetails` para 400 y 404.
- [ ] Escribir tests de la regla: 85.0%, 85.1%, fuente 0 y redondeo.
- [ ] Escribir tests de idempotencia, filtrado por usuario y orden.
- [ ] Escribir tests HTTP de la API y del healthcheck.
- [ ] Verificar cobertura ≥ 60% del módulo.
