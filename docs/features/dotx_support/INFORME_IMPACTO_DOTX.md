# INFORME_DE_IMPACTO_DOTX

## 1. Alcance y fuentes analizadas

Este informe analiza en profundidad el impacto funcional y técnico de la nueva capacidad de soporte de plantillas `.dotx` para exportación de memorias.

### 1.1. Documentación específica de la feature

- `docs/features/dotx_support/ANALISIS_RF_CU.md`
- `docs/features/dotx_support/REQUISITOS_FUNCIONALES.md`
- `docs/features/dotx_support/DISENO_TECNICO.md`
- `docs/features/dotx_support/GUIA_IMPLEMENTACION.md`
- `docs/features/dotx_support/IMPLEMENTACION_TECNICA.md`
- `docs/features/dotx_support/ESPECIFICACION_TEMPLATE_STORAGE_N8N.md`
- `docs/features/dotx_support/ROADMAP_IMPLEMENTACION_DOTX_N8N.md`

### 1.2. Documentación global del proyecto (relevante)

- `README.md`
- `AGENTS.md`
- `docs/development/GUIDELINES.md`
- `docs/development/backend/API_DESIGN.md`
- `docs/openapi.yaml`
- `docs/implementation/ROADMAP_DETALLADO.md`
- `docs/deployment/GUIA_DESPLIEGUE.md`
- `docs/DISENO_SISTEMA_EDIFICIA.md`

### 1.3. Código actual revisado para gap analysis

- Backend export actual:
  - `apps/api/src/Edificia.API/Controllers/ExportController.cs`
  - `apps/api/src/Edificia.Application/Export/Queries/ExportProject/ExportProjectHandler.cs`
  - `apps/api/src/Edificia.Infrastructure/Export/DocxExportService.cs`
  - `apps/api/src/Edificia.Infrastructure/DependencyInjection.cs`
  - `apps/api/src/Edificia.API/Program.cs`
- Frontend export actual:
  - `apps/web/src/lib/services/projectService.ts`
  - `apps/web/src/lib/hooks/useEditorActions.ts`

---

## 2. Resumen ejecutivo

La funcionalidad propuesta (`dotx_support`) **no está implementada actualmente** en el código productivo. El sistema actual exporta DOCX directamente desde el `content_tree_json` sin soporte de plantillas administrables, sin CRUD de plantillas y sin fallback por tipología de plantilla.

### Diagnóstico de alto nivel

1. **Estado actual:** Exportación DOCX existente y operativa (OpenXML), con endpoint `GET /api/projects/{id}/export`.
2. **Estado objetivo feature:** Gestión completa de plantillas `.dotx` (alta/listado/activación), renderizado basado en plantilla con preservación de estilos y fallback al motor legado.
3. **Gap:** Alto, transversal a dominio, aplicación, infraestructura, API, frontend, despliegue, OpenAPI y testing.
4. **Complejidad total estimada:** Media-Alta (afecta flujos core de exportación + nueva administración + almacenamiento binario).
5. **Estrategia de storage recomendada:** Delegar guardado/recuperación en `n8n` como broker de almacenamiento, con persistencia de metadatos en DB tras confirmación síncrona del flujo.

---

## 3. Estado actual vs estado objetivo

## 3.1. Estado actual (confirmado en código)

- Existe `ExportController` con `GET /api/projects/{id}/export`.
- El handler `ExportProjectHandler` toma `Project.ContentTreeJson` y llama a `IDocumentExportService`.
- `DocxExportService` genera un documento desde cero (`WordprocessingDocument.Create(...)`).
- No existe entidad de dominio `AppTemplate`.
- No existen repositorios/servicios de plantillas.
- No existe `TemplatesController`.
- No existe subida `multipart/form-data` para `.dotx`.
- Frontend no tiene ruta/admin UI de plantillas (`/admin/templates`).

## 3.2. Estado objetivo (documentación feature)

- CRUD administrativo de plantillas `.dotx` (mínimo: alta/listado/toggle; ideal: CRUD completo).
- Persistencia separada:
  - Metadatos en PostgreSQL.
  - Binario delegado por proveedor (`n8n` recomendado; `local` para dev/local).
- Selección de motor en export:
  - Si hay plantilla activa para tipología → render sobre `.dotx`.
  - Si no hay plantilla activa → fallback transparente a motor actual/legado.
- Integración opcional n8n post-upload y/o post-generación.
- Límites de tamaño de fichero y controles de seguridad.

