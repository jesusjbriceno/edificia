# PLAN_CU_IMPLEMENTACION_2026-03

## 1. Objetivo de esta iteración

Definir un plan de ejecución **por Casos de Uso (CU)** para evolucionar la gestión de plantillas `.dotx` desde el MVP actual a un modelo escalable y coherente con el flujo de exportación real.

Este plan cubre:
- Cambios funcionales.
- Tareas técnicas por capa (Domain/Application/Infrastructure/API/Web).
- Plan de pruebas.
- Tareas finales de revisión y documentación.

---

## 2. Alcance funcional (iteración)

### Incluido
1. Eliminar plantillas (con reglas de seguridad y consistencia).
2. Editar plantillas (metadatos y reemplazo de binario).
3. Permitir múltiples plantillas disponibles por tipo.
4. Definir una plantilla predeterminada por tipo.
5. Hacer efectivo el selector de plantilla en exportación (`templateId`) y nombre de archivo (`outputFileName`).

### No incluido en esta iteración (queda planificado)
1. CU-05 completo de catálogo dinámico de tipos (se deja preparado, pero no necesariamente cerrado end-to-end).
2. Automatizaciones post-export (CU-03).

---

## 3. Secuencia de ejecución recomendada (PR slicing)

1. **PR-1 (P0):** Modelo de dominio/DB para `IsAvailable` + `IsDefault`.
2. **PR-2 (P0):** API de gestión (delete, update, disponibilidad/predeterminada).
3. **PR-3 (P0):** Exportación con selección explícita de plantilla y nombre de archivo.
4. **PR-4 (P1):** Frontend admin (editar/eliminar/disponibilidad/predeterminada).
5. **PR-5 (P1):** Frontend export modal conectado con API real.
6. **PR-6 (P0):** Cierre de tests, hardening, documentación y revisión final.

Dependencia estricta: PR-1 -> PR-2 -> PR-3 -> PR-4/PR-5 -> PR-6.

---

## 4. Backlog por Caso de Uso

## CU-01 — Administrar plantilla documental (alta/edición/eliminación)

### Resultado esperado
El administrador puede crear, editar y eliminar plantillas, con validación `.dotx`, versionado y consistencia entre metadatos y storage.

### Tareas

#### A. Domain/Application
- [ ] Añadir operaciones de dominio para editar metadatos y reemplazar binario sin romper invariantes.
- [ ] Definir comandos y validadores:
  - [ ] `UpdateTemplateMetadataCommand`
  - [ ] `ReplaceTemplateBinaryCommand`
  - [ ] `DeleteTemplateCommand`
- [ ] Definir reglas de negocio:
  - [ ] No eliminar plantilla marcada como predeterminada sin reasignación/confirmación.
  - [ ] No permitir metadatos inválidos.

#### B. Infrastructure/API
- [ ] Extender `ITemplateRepository` con consultas/operaciones necesarias.
- [ ] Añadir endpoints:
  - [ ] `PUT /api/templates/{id}` (metadatos)
  - [ ] `PUT /api/templates/{id}/binary` (multipart para `.dotx`)
  - [ ] `DELETE /api/templates/{id}`
- [ ] Ejecutar borrado coordinado metadata + storage (`IFileStorageService.DeleteFileAsync`).

#### C. Frontend
- [ ] Añadir acciones de edición y eliminación en `/admin/templates`.
- [ ] Crear formulario de edición (metadatos + reemplazo de fichero).
- [ ] Añadir UX de confirmación en eliminación.

#### D. Tests
- [ ] Unit tests de handlers (casos OK/error).
- [ ] Tests de validadores (metadatos, mime/extensión, tamaño).
- [ ] Tests integración API (update/delete/reemplazo binario).
- [ ] Tests web para flujo editar/eliminar.

---

## CU-02 — Exportar proyecto con selector real + fallback

### Resultado esperado
El selector de plantilla del modal de exportación se aplica realmente en backend y mantiene fallback transparente al exportador estándar.

### Tareas

#### A. Application/API
- [ ] Extender contrato de export para admitir:
  - [ ] `templateId` opcional
  - [ ] `outputFileName` opcional
- [ ] Ajustar `ExportProjectQuery` y `ExportProjectHandler` para resolver plantilla en este orden:
  1. Plantilla seleccionada (`templateId`) si es válida y disponible.
  2. Predeterminada por tipo documental.
  3. Fallback al exportador estándar.
- [ ] Validar pertenencia de `templateId` al tipo esperado.

#### B. Frontend
- [ ] Adaptar `projectService.exportDocx` para enviar parámetros de exportación.
- [ ] Conectar `ExportDocxModal` con llamada real (`templateId`, `fileName`).
- [ ] Ajustar estados y mensajes del modal según respuesta backend.

#### C. Tests
- [ ] Unit tests de `ExportProjectHandler` para rutas:
  - [ ] con `templateId` válido
  - [ ] con `templateId` inválido/no disponible
  - [ ] sin `templateId` + predeterminada
  - [ ] fallback por fallo de plantilla
