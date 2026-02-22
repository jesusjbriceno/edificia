# **📅 Plan de Implementación Detallado — EdificIA**

**Estado:** MVP completado — pendiente de mejoras post-release

**Objetivo de versión:** MVP escalable y entregable profesional para TFM. Arquitectura preparada para crecer sin reescritura (Clean Architecture + CQRS + IA delegada a n8n).

**Metodología:** Git Flow (feature/... → develop → main)

**Sprint 0:** Configuración y Andamiaje (✅ Completado).

**Progreso Frontend Actual:**
- ✅ Componentes UI atómicos (Button, Input, Card, Badge)
- ✅ Componentes UI avanzados (Dropdown portal-based, Select refactorizado)
- ✅ Flujos de autenticación (Login, ForgotPassword, AuthGuard)
- ✅ Dashboard de Proyectos con Wizard
- ✅ Editor de Memorias con TipTap + Toolbar Premium + Cabecera multi-nivel
- ✅ SidebarNavigation con búsqueda recursiva de capítulos
- ✅ Administración de Usuarios (UserTable, UserRow, UserForm) — sin datos hardcoded
- ✅ Administración de Proyectos (ProjectManagement, ProjectRow, ProjectForm)
- ✅ Sistema de Notificaciones completo (NotificationBell, NotificationsList, notificationService → API)
- ✅ Página `/admin/notifications` para gestión de notificaciones
- ✅ Stores Zustand (useAuthStore, useEditorStore)
- ✅ Suite de tests centralizada en src/tests (Vitest)
- ✅ .github/copilot-instructions.md con guías del proyecto para GitHub Copilot

**Progreso Backend Actual:**
- ✅ Entidad `Notification` en Domain con métodos de fábrica y `MarkAsRead()`
- ✅ CQRS completo para notificaciones: `GetNotificationsQuery`, `MarkAsReadCommand`, `MarkAllAsReadCommand`
- ✅ `NotificationsController` con endpoints `GET /notifications`, `POST /{id}/read`, `POST /mark-all-read`
- ✅ `NotificationConfiguration` (EF Core Fluent API)
- ✅ Integración IA delegada a webhooks n8n (Flux Gateway y Google Gemini, intercambiables vía `AI_WEBHOOK_URL`)
- ✅ Exportación a DOCX funcional (OpenXml, mapeo TipTap → Word)
- ✅ Envío de emails operativo con Brevo (SMTP como fallback)

## **🏁 Fase 1: Cimientos del Sistema (Core & Shared)**

**Objetivo:** Establecer los patrones base, la base de datos y la gestión de errores.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **1.1** | feature/shared-kernel | • Implementar Result\<T\> pattern en Edificia.Shared. • Crear Excepciones de Dominio base. • Configurar GlobalExceptionHandler en API. | • Configurar axios o fetch wrapper con manejo de errores unificado. • Definir tipos base de respuesta API (Result). |
| **1.2** | feature/infra-persistence | • Configurar EdificiaDbContext con SnakeCase naming. • Implementar UnitOfWork (si aplica) o inyección de DbContext. • Configurar conexión Dapper en Infrastructure. | • N/A |
| **1.3** | feature/api-swagger | • Configurar Swagger con soporte para JWT (aunque se usará más tarde). • Definir ProblemDetails según RFC 7807\. | • Generar cliente API inicial (o tipos manuales) basados en Swagger. |

## **🏗️ Fase 2: Gestión de Proyectos (El CRUD)**

**Objetivo:** Que el usuario pueda crear, listar y configurar la estrategia de su proyecto.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **2.1** | feature/project-domain | • Definir Entidad Project (con Enums InterventionType). • Crear Migración EF Core (InitialCreate). • Crear CreateProjectCommand \+ Validador Fluent. | • Crear Zod Schema ProjectSchema. • Maquetar componentes UI: Card, Button, Badge (Tailwind v4). |
| **2.2** | feature/project-read | • Implementar GetProjectsQuery con Dapper (paginado). • Implementar GetProjectByIdQuery. | • Crear DashboardLayout.astro. • Implementar página dashboard.astro con Grid de proyectos. • Conectar API GET /projects. |
| **2.3** | feature/project-wizard | • Ajustar CreateProjectCommand para recibir IsLoeRequired. | • Implementar **Wizard React** (Modal): 1\. Datos Básicos. 2\. Selector (Obra Nueva vs Reforma). 3\. Normativa Local. • Conectar POST /projects. |

