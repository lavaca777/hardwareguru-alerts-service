# Contratos compartidos

> Este contrato es la frontera entre el gateway, el simulador y el servicio de alertas. Cualquier cambio requiere coordinación con el equipo. El repo `hardwareguru-infra` todavía no existe; este documento es la referencia provisional.

## Evento Kafka: `BuildGuardada`

Tópico: `builds_guardadas`  
Consumer group: `alerts-service`

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

### Requisitos

- `eventId`: UUID válido y único por evento.
- `userId`, `buildId`, `buildName`: strings no vacíos.
- `consumoEstimadoW`: entero no negativo.
- `potenciaFuenteW`: entero mayor que cero.
- `ocurridoEn`: fecha ISO 8601.

Un evento inválido se registra y se descarta sin bloquear el consumidor.

## API REST interna

Base: `http://localhost:5001`

### `GET /alerts?userId={userId}`

Lista las alertas del usuario, de más reciente a más antigua.

- `200 OK` → `Alerta[]`
- `400 Bad Request` → falta `userId`
- El gateway es responsable de inyectar el `userId` real del JWT.

### `PATCH /alerts/{id}/read`

Marca una alerta como leída. Es idempotente: si ya estaba leída, devuelve el mismo recurso.

- `200 OK` → `Alerta` actualizada
- `404 Not Found` → alerta inexistente

### `GET /healthz`

- `200 OK` → `"ok"` o estado de salud equivalente.
- No requiere autenticación.

## JSON de alerta

Debe coincidir con el tipo `Alerta` del frontend:

```json
{
  "id": "8f14e45f-ceea-4e8f-9d42-9c1f4b2a7c10",
  "buildId": "b2",
  "buildName": "PC Gamer 1440p",
  "porcentajeUso": 101,
  "mensaje": "Tu fuente de 500 W está al 101% de uso con esta build.",
  "sugerencia": "Haz undervolt o cambia a una fuente de mayor potencia.",
  "leida": false,
  "creadaEn": "2026-11-17T20:00:01Z"
}
```

## Errores

- La API usa `ProblemDetails` para 400 y 404.
- El consumidor registra eventos inválidos y duplicados con `ILogger`; no los propaga a HTTP.
- El gateway traduce los errores al formato del frontend.

## Propiedad del contrato

- El evento y la API los consume el API Gateway.
- El servicio de alertas es dueño de la persistencia y del estado `leida`.
- El frontend es dueño de la presentación; sus campos de `Alerta` son el contrato de salida.
