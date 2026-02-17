# **⚙️ Guía de Workflows n8n — EDIFICIA**

> **Versión:** 1.0  
> **Fecha:** Enero 2025  
> **Ubicación workflows:** `apps/n8n/`

## **1. Resumen**

EDIFICIA delega la generación de texto IA a **n8n** mediante un webhook POST. El backend (.NET) envía una petición estructurada (`AiGenerationRequest`) y recibe una respuesta normalizada (`N8nAiResponse`), independientemente del proveedor de IA subyacente.

Se proporcionan **dos workflows** intercambiables:

| Workflow | Archivo | Proveedor IA | Nodos |
|----------|---------|-------------|-------|
| **Gemini** | `workflow-gemini.json` | Google Gemini API | 8 nodos |
| **Flux Gateway** | `workflow-flux.json` | Flux Gateway (OpenAI-compatible) | 10 nodos |

> ⚠️ **Solo un workflow debe estar activo a la vez**, ya que ambos escuchan en la misma ruta `/webhook/generar-memoria`.

---

## **2. Arquitectura de los Flujos**

### 2.1 Diagrama — Gemini

```
┌──────────┐    ┌───────────────┐    ┌────────────┐
│ Webhook  │───▶│ Validate Auth │───▶│ Auth Valid? │
│ POST     │    │ (Code)        │    │ (IF)       │
└──────────┘    └───────────────┘    └─────┬──────┘
                                      true │    │ false
                                           ▼    ▼
     ┌─────────────┐    ┌─────────────┐   ┌────────────┐
     │ Build Prompt │───▶│ Call Gemini │   │ Respond    │
     │ (Code)       │    │ (HTTP Req)  │   │ 403        │
     └─────────────┘    └──────┬──────┘   └────────────┘
                               ▼
                    ┌──────────────────┐    ┌─────────────┐
                    │ Format Response  │───▶│ Respond 200 │
                    │ (Code)           │    │             │
                    └──────────────────┘    └─────────────┘
```

### 2.2 Diagrama — Flux Gateway

```
┌──────────┐    ┌───────────────┐    ┌────────────┐
│ Webhook  │───▶│ Validate Auth │───▶│ Auth Valid? │
│ POST     │    │ (Code)        │    │ (IF)       │
└──────────┘    └───────────────┘    └─────┬──────┘
                                      true │    │ false
                                           ▼    ▼
     ┌─────────────┐    ┌────────────┐   ┌────────────┐
     │ Build Prompt │───▶│ Login Flux │   │ Respond    │
     │ (Code)       │    │ (HTTP Req) │   │ 403        │
     └─────────────┘    └─────┬──────┘   └────────────┘
                              ▼
     ┌───────────────────┐    ┌───────────────────┐
     │ Build Chat Request│───▶│ Chat Completions  │
     │ (Code)            │    │ (HTTP Req)        │
     └───────────────────┘    └────────┬──────────┘
                                       ▼
                            ┌──────────────────┐    ┌─────────────┐
                            │ Format Response  │───▶│ Respond 200 │
                            │ (Code)           │    │             │
                            └──────────────────┘    └─────────────┘
```

---

## **3. Seguridad**

Ambos workflows validan la cabecera `X-Edificia-Auth` antes de procesar cualquier petición.

### 3.1 Mecanismo

1. El backend (`N8nAiService`) envía la cabecera `X-Edificia-Auth` con el valor de `AI__ApiSecret`.
2. El nodo **Validate Auth** (Code) compara el valor recibido contra la variable de entorno de n8n `EDIFICIA_API_SECRET`.
3. Si no coincide → **403 Forbidden** con JSON de error.
4. Si coincide → Los datos del body se pasan al siguiente nodo.

### 3.2 Configuración

El valor debe ser **idéntico** en ambos lados:

| Componente | Variable | Ejemplo |
|------------|----------|---------|
| Backend (.NET) | `AI__ApiSecret` | `mi-secreto-compartido-seguro` |
| n8n | `EDIFICIA_API_SECRET` (env var) | `mi-secreto-compartido-seguro` |

> 💡 Usa un valor aleatorio largo (32+ caracteres). Ejemplo: `openssl rand -hex 32`

---

## **4. Contrato de Datos**

### 4.1 Entrada (Request Body)

El backend envía este JSON al webhook:

```json
{
  "sectionCode": "MD.2.1.Cimentacion",
  "projectType": "NewConstruction",
  "technicalContext": {
    "projectTitle": "Vivienda Unifamiliar en Getafe",
    "interventionType": "Obra Nueva",
    "isLoeRequired": true,
    "address": "C/ Mayor 10, Getafe, Madrid",
    "localRegulations": "PGOU Getafe 2003",
    "existingContent": null
  },
  "userInstructions": "Haz énfasis en la impermeabilización."
}
```

| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| `sectionCode` | `string` | ✅ | Código de la sección de la memoria |
| `projectType` | `string` | ✅ | `NewConstruction`, `Reform`, `Extension` |
| `technicalContext` | `object` | ❌ | Metadatos del proyecto |
| `userInstructions` | `string` | ❌ | Instrucciones libres del usuario |