## **🔧 Fase 3.0: Refactor Previo \- Repositorio Base**

**Objetivo:** Crear `IBaseRepository<T>` y `BaseRepository<T>` genéricos para evitar duplicación en futuros repositorios. Se aplica antes de avanzar a nuevas entidades.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **3.0** | feature/base-repository | • Crear `IBaseRepository<T>` en Application/Interfaces con `GetByIdAsync`, `AddAsync`, `SaveChangesAsync`. • Crear `BaseRepository<T>` en Infrastructure con implementación EF Core genérica. • Refactorizar `IProjectRepository` e `ProjectRepository` para heredar del repositorio base. • Verificar que todos los tests siguen pasando. | • N/A |

## **🧠 Fase 3: El Motor de Normativa (JSON Engine)**

**Objetivo:** Renderizar el árbol de capítulos filtrado según la estrategia del proyecto.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **3.1** | feature/normative-tree | • Crear estructura JSON ContentTree en Entidad. • Endpoint GET /projects/{id}/tree. | • Crear archivo estático cte\_2024.json en /public. • Implementar utilidad TS filterTree(nodes, config) para ocultar ramas según Obra/Reforma. |
| **3.2** | feature/editor-shell | N/A | ✅ • Crear EditorLayout.astro. • Implementar **Sidebar de Navegación** (React) recursivo con enlaces Admin. • Gestionar selección de capítulo activo en Zustand (`useEditorStore`). • Añadir **búsqueda recursiva** de capítulos con filtrado en tiempo real (`searchTree`). |

## **📝 Fase 4: Editor y Persistencia (The Core)**

**Objetivo:** Escribir contenido y guardarlo (Offline first).

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **4.1** | feature/editor-tiptap | N/A | ✅ • Integrar **TipTap** en `EditorShell`. • Crear `EditorToolbar` con formato (Negrita, Cursiva, H1-H3, Listas, Citas, Undo/Redo). • Conectar editor al Store de Zustand. • `EditorHeader` multi-nivel con breadcrumbs contextuales (proyecto, tipo de intervención). |
| **4.2** | feature/offline-sync | • Crear endpoint PATCH /projects/{id}/sections. • Optimizar update con ExecuteUpdate de EF Core o SQL Raw para JSONB. | ⏳ • Configurar idb-keyval en Zustand. • Implementar lógica "Debounce Save": Guardar en local al escribir, sincronizar con API cada 5s si hay red. |

## **🔔 Fase 4.3: Sistema de Notificaciones (Completado)**

**Objetivo:** Sistema completo de notificaciones en tiempo real para alertar al usuario de eventos relevantes.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **4.3** | feature/pre-release-fixes | ✅ • Entidad `Notification` (Domain) con `Create()`, `MarkAsRead()`. • `NotificationConfiguration` (EF Core). • `GetNotificationsQuery` con Dapper. • `MarkAsReadCommand` + `MarkAllAsReadCommand`. • `NotificationsController`: `GET /notifications`, `POST /{id}/read`, `POST /mark-all-read`. | ✅ • `NotificationBell` (icono con contador de no leídas). • `NotificationsList` (dropdown con lista paginada). • `notificationService` conectado a API real. • Página `/admin/notifications` para administración. • Tests unitarios para `NotificationBell`, `NotificationsList` y `notificationService`. |

## **🤖 Fase 5: Inteligencia Artificial (Flux Gateway) — ✅ Completada**

**Decisión de arquitectura:** La integración IA se implementó mediante **delegación a webhooks n8n**, no llamando directamente a los modelos desde el backend. El backend solo hace `POST` al webhook configurado en `AI_WEBHOOK_URL`. El flujo n8n activo (Flux Gateway o Google Gemini) es intercambiable sin cambios en el código.

