# Stack Tecnológico

## Core

| Tecnología | Versión | Uso |
|---|---|---|
| C# | 12 | Lenguaje del servicio |
| .NET SDK | 8 | Compilación y ejecución |
| ASP.NET Core | 8 | API REST y host único con el worker |
| `BackgroundService` | .NET 8 | Consumidor Kafka en segundo plano |
| `Confluent.Kafka` | 2.16.0 | Cliente consumidor |
| `System.Text.Json` | .NET 8 | Deserialización del evento |

## Persistencia

| Tecnología | Uso |
|---|---|
| PostgreSQL | Base de datos propia del servicio |
| EF Core 8 + Npgsql | Entidad, migraciones y repositorio |

Base local: `hardwareguru_alerts` en el puerto **5436**. Se usa un puerto distinto al PostgreSQL nativo (`5432`) y al catálogo (`5435`).

## Tests

| Tecnología | Uso |
|---|---|
| xUnit | Tests unitarios y de integración |
| Moq | Dobles de repositorio y servicios |
| `Microsoft.AspNetCore.Mvc.Testing` | Tests HTTP de la API |
| Testcontainers | PostgreSQL real para probar idempotencia y migraciones *(requiere aprobación por ser dependencia adicional; alternativa: PostgreSQL local)* |
| Coverlet | Cobertura, meta ≥ 60% |

## Infraestructura local

| Servicio | Puerto |
|---|---|
| API de alertas | `5001` |
| PostgreSQL | `5436` |
| Kafka | `9092` |
| Tópico | `builds_guardadas` |
| Consumer group | `alerts-service` |

## Restricciones

- Se usa **.NET 8** para alinearse con el README y el resto del equipo. Verificar `dotnet --version` antes de implementar; el SDK no está instalado en la máquina actual.
- El servicio es interno: el gateway es el único cliente y ya validó el JWT.
- El contrato del evento es obligatorio y se documenta en `contracts.md`.
- `eventId` debe ser único en PostgreSQL; la idempotencia no puede depender solo de una consulta previa.
- La comparación del umbral usa decimales: `consumo * 100 > potencia * 85` (estrictamente mayor).
- `potenciaFuenteW <= 0` es un evento inválido para generar alertas.
- El consumidor no hace commit del offset antes de persistir.
- Kafka en modo KRaft; la creación automática de tópicos solo se permite en desarrollo.
- En Windows con Docker Desktop, el cliente local debe usar `127.0.0.1:9092` (no `localhost`), porque librdkafka puede resolver a IPv6 y quedarse esperando al coordinador.
- El consumidor está deshabilitado por defecto (`Kafka__Enabled=false`) para que la API y los tests no dependan de un broker.
- No se agregan librerías nuevas sin aprobación.
- Docker y Testcontainers requieren Docker Desktop en la máquina de desarrollo.
