# **📅 Plan de Implementación Detallado — EDIFICIA**

**Estado:** En Progreso (Frontend: Fases 1-5 completadas + mejoras pre-release)

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

## **🔔 Fase 4.3: Sistema de Notificaciones (Parcialmente implementado — Pendiente)**

**Objetivo:** Sistema completo de notificaciones en tiempo real para alertar al usuario de eventos relevantes.

**Estado actual:** La infraestructura de lectura y marcado está implementada, pero **no existe ningún mecanismo para crear notificaciones**. La entidad, los endpoints de consulta/marcado, la campanita del frontend y los tests del servicio están listos, pero el sistema es inerte porque `Notification.Create()` nunca se invoca desde ningún punto del código.

**Gaps identificados:**

| Área | Implementado | Pendiente |
| :---- | :---- | :---- |
| **Backend — Lectura** | ✅ `GetNotificationsQuery` (Dapper, LIMIT 50) | Paginación real |
| **Backend — Marcado** | ✅ `MarkAsReadCommand`, `MarkAllAsReadCommand` | — |
| **Backend — Creación** | ❌ | `CreateNotificationCommand` o `INotificationService` que invoque `Notification.Create()` |
| **Backend — Eliminación** | ❌ | Endpoints `DELETE /{id}` y `POST /clear-all` (el front los llama) |
| **Backend — Emisión** | ❌ | Definir puntos de emisión: ¿al crear proyecto? ¿al exportar? ¿al asignar usuario? |
| **Backend — Tipado** | ❌ | `NotificationType` enum (informativa, alerta, sistema) |
| **Backend — Validators** | ❌ | FluentValidation para commands |
| **Backend — Tests** | ❌ | Tests xUnit para handlers y entidad (0 tests backend actualmente) |
| **Frontend — Campanita** | ✅ `NotificationBell` + `NotificationsList` | Real-time (polling o SSE) |
| **Frontend — Endpoints fantasma** | ❌ `delete()`, `clearAll()`, `addTestNotification()` llaman endpoints inexistentes | Implementar en backend o eliminar del front |
| **Email** | ❌ | Integrar envío de email con creación de notificaciones (infraestructura de email ya existe) |

**Dependencia:** La emisión de notificaciones depende de la resolución de la Fase 8 (Gestión de Usuarios y Propiedad de Proyectos), ya que es necesario saber **a quién** se notifica (propietario del proyecto, usuarios asignados, administradores).

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **4.3** | feature/pre-release-fixes | ✅ • Entidad `Notification` (Domain) con `Create()`, `MarkAsRead()`. • `NotificationConfiguration` (EF Core). • `GetNotificationsQuery` con Dapper. • `MarkAsReadCommand` + `MarkAllAsReadCommand`. • `NotificationsController`: `GET /notifications`, `POST /{id}/read`, `POST /mark-all-read`. | ✅ • `NotificationBell` (icono con contador de no leídas). • `NotificationsList` (dropdown con lista paginada). • `notificationService` conectado a API real. • Página `/admin/notifications` para administración. • Tests unitarios para `NotificationBell`, `NotificationsList` y `notificationService`. |
| **4.3.1** | *pendiente de definir* | ⏳ *Pendiente de definir tras resolver Fase 8* | ⏳ *Pendiente* |

## **🤖 Fase 5: Inteligencia Artificial (Flux Gateway)**

**Objetivo:** Asistencia a la redacción segura.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **5.1** | feature/ai-infrastructure | • Implementar FluxAiService en Infra. • Configurar HttpClient y Caché de Tokens OAuth2. • Crear GenerateTextCommand. | • Crear componente AiAssistantButton. • Maquetar Modal de "Generando...". |
| **5.2** | feature/prompt-engine | • Crear sistema de Templates de Prompts. • Inyectar contexto (Nueva/Reforma) en el prompt. | • Conectar botón a endpoint POST /ai/generate. • Insertar respuesta en TipTap stream/texto. |

## **📤 Fase 6: Exportación y Cierre**

**Objetivo:** Salida física del entregable.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **6.1** | feature/export-docx | • Implementar servicio OpenXml. • Mapear JSON TipTap \-\> Estilos Word. • Endpoint GET /export. | • Botón "Exportar" en la TopBar. • Manejo de descarga de Blob. |
| **6.2** | feature/polish-ui | • Ajuste de validaciones finales. • Logging y métricas. | • Pantallas de carga (Skeletons). • Página 404 y Error Boundaries. |

