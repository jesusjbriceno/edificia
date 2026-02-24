# ESPECIFICACION_TEMPLATE_STORAGE_N8N

## 1. Objetivo

Definir el contrato técnico entre la API de EdificIA y n8n para delegar el almacenamiento y recuperación de plantillas `.dotx`.

Este contrato se diseña para:

- Minimizar desalineación entre metadatos de DB y binario.
- Permitir cambiar backend de almacenamiento sin tocar la API (`Drive`, `OneDrive`, `S3`, `Synology`, etc.).
- Estandarizar errores, idempotencia y trazabilidad.

---

## 2. Alcance funcional

Operaciones soportadas:

1. `UPLOAD_TEMPLATE`
2. `GET_TEMPLATE`
3. `DELETE_TEMPLATE`

Modelo de integración:

- **Síncrono** (request/response HTTP).
- La API persiste metadatos en PostgreSQL **solo tras** recibir `success=true` en `UPLOAD_TEMPLATE`.

---

## 3. Endpoint y versionado

- Método: `POST`
- URL recomendada: `/webhook/template-storage`
- Versionado en payload: `apiVersion`

Ejemplo URL completa:

`https://n8n.tudominio.com/webhook/template-storage`

Versión inicial del contrato:

- `apiVersion: "1.0"`

---

## 4. Seguridad

Headers obligatorios:

- `X-Edificia-Auth`: secreto compartido (`TemplateStorage__N8nApiSecret`)
- `X-Request-Id`: identificador de trazabilidad (UUID)
- `X-Idempotency-Key`: clave de idempotencia para operaciones mutables
- `Content-Type: application/json`

Reglas:

- Rechazar peticiones sin `X-Edificia-Auth` válido (`401`).
- Rechazar payload inválido (`400`).
- No loggear contenido base64 completo en texto plano.

---

## 5. Contrato de petición

## 5.1. Envelope común

```json
{
  "apiVersion": "1.0",
  "operation": "UPLOAD_TEMPLATE",
  "operationId": "b42e6f93-9c33-4e7a-b64d-2e8d0c8f8b0b",
  "timestampUtc": "2026-02-24T18:10:00Z",
  "tenantId": "default",
  "requestedBy": "admin@edificia.dev",
  "payload": {}
}
```

Campos:

- `apiVersion` (`string`, requerido)
- `operation` (`UPLOAD_TEMPLATE | GET_TEMPLATE | DELETE_TEMPLATE`, requerido)
- `operationId` (`uuid`, requerido)
- `timestampUtc` (`ISO-8601 UTC`, requerido)
- `tenantId` (`string`, opcional, default `default`)
- `requestedBy` (`string`, requerido)
- `payload` (`object`, requerido)

## 5.2. Payload por operación

### A) `UPLOAD_TEMPLATE`

```json
{
  "templateType": "MemoriaTecnica",
  "fileName": "Plantilla_Memoria_v3.dotx",
  "mimeType": "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
  "fileSizeBytes": 245781,
  "contentBase64": "UEsDBBQABgAIAAAAIQ..."
}
```

Validaciones mínimas:

- Extensión `.dotx`.
- MIME permitido.
- Tamaño máximo recomendado: `10MB`.
- `contentBase64` requerido y no vacío.

### B) `GET_TEMPLATE`

```json
{
  "storageKey": "templates/memoria/2026/02/plantilla-v3.dotx"
}
```

### C) `DELETE_TEMPLATE`

```json
{
  "storageKey": "templates/memoria/2026/02/plantilla-v3.dotx",
  "hardDelete": false
}
```

---

## 6. Contrato de respuesta

## 6.1. Envelope común

```json
{
  "apiVersion": "1.0",
  "operation": "UPLOAD_TEMPLATE",
  "operationId": "b42e6f93-9c33-4e7a-b64d-2e8d0c8f8b0b",
  "success": true,
  "code": "TEMPLATE_STORAGE_OK",
  "message": "Operation completed",
  "provider": "s3",
  "timestampUtc": "2026-02-24T18:10:01Z",
  "data": {}
}
```

Campos:

- `success` (`bool`, requerido)
- `code` (`string`, requerido)
- `message` (`string`, opcional)
- `provider` (`string`, opcional)
- `data` (`object`, opcional)

## 6.2. `data` por operación

### A) Respuesta `UPLOAD_TEMPLATE`

```json
{
  "storageKey": "templates/memoria/2026/02/plantilla-v3.dotx",
  "fileName": "Plantilla_Memoria_v3.dotx",
  "fileSizeBytes": 245781,
  "sha256": "8f11ef4f5f3d4c99...",
  "version": 3,
  "metadata": {
    "bucket": "edificia-templates",
    "region": "eu-west-1"
  }
}
```

### B) Respuesta `GET_TEMPLATE`

```json
{
  "storageKey": "templates/memoria/2026/02/plantilla-v3.dotx",
  "mimeType": "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
  "fileSizeBytes": 245781,
  "contentBase64": "UEsDBBQABgAIAAAAIQ..."
}
```