| ID | Feature Branch | Estado | Notas |
| :---- | :---- | :---- | :---- |
| **5.1** | feature/ai-infrastructure | ✅ | `FluxAiService` en Infrastructure. Webhook OAuth2 con caché de token. `GenerateTextCommand`. |
| **5.2** | feature/prompt-engine | ✅ | Templates de prompts con contexto Nueva/Reforma. Botón AiAssistant conectado a `POST /ai/generate`. Inserción en TipTap. |

## **📤 Fase 6: Exportación y Cierre**

**Objetivo:** Salida física del entregable.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **6.1** | feature/export-docx | ✅ Implementado. Servicio OpenXml. Mapeo JSON TipTap → Estilos Word. Endpoint `GET /export`. | ✅ Botón "Exportar" en TopBar. Descarga de Blob. |
| **6.1.1** | feature/export-dotx-template | • Permitir cargar un archivo `.dotx` (plantilla Word) que aplique estilos corporativos al documento exportado. • El servicio OpenXml abrirá el `.dotx` como base antes de mapear el contenido TipTap. • Almacenar la plantilla en Infrastructure (ruta configurable vía `Export__TemplatePath`). | • Añadir en ajustes de proyecto o configuración global un selector de archivo `.dotx`. |
| **6.2** | feature/polish-ui | • Ajuste de validaciones finales. • Logging y métricas. | • Pantallas de carga (Skeletons). • Página 404 y Error Boundaries. |

## **� Fase 7: Refactor \- Mapeos y Limpieza**

**Objetivo:** Centralizar los mapeos Request/DTO → Command/Query mediante operadores de conversión explícitos, aligerando los controladores y mejorando la mantenibilidad.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **7.1** | feature/refactor-mappings | • Añadir operadores `explicit operator` en cada Command/Query para convertir desde su Request DTO correspondiente (ej: `CreateProjectCommand` ← `CreateProjectRequest`). • Refactorizar todos los Controllers para usar los operadores en lugar de mapeos manuales inline. • Verificar que todos los tests siguen pasando. | • N/A |
| **7.2** | feature/refactor-sql-constants | • Extraer todas las consultas SQL raw de los Query Handlers de Dapper a clases de constantes centralizadas (ej: `ProjectQueries.cs` con `GetById`, `GetPaged`, `Count`). • Refactorizar los Handlers para referenciar las constantes en lugar de SQL inline. • Verificar que todos los tests siguen pasando. | • N/A |

**Contexto:** Según AGENTS.md, el mapeo debe ser **manual con operadores explícitos** (PROHIBIDO AutoMapper). La Feature 7.1 consolida los mapeos dispersos en los controllers dentro de los propios Commands/Queries. La Feature 7.2 centraliza las queries SQL de Dapper en ficheros de constantes por agregado, facilitando la revisión, reutilización y mantenimiento del SQL.

---

## **📋 Fase 8: Flujo de Revisión y Validación de Memorias**

**Objetivo:** Implementar el ciclo de vida completo de una memoria: Borrador → Pendiente de Revisión → Completado/Rechazado. Los editores (Architect/Collaborator) envían a revisión, los administradores (Admin) validan o rechazan.

**Reglas de Negocio:**

- **R-REV-1:** Un editor de la memoria (Architect, Collaborator con rol Editor/Owner en el proyecto) puede enviar el proyecto a revisión. El estado pasa de `Draft` o `InProgress` → `PendingReview`.
- **R-REV-2:** Solo usuarios con rol de aplicación `Admin` pueden aprobar o rechazar un proyecto en estado `PendingReview`.
- **R-REV-3:** Aprobar un proyecto cambia su estado a `Completed`. Se genera una notificación al creador y editores del proyecto.
- **R-REV-4:** Rechazar un proyecto lo devuelve a `Draft`. Requiere un motivo obligatorio que se incluye en la notificación al creador y editores.
- **R-REV-5:** Los proyectos en `PendingReview` son de solo lectura (no se puede editar el contenido de la memoria).
- **R-REV-6:** El dashboard del Admin muestra primero los proyectos pendientes de revisión como bandeja de tareas, y luego los últimos proyectos completados.
- **R-REV-7:** El botón "Enviar a Revisión" se muestra en la vista del editor (EditorShell) y en el modal de detalles del proyecto (ProjectDetailsModal), solo si el proyecto está en `Draft` o `InProgress`.
- **R-REV-8:** Un proyecto `Completed` puede ser archivado. Un proyecto `Archived` no puede cambiar de estado.