## **� Fase 7: Refactor \- Mapeos y Limpieza**

**Objetivo:** Centralizar los mapeos Request/DTO → Command/Query mediante operadores de conversión explícitos, aligerando los controladores y mejorando la mantenibilidad.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **7.1** | feature/refactor-mappings | • Añadir operadores `explicit operator` en cada Command/Query para convertir desde su Request DTO correspondiente (ej: `CreateProjectCommand` ← `CreateProjectRequest`). • Refactorizar todos los Controllers para usar los operadores en lugar de mapeos manuales inline. • Verificar que todos los tests siguen pasando. | • N/A |
| **7.2** | feature/refactor-sql-constants | • Extraer todas las consultas SQL raw de los Query Handlers de Dapper a clases de constantes centralizadas (ej: `ProjectQueries.cs` con `GetById`, `GetPaged`, `Count`). • Refactorizar los Handlers para referenciar las constantes en lugar de SQL inline. • Verificar que todos los tests siguen pasando. | • N/A |

**Contexto:** Según AGENTS.md, el mapeo debe ser **manual con operadores explícitos** (PROHIBIDO AutoMapper). La Feature 7.1 consolida los mapeos dispersos en los controllers dentro de los propios Commands/Queries. La Feature 7.2 centraliza las queries SQL de Dapper en ficheros de constantes por agregado, facilitando la revisión, reutilización y mantenimiento del SQL.

## **👥 Fase 8: Gestión de Usuarios, Propiedad de Proyectos y Permisos**

**Objetivo:** Establecer la relación entre usuarios y proyectos, definir quién puede ver/editar qué, y completar los flujos de administración de usuarios.

### Decisiones Arquitectónicas (Aprobadas)

**Modelo híbrido:** `CreatedByUserId` en Project + tabla `project_members` para acceso compartido/asignación.

**Reglas de negocio:**

| Regla | Descripción |
| :---- | :---- |
| **R1** | Un Admin es también un arquitecto potencial. Es el "Project Manager": puede crear, asignar, supervisar y validar cualquier proyecto. |
| **R2** | Un Architect puede crear proyectos. Al crearlos, se comparten automáticamente con todos los Admin activos. |
| **R3** | Un Architect puede compartir sus proyectos con otros Architects y Collaborators. |
| **R4** | Un Admin puede asignar Architects y Collaborators a cualquier proyecto, aunque no lo haya creado él. |
| **R5** | Un Admin valida/aprueba los proyectos finalizados (nuevo estado `PendingValidation` entre `Completed` y `Archived`). |
| **R6** | Root gestiona el sistema (usuarios, configuración) pero no aparece en la lógica de proyectos. |
| **R7** | Un Collaborator NO puede crear proyectos. Solo puede editar los que le asignen. |
| **R8** | Cada usuario solo ve los proyectos donde es creador o miembro (excepto Admin, que ve todos). |

**Matriz de permisos sobre proyectos:**

| Acción | Root | Admin | Architect | Collaborator |
| :---- | :---- | :---- | :---- | :---- |
| Crear proyecto | ❌ | ✅ | ✅ | ❌ |
| Ver cualquier proyecto | ❌ (no opera con proyectos) | ✅ (todos) | Solo los suyos / compartidos | Solo los asignados |
| Editar proyecto | ❌ | ✅ | ✅ (los suyos / compartidos) | ✅ (solo secciones asignadas) |
| Compartir proyecto | ❌ | ✅ | ✅ (los suyos) | ❌ |
| Asignar miembros | ❌ | ✅ (cualquier proyecto) | ❌ | ❌ |
| Exportar memoria | ❌ | ✅ | ✅ (los suyos) | ❌ |
| Enviar a validación | ❌ | ✅ | ✅ (los suyos) | ❌ |
| Validar/aprobar | ❌ | ✅ | ❌ | ❌ |
| Archivar | ❌ | ✅ | ❌ | ❌ |

---

### 8.1 Propiedad de Proyectos (Domain + Infrastructure)