---

## 4. Cobertura de requisitos funcionales (RF-01..RF-10)

| RF | Estado actual | Gap | Impacto |
|---|---|---|---|
| RF-01 Alta de plantillas | No implementado | Total | Backend + Frontend + Infra |
| RF-02 Tipificación | No implementado | Total | Domain + DB + UI |
| RF-03 Metadatos + versión + estado | No implementado | Total | Domain + DB + App |
| RF-04 CRUD/Listado/Activación | No implementado | Total | API + UI + Seguridad |
| RF-05 Render pixel-perfect por `SdtElement` | Parcial (OpenXML existe, pero no por plantilla) | Alto | Export Engine |
| RF-06 Fallback sin plantilla | No implementado (solo un motor) | Alto | Application/Export orchestration |
| RF-07 Auto-update TOC | No implementado en motor actual | Medio | Export Engine |
| RF-08 Almacenamiento agnóstico por env | No implementado para plantillas | Alto | Infrastructure + Deploy |
| RF-09 Límite tamaño upload | No implementado | Alto | API validation + frontend UX |
| RF-10 Hook n8n upload plantilla | No implementado | Medio | Integración asíncrona |

---

## 5. Impacto técnico por capa (Clean Architecture)

## 5.1. Domain (alto impacto)

### Cambios requeridos

- Nueva entidad `AppTemplate` (o nombre equivalente de dominio coherente con el proyecto).
- Definir invariantes de dominio:
  - `TemplateType` válido.
  - Política de versión (autoincremental por tipo).
  - Regla de activación única por tipo (si aplica negocio).
- Política de borrado (lógico/físico) alineada con auditoría.

### Riesgos

- Si no se modela bien la unicidad por tipo/estado, pueden coexistir múltiples plantillas activas y romper la selección del motor.

## 5.2. Application (alto impacto)

### Cambios requeridos

- Casos de uso nuevos (CQRS):
  - `CreateTemplate` (multipart + metadata).
  - `GetTemplatesPaged`.
  - `ToggleTemplateStatus`.
  - Opcionales: `UpdateTemplate`, `DeleteTemplate`.
- Contratos/DTOs request-response.
- Validadores FluentValidation:
  - Extensión `.dotx`.
  - MIME permitido.
  - límite tamaño (RF-09).
  - campos requeridos y longitudes.
- Refactor de `ExportProjectHandler` a selector de motor con fallback.

### Riesgos

- Acoplar lógica de filesystem al handler (debe evitarse; resolver en Infrastructure por interfaces).

## 5.3. Infrastructure (muy alto impacto)

### Cambios requeridos

- Implementar `IFileStorageService` con estrategia por proveedor (`TemplateStorage__Provider`).
- Implementar `N8nTemplateStorageService` (upload/get/delete síncronos por webhook).
- Mantener `LocalFileStorageService` para dev/local y contingencia.
- Implementar repositorio de plantillas (EF Core).
- Añadir `DbSet<AppTemplate>` + configuración + migración.
- Crear motor de render por plantilla (`TemplateDocxGenerator`) sobre OpenXML:
  - Apertura `.dotx`.
  - Conversión a `.docx`.
  - Binding por `SdtElement`.
  - `UpdateFieldsOnOpen` para TOC.
- Mantener motor actual como fallback (o renombrarlo a `LegacyDocxGenerator`).
- Caché L1 (`IMemoryCache`) para binarios de plantilla (opcional L2 Redis como optimización futura).

### Riesgos

- Memoria/LOH al manipular binarios concurrentes.
- Corrupción de plantillas por streams no controlados.
- Inconsistencia metadato↔archivo si no se gestiona compensación en errores.

## 5.4. API (alto impacto)

### Cambios requeridos

- Nuevo `TemplatesController` (rol Admin):
  - `POST /api/templates` (`[FromForm]`).
  - `GET /api/templates` paginado.
  - `PUT /api/templates/{id}/toggle-status`.
  - Opcionales CRUD ampliados.
- Mantener endpoint de export existente, pero con selector de motor interno.
- Manejo de errores ProblemDetails consistente.

### Riesgos

- Cargas grandes sin límite/timeout pueden degradar la API.

## 5.5. Frontend (alto impacto)

### Cambios requeridos

- Nueva ruta administrativa: `apps/web/src/pages/admin/templates.astro`.
- Nuevo componente de formulario con `FormData` para upload `.dotx`.
- Servicio frontend para endpoints de plantillas.
- Validación cliente (Zod): tipo, tamaño, campos.
- Estado visual de subida/errores/éxito.

