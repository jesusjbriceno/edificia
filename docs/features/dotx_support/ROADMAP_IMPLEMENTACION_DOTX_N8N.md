# ROADMAP_IMPLEMENTACION_DOTX_N8N

## 1. Objetivo

Ejecutar la feature de soporte de plantillas `.dotx` con almacenamiento delegado en `n8n`, manteniendo coherencia con Clean Architecture, `Git Flow`, `TDD` y gobernanza de `PRs`.

Documento rector obligatorio durante toda la ejecución: `AGENTS.md`.

## 1.1 Estado real a 2026-02 (baseline)

- Upload de plantillas `.dotx` implementado con validación avanzada (extensión/MIME/tamaño/OpenXML/tags).
- Gestión admin implementada en `/admin/templates` (alta, listado, activar/desactivar).
- Exportación híbrida implementada con fallback al motor estándar.
- Selección de plantilla actual: automática por plantilla activa de `MemoriaTecnica` (sin selector por usuario).

---

## 1.2 Objetivo de evolución (siguiente iteración)

Evolucionar desde el modelo actual (activa única por tipo) a un modelo escalable con:

1. Plantillas disponibles + plantilla predeterminada por tipo documental.
2. Selector de plantilla en el flujo de exportación.
3. Catálogo dinámico de tipos de plantilla administrable por Admin/SuperAdmin.

---

## 2. Estrategia de entrega

- Entrega incremental por fases cortas.
- Cada fase termina con tests en verde + `PR` aprobada.
- Regla: no avanzar de fase con deuda crítica abierta.

---

## 3. Git Flow obligatorio

## 3.1. Ramas

- Base: `develop`
- Feature principal: `feature/dotx-support-n8n-storage`
- Sub-features recomendadas:
  - `feature/dotx-domain-persistence`
  - `feature/dotx-n8n-storage-adapter`
  - `feature/dotx-export-selector`
  - `feature/dotx-admin-ui`
  - `feature/dotx-tests-hardening`

## 3.2. Reglas

1. Prohibido commit directo en `main` y `develop`.
2. Todo merge entra vía `PR`.
3. Commits con Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, etc.).
4. Rebase o merge con `develop` antes de pedir aprobación final.

---

## 4. Política TDD obligatoria

## 4.1. Ciclo

1. **Red:** escribir test que falla.
2. **Green:** implementar mínimo para pasar.
3. **Refactor:** limpiar sin romper tests.

## 4.2. Cobertura mínima por bloque

- Dominio y aplicación: tests unitarios obligatorios.
- Integración API (`multipart/form-data`, contract n8n): tests de integración obligatorios.
- Frontend formularios/servicios: unit + integración.

## 4.3. Regla de aceptación

- No se acepta `PR` sin evidencia de tests nuevos/actualizados para el cambio funcional.

---

## 5. Fases de implementación

## FASE 0 — Preparación y contrato

**Entrada:** especificación `template-storage` aprobada funcionalmente.

### Tareas

1. Congelar contrato `ESPECIFICACION_TEMPLATE_STORAGE_N8N.md` v1.0.
2. Definir claves de entorno `TemplateStorage__*`.
3. Definir estrategia de fallback (`n8n` principal, `local` contingencia).

### TDD

- Test de validación de configuración (provider + secrets obligatorios).

### Salida

- Contrato y configuración cerrados.

---

## FASE 1 — Dominio + persistencia (DB)

### Tareas

1. Crear entidad `AppTemplate` + configuración EF.
2. Añadir `DbSet<AppTemplate>`.
3. Migración `AddTemplateSystem`.
4. Repositorio de plantillas.

### TDD

- Tests de invariantes de dominio:
  - tipo válido,
  - activación única por tipo,
  - versionado.
- Tests de repositorio EF (integration).

### Salida

- Persistencia de metadatos estable y versionada.

---

## FASE 2 — Adapter de storage delegado (`n8n`)

### Tareas

1. Implementar `IFileStorageService`.
2. Implementar `N8nTemplateStorageService`:
   - `UPLOAD_TEMPLATE`, `GET_TEMPLATE`, `DELETE_TEMPLATE`.
3. Implementar `LocalFileStorageService` (dev/contingencia).
4. Resolver selección por `TemplateStorage__Provider`.

### TDD

- Unit tests del adapter n8n:
  - mapeo request/response,
  - errores por código,
  - timeout/retry.
- Tests de idempotencia (mock de respuestas repetidas).

### Salida

- API capaz de almacenar/leer plantillas vía n8n con fallback local.

---

## FASE 3 — API Templates + reglas de negocio

### Tareas

1. `TemplatesController` (Admin):
   - `POST /api/templates`
   - `GET /api/templates`
   - `PUT /api/templates/{id}/toggle-status`
2. Validadores FluentValidation:
   - extensión, MIME, tamaño, campos.
3. Persistir DB solo tras `success=true` del webhook.
4. Invalidación de caché por `templateType`.

### TDD

- Integration tests de endpoint upload (`multipart/form-data`).
- Casos de error:
  - webhook fail,
  - timeout,
  - validación de archivo.

### Salida

- CRUD MVP de plantillas disponible y seguro.

---

## FASE 4 — Export selector + fallback

### Tareas

1. Refactor `ExportProjectHandler`:
   - resolver plantilla activa,
   - usar `TemplateDocxGenerator` cuando exista,
   - fallback a motor legado cuando no exista.
2. Añadir `UpdateFieldsOnOpen` para TOC.
3. Integrar `IMemoryCache` L1 para plantillas.

### TDD

