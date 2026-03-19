# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# EdificIA — Claude Code Agent Guide

> **Rol:** Arquitecto de Software Senior especializado en .NET 10 Clean Architecture + Astro/React.
> Mantener coherencia estricta con este documento y con `AGENTS.md`.

---

## 1. Identidad del Proyecto

**EdificIA** es un SaaS para la generación automatizada y asistida por IA de **Memorias de Proyecto de Ejecución** en España (CTE/LOE).

- **Lógica core:** Discrimina entre Obra Nueva y Reforma (exención LOE Art. 2.2) adaptando dinámicamente el árbol de contenidos normativos.
- **Repositorio:** https://github.com/jesusjbriceno/edificia
- **Local Web:** http://localhost:4321 | **Local API:** http://localhost:5000
- **Producción Web:** https://edificia.jesusjbriceno.dev | **Producción API:** https://api-edificia.jesusjbriceno.dev

---

## 2. Stack Tecnológico (Estricto — No negociable)

### Backend (`apps/api`) — .NET 10

| Capa | Tecnología |
|---|---|
| Arquitectura | Clean Architecture + CQRS (MediatR) |
| Writes (Commands) | Entity Framework Core → PostgreSQL 16 |
| Reads (Queries) | Dapper (SQL Raw) — **Prohibido retornar entidades de dominio** |
| Validación | FluentValidation (en pipeline MediatR, no en Controllers) |
| Mapeo DTOs | **Manual con operadores explícitos — PROHIBIDO AutoMapper** |
| Auth | JWT Bearer + Refresh Token Rotation + RBAC |
| Caché | Redis (StackExchange.Redis) + L1 MemoryCache |
| Testing | xUnit + Moq |
| Logging | Serilog |
| IA | N8nAiService (webhook → n8n → Flux Gateway / Gemini) |

### Frontend (`apps/web`) — Astro 4 + React 18

| Capa | Tecnología |
|---|---|
| Shell | Astro 4 (SSR, Islands Architecture) |
| Interactividad | React 18 + TypeScript Strict |
| Estilos | **Tailwind CSS v4** (tema oscuro: `bg-dark-bg`, `text-white`, `border-white/5`) |
| Estado | Zustand + IndexedDB (idb-keyval) |
| Editor | TipTap (Headless WYSIWYG) |
| Formularios | react-hook-form + Zod |
| Testing | Vitest + Testing Library + Storybook v8 |

### Infraestructura

- **DB:** PostgreSQL 16, convención **snake_case** (plugin EFCore.NamingConventions)
- **Caché:** Redis 7
- **Contenido de Memoria:** JSONB (`Projects.content_tree_json`) — **No es tabla relacional**
- **Contenedores:** Docker Compose (`docker-compose.yml`)
- **PaaS:** Coolify v4

---

## 3. Mapa del Monorepo

```
/
├── CLAUDE.md                   # Este archivo (guía para Claude Code)
├── AGENTS.md                   # Contexto Maestro del proyecto
├── docker-compose.yml
├── apps/
│   ├── api/                    # Solución .NET 10
│   │   ├── src/
│   │   │   ├── Edificia.Domain         # Entidades, ValueObjects, Enums, Constants
│   │   │   ├── Edificia.Shared         # Kernel: Result<T>, Error handling
│   │   │   ├── Edificia.Application    # CQRS Handlers, Validators, Interfaces, DTOs
│   │   │   ├── Edificia.Infrastructure # EF Context, Dapper, Services, Migrations
│   │   │   └── Edificia.API            # Controllers, Swagger, Middleware
│   │   └── tests/
│   │       └── Edificia.Application.Tests/  # xUnit + Moq
│   ├── web/                    # Astro + React
│   │   ├── public/normativa/   # JSONs estáticos (cte_2024.json)
│   │   └── src/
│   │       ├── components/
│   │       │   ├── ui/         # Átomos: Button, Input, Card, Badge, Modal
│   │       │   ├── Admin/      # TemplateManagement, UserManagement, etc.
│   │       │   └── Editor/     # EditorShell, ProjectEditor, SidebarNavigation, ExportDocxModal
│   │       ├── pages/          # Rutas Astro (dashboard, admin/*, projects/[id])
│   │       ├── store/          # Zustand: useAuthStore, useEditorStore, useToastStore
│   │       ├── lib/
│   │       │   ├── services/   # authService, projectService, templateService, etc.
│   │       │   └── utils/      # contentTree, syncManager, sanitizeHtml
│   │       └── tests/          # Todos los tests en src/tests/ (imports con @/)
│   └── n8n/                    # Workflows: Flux Gateway, Gemini, TFM
└── docs/                       # 32 documentos técnicos y funcionales
    ├── development/
    │   ├── GUIDELINES.md       # Estándares de código (leer siempre)
    │   └── backend/API_DESIGN.md
    └── features/dotx_support/  # Documentación de la feature activa
```

