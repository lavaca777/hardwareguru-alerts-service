# 003 — Plan de implementación

## Pasos

1. Crear `.gitignore` para .NET, Rider/VS, `bin/`, `obj/`, `.env`, `coverage/`.
2. Crear `.env.example` con PostgreSQL, Kafka y puertos.
3. Escribir el Dockerfile multi-stage y verificar el healthcheck.
4. Crear `docker-compose.yml` con PostgreSQL, Kafka KRaft y la API.
5. Crear el inicializador del tópico `builds_guardadas`.
6. Crear el workflow de GitHub Actions.
7. Actualizar el README con arranque, API, evento y pruebas.
8. Probar el flujo manual: evento → alerta → leer alerta.
9. Verificar cobertura y build de la imagen.

## Notas técnicas

- Usar la imagen oficial de Apache Kafka en modo KRaft y fijar la versión al implementar.
- El healthcheck de PostgreSQL debe usar `pg_isready`.
- El healthcheck de la API puede usar `curl` o `wget`; si la imagen runtime no lo incluye, instalarlo en una capa mínima.
- En compose, el connection string usa `Host=postgres`, no `localhost`.
- En ejecución local con `dotnet run`, Kafka y PostgreSQL usan `localhost`.
- El tópico puede crearse con un contenedor de inicialización o con auto-create solo en desarrollo.
- No incluir credenciales reales en el compose; usar valores locales documentados.
- El CI no debe necesitar Kafka real para los tests unitarios; los tests de integración con PostgreSQL usan Testcontainers.
- Docker Desktop debe estar corriendo para compose y Testcontainers.
- No modificar la lógica de 001/002 desde esta feature.