### **8.1 — Modelo de dominio: nuevo estado `PendingReview`**

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.1.1** | feature/review-workflow | • Añadir `PendingReview = 4` al enum `ProjectStatus`. • Añadir método `SubmitForReview()` en `Project.cs` que valide que el estado actual sea `Draft` o `InProgress` y lo cambie a `PendingReview`. • Añadir método `Reject()` que valide estado `PendingReview` → `Draft`. • Modificar `Complete()` para que solo permita transición desde `PendingReview`. • Añadir validación en `UpdateSectionContent()` para rechazar ediciones si `Status == PendingReview` o `Completed`. • Tests unitarios para todas las transiciones de estado (válidas e inválidas). | • Añadir `PendingReview` al tipo `ProjectStatus` en `lib/types.ts`. • Añadir variante de badge para `PendingReview` (`warning` o `purple`) en `ProjectCard` y `ProjectRow`. |

### **8.2 — Commands de cambio de estado**

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.2.1** | feature/review-workflow | • Crear `SubmitForReviewCommand(ProjectId)` + Handler: buscar proyecto, llamar `SubmitForReview()`, guardar, crear notificaciones para todos los Admin activos (título: "Proyecto pendiente de revisión", mensaje con nombre del proyecto y quién lo envía). • Crear `SubmitForReviewValidator`: ProjectId requerido. | • Añadir método `submitForReview(projectId)` en `projectService.ts` → `POST /projects/:id/submit-review`. |
| **8.2.2** | feature/review-workflow | • Crear `ApproveProjectCommand(ProjectId)` + Handler: validar que sea Admin (inyectar `IHttpContextAccessor` o pasar userId), buscar proyecto, llamar `Complete()`, guardar, crear notificaciones al creador y editores del proyecto. • Crear `ApproveProjectValidator`. | • Añadir método `approve(projectId)` en `projectService.ts` → `POST /projects/:id/approve`. |
| **8.2.3** | feature/review-workflow | • Crear `RejectProjectCommand(ProjectId, Reason)` + Handler: validar que sea Admin, buscar proyecto, llamar `Reject()`, guardar, crear notificaciones al creador y editores con el motivo de rechazo. • Crear `RejectProjectValidator`: ProjectId y Reason requeridos, Reason máx. 500 caracteres. | • Añadir método `reject(projectId, reason)` en `projectService.ts` → `POST /projects/:id/reject`. |

### **8.3 — Endpoints API**

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.3.1** | feature/review-workflow | • Añadir endpoint `POST /projects/{id}/submit-review` en `ProjectsController`. Requiere policy `ActiveUser`. Extraer userId del JWT para la notificación. | • N/A (ya cubierto por 8.2.1). |
| **8.3.2** | feature/review-workflow | • Añadir endpoint `POST /projects/{id}/approve` en `ProjectsController`. Requiere policy `RequireAdmin`. | • N/A (ya cubierto por 8.2.2). |
| **8.3.3** | feature/review-workflow | • Añadir endpoint `POST /projects/{id}/reject` en `ProjectsController`. Requiere policy `RequireAdmin`. Recibe `{ reason: string }` en body. | • N/A (ya cubierto por 8.2.3). |
| **8.3.4** | feature/review-workflow | • Actualizar `GetProjectsQuery` y `GetProjectsValidator` para aceptar `PendingReview` como valor de filtro de status. • Crear query `GetPendingReviewProjectsQuery` (Dapper) que retorne proyectos con `status = 'PendingReview'` ordenados por fecha de envío a revisión (más antiguos primero). | • N/A. |

