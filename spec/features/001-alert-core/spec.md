# 001 — Alert core

## Responsabilidad

Decidir si una build guardada supera el umbral energético, persistir la alerta de forma idempotente y exponer su consulta y marcado como leída. No consume Kafka todavía; esa conexión se implementa en 002.

## Comportamiento

### Regla de riesgo

- Calcular el uso de la fuente con decimales: `consumoEstimadoW / potenciaFuenteW * 100`.
- Generar alerta solo si el uso es **estrictamente mayor que 85%**.
- Un uso de exactamente `85.0%` no genera alerta.
- Si `potenciaFuenteW <= 0`, registrar el evento como inválido y no generar alerta.
- Guardar el porcentaje como entero redondeado para mostrarlo en el frontend.
- La comparación se hace con el valor decimal, no con el porcentaje redondeado.

### Idempotencia

- Cada evento tiene un `eventId` UUID.
- La tabla tiene una restricción única sobre `eventId`.
- Procesar dos veces el mismo evento produce una sola alerta.
- Un duplicado se considera éxito; no debe romper el consumidor.

### Mensaje y sugerencia

- `mensaje`: indica la potencia de la fuente y el porcentaje de uso de la build.
- `sugerencia`: recomienda undervolt o una fuente de mayor potencia.
- El `buildName` se conserva para que el frontend muestre la alerta correcta.

### Consulta

- `GET /alerts?userId={id}` devuelve las alertas del usuario, más recientes primero.
- `userId` es obligatorio.
- Las alertas leídas y pendientes se devuelven juntas; el frontend filtra.
- El servicio es interno y confía en el `userId` que le pasa el gateway.

### Marcado como leída

- `PATCH /alerts/{id}/read` marca la alerta como leída.
- Si ya estaba leída, devuelve la misma alerta sin error.
- Si el ID no existe, responde `404`.
- Devuelve el recurso actualizado para que el gateway pueda pasarlo al frontend.

## Estructura de datos

Los nombres internos de C# van en inglés; el JSON de integración mantiene los nombres del contrato del frontend.

```csharp
public sealed record BuildSavedEvent(
    Guid EventId,
    string UserId,
    string BuildId,
    string BuildName,
    int EstimatedConsumptionW,
    int PsuPowerW,
    DateTimeOffset OccurredAt);

public sealed class Alert
{
    public Guid Id { get; init; }
    public Guid EventId { get; init; }
    public string UserId { get; init; }
    public string BuildId { get; init; }
    public string BuildName { get; init; }
    public int UsagePercentage { get; init; }
    public string Message { get; init; }
    public string Suggestion { get; init; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}
```

La API mapea `UsagePercentage` a `porcentajeUso`, `Message` a `mensaje`, `Suggestion` a `sugerencia`, `IsRead` a `leida` y `CreatedAt` a `creadaEn`.

Contrato JSON de salida:

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

## API pública

```http
GET  /alerts?userId={userId}   → 200 Alerta[] | 400 ProblemDetails
PATCH /alerts/{id}/read        → 200 Alerta   | 404 ProblemDetails
GET  /healthz                  → 200 "ok"
```

## Criterios de aceptación

- [ ] Un uso de 85% no genera alerta; uno de 85.1% sí.
- [ ] Una fuente con potencia 0 se descarta sin romper el proceso.
- [ ] El porcentaje mostrado es entero y coherente con el consumo real.
- [ ] Procesar el mismo `eventId` dos veces deja una sola fila.
- [ ] `GET /alerts?userId=` filtra por usuario y ordena por fecha descendente.
- [ ] `PATCH /alerts/{id}/read` es idempotente y devuelve la alerta actualizada.
- [ ] Un ID inexistente devuelve 404 con `ProblemDetails`.
- [ ] La cobertura del módulo es ≥ 60%.