### Riesgos

- No respetar `multipart/form-data` correctamente (no fijar `Content-Type` manualmente).

## 5.6. DevOps/Deploy (medio-alto impacto)

### Cambios requeridos

- Definir y documentar variable de entorno de storage de plantillas.
- Asegurar volumen persistente en compose/producción para binarios `.dotx`.
- Permisos de escritura del proceso en path de storage.

### Riesgos

- Configurar path no persistente y perder plantillas al recrear contenedores.

---

## 6. Inconsistencias detectadas en documentación

Se detectan puntos a unificar antes de implementación final:

1. **Ruta de export en ejemplos:**
   - Estado real y OpenAPI: `GET /api/projects/{id}/export`.
   - En un diagrama aparece `POST /api/export/project/123` (debe corregirse para evitar ambigüedad).

2. **Estrategia de caché:**
   - Guía técnica propone L1 `IMemoryCache` (coherente con nodo único).
   - Implementación técnica menciona L1+Redis L2.
   - Recomendación: fijar baseline en L1 y dejar L2 como optimización posterior.

3. **Nombres de configuración de storage:**
   - Se proponen `Storage__TemplatesBasePath`, `Templates__VolumePath`, `Export__TemplatePath` en distintos documentos.
   - Recomendación: estandarizar una sola sección (p. ej. `Storage:TemplatesBasePath`).

4. **Alcance CRUD:**
   - RF pide CRUD completo.
   - Guía de implementación Fase 3 define solo POST/GET/toggle.
   - Recomendación: declarar MVP explícito (POST+GET+toggle) y backlog para update/delete.

5. **Estrategia de storage principal:**
  - Algunos textos describen filesystem/volumen como estrategia principal.
  - Recomendación actual: estrategia principal `n8n` en producción; `local` solo para dev/local o contingencia.

6. **Nomenclatura de configuración:**
  - Se consolidan claves bajo `TemplateStorage__*`.
  - Recomendación: evitar mezclar `Storage__TemplatesBasePath`, `Templates__VolumePath` y `Export__TemplatePath`.

---

## 7. Riesgos críticos y mitigaciones

## 7.1. Riesgo de memoria (LOH / concurrencia)

- **Riesgo:** Saturación de RAM al procesar plantillas grandes o múltiples exportaciones simultáneas.
- **Mitigación:**
  - `RecyclableMemoryStreamManager`.
  - Límite de tamaño upload (RF-09).
  - Límite de concurrencia por semáforo en export.
  - Timeouts + `CancellationToken` end-to-end.

## 7.2. Consistencia metadato-archivo

- **Riesgo:** Guardar DB sin archivo o archivo sin DB.
- **Mitigación:**
  - En estrategia `n8n`: persistir DB únicamente después de `OK` del flujo síncrono.
  - Flujo write-order controlado + compensación (`DeleteFileAsync`) ante fallo DB.
  - Logging estructurado con `templateId`.

## 7.5. Dependencia de disponibilidad n8n

- **Riesgo:** El alta/recuperación de plantillas depende de la salud del servicio n8n.
- **Mitigación:**
  - Timeouts, retry acotado e idempotencia por `operationId`.
  - Circuit breaker y métricas de error.
  - Fallback operativo por proveedor `local` (feature flag) para contingencias.

## 7.3. Seguridad de upload

- **Riesgo:** Subida de archivos no válidos o payloads maliciosos.
- **Mitigación:**
  - Validar extensión y MIME.
  - Sanitizar filename y generar nombre interno seguro.
  - Limitar tamaño y contenido.
  - Restringir endpoints a `RequireAdmin`.

## 7.4. Calidad de render

- **Riesgo:** Pérdida de estilos corporativos o binding parcial.
- **Mitigación:**
  - Tests de integración sobre plantillas reales.
  - Contrato de tags de `Content Controls` documentado y versionado.

---

## 8. Cambios requeridos (plan técnico recomendado)

## Fase A — Modelo y persistencia

1. Entidad `AppTemplate` + configuración EF.
2. `DbSet` + migración.
3. Repositorio de plantillas.

## Fase B — Storage y servicios de export

1. `IFileStorageService` + implementación `n8n` (principal) y `local` (fallback).
2. `ITemplateResolver` / `ITemplateRepository`.
3. `TemplateDocxGenerator` (OpenXML + `SdtElement` + `UpdateFieldsOnOpen`).
4. Refactor `ExportProjectHandler` para selector de motor con fallback.

