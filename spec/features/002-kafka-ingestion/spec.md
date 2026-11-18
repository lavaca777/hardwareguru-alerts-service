# 002 — Kafka ingestion

## Responsabilidad

Conectar el caso de uso de alertas con el tópico `builds_guardadas`, deserializar eventos, ejecutar la regla y confirmar offsets solo después de persistir. No modifica la API REST; usa la lógica de 001.

## Comportamiento

### Consumo

- Tópico: `builds_guardadas`.
- Grupo: `alerts-service`.
- `AutoOffsetReset=Earliest`.
- Auto-commit deshabilitado; el offset se confirma manualmente.
- El consumidor corre como `BackgroundService` junto con la API.
- Se detiene de forma limpia al apagar la aplicación.

### Procesamiento

- Deserializar JSON con nombres de campos del contrato.
- Validar los campos obligatorios y el UUID del evento.
- Invocar `ProcessBuildEvent`.
- Si la build supera el umbral, persistir la alerta.
- Si es duplicada, tratarla como procesada y confirmar el offset.
- Si está por debajo del umbral, no persistir y confirmar.

### Errores

- JSON inválido o campos obligatorios faltantes: log de advertencia y commit para no bloquear el tópico.
- Error transitorio de PostgreSQL: reintentar hasta 3 veces con espera; si falla, no confirmar el offset.
- Error de conexión con Kafka: registrar y reintentar según el cliente hasta el shutdown.
- Nunca confirmar un offset antes de persistir una alerta válida.
- No loguear el payload completo si contiene datos sensibles; sí el `eventId`/`buildId`.

### Configuración

```env
Kafka__BootstrapServers=localhost:9092
Kafka__Topic=builds_guardadas
Kafka__GroupId=alerts-service
```

## Criterios de aceptación

- [x] Un evento de 85.1% crea una alerta; uno de 85.0% no.
- [x] Un evento repetido no crea una segunda alerta.
- [x] Un evento inválido no tumba el consumidor.
- [x] El offset se confirma solo después del procesamiento.
- [x] El consumidor se detiene limpiamente al apagar el host.
- [x] Los errores de persistencia no se ocultan.
- [x] La cobertura del módulo es ≥ 60%.