**Descripción:** Añadir propiedad y membresía a proyectos. Modelo de datos nuevo.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.1.1** | feature/project-ownership | • Añadir `CreatedByUserId` (Guid, required) a entidad `Project`. • Crear entidad `ProjectMember` en Domain (`ProjectId`, `UserId`, `Role` enum: `Owner`, `Editor`, `Viewer`). • Crear enum `ProjectMemberRole` en Domain/Enums. • Crear `ProjectMemberConfiguration` en Infrastructure (tabla `project_members`, PK compuesto `ProjectId+UserId`, índices). • Migración EF Core: `AddProjectOwnership`. • Actualizar `ProjectConfiguration` para incluir `CreatedByUserId` (FK a `asp_net_users`). | • N/A (datos, sin UI aún) |
| **8.1.2** | feature/project-ownership | • Actualizar `CreateProjectCommand` para incluir `CreatedByUserId`. • Actualizar `CreateProjectHandler` para: asignar `CreatedByUserId`, crear automáticamente `ProjectMember(Owner)` para el creador, crear `ProjectMember(Editor)` para todos los Admin activos (R2). • Actualizar `ProjectsController.Create` para inyectar `CreatedByUserId` desde JWT. • Tests unitarios para la lógica de auto-asignación. | • N/A |

### 8.2 Scoping de Proyectos por Usuario (Queries + Authorization)

**Descripción:** Filtrar proyectos por usuario según rol. Admin ve todos, el resto solo los suyos/asignados.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.2.1** | feature/project-scoping | • Actualizar `GetProjectsQuery` para incluir `UserId` y `UserRole`. • Reescribir `ProjectSqlQueries` para hacer JOIN con `project_members` cuando el rol NO es Admin. • Admin: `SELECT * FROM projects` (sin filtro). • Architect/Collaborator: `SELECT p.* FROM projects p INNER JOIN project_members pm ON p.id = pm.project_id WHERE pm.user_id = @UserId`. • Actualizar `GetProjectByIdHandler` para verificar acceso (miembro o Admin). • Tests para cada escenario de visibilidad. | • Actualizar `projectService.ts` para que el endpoint devuelva solo los proyectos del usuario autenticado (sin cambio de API, el backend filtra). |
| **8.2.2** | feature/project-scoping | • Crear `IProjectAuthorizationService` en Application/Interfaces con métodos `CanAccess(projectId, userId)`, `CanEdit(projectId, userId)`, `CanManageMembers(projectId, userId)`. • Implementar en Infrastructure consultando `project_members` + rol del usuario. • Aplicar en `ProjectsController`: verificar acceso antes de cada operación. • Aplicar políticas: `[Authorize(Policy = RequireArchitect)]` en endpoints de creación y edición. | • N/A |

### 8.3 Compartir y Asignar Proyectos

**Descripción:** Endpoints y UI para gestionar miembros de proyectos.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.3.1** | feature/project-members | • Crear `AddProjectMemberCommand` (ProjectId, UserId, Role). Validator: solo Owner/Admin puede añadir miembros, Architect puede compartir con Architect/Collaborator (R3/R4). • Crear `RemoveProjectMemberCommand` (ProjectId, UserId). Validator: no se puede eliminar al Owner. • Crear `GetProjectMembersQuery` (Dapper). • Crear `IProjectMemberRepository` + implementación. • Endpoints: `POST /projects/{id}/members`, `DELETE /projects/{id}/members/{userId}`, `GET /projects/{id}/members`. • Tests para todas las combinaciones de permisos. | • Crear componente `ProjectMembers` (lista de miembros con avatar, rol, botón quitar). • Crear modal `AddMemberModal` (buscador de usuarios + selector de rol). • Integrar en vista de detalle del proyecto. |
| **8.3.2** | feature/project-members | • Crear `UpdateProjectMemberRoleCommand` (cambiar rol de un miembro: Editor ↔ Viewer). • Solo Admin o Owner pueden cambiar roles. • Tests. | • Dropdown de cambio de rol en `ProjectMembers`. |

### 8.4 Validación de Proyectos (Flujo de Aprobación)

**Descripción:** El Admin valida las memorias finalizadas antes de archivarlas.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.4.1** | feature/project-validation | • Añadir `PendingValidation = 4` al enum `ProjectStatus`. • Crear `SubmitForValidationCommand` (solo Owner/Architect del proyecto). Cambia estado `Completed → PendingValidation`. • Crear `ValidateProjectCommand` (solo Admin). Cambia estado `PendingValidation → Archived` (aprobado) o `PendingValidation → InProgress` (rechazado con motivo). • Añadir campo nullable `ValidationNotes` a `Project`. • Endpoints: `POST /projects/{id}/submit-validation`, `POST /projects/{id}/validate`. • Tests para el flujo completo de estados. | • Botón "Enviar a validación" visible para Architect cuando estado = `Completed`. • Vista de "Proyectos pendientes de validación" para Admin. • Modal de validación: aprobar / rechazar con notas. • Indicador visual del estado `PendingValidation` en listados. |