## Fase C — API administrativa

1. `TemplatesController` + políticas admin.
2. CQRS + validadores + ProblemDetails.
3. OpenAPI actualizado con nuevos endpoints y `multipart/form-data`.

## Fase D — Frontend administrativo

1. Página `/admin/templates`.
2. `TemplateForm` con `FormData` y validación Zod.
3. Listado y activación/desactivación.

## Fase E — Operación, observabilidad y hardening

1. Variables env estandarizadas (`TemplateStorage__*`) y secretos webhook.
2. Métricas/logging de export y subida.
3. Test plan (unit/integration/e2e) y smoke tests.

---

## 13. Evaluación de la opción n8n para storage

## 13.1. Ventajas

- Reduce acoplamiento de la API con proveedores concretos (Drive/OneDrive/S3/NAS).
- Simplifica la lógica de persistencia en backend (un único contrato HTTP).
- Mejora control de desalineación DB/binario al confirmar DB tras respuesta `OK`.
- Permite cambiar backend de storage sin desplegar API.

## 13.2. Costes/Riesgos

- Dependencia operacional de `n8n` para operaciones críticas de plantillas.
- Mayor latencia en upload/lectura frente a acceso directo local.
- Necesidad de diseñar contratos robustos (idempotencia, errores, timeouts).

## 13.3. Recomendación final

**Sí, la opción `n8n` es recomendable como estrategia principal en producción**, siempre que se implemente en modo síncrono con contrato fuerte (`success`, `storageKey`, `operationId`) y con mitigaciones de resiliencia.

**Patrón recomendado:**

1. API recibe upload.
2. API invoca webhook `n8n` de storage (síncrono).
3. Si `success=true`, API persiste metadatos en DB.
4. Si error/timeout, API no persiste DB y devuelve ProblemDetails.
5. Para export, API resuelve plantilla activa y recupera contenido vía proveedor configurado.

---

## 9. Estrategia de testing y validación

## 9.1. Backend

- Unit tests:
  - Validadores de upload.
  - Selector de motor (con plantilla / sin plantilla).
  - Reglas de activación por tipo.
- Integration tests:
  - Upload real `multipart/form-data`.
  - Persistencia metadatos + archivo.
  - Export sobre `.dotx` real y fallback.

## 9.2. Frontend

- Unit/integration:
  - Formulario y validaciones.
  - Estados de carga/errores.
  - Servicio API de plantillas.

## 9.3. Criterios de aceptación operativos

- Subir plantilla válida y verla activa en listado.
- Exportar proyecto con plantilla activa: documento con estilos de plantilla.
- Desactivar plantilla y exportar: fallback correcto sin error de usuario.
- Reiniciar contenedor API sin perder plantillas almacenadas.

---

## 10. Decisiones abiertas (a cerrar antes de implementar)

1. **Taxonomía `TemplateType`:** enum cerrado vs catálogo configurable.
2. **Scope de plantilla:** global por tipología vs por organización/proyecto.
3. **Versionado:** auto por tipo, semver manual o timestamp.
4. **Borrado:** lógico obligatorio vs físico permitido.
5. **Caché L2 Redis:** entra en MVP o fase de optimización.
6. **Webhook n8n:** síncrono fire-and-forget o con outbox/retry.

---

## 11. Recomendación de ejecución

Se recomienda implementar como **MVP incremental** en 2 iteraciones:

- **Iteración 1 (core funcional):** RF-01, RF-02, RF-03, RF-05, RF-06, RF-08, RF-09.
- **Iteración 2 (completitud y operación):** RF-04 completo, RF-07 robusto, RF-10, hardening, observabilidad y mejoras de rendimiento.

Esta estrategia minimiza riesgo en producción y permite entregar valor temprano sin comprometer la arquitectura Clean + CQRS del proyecto.

---

## 12. Conclusión

La feature de soporte `.dotx` es coherente con el roadmap (`6.1.1`) y aporta alto valor de negocio (salidas corporativas pixel-perfect). El impacto es transversal y exige cambios coordinados en todas las capas. El estado actual del código cubre solo la base de exportación DOCX; por tanto, la implementación de `dotx_support` debe abordarse como una ampliación arquitectónica completa, no como ajuste menor. Con el contexto operativo actual, la estrategia de storage delegado en `n8n` es la opción más flexible y controlable para producción.
