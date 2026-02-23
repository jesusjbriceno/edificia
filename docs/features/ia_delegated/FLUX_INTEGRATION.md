# Integración con Flux Gateway — EdificIA (n8n)

Este documento resume la configuración necesaria para integrar el workflow n8n de Flux de EdificIA con la API de Flux Gateway (ver `flux-openapi.json`). Complementa a `GUIA_WORKFLOWS_N8N.md`.

> **Repositorio de Flux Gateway:** [github.com/jesusjbriceno/flux-free-gateway](https://github.com/jesusjbriceno/flux-free-gateway)

---

## 1. Endpoints utilizados por el workflow

- `POST /api/v1/auth/app` — Intercambia `clientId` + `clientSecret` por un token JWT. El workflow de n8n utiliza este endpoint para obtener el Bearer token.
- `POST /api/v1/chat` — Envía una petición de chat con `{ messages }` o `{ message }` y recibe `{ content, usage, model_used, provider }`.

---

## 2. Credenciales n8n / Variables de entorno

Configura las siguientes variables en tu entorno n8n (Docker Compose, `.env` o desde Settings → Variables en la UI de n8n):

| Variable | Descripción |
|---|---|
| `EDIFICIA_API_SECRET` | Clave compartida validada por el workflow (`X-Edificia-Auth`). |
| `FLUX_CLIENT_ID` | ID de cliente de la aplicación Flux (usar credenciales n8n o variable de entorno). |
| `FLUX_CLIENT_SECRET` | Secreto de cliente de la aplicación Flux. |
| `FLUX_MODEL` _(opcional)_ | Modelo por defecto a solicitar (por ejemplo, `flux-pro`). |

En el `.env.example` del repositorio se incluyen marcadores de posición por comodidad:

```dotenv
FLUX_CLIENT_ID=CHANGE_ME_flux_client_id
FLUX_CLIENT_SECRET=CHANGE_ME_flux_client_secret
```

---

## 3. Notas sobre el contrato Request/Response

- El workflow espera que la respuesta de Flux `/api/v1/chat` incluya `content` (string) en el nivel superior o un array `choices` compatible con OpenAI. El workflow normaliza ambos formatos al contrato `{ generatedText, usage }` que consume el backend.
- Los tokens pueden leerse de `response.usage.total_tokens` o `response.usage.tokens` según la implementación del proveedor.

---

## 4. Resolución de problemas

| Síntoma | Causa probable | Solución |
|---|---|---|
| 401/403 en login | `FLUX_CLIENT_ID` o `FLUX_CLIENT_SECRET` incorrectos, o la aplicación está inactiva en Flux | Verificar las credenciales en el panel de Flux |
| Sin `content` en la respuesta | El payload no incluye `messages` ni `choices` | Revisar el payload en n8n → Executions y comprobar los nodos de respuesta |

---

## 5. Referencias

- Especificación de la API: `docs/features/ia_delegated/flux-openapi.json`
- Workflow: `apps/n8n/workflow-flux.json`
- Repositorio Flux Gateway: [github.com/jesusjbriceno/flux-free-gateway](https://github.com/jesusjbriceno/flux-free-gateway)