### **8.4 — Frontend: Botón "Enviar a Revisión"**

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.4.1** | feature/review-workflow | • N/A | • **EditorShell.tsx**: Añadir botón "Enviar a Revisión" (icono `SendHorizonal` o similar) en la barra superior, junto al botón de exportar. Visible solo si `status` es `Draft` o `InProgress`. Al pulsar: confirmación modal → `projectService.submitForReview()` → toast de éxito → recargar estado. • Almacenar `projectStatus` en `useEditorStore` (nuevo campo de estado). |
| **8.4.2** | feature/review-workflow | • N/A | • **ProjectDetailsModal.tsx**: Añadir botón "Enviar a Revisión" en el footer del modal, junto a "Continuar con la Memoria". Visible solo si el proyecto está en `Draft` o `InProgress`. Misma lógica de confirmación y llamada a API. |
| **8.4.3** | feature/review-workflow | • N/A | • **EditorShell.tsx / EditorToolbar.tsx**: Si `projectStatus === 'PendingReview'` o `'Completed'`, deshabilitar el editor TipTap (modo solo lectura). Mostrar un banner informativo: "Esta memoria está pendiente de revisión" o "Esta memoria ha sido aprobada". |

### **8.5 — Frontend: Dashboard del Admin con bandeja de revisión**

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.5.1** | feature/review-workflow | • N/A | • **Crear componente `ReviewQueue.tsx`**: Lista/tabla de proyectos con `status: PendingReview`. Muestra: título, autor (createdByUser), fecha de envío, tipo de intervención. Cada fila tiene botones "Aprobar" y "Rechazar". "Aprobar" con confirmación modal. "Rechazar" con modal que solicita motivo (textarea obligatorio, máx. 500 chars). |
| **8.5.2** | feature/review-workflow | • N/A | • **Modificar `DashboardProjects.tsx`** (o crear vista alternativa para Admin): Si el usuario es Admin, mostrar primero la sección "Proyectos Pendientes de Revisión" (`ReviewQueue`) y debajo "Últimos Proyectos Completados" (grid filtrado por status `Completed`, orden por `updatedAt` desc). Si no es Admin, mantener el dashboard actual. |
| **8.5.3** | feature/review-workflow | • N/A | • **Actualizar `ProjectManagement.tsx`** (admin): Añadir `PendingReview` al dropdown de filtro de estado. Añadir acciones "Aprobar" y "Rechazar" en `ProjectActionsDropdown` cuando el proyecto está en `PendingReview` y el usuario es Admin. |

### **8.6 — Tests**

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.6.1** | feature/review-workflow | • Tests unitarios Domain: `SubmitForReview()` desde Draft/InProgress OK, desde Completed/Archived falla. `Complete()` solo desde PendingReview. `Reject()` solo desde PendingReview → Draft. `UpdateSectionContent()` rechazado si PendingReview/Completed. | • N/A |
| **8.6.2** | feature/review-workflow | • Tests Application: Handler `SubmitForReview` genera notificaciones a Admins. Handler `Approve` genera notificaciones a editores. Handler `Reject` genera notificaciones con motivo. Validadores correctos. | • Tests Vitest: botón "Enviar a Revisión" visible/oculto según estado. `ReviewQueue` renderiza correctamente. Badge `PendingReview` muestra variante correcta. |

### **Resumen de impacto técnico**

**Backend (API .NET):**

| Componente | Cambio |
|---|---|
| `ProjectStatus.cs` | Nuevo valor: `PendingReview = 4` |
| `Project.cs` | Nuevos métodos: `SubmitForReview()`, `Reject()`. Modificar `Complete()` y `UpdateSectionContent()` con guardas de estado |
| Nuevos Commands (3) | `SubmitForReviewCommand`, `ApproveProjectCommand`, `RejectProjectCommand` + Handlers + Validators |
| `ProjectsController.cs` | 3 nuevos endpoints: `POST submit-review`, `POST approve`, `POST reject` |
| `GetProjectsValidator.cs` | Aceptar `PendingReview` en `AllowedStatuses` |
| `Notification` | Generación automática de notificaciones en los handlers de aprobación/rechazo |
| Request DTOs | Nuevo: `RejectProjectRequest { Reason }` |
| SQL Queries (Dapper) | Nueva query para proyectos pendientes de revisión |

**Frontend (Astro/React):**

