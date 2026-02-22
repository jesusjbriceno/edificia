# Contexto del Proyecto EdificIA — Información Auxiliar para TFM

> **INSTRUCCIÓN DE USO:** Este archivo **debe subirse a la carpeta de Google Drive** identificada por `FOLDER_ID_EDIFICIA_MD` junto al resto de la documentación del proyecto. El flujo n8n lo descargará y procesará automáticamente como contexto adicional para la generación de la memoria y las diapositivas del TFM.

---

## 1. Identificadores del Proyecto

| Campo | Valor |
|---|---|
| **Nombre** | EdificIA |
| **URL aplicación en producción** | https://edificia.jesusjbriceno.dev |
| **URL API (Swagger)** | https://api-edificia.jesusjbriceno.dev/swagger |
| **Repositorio GitHub** | https://github.com/jesusjbriceno/edificia |
| **Licencia** | Apache 2.0 |
| **Curso / Máster** | Máster en Desarrollo de Aplicaciones con IA |
| **Fecha de entrega** | Febrero 2026 |

---

## 2. Descripción del Proyecto

EdificIA es un **SaaS profesional** para la redacción automatizada y asistida por IA de Memorias de Proyecto de Ejecución en España (normativa CTE/LOE). El sistema discrimina automáticamente entre **Obra Nueva** y **Reforma** (exención LOE Art. 2.2), adaptando el árbol de contenidos normativos al tipo de intervención.

### Propuesta de valor diferencial

- **Discriminación normativa inteligente:** El árbol de capítulos CTE se filtra automáticamente según el tipo de obra. Una reforma interior elimina los capítulos de Cimentación o Estructura al no ser necesarios.
- **IA soberana y delegada:** La generación de texto no depende de un proveedor fijo. Se delega a flujos n8n intercambiables mediante la variable `AI_WEBHOOK_URL`.
- **Modo Túnel (offline):** Estado del editor persistido en IndexedDB, operativo sin conexión.
- **Arquitectura escalable:** Clean Architecture + CQRS permite añadir nuevas funcionalidades sin reescritura.

---

## 3. Flux Gateway — Contexto y Rol en el Proyecto

**Flux Gateway es una herramienta desarrollada como pieza accesoria del ecosistema** para dar soporte de IA al proyecto EdificIA, concretamente para disponer de un proveedor de IA soberana y controlada. Se implementó como una pasarela OAuth2 (Client Credentials) que expone una interfaz compatible con OpenAI sobre el modelo Flux.

Sin embargo, EdificIA **no está acoplado a Flux Gateway**. La arquitectura de IA delegada permite usar cualquier proveedor:

- **Flux Gateway** (`workflow-flux.json`): IA soberana, autenticación OAuth2, orientada a producción privada.
- **Google Gemini** (`workflow-gemini.json`): Proveedor cloud, API key simple, menor latencia.
- **Cualquier otro proveedor** (OpenAI, Anthropic, Ollama/LM Studio para uso local): basta con adaptar el workflow n8n correspondiente.

El cambio de proveedor **no requiere modificaciones en el código del backend** (.NET). Solo se actualiza la variable de entorno `AI_WEBHOOK_URL` apuntando al webhook del flujo n8n elegido.

---

## 4. Vistas de la Aplicación — Mapa Completo

A continuación se describe cada vista implementada, con referencia al nombre de captura de pantalla que debe incluirse en las diapositivas correspondientes. **Las capturas deben reemplazar los placeholders** `📷 [IMAGEN: img_0X.png]` al preparar la presentación final.

### Vista 1: Autenticación — `img_01_login.png`
- **Ruta:** `/` (página raíz)
- **Descripción:** Formulario de login con fondo arquitectónico premium. Enlace a recuperación de contraseña. JWT + Refresh Tokens. Guard de autenticación activo.
- **Aspectos a destacar:** Diseño profesional orientado al sector AEC (Arquitectura, Ingeniería, Construcción). Validación con Zod.

### Vista 2: Dashboard de Proyectos — `img_02_dashboard.png`
- **Ruta:** `/dashboard`
- **Descripción:** Grid de tarjetas de proyectos activos del usuario. Botón "Nuevo Proyecto" lanza un Wizard de creación. Sidebar con navegación principal. Header con búsqueda, notificaciones y menú de usuario.
- **Aspectos a destacar:** Wizard multi-paso para alta de proyecto (Título, Descripción, Tipo de intervención: Obra Nueva / Reforma). El tipo seleccionado determina la estructura del árbol normativo.

### Vista 3: Editor de Memoria Técnica — `img_03_editor.png`
- **Ruta:** `/projects/:id`
- **Descripción:** Layout de dos columnas: árbol lateral de capítulos CTE (sidebar) + editor central TipTap (editor WYSIWYG headless). La barra de herramientas ofrece formato básico (negrita, cursiva, encabezados, listas). El sidebar incluye búsqueda en tiempo real que filtra recursivamente el árbol.
- **Aspectos a destacar:** Cabecera multi-nivel con tipo de intervención en contexto. Estado guardado automáticamente en IndexedDB (Modo Túnel offline). Botón "Generar con IA" que invoca el webhook n8n del backend.