- [ ] Tests frontend de modal + llamada a servicio con opciones.
- [ ] Regression tests de descarga en editor.

---

## CU-04 — Gestionar disponibilidad/predeterminación

### Resultado esperado
Se permiten múltiples plantillas disponibles por tipo y exactamente una predeterminada por tipo.

### Tareas

#### A. Modelo y Persistencia
- [ ] Sustituir `IsActive` por:
  - [ ] `IsAvailable`
  - [ ] `IsDefault`
- [ ] Migración DB:
  - [ ] eliminar índice único parcial por `is_active`
  - [ ] crear índice único parcial para `is_default=true` por tipo
  - [ ] migrar datos de estado actual a nuevo modelo

#### B. Application/API
- [ ] Reemplazar `ToggleTemplateStatus` por operaciones explícitas:
  - [ ] `SetTemplateAvailability`
  - [ ] `SetTemplateAsDefault`
- [ ] Validar coherencia de reglas al cambiar predeterminada.

#### C. Frontend
- [ ] Actualizar listado admin para mostrar dos estados:
  - [ ] Disponible / No disponible
  - [ ] Predeterminada / No predeterminada
- [ ] Permitir varias disponibles del mismo tipo.

#### D. Tests
- [ ] Unit tests de reglas de unicidad predeterminada.
- [ ] Tests integración de migración y constraints.
- [ ] Tests UI de cambios de estado.

---

## CU-05 — Tipos de plantilla dinámicos (parcial de preparación)

### Resultado esperado en esta iteración
Dejar preparado el terreno para tipos dinámicos sin cerrar completamente la gestión UI Admin/SuperAdmin.

### Tareas
- [ ] Definir diseño técnico de entidad `template_types`.
- [ ] Definir contrato API mínimo para consulta de tipos.
- [ ] Refactor de validaciones para desacoplar `MemoriaTecnica` hardcoded.
- [ ] Crear backlog técnico de implementación completa (siguiente iteración).

### Tests mínimos
- [ ] Unit tests de resolución de reglas por tipo (si se introduce refactor).

---

## 5. Tareas transversales de revisión técnica (obligatorias)

### Revisión de implementación
- [ ] Revisar consistencia de reglas de negocio en Domain/Application.
- [ ] Revisar seguridad y permisos de endpoints Admin.
- [ ] Revisar manejo de errores y códigos `ProblemDetails`.
- [ ] Revisar compatibilidad retro (fallback export).

### Revisión de rendimiento/operación
- [ ] Verificar carga/descarga de binarios grandes sin regresión.
- [ ] Verificar invalidación de caché de plantillas en cambios de estado.
- [ ] Verificar resiliencia de storage remoto (`n8n`) y fallback local.

---

## 6. Tareas finales de documentación (obligatorias)

- [ ] Actualizar `docs/openapi.yaml` con nuevos endpoints/contratos.
- [ ] Actualizar `docs/development/backend/API_DESIGN.md`.
- [ ] Actualizar documentación de feature en `docs/features/dotx_support/`:
  - [ ] `REQUISITOS_FUNCIONALES.md`
  - [ ] `ANALISIS_RF_CU.md`
  - [ ] `ROADMAP_IMPLEMENTACION_DOTX_N8N.md`
  - [ ] `TAREAS_DESARROLLO_DOTX.md`
- [ ] Actualizar `README.md` con estado funcional real del módulo.
- [ ] Añadir notas de migración para despliegue (DB + configuración).

---

## 7. Plan de ejecución de tests (última fase antes de merge)

## 7.1 Backend (.NET)
- [ ] Ejecutar unit tests de plantillas.
- [ ] Ejecutar unit tests de exportación.
- [ ] Ejecutar tests integración de API afectados.
- [ ] Revisar cobertura de nuevos handlers/validadores.

## 7.2 Frontend (Astro/React)
- [ ] Ejecutar tests de `TemplateManagement` y `TemplateUploadForm`.
- [ ] Ejecutar tests de exportación (`ExportDocx`, `useEditorActions`).
- [ ] Añadir/ejecutar tests de nuevos formularios de edición.

## 7.3 Validación final de regresión funcional
- [ ] Flujo Admin completo: alta -> editar -> disponibilidad -> predeterminada -> eliminar.
- [ ] Flujo exportación: selector real + fallback.
- [ ] Verificación de mensajes de error guiados al usuario.

---

## 8. Criterio de cierre (Definition of Done de iteración)

- [ ] Todos los CUs planificados para esta iteración completados.
- [ ] Tests backend/frontend en verde.
- [ ] OpenAPI y documentación funcional/técnica actualizadas.
- [ ] Sin errores críticos abiertos en revisión.
- [ ] PRs fusionadas en orden y sin deuda bloqueante.

---

## 9. Notas de gobierno de cambios

- Mantener cambios pequeños por PR, con trazabilidad CU -> tarea -> test.
- Evitar introducir nuevas capacidades fuera de alcance sin RFC previa.
- Priorizar compatibilidad y fallback para no interrumpir exportaciones en producción.
