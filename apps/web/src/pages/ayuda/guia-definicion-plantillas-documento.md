---
layout: ../../layouts/HelpMarkdownLayout.astro
title: Guía de definición de plantillas
---

# 📘 Guía de definición de plantillas para EdificIA

**Objetivo:** crear una plantilla de Word que EdificIA pueda usar para exportar memorias con estilo corporativo.

**Nivel:** básico (paso a paso, sin conocimientos técnicos avanzados).

---

## 1) Requisitos previos

- Microsoft Word (recomendado Microsoft 365 o Word 2019+).
- Tener claro qué partes del documento van a ser dinámicas (las que rellena EdificIA).
- Un documento de referencia de la memoria (si ya tenéis uno en `.docx`, mejor).

---

## 2) Crear el archivo base

1. Abre Word.
2. Crea un documento nuevo en blanco.
3. Guarda inmediatamente con un nombre de trabajo (por ejemplo: `plantilla-memoria-v1.docx`).

> Consejo: guarda pronto para no perder el formato si Word se cierra.

---

## 3) Activar la pestaña "Programador" (Developer)

1. En Word, ve a **Archivo > Opciones**.
2. En el menú izquierdo, abre **Personalizar cinta de opciones**.
3. En la columna de la derecha, marca **Programador**.
4. Pulsa **Aceptar**.

Ahora verás la pestaña **Programador** en la parte superior.

---

## 4) Diseñar la plantilla (estilo corporativo)

En esta fase trabajas como en un documento normal de Word:

- Define estilos de títulos (Título 1, Título 2, Título 3).
- Configura tipografía, márgenes, espaciados y numeraciones.
- Añade cabecera/pie, logotipo, portada, tabla de contenidos, etc.

✅ **Importante para EdificIA:**

- Todo el formato visual se conserva.
- Lo dinámico se insertará en los **Content Controls** (paso siguiente).

---

## 5) Insertar Content Controls (controles de contenido)

Los **Content Controls** son “cajas inteligentes” donde EdificIA inyecta contenido.

### 5.1 Insertar un control de texto enriquecido

1. Coloca el cursor donde quieras contenido dinámico.
2. Ve a **Programador**.
3. Pulsa **Control de contenido de texto enriquecido**.
4. Aparecerá un bloque gris en el documento.

### 5.2 Configurar el control (lo más importante)

1. Haz clic sobre el control insertado.
2. En **Programador**, pulsa **Propiedades**.
3. Rellena:
   - **Título**: nombre legible (ejemplo: `Descripción Proyecto`).
   - **Etiqueta (Tag)**: clave técnica que usa el backend.
4. Pulsa **Aceptar**.

⚠️ **Regla clave:** la **Etiqueta (Tag)** debe respetar el contrato funcional acordado con backend.

---

## 6) Contrato de `Tags` (recomendado)

### 6.1 Estado actual del proyecto

- El flujo de exportación con plantilla Word está activo para `TemplateType = MemoriaTecnica`.
- El sistema mantiene fallback al exportador estándar si hay incidencias.
- Aunque el contrato de `tags` no está bloqueado de forma rígida en esta guía, **se recomienda versionarlo** para evitar desalineaciones entre plantilla y backend.

### 6.2 Plantilla de contrato (ejemplo base)

| Tag | Descripción | Origen de dato | Obligatorio | Ejemplo |
|---|---|---|---|---|
| `ProjectTitle` | Título del proyecto | `Project.Title` | Sí | Reforma integral vivienda |
| `ProjectAddress` | Dirección del proyecto | `Project.Address` | No | Calle Mayor 123, Madrid |
| `InterventionType` | Tipo de intervención | `Project.InterventionType` | Sí | Reform |
| `IsLoeRequired` | Indicador LOE | `Project.IsLoeRequired` | Sí | Sí/No |
| `MD.01` | Sección memoria descriptiva 01 | Árbol de contenido (`sectionId`) | No | Agentes intervinientes |
| `MC.01` | Sección memoria constructiva 01 | Árbol de contenido (`sectionId`) | No | Sistema estructural |

### 6.3 Reglas de naming recomendadas

- Sin espacios en `Tag`.
- Usar prefijos consistentes (`MD.*`, `MC.*`, etc.) o claves semánticas estables.
- Evitar cambios de `Tag` en plantillas ya publicadas sin versionar (`v2`, `v3`, ...).

---

## 7) Repetir para todos los bloques dinámicos

Repite el paso 5 para cada zona variable:

- título del proyecto
- emplazamiento
- apartados de memoria descriptiva
- apartados de memoria constructiva
- notas finales

✅ Puedes combinar texto fijo + controles dinámicos en el mismo párrafo.

---

