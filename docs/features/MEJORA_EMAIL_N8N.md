# 📧 Mejora Futura: Delegación del Envío de Emails a n8n

> **Estado:** Pendiente de implementación  
> **Prioridad:** Media  
> **Origen:** Detección durante corrección de bug USERS_CREATE_01 (timeout por envío SMTP síncrono)

---

## 1. Contexto y Motivación

Actualmente, el envío de emails (bienvenida, reset de contraseña, etc.) se realiza de forma **fire-and-forget** directamente desde los handlers de la API .NET, utilizando el servicio `IEmailService` registrado en `Edificia.Infrastructure`.

### Problemas del enfoque actual

1. **Sin garantía de entrega:** Si el envío falla (SMTP caído, credenciales expiradas), el email se pierde silenciosamente.
2. **Sin reintentos:** No existe lógica de retry ante fallos transitorios.
3. **Sin fallback:** Si el proveedor SMTP principal falla, no hay alternativa automática.
4. **Sin trazabilidad:** No se registra si el email fue enviado, falló o está pendiente.
5. **Acoplamiento:** La lógica de envío está embebida en los handlers de dominio.

---

## 2. Propuesta: Delegación a n8n

### Arquitectura objetivo

```
[API .NET] --HTTP POST--> [n8n Webhook] --> [Lógica de envío con fallback]
                                                  |
                                           ┌──────┴──────┐
                                           │              │
                                       [SMTP/Brevo]  [Google SMTP]
                                       (Principal)    (Fallback)
```

### Flujo propuesto

1. **API .NET** envía un POST al webhook de n8n con:
   - `templateType`: Tipo de plantilla (`welcome`, `password-reset`, `notification`, etc.)
   - `recipient`: Email destinatario
   - `data`: Objeto con las variables de la plantilla (nombre, contraseña temporal, etc.)
   - `metadata`: Información de trazabilidad (userId, requestId, timestamp)

2. **n8n Workflow** recibe la solicitud y:
   - Selecciona la plantilla HTML según `templateType`
   - Renderiza el contenido con las variables de `data`
   - Intenta envío por el **proveedor principal** (SMTP/Brevo)
   - Si falla → **fallback automático** a Google SMTP (OAuth2 o App Password)
   - Registra el resultado (éxito/fallo) en logs o base de datos
   - Opcionalmente, actualiza un campo de estado en la API

---

## 3. Cambios Necesarios

### 3.1. Backend (.NET)

- **Nuevo servicio:** `IEmailDispatcherService` con método `DispatchAsync(EmailRequest request)`
  - Envía HTTP POST al webhook de n8n
  - Incluye autenticación (API Key o token compartido)
  - Mantiene fire-and-forget desde el handler

- **DTO de envío:**
  ```csharp
  public sealed record EmailDispatchRequest(
      string TemplateType,    // "welcome" | "password-reset" | "notification"
      string Recipient,
      Dictionary<string, string> Data,
      Guid? UserId,
      string? RequestId
  );
  ```

- **Reemplazar** las llamadas directas a `IEmailService.SendAsync()` por `IEmailDispatcherService.DispatchAsync()`

### 3.2. n8n Workflow

- **Webhook receptor:** Endpoint autenticado que recibe las solicitudes
- **Nodo de plantillas:** Renderizado de templates HTML por tipo
- **Nodo SMTP principal:** Envío vía Brevo/SMTP configurado
- **Nodo de error + fallback:** Si falla el principal, envío vía Google SMTP
- **Nodo de logging:** Registro del resultado en PostgreSQL o endpoint de callback

### 3.3. Plantillas de Email

Definir plantillas HTML para cada `templateType`:

| Template Type      | Variables                              | Uso                          |
|--------------------|----------------------------------------|------------------------------|
| `welcome`          | `fullName`, `email`, `temporaryPassword` | Creación de usuario          |
| `password-reset`   | `fullName`, `temporaryPassword`        | Recuperación de contraseña   |
| `password-changed` | `fullName`                             | Confirmación de cambio       |
| `notification`     | `title`, `message`, `actionUrl`        | Notificaciones generales     |

### 3.4. Configuración

```json
// appsettings.json
{
  "EmailDispatcher": {
    "Provider": "n8n",
    "N8nWebhookUrl": "https://n8n.edificia.jesusjbriceno.dev/webhook/email",
    "ApiKey": "...",
    "FallbackToLocal": true
  }
}
```

---

## 4. Beneficios

- ✅ **Reintentos automáticos** configurables en n8n
- ✅ **Fallback transparente** SMTP → Google
- ✅ **Trazabilidad completa** de emails enviados/fallidos
- ✅ **Desacoplamiento** de la lógica de envío del dominio
- ✅ **Plantillas centralizadas** y editables sin redespliegue de la API
- ✅ **Escalabilidad** — n8n puede gestionar colas y rate limiting

---

## 5. Riesgos y Mitigaciones

| Riesgo                         | Mitigación                                          |
|--------------------------------|-----------------------------------------------------|
| n8n no disponible              | `FallbackToLocal: true` — envío directo desde .NET  |
| Latencia webhook               | Fire-and-forget, no bloquea la respuesta HTTP       |
| Seguridad datos en tránsito    | HTTPS + API Key en headers                          |
| Pérdida de emails en cola      | Persistencia de jobs en n8n + dead-letter logging   |

---

## 6. Criterios de Aceptación

- [ ] Los emails de bienvenida se envían correctamente vía n8n
- [ ] Los emails de reset de contraseña se envían correctamente vía n8n
- [ ] Si SMTP principal falla, se usa Google SMTP como fallback
- [ ] Se registra en logs cada intento de envío (éxito/fallo/fallback)
- [ ] Si n8n no está disponible, la API envía directamente (fallback local)
- [ ] No hay impacto en el tiempo de respuesta de los endpoints de la API
