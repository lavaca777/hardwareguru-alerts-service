# Roadmap

## Orden de implementación

| # | Feature | Dependencias | Descripción |
|---|---|---|---|
| 001 | `alert-core` | — | Regla del 85%, entidad, persistencia idempotente, casos de uso y API REST. |
| 002 | `kafka-ingestion` | 001 | Consumidor de `builds_guardadas`, validación, idempotencia operativa y commits seguros. |
| 003 | `runtime-and-delivery` | 001, 002 | Dockerfile, compose local, healthcheck, CI, cobertura y README. |

## Fuera de este repo

La integración `/api/alerts` en el gateway, el JWT y la pantalla del frontend dependen de los repos 5 y 7. El servicio de alertas deja el contrato listo para que el gateway lo consuma. No se implementa aquí.

## Criterio para avanzar

Una feature está completa cuando:

1. Todas las tareas de su `tasks.md` están marcadas `[x]`.
2. Los criterios de aceptación de su `spec.md` se cumplen.
3. `dotnet test` pasa y la cobertura del módulo es ≥ 60%.
4. `dotnet build` y `dotnet format --verify-no-changes` pasan.
5. No quedan `Console.WriteLine`, secretos ni logs con datos sensibles.
6. El README se actualiza si cambió la forma de ejecutar o configurar el servicio.
