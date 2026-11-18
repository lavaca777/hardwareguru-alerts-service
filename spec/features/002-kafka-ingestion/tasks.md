# 002 — Tareas

- [x] Agregar `Confluent.Kafka` y las opciones de configuración.
- [x] Implementar `KafkaConsumerService` con grupo, tópico y auto-commit deshabilitado.
- [x] Deserializar y validar el evento `BuildGuardada`.
- [x] Invocar `ProcessBuildEvent` y manejar la idempotencia.
- [x] Confirmar offsets después del procesamiento o de un evento inválido.
- [x] Implementar reintentos para errores transitorios de persistencia.
- [x] Implementar shutdown limpio y logs con `ILogger`.
- [x] Escribir tests del handler para umbral, duplicado y evento inválido.
- [x] Escribir tests del comportamiento de commit/no commit.
- [x] Documentar y probar un productor de evento local.
- [x] Verificar cobertura ≥ 60% del módulo.