| Componente | Cambio |
|---|---|
| `lib/types.ts` | Añadir `PendingReview` a `ProjectStatus` |
| `projectService.ts` | 3 nuevos métodos: `submitForReview()`, `approve()`, `reject()` |
| `useEditorStore.ts` | Nuevo campo: `projectStatus` |
| `EditorShell.tsx` | Botón "Enviar a Revisión" + modo solo lectura si PendingReview/Completed |
| `ProjectDetailsModal.tsx` | Botón "Enviar a Revisión" |
| `ProjectCard.tsx` / `ProjectRow.tsx` | Badge para `PendingReview` |
| `ReviewQueue.tsx` | **Nuevo componente** — bandeja de revisión para Admin |
| `DashboardProjects.tsx` | Sección prioritaria de pendientes para Admin |
| `ProjectManagement.tsx` | Filtro + acciones de aprobación/rechazo |
| `ProjectActionsDropdown.tsx` | Acciones contextuales según estado + rol |

**Diagrama de transiciones de estado:**

```
    ┌─────────┐
    │  Draft  │◄──────────────────┐
    └────┬────┘                   │
         │ (editar)               │ Reject(reason)
         ▼                        │
   ┌───────────┐                  │
   │ InProgress│                  │
   └─────┬─────┘                  │
         │                        │
         │ SubmitForReview()      │
         ▼                        │
  ┌──────────────┐                │
  │PendingReview │────────────────┘
  │  (readonly)  │
  └──────┬───────┘
         │ Approve()
         ▼
   ┌───────────┐
   │ Completed │
   └─────┬─────┘
         │ Archive()
         ▼
   ┌──────────┐
   │ Archived │
   └──────────┘
```

## **🚦 Definición de Hecho (DoD)**

Para considerar una **Feature** cerrada:

1. \[ \] Código compila sin warnings.  
2. \[ \] Tests unitarios (xUnit/Vitest) en verde.  
3. \[ \] Clean Architecture respetada (dependencias correctas).  
4. \[ \] Validaciones (Fluent/Zod) implementadas.  
5. \[ \] Funciona en Docker (docker-compose up).

---

## **🔮 Fase 9: Mejoras Post-Release (Backlog)**

> Funcionalidades identificadas para versiones futuras. No bloquean la v1.0.0.

### **9.1 — Soporte para múltiples normativas**

| ID | Feature Branch | Descripción |
| :---- | :---- | :---- |
| **9.1.1** | feature/multi-normativa | Actualmente solo existe `cte_2024.json` en `/public/normativa/`. Permitir cargar y seleccionar otras normativas (p. ej. normativas autonómicas, versiones anteriores del CTE, RITE). El wizard de creación de proyecto incluiría un selector de normativa. El árbol de contenidos se filtraría según la normativa activa del proyecto. |

### **9.2 — Delegación de emails a n8n**

| ID | Feature Branch | Descripción |
| :---- | :---- | :---- |
| **9.2.1** | feature/email-n8n-delegation | **Estado actual:** Brevo funciona correctamente como proveedor principal. SMTP disponible como alternativa. **Propuesta:** Reemplazar `IEmailService` por `IEmailDispatcherService` que hace `POST` a un webhook n8n. El flujo n8n gestionaría: selección de plantilla HTML por `templateType` (`welcome`, `password-reset`, `notification`), envío por Brevo con fallback a SMTP, y registro de trazabilidad. Simplifica el código backend y centraliza la lógica de envío. Ver especificación completa en [`docs/features/MEJORA_EMAIL_N8N.md`](../features/MEJORA_EMAIL_N8N.md). |

### **9.3 — Soporte para IAs locales (Ollama / LM Studio)**

| ID | Feature Branch | Descripción |
| :---- | :---- | :---- |
| **9.3.1** | feature/ai-local-ollama | **Motivación:** Permitir ejecutar la IA completamente offline o en entornos sin acceso a APIs externas (privacidad, costes). **Implementación propuesta:** Crear un nuevo flujo n8n que actúe como adaptador hacia Ollama o LM Studio (ambos exponen una API REST compatible con OpenAI). El backend no requiere cambios: solo actualizar `AI_WEBHOOK_URL` en las variables de entorno al nuevo webhook. El flujo n8n seleccionaría el modelo local y adaptaría el prompt. |