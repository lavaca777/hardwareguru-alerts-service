# Misión del proyecto

## ¿Qué es?

Servicio interno de HardwareGuru que escucha las builds guardadas y detecta cuándo el consumo estimado supera el 85% de la potencia de la fuente. Genera alertas persistentes y permite consultarlas y marcarlas como leídas.

## ¿Para quién?

Para los usuarios de HardwareGuru, a través del API Gateway y del frontend. El servicio no lo consume directamente el navegador.

## ¿Qué problema resuelve?

Una build puede guardarse con una fuente que funciona en reposo pero queda al límite en picos de carga. Sin una auditoría asíncrona, el usuario no se entera hasta que apaga el equipo o tiene inestabilidad. El servicio detecta ese riesgo después de guardar, lo persiste y lo deja disponible en la bandeja de alertas.

## ¿Qué NO es?

- No autentica ni registra usuarios; eso vive en el Auth Service.
- No crea, edita ni elimina builds; eso pertenece al simulador/gateway.
- No calcula rendimiento ni consumo; recibe esos datos en el evento.
- No es el frontend ni el BFF.
- No expone Kafka ni PostgreSQL al navegador.
- No genera alertas por builds que no hayan sido guardadas.