### Vista 4: Panel de Administración — `img_04_admin.png`
- **Ruta:** `/admin/users`, `/admin/projects`, `/admin/notifications`
- **Descripción:** Panel accesible solo para roles Admin/SuperAdmin. Gestión completa de usuarios (CRUD con roles), proyectos (con estados: En Ejecución / En Espera / Finalizado) y notificaciones del sistema (campana con contador de no leídas).
- **Aspectos a destacar:** Tabla de usuarios con búsqueda y filtrado. Formularios validados con Zod. Dropdown con portal para evitar clipping en layouts complejos.

### Vista 5: Flujo de Integración IA — `img_05_ai_flow.png`
- **Descripción:** Diagrama del flujo de integración IA delegada. El backend .NET envía un contexto técnico al webhook n8n. n8n procesa y llama al proveedor de IA (Flux Gateway o Gemini). La respuesta normalizada retorna al backend. El frontend muestra el texto generado en el editor.
- **Nota:** Esta es una captura del flujo en n8n o un diagrama de arquitectura, no una vista de la aplicación web.

### Vista 6: Infraestructura de despliegue — `img_06_deploy.png`
- **Descripción:** Diagrama del entorno de producción: Coolify v4 como PaaS self-hosted, Traefik como reverse proxy con TLS automático (Let's Encrypt), contenedores Docker para API (.NET 8) y Web (Astro), PostgreSQL y Redis como servicios auxiliares.
- **Nota:** Captura del panel de Coolify o diagrama de arquitectura de despliegue.

---

## 5. Instrucciones Específicas para el Prompt de la Memoria TFM

Al generar la memoria académica, el modelo debe tener en cuenta:

1. **Estructura académica obligatoria:** Resumen (máx. 300 palabras) → Introducción → Objetivos → Metodología → Desarrollo técnico → Conclusiones y trabajo futuro.

2. **Tono:** Académico-técnico. Combinar rigor formal con descripción precisa de las decisiones de diseño e implementación.

3. **Fundamentos teóricos a mencionar:**
   - Clean Architecture (Robert C. Martin) y sus beneficios en mantenibilidad.
   - CQRS + Mediator pattern y su rol en la separación de responsabilidades.
   - Islands Architecture (Astro) para optimización de rendimiento frontend.
   - Delegación de IA a orquestadores (n8n) como patrón de desacoplamiento.

4. **Aspectos diferenciales a resaltar:**
   - EdificIA no es un generador de texto genérico: entiende la normativa española (CTE/LOE).
   - Flux Gateway fue desarrollado expresamente como herramienta accesoria del proyecto, pero la arquitectura permite usar cualquier IA.
   - El "Modo Túnel" (offline) es una necesidad real del sector: arquitectos que trabajan en obra sin conexión.

5. **Trabajo futuro documentado (Fase 9 del Roadmap):**
   - **9.1 Multi-normativa:** Soporte para normativas autonómicas y proyectos de rehabilitación energética.
   - **9.2 Email delegado:** Migrar el servicio de notificaciones a flujos n8n para mayor flexibilidad.
   - **9.3 IA local (Ollama/LM Studio):** Workflow n8n para IA completamente offline.
   - **6.1.1 Plantilla DOTX:** Exportación con plantilla corporativa personalizada.

---

## 6. Instrucciones Específicas para el Prompt de las Diapositivas

Al generar las diapositivas, el modelo debe:

1. **Formato de respuesta:** Devolver ÚNICAMENTE un array JSON válido con exactamente 15 diapositivas. Sin texto adicional, sin bloques de código markdown (no usar ```json```).

2. **Estructura de cada slide según tipo:**
   - `portada`: `{ "type": "portada", "title": "EdificIA", "subtitle": "...", "autores": "...", "fecha": "Febrero 2026" }`
   - `contenido`: `{ "type": "contenido", "title": "...", "bullets": ["texto1", "texto2", ...] }` (máx. 5 bullets por slide)
   - `imagen`: `{ "type": "imagen", "title": "...", "image_placeholder": "img_0X_nombre.png", "caption": "..." }`
   - `cierre`: `{ "type": "cierre", "title": "Gracias | Preguntas" }`

3. **Distribución de las 15 diapositivas:**
   1. **Portada** (portada): EdificIA + subtítulo + Máster en Desarrollo IA + Febrero 2026
   2. **Agenda** (contenido): Índice de la presentación (6-7 puntos)
   3. **El problema** (contenido): Pain points del sector construcción en España (documentación manual, CTE/LOE complejo)
   4. **La solución: EdificIA** (contenido): Propuesta de valor, discriminación Obra Nueva/Reforma
   5. **Arquitectura técnica** (contenido): Clean Architecture + CQRS + stack (.NET 8, Astro, PostgreSQL, n8n, Docker)
   6. **Demo: Login y autenticación** (imagen): `img_01_login.png`
   7. **Demo: Dashboard de proyectos** (imagen): `img_02_dashboard.png`
   8. **Demo: Editor de Memoria** (imagen): `img_03_editor.png`
   9. **Demo: Panel de Administración** (imagen): `img_04_admin.png`
   10. **IA delegada: arquitectura** (contenido): Flux Gateway (herramienta accesoria soberana) + n8n + Gemini, intercambiables vía `AI_WEBHOOK_URL`
   11. **Demo: Flujo IA en n8n** (imagen): `img_05_ai_flow.png`
   12. **Despliegue en producción** (contenido): Docker + Coolify + Traefik + TLS automático
   13. **Resultados obtenidos** (contenido): MVP completo, todas las fases implementadas, en producción
   14. **Conclusiones y trabajo futuro** (contenido): Logros académicos + Fase 9 del roadmap
   15. **Cierre** (cierre): "Gracias | Preguntas"

4. **Mencionar siempre en el slide de IA:** Flux Gateway fue desarrollado como herramienta accesoria del proyecto para proveer IA soberana. La arquitectura permite intercambiar el proveedor (Flux, Gemini, Ollama) sin modificar el código del backend.

---

## 7. Assets Necesarios Antes de la Presentación Final

Una vez generadas las diapositivas por el flujo n8n, hay que **reemplazar manualmente** los placeholders de imagen:

| Placeholder | Descripción | Cómo obtenerla |
|---|---|---|
| `img_01_login.png` | Captura de la pantalla de login | https://edificia.jesusjbriceno.dev |
| `img_02_dashboard.png` | Captura del dashboard de proyectos | https://edificia.jesusjbriceno.dev/dashboard |
| `img_03_editor.png` | Captura del editor de memoria activo | Abrir un proyecto existente |
| `img_04_admin.png` | Captura del panel de administración | https://edificia.jesusjbriceno.dev/admin/users |
| `img_05_ai_flow.png` | Captura del flujo n8n de IA | Panel n8n → Workflows |
| `img_06_deploy.png` | Captura del panel de Coolify | Panel Coolify de producción |
---

## 8. Herramientas de Desarrollo Utilizadas

### VS Code Agent Skills (`.agents/skills/`)

Durante el desarrollo se utilizaron las siguientes **skills de agente** instaladas en VS Code, que proporcionan instrucciones especializadas y contexto de dominio al asistente IA:

| Skill | Descripción |
|---|---|
| `astro` | Proyectos Astro: CLI, estructura de proyecto, configuración y adaptadores (SSR). Usado para el desarrollo del frontend con Astro 4. |
| `cqrs-implementation` | Implementación de CQRS para arquitecturas escalables. Usado para los Command/Query Handlers del backend .NET. |
| `docker-expert` | Docker: multi-stage builds, optimización de imágenes, seguridad, Docker Compose y patrones de despliegue en producción. |
| `dotnet-backend-patterns` | Patrones .NET: Clean Architecture, Result Pattern, FluentValidation, EF Core, Dapper. Núcleo del backend EdificIA. |
| `git-advanced-workflows` | Flujos Git avanzados: rebasing, cherry-picking, worktrees, Git Flow con feature branches y PRs. |
| `n8n-workflow-patterns` | Patrones arquitecturales probados para flujos n8n: webhooks, HTTP APIs, bases de datos, agentes IA y tareas programadas. |
| `postgresql-table-design` | Diseño de esquemas PostgreSQL: tipos de datos, indexación, JSONB, constraints y patrones de rendimiento. |
| `tailwind-design-system` | Sistemas de diseño con Tailwind CSS v4, design tokens y patrones responsive para el frontend. |
| `ui-ux-pro-max` | Inteligencia de diseño UI/UX: paletas, tipografía, componentes (glassmorphism, bento grid, dark mode). |

### MCP Context7 (Servidor de Documentación en Tiempo Real)

Se utilizó el servidor MCP **Context7** (`mcp_context7_resolve-library-id` + `mcp_context7_get-library-docs`) para obtener documentación actualizada de bibliotecas y APIs directamente desde VS Code durante el desarrollo:

- **n8n**: Documentación de nodos, expresiones y patrones de flujo (Trust Score 9.7, 1132 snippets).
- **Google Slides REST API**: Referencia de `presentations.create` y `presentations.batchUpdate` — confirmó que n8n no tiene nodo nativo y que la integración requiere HTTP Request con `googleDriveOAuth2Api`.
- **.NET / EF Core / FluentValidation**: Consultas de patrones y APIs durante el desarrollo del backend.
- **Astro / TailwindCSS**: Documentación de configuración y componentes durante el desarrollo del frontend.

> **Nota técnica:** Context7 permitió resolver en tiempo real la estrategia de integración con Google Slides API (batchUpdate con objectIds pre-asignados, unidades EMU, scope OAuth2 reutilizable desde Drive), lo que habría requerido una búsqueda manual extensa.