## 8) Añadir tabla de contenidos (opcional, recomendado)

1. Ve a **Referencias > Tabla de contenido**.
2. Inserta una tabla automática.
3. Mantén títulos con estilos de Word (`Título 1/2/3`) para que funcione bien.

En EdificIA se fuerza la actualización de campos al abrir, así que Word recalculará índice y numeración de páginas.

---

## 9) Guardar como plantilla de Word

1. Ve a **Archivo > Guardar como**.
2. Elige ubicación.
3. En **Tipo**, selecciona **Plantilla de Word**.
4. Guarda el archivo.

Nombre recomendado:

- `memoria-tecnica-corporativa-v1`

---

## 10) Subir a EdificIA

1. Entra como **Admin**.
2. Ve a **/admin/templates**.
3. Sube el archivo Word compatible (selección de archivo o arrastrar y soltar en la zona **Drag & Drop**).
4. Asigna tipo `MemoriaTecnica`.
5. Activa la plantilla.

### 10.1) ¿Cómo se aplica la plantilla en exportación?

Estado actual:

- La exportación usa automáticamente la plantilla **activa** para `MemoriaTecnica`.
- Si no hay plantilla activa o falla el render con plantilla, se aplica fallback al exportador estándar.

Evolución prevista:

- Se incorporará selector de plantilla en el flujo de exportación cuando existan varias plantillas disponibles para el mismo tipo documental.
- Si solo hay una disponible, se seleccionará por defecto.

---

## 11) Prueba rápida (checklist)

- [ ] Se puede exportar un proyecto en `.docx`.
- [ ] Se aplican estilos corporativos (tipos de letra, cabecera, etc.).
- [ ] Los bloques dinámicos aparecen rellenos.
- [ ] La tabla de contenidos se actualiza al abrir en Word.
- [ ] Si desactivas la plantilla, la exportación sigue funcionando (fallback).

---

## 11.1) Validación automática en subida (backend)

Al subir una plantilla en `/admin/templates`, EdificIA valida automáticamente:

- Formato Word OpenXML compatible.
- Tamaño máximo: **10 MB**.
- MIME compatible con plantilla Word OpenXML.
- Que el archivo se pueda abrir como OpenXML válido.
- Que exista cuerpo principal del documento (`word/document.xml`).
- Que haya **Content Controls con `Tag`**.

Para `TemplateType = MemoriaTecnica`, además se exigen estos `Tag` mínimos:

- `ProjectTitle`
- `MD.01`
- `MC.01`

Si alguno falla, la API devuelve error de validación (`Template.InvalidFormat`) y no guarda la plantilla.

---

## 12) Errores típicos y cómo resolverlos

### Problema: “No se rellena un bloque”

- Revisa la **Etiqueta (Tag)** del control.
- Comprueba que coincide exactamente con la clave esperada.
- Evita espacios extra o mayúsculas/minúsculas distintas si vuestro contrato es estricto.

### Problema: “Se rompe el formato al rellenar”

- Usa controles de **texto enriquecido** en lugar de texto plano.
- Aplica estilo al contenedor/párrafo de Word, no al texto manual dentro del control.

### Problema: “No aparece en exportación”

- Verifica que la plantilla está **activa** en `/admin/templates`.
- Reexporta el documento.

### Problema: “La subida falla por `Tag` obligatorios”

- En la vista de `/admin/templates`, el mensaje de error indica los `Tag` faltantes (por ejemplo: `ProjectTitle`, `MD.01`, `MC.01`).
- Abre la plantilla en Word y añade un **Content Control de texto enriquecido** por cada `Tag` faltante.
- En **Propiedades** del control, configura la **Etiqueta (Tag)** exactamente con el valor esperado.
- Guarda de nuevo como plantilla de Word y vuelve a subir.

---

## 13) Buenas prácticas recomendadas

- Versiona tus plantillas (`v1`, `v2`, `v3`).
- Cambia una cosa cada vez y prueba exportación.
- Evita diseños excesivamente complejos en primera versión.
- Guarda una plantilla “estable” siempre activa y prueba cambios en una nueva versión.

---

## 14) Plantilla mínima sugerida (MVP)

Para empezar rápido, crea una plantilla con:

1. Portada con estilo corporativo.
2. Un bloque dinámico para título de proyecto.
3. Un bloque dinámico para descripción principal.
4. Tabla de contenidos.
5. Pie de página con versión de plantilla.

Con eso ya tienes una base funcional y fácil de mantener.

---

## Recursos

- [Descargar plantilla base](/resources/memoria-tecnica-base-v1)
- [Centro de ayuda](/ayuda)
- [Guía de definición de plantillas](/ayuda/guia-definicion-plantillas-documento)