---

## 4. Dependency Rule (Obligatoria)

```
Domain → (nada)
Shared → (nada)
Application → Domain + Shared
Infrastructure → Application + Domain
API → Todo
```

**Nunca** invertir la dirección de dependencias.

---

## 5. Patrones de Código Obligatorios

### 5.1 Result Pattern (Backend)

```csharp
// CORRECTO: Nunca lanzar excepciones para control de flujo
public async Task<Result<ProjectResponse>> Handle(...)
{
    var project = await _repo.GetByIdAsync(id, ct);
    if (project is null)
        return Result.Failure<ProjectResponse>(ProjectErrors.NotFound(id));

    return Result.Success((ProjectResponse)project);
}
```

### 5.2 Controladores Thin

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateProjectRequest request, CancellationToken ct)
{
    var command = new CreateProjectCommand(request.Title, request.InterventionType, ...);
    var result = await _mediator.Send(command, ct);
    return result.IsSuccess ? CreatedAtAction(...) : HandleFailure(result);
}
```

### 5.3 Mapeo Manual de DTOs

```csharp
public record ProjectResponse(Guid Id, string Title, ...)
{
    public static explicit operator ProjectResponse(Project entity) => new(
        entity.Id,
        entity.Title,
        ...
    );
}
```

### 5.4 Validadores FluentValidation

```csharp
public class CreateProjectValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
```

### 5.5 Queries con Dapper

```csharp
// Queries: usar Dapper, retornar DTOs/Records — NUNCA entidades de dominio
public async Task<IReadOnlyList<ProjectSummaryDto>> GetPagedAsync(...)
{
    const string sql = "SELECT id, title, status FROM projects WHERE ...";
    var results = await _db.QueryAsync<ProjectSummaryDto>(sql, new { ... });
    return results.AsList();
}
```

### 5.6 Formularios React

```typescript
// Todo formulario con esquema Zod exportado + zodResolver
const projectSchema = z.object({
  title: z.string().min(1, 'El título es obligatorio').max(200),
});
export type ProjectFormData = z.infer<typeof projectSchema>;

