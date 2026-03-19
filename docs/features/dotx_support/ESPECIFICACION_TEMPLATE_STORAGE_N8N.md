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

## 3. Endpoints y versionado

La implementación usa **dos webhooks separados** para aislar responsabilidades y simplificar los flujos n8n:

| Webhook | Método | Operaciones |
|---|---|---|
| `/webhook/template-store` | `POST` | `UPLOAD_TEMPLATE`, `DELETE_TEMPLATE` |
| `/webhook/template-retrieve` | `POST` | `GET_TEMPLATE` |

Ejemplos URL completas:

```
https://n8n.tudominio.com/webhook/template-store
https://n8n.tudominio.com/webhook/template-retrieve
```

Versionado en payload: `apiVersion`

Versión inicial del contrato:

- `apiVersion: "1.0"`

> **Nota:** El diseño original contemplaba un único endpoint `/webhook/template-storage`. La implementación final dividió las operaciones en dos webhooks para simplificar el routing interno de n8n y separar las credenciales de escritura (Google Drive upload/delete) de las de lectura (download).

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
  "storageKey": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms"
}
```

> **Nota:** `storageKey` es el **File ID de Google Drive** (string opaco). No es una ruta de fichero. Se obtiene como retorno de `UPLOAD_TEMPLATE` y se persiste en la columna `storage_key` de la entidad `AppTemplate`.

### C) `DELETE_TEMPLATE`

```json
{
  "storageKey": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms",
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
  "provider": "google-drive",
  "data": {}
}
```

> **Nota:** El campo `timestampUtc` en la respuesta era parte del diseño original. Los flujos n8n actuales no lo incluyen. El campo `provider` indica el backend de almacenamiento activo (`google-drive` en la implementación actual).

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
  "storageKey": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms",
  "fileName": "Plantilla_Memoria_v3.dotx",
  "fileSizeBytes": 245781,
  "version": 1
}
```

> `storageKey` es el **File ID de Google Drive** devuelto tras el upload. La API lo persiste en DB para usarlo en `GET_TEMPLATE` y `DELETE_TEMPLATE`.
> Los campos `sha256` y `metadata` (bucket/region) eran parte del diseño original orientado a S3; la implementación Google Drive no los incluye.

### B) Respuesta `GET_TEMPLATE`

```json
{
  "storageKey": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms",
  "mimeType": "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
  "fileSizeBytes": 245781,
  "contentBase64": "UEsDBBQABgAIAAAAIQ..."
}
```

### C) Respuesta `DELETE_TEMPLATE`

```json
{
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

1. API resuelve plantilla seleccionada (`templateId`) o predeterminada por tipo en DB.
2. API intenta L1 cache.
3. Cache miss: API llama `GET_TEMPLATE` al webhook.
4. API renderiza DOCX con OpenXML.
5. API devuelve `.docx` al cliente.

---

## 11. Ejemplos rápidos (curl)

## 11.1. Upload → `/webhook/template-store`

```bash
curl -X POST "https://n8n.tudominio.com/webhook/template-store" \
  -H "Content-Type: application/json" \
  -H "X-Edificia-Auth: <SECRET>" \
  -H "X-Request-Id: 95e3de87-213e-4f49-a99f-d2cb3dfe6b62" \
  -H "X-Idempotency-Key: upload-95e3de87-213e-4f49-a99f-d2cb3dfe6b62" \
  -d '{
    "apiVersion":"1.0",
    "operation":"UPLOAD_TEMPLATE",
    "operationId":"95e3de87-213e-4f49-a99f-d2cb3dfe6b62",
    "timestampUtc":"2026-02-24T18:10:00Z",
    "tenantId":"default",
    "requestedBy":"api-edificia",
    "payload":{
      "templateType":"MemoriaTecnica",
      "fileName":"Plantilla_Memoria_v3.dotx",
      "mimeType":"application/vnd.openxmlformats-officedocument.wordprocessingml.template",
      "fileSizeBytes":245781,
      "contentBase64":"UEsDB..."
    }
  }'
```

## 11.2. Get → `/webhook/template-retrieve`

```bash
curl -X POST "https://n8n.tudominio.com/webhook/template-retrieve" \
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
      "storageKey":"1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms"
    }
  }'
```

## 11.3. Delete → `/webhook/template-store`

```bash
curl -X POST "https://n8n.tudominio.com/webhook/template-store" \
  -H "Content-Type: application/json" \
  -H "X-Edificia-Auth: <SECRET>" \
  -H "X-Request-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "X-Idempotency-Key: delete-3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -d '{
    "apiVersion":"1.0",
    "operation":"DELETE_TEMPLATE",
    "operationId":"3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "timestampUtc":"2026-02-24T18:15:00Z",
    "tenantId":"default",
    "requestedBy":"api-edificia",
    "payload":{
      "storageKey":"1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms",
      "hardDelete":false
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
