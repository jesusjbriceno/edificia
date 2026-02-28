# Informe de Cambios y Evolución `.dotx` (2026-02)

## 1. Estado real actual (baseline)

### Implementado y operativo

- Validación backend de subida `.dotx`:
  - Extensión, tamaño, MIME permitido.
  - Firma ZIP/OpenXML y tipo real `Template`.
  - Reglas de `Tag` obligatorios para `MemoriaTecnica` (`ProjectTitle`, `MD.01`, `MC.01`).
- Gestión admin en `/admin/templates`:
  - Alta de plantilla.
  - Listado de plantillas.
  - Activar/desactivar.
  - Mensajes guiados de validación.
  - Drag & Drop + selección manual.
- Exportación:
  - Selección automática de plantilla activa para `TemplateType = MemoriaTecnica`.
  - Fallback al exportador estándar si no hay plantilla activa o falla el render con plantilla.

### Restricciones actuales (colisiones detectadas)

1. Selección de plantilla en exportación no configurable por usuario/proyecto.
2. No existe estado separado `Disponible` vs `Predeterminada`; solo `IsActive`.
3. No existe catálogo administrable de tipos de plantilla.
4. Flujo de exportación usa tipo por defecto fijo (`MemoriaTecnica`).
5. UI de templates está en vista única (listado + formulario), no separada por páginas.

---

## 2. Objetivo de evolución

Evolucionar de un modelo de plantilla única activa por tipo hacia un modelo escalable para múltiples tipos documentales y selección explícita de plantilla en exportación.

### Principios

- Mantener compatibilidad hacia atrás durante transición.
- Evitar ruptura de exportación existente.
- Introducir cambios en fases incrementales con rollback claro.

---

## 3. Modelo objetivo propuesto

### 3.1 Estado de plantilla

- `IsAvailable`: define si la plantilla se puede usar en selector de exportación.
- `IsDefault`: define plantilla predeterminada para un tipo documental.
- Regla: solo una `IsDefault = true` por tipo documental.

### 3.2 Tipos de plantilla

- Pasar de tipo “hardcoded” a catálogo persistido en DB (`template_types`).
- Gestión Admin/SuperAdmin:
  - Alta/baja lógica.
  - Nombre visible.
  - Código interno estable.
  - Estado activo del tipo.

### 3.3 Exportación

- Endpoint de export debe aceptar:
  - `templateId` opcional.
  - `outputFileName` opcional.
- Si no se envía `templateId`:
  - usar predeterminada del tipo documental;
  - si no existe, fallback a motor estándar.

### 3.4 UX

- Plantillas:
  - Vista índice (resumen + listado + acciones).
  - Vista separada para alta/edición.
- Exportación:
  - Panel/modal con selector de plantilla disponible.
  - Nombre sugerido editable para el documento.
  - Preselección automática cuando solo existe una opción.

---

## 4. Plan incremental (alto nivel)

### Fase 1 — UX sin ruptura (rápida)

- Separar vista de listado y formulario de templates.
- Añadir estado rápido y explicación de asignación actual.
- En export UI, añadir panel de selección manteniendo API actual (sin `templateId`).

### Fase 2 — Backend de selección de plantilla

- Extender contrato de export para aceptar `templateId` y `outputFileName`.
- Añadir consulta de plantillas disponibles por tipo.
- Mantener fallback automático.

### Fase 3 — Modelo escalable de estados

- Introducir `IsAvailable` + `IsDefault`.
- Migrar reglas y endpoints de activación.
- Garantizar unicidad de default por tipo.

### Fase 4 — Catálogo dinámico de tipos

- Añadir entidad/catálogo de tipos de plantilla.
- UI admin/superadmin para tipos.
- Reglas de validación de tags por tipo (configurable/versionable).

### Fase 5 — Cierre documental integral (obligatoria)

- Actualizar documentación de feature y documentación general:
  - `README.md`
  - `AGENTS.md`
  - guías de ayuda
  - diseño técnico
  - casos de uso
  - roadmap y tareas
  - OpenAPI/API design

---

## 5. Riesgos y mitigaciones

- **Riesgo de ruptura en export actual**: mantener fallback y compatibilidad de endpoint durante transición.
- **Complejidad en estado de plantillas**: migrar en dos pasos (`IsActive` → `IsAvailable/IsDefault`) con migración reversible.
- **Inconsistencia documental**: incluir fase final obligatoria de sincronización documental en Definition of Done.

---

## 6. Criterio de aceptación de la evolución

- El usuario puede elegir plantilla al exportar cuando haya más de una disponible.
- Si solo hay una plantilla disponible para el tipo, se selecciona automáticamente.
- Si no hay plantilla válida/seleccionada, el sistema mantiene exportación por fallback.
- El catálogo de tipos de plantilla se gestiona sin cambios de código.
- Toda la documentación técnica/funcional y de ayuda queda sincronizada con comportamiento real.
