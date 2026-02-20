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

## **👥 Fase 8: Gestión de Usuarios, Propiedad de Proyectos y Permisos (Pendiente de definir)**

**Objetivo:** Establecer la relación entre usuarios y proyectos, definir quién puede ver/editar qué, y completar los flujos de administración de usuarios que actualmente están incompletos o desalineados entre frontend y backend.

**Estado actual — Análisis de gaps:**

### 8.1 Gestión de Usuarios

**Lo que funciona:**
- Creación de usuarios por Admin/Root con password temporal auto-generada (12 chars, mixed).
- Email de bienvenida enviado con credenciales temporales (SMTP o Brevo).
- `MustChangePassword` flag → JWT con claim `pwd_change_required` → solo puede acceder a `/auth/change-password`.
- Reset de contraseña por Admin con email de notificación.
- Toggle activar/desactivar con jerarquía de roles (no puedes desactivarte a ti mismo, no puedes tocar roles superiores).
- Roles definidos: `Root`, `Admin`, `Architect`, `Collaborator`.

**Gaps identificados:**
- **Frontend: Rol "Supervisor" fantasma.** `UserForm.tsx` incluye "Supervisor" como opción de rol, pero el backend solo acepta `Admin`, `Architect`, `Collaborator`. Crear un usuario con rol "Supervisor" dará error 400.
- **Frontend: Botón "Eliminar usuario" sin endpoint.** `userService.ts` tiene `remove()` que llama `DELETE /users/:id`, pero `UsersController` no tiene `[HttpDelete]`. Dará 405.
- **Falta campo `CollegiateNumber` en el formulario.** El backend lo acepta y almacena, el frontend no lo expone en `UserForm.tsx`.
- **Sin confirmación visual de email enviado.** Tras crear usuario o resetear contraseña, el admin no sabe si el email llegó.

### 8.2 Propiedad de Proyectos — Gap Crítico

**Situación actual:** La entidad `Project` **no tiene ningún campo `UserId`, `CreatedByUserId` ni `OwnerId`**. Los proyectos son "globales" — cualquier usuario autenticado con política `ActiveUser` puede ver y editar todos los proyectos. No hay scoping por usuario.

**Consecuencias:**
- Un Collaborator ve los mismos proyectos que un Root.
- No se puede saber quién creó un proyecto.
- No se pueden asignar proyectos a arquitectos específicos.
- No hay supervisión ni delegación de trabajo.
- El sistema de notificaciones no sabe a quién notificar.

**Decisiones pendientes:**
1. ¿Añadir `CreatedByUserId` (Guid FK) a `Project`? → Propiedad simple.
2. ¿Añadir tabla `project_members` (ProjectId, UserId, Role)? → Acceso compartido/asignación.
3. ¿Un Architect solo ve sus proyectos? ¿Un Admin ve todos? ¿Root ve todo?
4. ¿Se pueden "compartir" proyectos con otros usuarios?
5. ¿Qué significa "Collaborator" en relación a proyectos?

### 8.3 Jerarquía de Permisos — Diseño pendiente

**Modelo actual de políticas (DI):**

| Política | Roles permitidos | Uso actual |
| :---- | :---- | :---- |
| `ActiveUser` | Cualquier rol activo sin `MustChangePassword` | Endpoints generales |
| `RequireRoot` | Root | No se usa en controllers |
| `RequireAdmin` | Root, Admin | `UsersController` (CRUD usuarios) |
| `RequireArchitect` | Root, Admin, Architect | No se usa en controllers |

**Nota:** `ProjectsController` solo usa `[Authorize]` (cualquier usuario autenticado). No aplica ninguna política granular.

**Preguntas pendientes:**
- ¿Qué permisos tiene cada rol sobre proyectos? (ver, crear, editar, exportar, archivar)
- ¿Un Collaborator puede crear proyectos o solo editar los que le asignen?
- ¿El Architect es "dueño" de sus proyectos? ¿Puede un Admin reasignarlos?
- ¿Necesitamos un rol "Supervisor" real (lectura + supervisión sin edición)?

### 8.4 Envío de Emails — Estado actual

**Implementado:** 
- ✅ Email de bienvenida con credenciales temporales (al crear usuario).
- ✅ Email de reset de contraseña con nueva temporal.
- ✅ Doble proveedor configurado (SMTP / Brevo API).

**No implementado:**
- ❌ Email al desactivar/reactivar cuenta.
- ❌ Email de notificación de eventos del proyecto.
- ❌ Template engine para emails (actualmente HTML hardcodeado en los handlers).

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **8.1** | *pendiente de definir* | ⏳ *Definir tras análisis — Gestión de usuarios* | ⏳ *Corregir UserForm.tsx: eliminar "Supervisor", añadir CollegiateNumber, eliminar botón delete* |
| **8.2** | *pendiente de definir* | ⏳ *Definir tras análisis — Propiedad de proyectos* | ⏳ *Pendiente* |
| **8.3** | *pendiente de definir* | ⏳ *Definir tras análisis — Permisos y autorización* | ⏳ *Pendiente* |
| **8.4** | *pendiente de definir* | ⏳ *Definir tras análisis — Templates de email* | ⏳ *N/A* |

## **�🚦 Definición de Hecho (DoD)**

Para considerar una **Feature** cerrada:

1. \[ \] Código compila sin warnings.  
2. \[ \] Tests unitarios (xUnit/Vitest) en verde.  
3. \[ \] Clean Architecture respetada (dependencias correctas).  
4. \[ \] Validaciones (Fluent/Zod) implementadas.  
5. \[ \] Funciona en Docker (docker-compose up).