### 4.2 Salida (Response Body)

El workflow debe responder con:

```json
{
  "generatedText": "<p>El sistema de cimentación proyectado se compone de...</p>",
  "usage": {
    "model": "gemini-2.0-flash",
    "tokens": 1250
  }
}
```

| Campo | Tipo | Obligatorio | Descripción |
|-------|------|-------------|-------------|
| `generatedText` | `string` | ✅ | Contenido HTML generado |
| `usage.model` | `string` | ❌ | Modelo utilizado |
| `usage.tokens` | `number` | ❌ | Tokens consumidos |

---

## **5. Importar Workflows en n8n**

### 5.1 Vía Interfaz Web

1. Abre n8n en el navegador (por defecto: `http://localhost:5678`).
2. Ve al menú lateral → **Workflows**.
3. Haz clic en el botón **⋮** (tres puntos) → **Import from File**.
4. Selecciona el archivo JSON deseado:
   - `apps/n8n/workflow-gemini.json` — para Gemini
   - `apps/n8n/workflow-flux.json` — para Flux Gateway
5. El workflow se importará en estado **inactivo**.

### 5.2 Vía CLI (n8n CLI)

```bash
# Importar workflow Gemini
n8n import:workflow --input=apps/n8n/workflow-gemini.json

# Importar workflow Flux
n8n import:workflow --input=apps/n8n/workflow-flux.json
```

### 5.3 Vía API REST de n8n

```bash
curl -X POST http://localhost:5678/api/v1/workflows \
  -H "X-N8N-API-KEY: tu-api-key" \
  -H "Content-Type: application/json" \
  -d @apps/n8n/workflow-gemini.json
```

---

## **6. Variables de Entorno Requeridas**

Configura estas variables de entorno en la instancia de n8n (via docker-compose, `.env`, o la UI de n8n).

### 6.1 Variables Comunes (ambos workflows)

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `EDIFICIA_API_SECRET` | Shared secret para validar cabecera `X-Edificia-Auth` | `a1b2c3d4...` (32+ chars) |

### 6.2 Variables para Gemini

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `GEMINI_API_KEY` | API Key de Google AI Studio | `AIzaSy...` |
| `GEMINI_MODEL` | Modelo a usar (opcional, default: `gemini-2.0-flash`) | `gemini-2.0-flash` |

> 🔑 Obtén tu API Key en: https://aistudio.google.com/apikey

### 6.3 Variables para Flux Gateway

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `FLUX_CLIENT_ID` | Client ID de la aplicación registrada en Flux Gateway | `app_abc123` |
| `FLUX_CLIENT_SECRET` | Client Secret de la aplicación registrada en Flux Gateway | `shh-very-secret` |
| `FLUX_MODEL` | Modelo a usar (opcional, default: `flux-pro`) | `flux-pro` |

### 6.4 Configuración en Docker Compose

Si n8n se ejecuta con Docker Compose, añade las variables al servicio `n8n`:

```yaml
services:
  n8n:
    image: n8nio/n8n:latest
    environment:
      # Seguridad EDIFICIA
      - EDIFICIA_API_SECRET=${N8N_API_SECRET}
      # Para workflow Gemini
      - GEMINI_API_KEY=${GEMINI_API_KEY}
      - GEMINI_MODEL=${GEMINI_MODEL:-gemini-2.0-flash}
      # Para workflow Flux (si se usa)
      - FLUX_CLIENT_ID=${FLUX_CLIENT_ID}
      - FLUX_CLIENT_SECRET=${FLUX_CLIENT_SECRET}
      - FLUX_MODEL=${FLUX_MODEL:-flux-pro}
```

---

## **7. Prompt Engineering**

Ambos workflows construyen el prompt de la misma forma en el nodo **Build Prompt**:

### 7.1 System Prompt

```
Eres un arquitecto técnico experto en redacción de memorias de proyectos de
construcción en España, siguiendo el Código Técnico de la Edificación (CTE) 
y la Ley de Ordenación de la Edificación (LOE). Genera contenido técnico 
profesional, preciso y bien estructurado. Responde siempre en español y en 
formato HTML limpio, sin envolver en bloques de código markdown. No incluyas 
encabezados <h1> ni <h2> del título de la sección; comienza directamente con 
el contenido.
```

### 7.2 User Prompt (dinámico)

Se construye concatenando los campos del `AiGenerationRequest`:

```
Genera el contenido para la sección "MD.2.1.Cimentacion" de una memoria de proyecto.
Tipo de proyecto: NewConstruction
Título del proyecto: Vivienda Unifamiliar en Getafe
Tipo de intervención: Obra Nueva
LOE requerida: Sí
Ubicación: C/ Mayor 10, Getafe, Madrid
Normativa local aplicable: PGOU Getafe 2003

Instrucciones adicionales del usuario: Haz énfasis en la impermeabilización.
```

### 7.3 Personalización