const { register, handleSubmit } = useForm<ProjectFormData>({
  resolver: zodResolver(projectSchema),
});
```

### 5.7 TypeScript (Prohibiciones)

- `any` **prohibido**. Siempre tipar Props e interfaces.
- Orden de clases Tailwind: Layout → Spacing → Sizing → Visual

---

## 6. Entidades de Dominio Clave

### Project (Aggregate Root)

- `InterventionType`: `NewConstruction | Reform | Extension`
- `IsLoeRequired`: boolean (exención LOE Art. 2.2)
- `Status`: `Draft | InProgress | PendingReview | Completed | Archived`
- `ContentTreeJson` (JSONB): árbol de contenidos — **no normalizado**
- Métodos: `StartRedaction()`, `SubmitForReview()`, `Approve()`, `Reject()`, `Archive()`

### AppTemplate

- `IsAvailable`: plantilla seleccionable para exportación
- `IsDefault`: plantilla predeterminada por `TemplateType` (máx. 1 por tipo)
- `TemplateType`: actualmente `"MemoriaTecnica"` (hardcoded, evolución a catálogo)
- `Version`: se incrementa en cada actualización
- Fallback automático al exportador estándar si la plantilla falla

### TemplateParam (Catálogo Global de Placeholders)

- `Key`: código del placeholder en mayúsculas con guiones bajos (ej: `PROJECT_TITLE`)
- `SourceCode`: enum de fuente de datos (`PROJECT_TITLE`, `PROJECT_ADDRESS`, `EXPORT_DATE`, etc.)
- `Formatter`: opcional (`UPPERCASE`, `LOWERCASE`, `TRIM`)
- `IsActive`: habilitar/deshabilitar globalmente

---

## 7. Feature Activa: `feature/dotx_placeholders_core`

### Estado actual (2026-03)

**Implementado y operativo:**
- Upload `.dotx` / `.docx` con validación avanzada (extensión, MIME, tamaño, OpenXML, tags obligatorios)
- Gestión admin en `/admin/templates` (alta, listado, disponibilidad, predeterminada)
- **Almacenamiento delegado vía n8n → Google Drive** (`N8nTemplateStorageService`):
  - Webhook `template-store` → `UPLOAD_TEMPLATE` + `DELETE_TEMPLATE`
  - Webhook `template-retrieve` → `GET_TEMPLATE`
  - Proveedor local (`LocalFileStorageService`) para desarrollo
- Exportación híbrida con fallback al motor estándar
- `ExportDocxModal` con selector de plantilla y nombre de archivo editable
- Catálogo de `TemplateParam` con `SourceCode` y `Formatter`
- `TemplatePlaceholderService` + `TemplateParameterResolver` implementados
- `TemplateParamsController` con gobernanza: activar/desactivar parámetros

**Próximos pasos (roadmap):**
1. Catálogo dinámico de tipos de plantilla (de hardcoded `"MemoriaTecnica"` a tabla `template_types`)
2. Tests de integración end-to-end de resolución de placeholders
3. Cierre documental integral (Fase 11 del roadmap)

### Servicios de resolución de placeholders

```
Application/Export/Services/
├── TemplatePlaceholderService.cs   # Orquesta: obtiene params activos → resuelve
└── TemplateParameterResolver.cs    # Resuelve SourceCode → valor proyecto + aplica Formatter
```

---

## 8. Flujos Críticos

### Nueva Feature End-to-End

1. Definir entidad/ValueObject en `Edificia.Domain`
2. Definir Request/Response records en `Edificia.Application`
3. Implementar Command/Query + Handler + Validator
4. Implementar persistencia en `Edificia.Infrastructure`
5. Exponer en Controller de `Edificia.API` con Swagger
6. Crear componente React + servicio tipado en `apps/web`

### Integración IA

- **Frontend:** NUNCA llama a la IA directamente
- **Backend:** `N8nAiService` gestiona OAuth2 y caché
- **Privacidad:** datos personales eliminados del prompt antes de enviar a Flux

### Exportación `.dotx`

1. `ExportController` recibe `ExportProjectQuery` (`projectId`, `templateId?`, `outputFileName?`)
2. `ExportProjectHandler` resuelve plantilla (seleccionada → predeterminada → fallback estándar)
3. `TemplatePlaceholderService.ResolveAsync(project)` → diccionario `Key → valor`
4. `TemplateDocxGenerator` sustituye Content Controls en el `.dotx`
5. Si falla, fallback transparente al exportador DOCX estándar

---

## 9. Base de Datos — Reglas

- **Convención:** snake_case para tablas y columnas (plugin `EFCore.NamingConventions`)
- **Writes:** EF Core (Change Tracker + transacciones)
- **Reads:** Dapper con SQL directo
- **JSONB:** `projects.content_tree_json` — actualizar via PATCH por sección
- **Migraciones:** siempre con nombre descriptivo (`Add<Feature>`, `Harden<Feature>Constraints`)

---

## 10. Testing

### Backend (xUnit + Moq)

- Ubicación: `apps/api/tests/Edificia.Application.Tests/`
- Estructura: `<Feature>/Commands/`, `<Feature>/Queries/`, `<Feature>/Validators/`
- Un test por caso de uso (éxito + fallos principales)
- Moq para repositorios e interfaces — **no mockear la DB directamente**

### Frontend (Vitest)

- Ubicación: `apps/web/src/tests/` (todos los tests centralizados)
- Imports con alias `@/` (resuelto a `src/`)
- Tests de stores de Zustand: unitarios
- Tests de formularios: unit + integración (Zod validation → submit)
- Storybook: `npm run storybook` → `localhost:6006`

---

## 11. Git Flow (Obligatorio)

```
main        ← Solo desde develop via PR aprobada (producción)
develop     ← Integración. Nunca commit directo.
feature/*   ← Una rama por feature, creada desde develop
hotfix/*    ← Correcciones urgentes desde main
```

### Flujo de trabajo

```bash
git checkout develop && git pull
git checkout -b feature/<nombre-descriptivo>
# ... desarrollo ...
git push -u origin feature/<nombre-descriptivo>
gh pr create --base develop --title "feat: ..." --body "..."
```

### Conventional Commits

| Prefijo | Uso |
|---|---|
| `feat:` | Nueva funcionalidad |
| `fix:` | Corrección de bug |
| `docs:` | Solo documentación |
| `refactor:` | Sin cambio funcional |
| `test:` | Tests nuevos o corregidos |
| `chore:` | Mantenimiento (deps, configs) |

**PROHIBIDO:** Commit directo en `develop` o `main`.

---

## 12. Comandos de Desarrollo

```bash
# Infraestructura
docker-compose up -d          # PostgreSQL 16 + Redis + MailHog

# Backend
cd apps/api
dotnet build                  # Compilar toda la solución
dotnet test                   # Ejecutar tests xUnit
dotnet test --filter "FullyQualifiedName~<TestClass>"  # Ejecutar un test específico
dotnet run --project src/Edificia.API  # API en :5000

# Migraciones EF Core
dotnet ef migrations add <Nombre> --project src/Edificia.Infrastructure --startup-project src/Edificia.API
dotnet ef database update --project src/Edificia.Infrastructure --startup-project src/Edificia.API

# Frontend
cd apps/web
npm run dev                   # Dev server :4321
npm run build                 # Build de producción
npm run test                  # Vitest (todos)
npx vitest run src/tests/<archivo>.test.ts  # Ejecutar un test específico
npm run storybook             # Storybook :6006
```

---

## 13. Reglas de Seguridad

- Nunca exponer datos personales al webhook de IA
- Sanitizar HTML de entrada del editor (`sanitizeHtml.ts`)
- Validar todo input en FluentValidation (backend) y Zod (frontend)
- RBAC: `RequireAdmin` para endpoints de gestión (`/api/templates`, `/api/users`, `/api/template-params`)
- Refresh Token Rotation: revocar token anterior en cada refresco

---

## 14. Idioma en el Código

- **Variables, métodos, clases:** Inglés
- **Strings de dominio (UI, mensajes):** Español (ej: `"Memoria Descriptiva"`, `"Obra Nueva"`)
- **Comentarios:** Español preferido para contexto de negocio, inglés para lógica técnica

---

## 15. Documentación de Referencia

| Documento | Descripción |
|---|---|
| `AGENTS.md` | Contexto maestro del proyecto |
| `docs/development/GUIDELINES.md` | Estándares de código obligatorios |
| `docs/development/backend/API_DESIGN.md` | Contratos de los 24 endpoints |
| `docs/features/dotx_support/ROADMAP_IMPLEMENTACION_DOTX_N8N.md` | Roadmap feature activa |
| `docs/features/dotx_support/INFORME_CAMBIOS_EVOLUCION_DOTX_2026-02.md` | Estado actual + evolución planificada |
| `docs/development/openapi.yaml` | Especificación OpenAPI |

> **Tip:** Para consultar documentación actualizada de frameworks (.NET, Astro, React, TipTap, etc.), usa el servidor MCP **Context7** si está disponible en el entorno. Proporciona documentación en tiempo real de las bibliotecas del proyecto.