- Unit tests selector de motor:
  - con plantilla,
  - sin plantilla,
  - plantilla corrupta (fallback/errores controlados).
- Integration test export end-to-end.

### Salida

- Exportación híbrida funcionando con fallback transparente.

---

## FASE 5 — Frontend Admin Templates

### Tareas

1. Crear `apps/web/src/pages/admin/templates.astro`.
2. Crear `TemplateForm` + listado/toggle.
3. Servicio frontend para `/api/templates`.
4. UX de errores de upload y estados de carga.

### TDD

- Unit tests Zod/form.
- Integration tests UI:
  - upload ok,
  - upload inválido,
  - toggle status.

### Salida

- Administración de plantillas operativa desde UI.

---

## FASE 6 — Hardening, observabilidad y release

### Tareas

1. Logging estructurado con `operationId`/`requestId`.
2. Métricas de latencia/error para webhook n8n.
3. Documentación OpenAPI de endpoints templates.
4. Runbook de contingencia (`provider=local`).

### TDD

- Smoke tests de regresión export y templates.

### Salida

- Feature lista para producción controlada.

---

## FASE 7 — Actualización de documentación del proyecto

### Tareas

1. Actualizar `docs/openapi.yaml` con endpoints de templates.
2. Actualizar `docs/development/backend/API_DESIGN.md` con contratos request/response y errores.
3. Actualizar `README.md` (sección de features/documentación) con soporte `.dotx` y storage delegado.
4. Actualizar `docs/implementation/ROADMAP_DETALLADO.md` con estado real de `6.1.1`.
5. Añadir guía operativa de contingencia (`provider=local`) y troubleshooting básico n8n.

### TDD / Validación documental

- Verificar consistencia entre documentación y comportamiento real de endpoints.
- Validar ejemplos de request/response contra implementación final.

### Salida

- Documentación funcional/técnica sincronizada con el código implementado.

---

## FASE 8 — UX de plantillas por vistas y exportación guiada

### Tareas

1. Separar en frontend la vista de listado de plantillas y la vista/formulario de alta/edición.
2. Añadir estado rápido de disponibilidad y plantilla predeterminada por tipo.
3. Añadir panel/modal de exportación en editor con selector de plantilla y nombre sugerido de archivo.

### Salida

- Gestión de plantillas y exportación con UX escalable para crecimiento funcional.

---

## FASE 9 — Modelo de dominio escalable (`Disponible` + `Predeterminada`)

### Tareas

1. Extender modelo de plantilla para separar disponibilidad y predeterminación.
2. Garantizar unicidad de plantilla predeterminada por tipo documental.
3. Actualizar APIs y reglas de negocio de activación/desactivación.

### Salida

- Múltiples plantillas por tipo disponibles para selección y una predeterminada controlada.

---

## FASE 10 — Catálogo dinámico de tipos de plantilla

### Tareas

1. Introducir catálogo persistido de tipos de plantilla (`template_types`).
2. Exponer APIs y UI admin/superadmin para alta/baja/edición de tipos.
3. Versionar reglas de validación de `tags` por tipo documental.

### Salida

- Alta escalabilidad sin cambios de código para introducir nuevos tipos documentales.

---

## FASE 11 — Cierre documental integral (obligatoria y final)

### Tareas

1. Actualizar toda la documentación de la feature (`dotx_support`) al estado final.
2. Actualizar documentación general (`README.md`, `AGENTS.md`, `docs/openapi.yaml`, `API_DESIGN.md`, `ROADMAP_DETALLADO.md`).
3. Actualizar guía de ayuda en aplicación (`/ayuda/*`) alineada a comportamiento real.
4. Ejecutar revisión cruzada de consistencia (funcional, técnica y UX) antes de cerrar la feature.

### Criterio de cierre

- No se cierra la feature `.dotx` si existe desalineación entre código y documentación.

---

## 6. Política de PRs y aprobaciones

## 6.1. Requisitos mínimos de PR

Cada `PR` debe incluir:

1. Objetivo funcional y alcance.
2. Riesgos y mitigaciones.
3. Evidencia de tests ejecutados.
4. Cambios de configuración/env.
5. Impacto en docs/OpenAPI (si aplica).

## 6.2. Aprobaciones

- Mínimo recomendado: **2 aprobaciones**
  - 1 aprobación backend/arquitectura.
  - 1 aprobación frontend o QA (según alcance).
- Bloqueantes:
  - checks CI en verde,
  - conflictos resueltos,
  - sin comentarios críticos abiertos.

## 6.3. Merge policy

- Merge a `develop` solo tras aprobación completa.
- `squash merge` para PRs pequeñas/medias; `merge commit` si conviene preservar historial de fase.

---

## 7. Definition of Done (DoD)

Una fase se considera finalizada cuando:

1. Código implementado según alcance de fase.
2. Tests de fase en verde (unit/integration).
3. Documentación actualizada.
4. `PR` aprobada y mergeada a `develop`.
5. Sin incidencias críticas abiertas.

---

## 8. Orden sugerido de ejecución (2 iteraciones)

## Iteración 1 (Core)

- Fase 0 → Fase 4

Objetivo: tener backend funcional completo (storage n8n + export híbrido + fallback).

## Iteración 2 (Operación y UX)

- Fase 5 → Fase 6

Objetivo: cerrar administración UI, observabilidad y preparación de release.

---

## 9. Dependencias de entrada

1. Servidor n8n accesible desde API.
2. Secreto compartido configurado en ambos lados.
3. Workflow `template-storage` desplegado.
4. Backend de storage elegido y credenciales válidas en n8n.