Para modificar el prompt, edita el nodo **Build Prompt** (Code) en el workflow. El system prompt y la lógica de construcción del user prompt están claramente delimitados con comentarios.

---

## **8. Pruebas**

### 8.1 Test Manual con curl

```bash
# Asegúrate de que el workflow esté activo en n8n

curl -X POST http://localhost:5678/webhook/generar-memoria \
  -H "Content-Type: application/json" \
  -H "X-Edificia-Auth: tu-secreto-compartido" \
  -d '{
    "sectionCode": "MD.1.Objeto",
    "projectType": "NewConstruction",
    "technicalContext": {
      "projectTitle": "Test Project",
      "interventionType": "Obra Nueva",
      "isLoeRequired": true,
      "address": "Madrid",
      "localRegulations": null,
      "existingContent": null
    },
    "userInstructions": null
  }'
```

### 8.2 Test de Seguridad (debe devolver 403)

```bash
# Sin cabecera
curl -X POST http://localhost:5678/webhook/generar-memoria \
  -H "Content-Type: application/json" \
  -d '{"sectionCode": "test"}'

# Con cabecera incorrecta
curl -X POST http://localhost:5678/webhook/generar-memoria \
  -H "Content-Type: application/json" \
  -H "X-Edificia-Auth: valor-incorrecto" \
  -d '{"sectionCode": "test"}'
```

Respuesta esperada (403):
```json
{
  "error": "Forbidden",
  "message": "Invalid or missing X-Edificia-Auth header"
}
```

### 8.3 Verificación desde el Backend

1. Configura `appsettings.Local.json`:
   ```json
   {
     "AI": {
       "WebhookUrl": "http://localhost:5678/webhook/generar-memoria",
       "ApiSecret": "tu-secreto-compartido",
       "TimeoutSeconds": 120
     }
   }
   ```
2. Arranca el backend y n8n.
3. Usa el endpoint de generación de sección para disparar el flujo completo.

---

## **9. Cambiar de Proveedor**

Para cambiar entre Gemini y Flux:

1. **Desactivar** el workflow actual en n8n (toggle off).
2. **Activar** el workflow del nuevo proveedor (toggle on).
3. Verificar que las variables de entorno del nuevo proveedor estén configuradas.
4. **No se requiere ningún cambio en el backend** — el contrato es idéntico.

---

## **10. Resolución de Problemas**

| Problema | Causa probable | Solución |
|----------|---------------|----------|
| 403 Forbidden | `EDIFICIA_API_SECRET` no coincide con `AI__ApiSecret` | Verificar que ambos valores son idénticos |
| 500 Error n8n | Variables de entorno no configuradas | Revisar que `GEMINI_API_KEY` o `FLUX_CLIENT_ID`/`FLUX_CLIENT_SECRET` existen |
| Timeout | Modelo lento o red | Aumentar `AI__TimeoutSeconds` y el timeout del nodo HTTP Request |
| Respuesta vacía | El modelo no generó contenido | Revisar ejecución en n8n → Executions para ver el response completo |
| Bloques ```` ```html ```` ``` en respuesta | Modelo envuelve HTML en markdown | El nodo Format Response ya limpia esto automáticamente |
| `EDIFICIA_API_SECRET env var not configured` | Variable no definida en n8n | Añadir la variable en Docker Compose o Settings → Variables |

---

## **11. Nodos por Workflow**

### 11.1 Gemini (8 nodos)

| # | Nodo | Tipo | Función |
|---|------|------|---------|
| 1 | Webhook | `webhook` | Recibe POST `/webhook/generar-memoria` |
| 2 | Validate Auth | `code` | Valida `X-Edificia-Auth` header |
| 3 | Auth Valid? | `if` | Bifurca según validación |
| 4 | Respond 403 | `respondToWebhook` | Devuelve error 403 |
| 5 | Build Prompt | `code` | Construye system + user prompt y body Gemini |
| 6 | Call Gemini | `httpRequest` | POST a Google Gemini API |
| 7 | Format Response | `code` | Normaliza respuesta al contrato |
| 8 | Respond 200 | `respondToWebhook` | Devuelve `{ generatedText, usage }` |

### 11.2 Flux Gateway (10 nodos)

| # | Nodo | Tipo | Función |
|---|------|------|---------|
| 1 | Webhook | `webhook` | Recibe POST `/webhook/generar-memoria` |
| 2 | Validate Auth | `code` | Valida `X-Edificia-Auth` header |
| 3 | Auth Valid? | `if` | Bifurca según validación |
| 4 | Respond 403 | `respondToWebhook` | Devuelve error 403 |
| 5 | Build Prompt | `code` | Construye system + user prompt |
| 6 | Login Flux | `httpRequest` | POST login con email/password |
| 7 | Build Chat Request | `code` | Combina token + prompts en body OpenAI |
| 8 | Chat Completions | `httpRequest` | POST a `/chat/completions` con Bearer |
| 9 | Format Response | `code` | Normaliza respuesta al contrato |
| 10 | Respond 200 | `respondToWebhook` | Devuelve `{ generatedText, usage }` |
