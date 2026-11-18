# 002 — Plan de implementación

## Pasos

1. Agregar `Confluent.Kafka` al proyecto de infraestructura.
2. Crear `KafkaOptions` y `KafkaConsumerService : BackgroundService`.
3. Configurar el consumer con `EnableAutoCommit=false` y `AutoOffsetReset=Earliest`.
4. Implementar deserialización y validación del evento.
5. Invocar el caso de uso de 001 y manejar duplicados.
6. Implementar reintentos acotados para errores transitorios de persistencia.
7. Confirmar offset después de éxito, duplicado o evento inválido.
8. Agregar logs estructurados con `ILogger`.
9. Escribir tests del handler de eventos y del commit con un consumer simulado.
10. Documentar el productor de prueba para el entorno local.

## Notas técnicas

- Separar el procesamiento del evento en una clase de aplicación testeable; el `BackgroundService` solo consume y confirma offsets.
- Un duplicado no es un error funcional.
- Un evento inválido se descarta con log; no se reintenta indefinidamente.
- Para errores de DB, no confirmar el offset; el mensaje se reprocesa al reiniciar.
- El consumidor debe respetar el `CancellationToken` del host.
- El tópico se crea en el compose de desarrollo; en producción se coordina con infraestructura.
- No usar `Console.WriteLine`.
