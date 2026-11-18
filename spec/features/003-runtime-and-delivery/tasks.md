# 003 — Tareas

- [ ] Crear `.gitignore` y `.env.example`.
- [ ] Crear Dockerfile multi-stage con usuario no-root.
- [ ] Agregar healthcheck de la API.
- [ ] Crear compose con PostgreSQL 16 en 5436, Kafka KRaft en 9092 y API en 5001.
- [ ] Crear el tópico `builds_guardadas` en el entorno local.
- [ ] Configurar healthchecks y dependencias del compose.
- [ ] Crear workflow de GitHub Actions con restore, format, build y tests.
- [ ] Publicar cobertura como artefacto.
- [ ] Agregar build de imagen Docker al CI.
- [ ] Actualizar README con requisitos, comandos y ejemplos de API/evento.
- [ ] Producir un evento de prueba y verificar alerta + idempotencia.
- [ ] Verificar que `/healthz` responda desde el contenedor.
- [ ] Verificar cobertura ≥ 60%.
