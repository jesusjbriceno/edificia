# **📅 Plan de Implementación Detallado \- EDIFICIA**

**Estado:** Pendiente de Inicio

**Metodología:** Git Flow (feature/... \-\> develop \-\> main)

**Sprint 0:** Configuración y Andamiaje (Completado).

## **🏁 Fase 1: Cimientos del Sistema (Core & Shared)**

**Objetivo:** Establecer los patrones base, la base de datos y la gestión de errores.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **1.1** | feature/shared-kernel | • Implementar Result\<T\> pattern en Edificia.Shared. • Crear Excepciones de Dominio base. • Configurar GlobalExceptionHandler en API. | • Configurar axios o fetch wrapper con manejo de errores unificado. • Definir tipos base de respuesta API (Result). |
| **1.2** | feature/infra-persistence | • Configurar AgmaDbContext con SnakeCase naming. • Implementar UnitOfWork (si aplica) o inyección de DbContext. • Configurar conexión Dapper en Infrastructure. | • N/A |
| **1.3** | feature/api-swagger | • Configurar Swagger con soporte para JWT (aunque se usará más tarde). • Definir ProblemDetails según RFC 7807\. | • Generar cliente API inicial (o tipos manuales) basados en Swagger. |

## **🏗️ Fase 2: Gestión de Proyectos (El CRUD)**

**Objetivo:** Que el usuario pueda crear, listar y configurar la estrategia de su proyecto.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **2.1** | feature/project-domain | • Definir Entidad Project (con Enums InterventionType). • Crear Migración EF Core (InitialCreate). • Crear CreateProjectCommand \+ Validador Fluent. | • Crear Zod Schema ProjectSchema. • Maquetar componentes UI: Card, Button, Badge (Tailwind v4). |
| **2.2** | feature/project-read | • Implementar GetProjectsQuery con Dapper (paginado). • Implementar GetProjectByIdQuery. | • Crear DashboardLayout.astro. • Implementar página dashboard.astro con Grid de proyectos. • Conectar API GET /projects. |
| **2.3** | feature/project-wizard | • Ajustar CreateProjectCommand para recibir IsLoeRequired. | • Implementar **Wizard React** (Modal): 1\. Datos Básicos. 2\. Selector (Obra Nueva vs Reforma). 3\. Normativa Local. • Conectar POST /projects. |

## **🧠 Fase 3: El Motor de Normativa (JSON Engine)**

**Objetivo:** Renderizar el árbol de capítulos filtrado según la estrategia del proyecto.

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **3.1** | feature/normative-tree | • Crear estructura JSON ContentTree en Entidad. • Endpoint GET /projects/{id}/tree. | • Crear archivo estático cte\_2024.json en /public. • Implementar utilidad TS filterTree(nodes, config) para ocultar ramas según Obra/Reforma. |
| **3.2** | feature/editor-shell | N/A | • Crear EditorLayout.astro. • Implementar **Sidebar de Navegación** (React) recursivo. • Gestionar selección de capítulo activo en Zustand. |

## **📝 Fase 4: Editor y Persistencia (The Core)**

**Objetivo:** Escribir contenido y guardarlo (Offline first).

| ID | Feature Branch | Tareas Backend (.NET) | Tareas Frontend (Astro/React) |
| :---- | :---- | :---- | :---- |
| **4.1** | feature/editor-tiptap | N/A | • Integrar **TipTap** en componente React. • Crear Toolbar flotante. • Conectar editor al Store de Zustand. |
| **4.2** | feature/offline-sync | • Crear endpoint PATCH /projects/{id}/sections. • Optimizar update con ExecuteUpdate de EF Core o SQL Raw para JSONB. | • Configurar idb-keyval en Zustand. • Implementar lógica "Debounce Save": Guardar en local al escribir, sincronizar con API cada 5s si hay red. |

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

## **🚦 Definición de Hecho (DoD)**

Para considerar una **Feature** cerrada:

1. \[ \] Código compila sin warnings.  
2. \[ \] Tests unitarios (xUnit/Vitest) en verde.  
3. \[ \] Clean Architecture respetada (dependencias correctas).  
4. \[ \] Validaciones (Fluent/Zod) implementadas.  
5. \[ \] Funciona en Docker (docker-compose up).