### C) Respuesta `DELETE_TEMPLATE`

```json
{
  "storageKey": "templates/memoria/2026/02/plantilla-v3.dotx",
  "deleted": true
}
```

---

## 7. Códigos de error

| Code | HTTP | Descripción |
|---|---:|---|
| `AUTH_INVALID` | 401 | Cabecera `X-Edificia-Auth` ausente o inválida |
| `REQUEST_INVALID` | 400 | Envelope inválido o campos requeridos ausentes |
| `UNSUPPORTED_OPERATION` | 400 | Operación no soportada |
| `FILE_TOO_LARGE` | 413 | Tamaño de plantilla excede límite |
| `MIME_NOT_ALLOWED` | 415 | Tipo de archivo no permitido |
| `TEMPLATE_NOT_FOUND` | 404 | `storageKey` no existe |
| `STORAGE_PROVIDER_ERROR` | 502 | Fallo del backend de storage delegado |
| `TIMEOUT` | 504 | Timeout en la operación del flujo |
| `IDEMPOTENCY_CONFLICT` | 409 | Mismo `X-Idempotency-Key` con payload distinto |
| `INTERNAL_ERROR` | 500 | Error inesperado del workflow |

---

## 8. Idempotencia

Aplicación:

- Obligatoria para `UPLOAD_TEMPLATE` y `DELETE_TEMPLATE`.
- Recomendada para `GET_TEMPLATE` (opcional).

Reglas:

1. Si llega la misma `X-Idempotency-Key` con el mismo payload, responder la misma salida previa.
2. Si llega la misma clave con payload distinto, responder `409 IDEMPOTENCY_CONFLICT`.
3. TTL recomendado del registro de idempotencia: `24h`.

---

## 9. Timeout, retry y resiliencia

- Timeout API→n8n recomendado: `60s`.
- Reintentos API recomendados: `2` con backoff exponencial corto (solo en `5xx`/`timeout`).
- No reintentar en `4xx` funcionales.

Requisito de diseño:

- `operationId` y `X-Request-Id` deben propagarse en logs de API y n8n.

---

## 10. Secuencia operativa recomendada

## 10.1. Upload de plantilla

1. API valida request y fichero.
2. API llama webhook `UPLOAD_TEMPLATE`.
3. Si `success=true`, API persiste metadatos (`storageKey`, `sha256`, etc.) en DB.
4. Si error, API retorna ProblemDetails y no persiste DB.

## 10.2. Exportación con plantilla

1. API resuelve plantilla activa en DB.
2. API intenta L1 cache.
3. Cache miss: API llama `GET_TEMPLATE` al webhook.
4. API renderiza DOCX con OpenXML.
5. API devuelve `.docx` al cliente.

---

## 11. Ejemplos rápidos (curl)

## 11.1. Upload

```bash
curl -X POST "https://n8n.tudominio.com/webhook/template-storage" \
  -H "Content-Type: application/json" \
  -H "X-Edificia-Auth: <SECRET>" \
  -H "X-Request-Id: 95e3de87-213e-4f49-a99f-d2cb3dfe6b62" \
  -H "X-Idempotency-Key: upload-memoria-v3-20260224" \
  -d '{
    "apiVersion":"1.0",
    "operation":"UPLOAD_TEMPLATE",
    "operationId":"95e3de87-213e-4f49-a99f-d2cb3dfe6b62",
    "timestampUtc":"2026-02-24T18:10:00Z",
    "tenantId":"default",
    "requestedBy":"admin@edificia.dev",
    "payload":{
      "templateType":"MemoriaTecnica",
      "fileName":"Plantilla_Memoria_v3.dotx",
      "mimeType":"application/vnd.openxmlformats-officedocument.wordprocessingml.template",
      "fileSizeBytes":245781,
      "contentBase64":"UEsDB..."
    }
  }'
```

## 11.2. Get

```bash
curl -X POST "https://n8n.tudominio.com/webhook/template-storage" \
  -H "Content-Type: application/json" \
  -H "X-Edificia-Auth: <SECRET>" \
  -H "X-Request-Id: 15fe0da2-a872-4f24-b7bf-8fcb14926c24" \
  -d '{
    "apiVersion":"1.0",
    "operation":"GET_TEMPLATE",
    "operationId":"15fe0da2-a872-4f24-b7bf-8fcb14926c24",
    "timestampUtc":"2026-02-24T18:12:00Z",
    "tenantId":"default",
    "requestedBy":"api-edificia",
    "payload":{
      "storageKey":"templates/memoria/2026/02/plantilla-v3.dotx"
    }
  }'
```

---

## 12. Criterios de aceptación del contrato

1. El flujo devuelve envelope conforme para las 3 operaciones.
2. `UPLOAD_TEMPLATE` y `DELETE_TEMPLATE` soportan idempotencia real.
3. Errores se devuelven con `code` consistente y HTTP correcto.
4. `operationId` y `X-Request-Id` aparecen en logs de ambos lados.
5. API puede persistir DB sin riesgo de desalineación en caso nominal.
