# 003 — Tareas

- [x] Crear `.gitignore` y `.env.example`.
- [x] Crear Dockerfile multi-stage con usuario no-root.
- [x] Agregar healthcheck de la API.
- [x] Crear compose con PostgreSQL 16 en 5436, Kafka KRaft en 9092 y API en 5001.
- [x] Crear el tópico `builds_guardadas` en el entorno local.
- [x] Configurar healthchecks y dependencias del compose.
- [x] Crear workflow de GitHub Actions con restore, format, build y tests.
- [x] Publicar cobertura como artefacto.
- [x] Agregar build de imagen Docker al CI.
- [x] Actualizar README con requisitos, comandos y ejemplos de API/evento.
- [x] Producir un evento de prueba y verificar alerta + idempotencia.
- [x] Verificar que `/healthz` responda desde el contenedor.
- [x] Verificar cobertura ≥ 60%.
