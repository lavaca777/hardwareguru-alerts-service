# 002 — Tareas

- [ ] Agregar `Confluent.Kafka` y las opciones de configuración.
- [ ] Implementar `KafkaConsumerService` con grupo, tópico y auto-commit deshabilitado.
- [ ] Deserializar y validar el evento `BuildGuardada`.
- [ ] Invocar `ProcessBuildEvent` y manejar la idempotencia.
- [ ] Confirmar offsets después del procesamiento o de un evento inválido.
- [ ] Implementar reintentos para errores transitorios de persistencia.
- [ ] Implementar shutdown limpio y logs con `ILogger`.
- [ ] Escribir tests del handler para umbral, duplicado y evento inválido.
- [ ] Escribir tests del comportamiento de commit/no commit.
- [ ] Documentar y probar un productor de evento local.
- [ ] Verificar cobertura ≥ 60% del módulo.
