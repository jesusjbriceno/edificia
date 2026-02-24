# TAREAS_DESARROLLO_DOTX

## 1. Objetivo

Backlog técnico clasificado para ejecutar la feature `.dotx` con storage delegado en `n8n`, siguiendo Clean Architecture, Código Limpio, `TDD` y `Git Flow`.

Referencia obligatoria: `AGENTS.md`.

---

## 2. Clasificación por bloque

## A. Domain + Application Core (Prioridad P0)

1. Crear entidad `AppTemplate` (invariantes + ciclo de vida).
2. Crear `ITemplateRepository` en Application.
3. Definir comandos/queries MVP:
   - `CreateTemplateCommand`
   - `GetTemplatesQuery`
   - `ToggleTemplateStatusCommand`
4. Definir DTOs request/response.

**Salida esperada:** Modelo de negocio estable para plantillas.

## B. Infrastructure Persistence (P0)

1. Configuración EF de `AppTemplate`.
2. Añadir `DbSet<AppTemplate>` en `EdificiaDbContext`.
3. Implementar `TemplateRepository`.
4. Crear migración `AddTemplateSystem`.

**Salida esperada:** Persistencia metadatos operativa.

## C. Storage Strategy (n8n/local) (P0)

1. Definir interfaz `IFileStorageService`.
2. Implementar `N8nTemplateStorageService`.
3. Implementar `LocalFileStorageService` (contingencia/dev).
4. Resolver proveedor por `TemplateStorage__Provider`.

**Salida esperada:** Guardado/lectura delegado funcionando.

## D. API Templates (P0)

1. `TemplatesController` (Admin):
   - `POST /api/templates`
   - `GET /api/templates`
   - `PUT /api/templates/{id}/toggle-status`
2. Validación FluentValidation (extensión/MIME/tamaño).
3. Persistencia DB solo tras `success=true` webhook.

**Salida esperada:** API MVP funcional y segura.

## E. Export Engine Integration (P0)

1. Refactor `ExportProjectHandler` con selector de motor.
2. Integrar `TemplateDocxGenerator` + fallback legado.
3. L1 cache `IMemoryCache` por `templateType`.
4. `UpdateFieldsOnOpen` para TOC.

**Salida esperada:** Export híbrido estable.

## F. Frontend Admin Templates (P1)

1. Página `admin/templates.astro`.
2. Componente `TemplateForm` con `FormData`.
3. Servicio API templates.
4. UX de validación y errores.

**Salida esperada:** Gestión desde UI.

## G. Hardening + Observabilidad (P1)

1. Trazabilidad `operationId`/`requestId`.
2. Métricas latencia/error webhook.
3. Runbook de fallback `provider=local`.
4. Documentación OpenAPI actualizada.

**Salida esperada:** Preparación producción.

## H. Actualización de Documentación del Proyecto (P1)

1. Actualizar `docs/openapi.yaml` con módulo Templates.
2. Actualizar `docs/development/backend/API_DESIGN.md`.
3. Actualizar `README.md` (feature `.dotx` y estrategia `n8n`).
4. Actualizar `docs/implementation/ROADMAP_DETALLADO.md` con estado de la fase `6.1.1`.
5. Añadir troubleshooting y guía operativa mínima en docs de feature.

**Salida esperada:** Documentación sincronizada con implementación real.

---

## 3. Plan TDD por bloque

- **Domain:** tests de invariantes y transiciones.
- **Application:** tests de handlers y validadores.
- **Infrastructure:** tests de integración DB y adapters n8n/local.
- **API:** tests de endpoints `multipart/form-data` y errores.
- **Frontend:** tests de formulario y flujos UI.

Regla: cada tarea funcional debe tener tests nuevos/actualizados en la misma PR.

---

## 4. Secuencia de ejecución recomendada

1. A → B → C → D → E (Core Backend)
2. F (Frontend)
3. G (Hardening)
4. H (Cierre documental)

---

## 5. Criterio de “Done” por tarea

1. Código implementado.
2. Tests en verde.
3. Docs actualizadas si aplica.
4. PR aprobada sin bloqueos críticos.