### 8.5 Correcciones Frontend (Alineación con Backend)

**Descripción:** Corregir las inconsistencias detectadas entre frontend y backend.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.5.1** | feature/fix-user-form | • N/A | • Eliminar rol "Supervisor" del schema Zod y del `<Select>` en `UserForm.tsx`. • Añadir campo `CollegiateNumber` al formulario (visible condicionalmente para Architect). • Eliminar botón "Eliminar" de `UserRow.tsx` y método `remove()` de `userService.ts` (o implementar soft-delete en backend si se decide). |
| **8.5.2** | feature/fix-user-form | • Decidir: implementar `DELETE /users/{id}` como soft-delete (set `IsActive = false` + anonimizar email) o eliminar del frontend. Si se implementa: crear `DeleteUserCommand` + Handler con validación de jerarquía. | • Ajustar según decisión. |

### 8.6 Templates de Email

**Descripción:** Mejorar el sistema de emails con templates y nuevos eventos.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.6.1** | feature/email-templates | • Crear `IEmailTemplateService` con métodos: `BuildWelcomeEmail()`, `BuildPasswordResetEmail()`, `BuildProjectSharedEmail()`, `BuildProjectValidationEmail()`. • Migrar HTML hardcodeado de `CreateUserHandler` y `ResetUserPasswordHandler` al nuevo servicio. • Plantillas con tokens reemplazables (`{{FullName}}`, `{{Email}}`, `{{Password}}`, etc.). | • N/A |
| **8.6.2** | feature/email-templates | • Enviar email al compartir proyecto (destinatario: nuevo miembro). • Enviar email al enviar a validación (destinatario: Admin). • Enviar email al aprobar/rechazar validación (destinatario: Owner del proyecto). | • N/A |

### Orden de implementación recomendado

```
8.5.1 → 8.1.1 → 8.1.2 → 8.2.1 → 8.2.2 → 8.3.1 → 8.3.2 → 8.4.1 → 8.5.2 → 8.6.1 → 8.6.2
  │         │                  │                  │            │               │
  │         └── Domain +       └── Queries +      └── Share/   └── Validation  └── Email
  │             Infra              Authorization       Assign       flow           templates
  └── Quick fix frontend
```

**Dependencias:**
- 8.1 es prerequisito de todo lo demás (modelo de datos).
- 8.2 depende de 8.1 (necesita `project_members` para filtrar).
- 8.3 depende de 8.2 (necesita autorización para verificar permisos de compartir).
- 8.4 depende de 8.2 (necesita autorización + scoping).
- 8.6 depende de 8.3 y 8.4 (necesita los eventos que disparan los emails).
- Fase 4.3 (Notificaciones) depende de 8.3 (necesita saber a quién notificar).

---

## **📋 Fase 9: Revisión Final de Documentación**

**Objetivo:** Alinear toda la documentación del proyecto con el estado real del código tras la implementación de la Fase 8.

| ID | Feature Branch | Tareas |
| :---- | :---- | :---- |
| **9.1** | feature/docs-final-review | • Revisar y actualizar `ANALISIS_DETALLADO.md` con el modelo de propiedad de proyectos. • Actualizar `openapi.yaml` con los nuevos endpoints (members, validation). • Actualizar `AGENTS.md` con las reglas de negocio aprobadas. • Actualizar `ERS_EDIFICIA_Lite.md` con los requisitos de permisos. • Actualizar `VIEWS_ANALYSIS.md` con las nuevas vistas (miembros, validación). • Actualizar `MANUAL_CLIENTE_EDIFICIA.md` con flujos de usuario actualizados. • Verificar que ROADMAP refleja el estado final de todas las fases. |

## **�🚦 Definición de Hecho (DoD)**

Para considerar una **Feature** cerrada:

1. \[ \] Código compila sin warnings.  
2. \[ \] Tests unitarios (xUnit/Vitest) en verde.  
3. \[ \] Clean Architecture respetada (dependencias correctas).  
4. \[ \] Validaciones (Fluent/Zod) implementadas.  
5. \[ \] Funciona en Docker (docker-compose up).