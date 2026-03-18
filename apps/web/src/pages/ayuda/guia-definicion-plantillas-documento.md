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

## 3) Diseñar la plantilla (estilo corporativo)

En esta fase trabajas como en un documento normal de Word:

- Define estilos de títulos (Título 1, Título 2, Título 3).
- Configura tipografía, márgenes, espaciados y numeraciones.
- Añade cabecera/pie, logotipo, portada, tabla de contenidos, etc.

✅ **Importante para EdificIA:**

- Todo el formato visual (estilos, cabeceras, pies de página) se conserva.
- El contenido dinámico se inyecta mediante **placeholders** (paso siguiente).

---

## 4) Insertar placeholders de metadatos

Los placeholders son marcas en el texto que EdificIA sustituye por los datos reales del proyecto durante la exportación.

### 4.1 Placeholders de texto con dobles llaves `{{...}}`

Escribe directamente en el cuerpo del documento (portada, cabecera, pie de página):

| Placeholder | Valor inyectado |
|---|---|
| `{{PROJECT_TITLE}}` | Título del proyecto |
| `{{PROJECT_ADDRESS}}` | Dirección del emplazamiento |
| `{{INTERVENTION_TYPE}}` | Tipo de intervención (Obra Nueva / Reforma / Ampliación) |
| `{{IS_LOE_REQUIRED}}` | Indicador LOE (Sí / No) |
| `{{EXPORT_DATE}}` | Fecha de exportación |
| `{{PROJECT_CODE}}` | Código del proyecto |

**Ejemplo de portada:**

```
MEMORIA DE PROYECTO DE EJECUCIÓN
{{PROJECT_TITLE}}
{{PROJECT_ADDRESS}}
```

> EdificIA reemplazará `{{PROJECT_TITLE}}` por el título real del proyecto al exportar.

### 4.2 Content Controls (controles de contenido SDT) — avanzado

Para usuarios con conocimientos técnicos, también se pueden usar **Content Controls** con etiquetas `Tag` estables. Este mecanismo es compatible y se puede combinar con `{{...}}`.

---

## 5) Cómo funciona la exportación con plantilla

EdificIA usa un sistema de exportación **híbrido** según el contenido de la plantilla:

| Situación | Comportamiento |
|---|---|
| La plantilla tiene Content Controls con `Tag` | EdificIA sustituye los controles y conserva el resto del body |
| La plantilla tiene `{{...}}` en el body (portada, etc.) | EdificIA sustituye los placeholders y **añade** el árbol de contenido al final |
| La plantilla no tiene ni SDT ni `{{...}}` en el body | EdificIA usa la plantilla como fuente de estilos y regenera el body completo |
| Cabecera / pie con `{{...}}` | Se sustituyen siempre, independientemente del path |

El árbol de contenidos del proyecto (capítulos y secciones) se inserta siempre con los estilos de títulos de la plantilla (`Heading1 / Heading2 / Heading3`).

---

## 6) Guardar la plantilla

EdificIA acepta tanto `.docx` (recomendado) como `.dotx`:

1. Ve a **Archivo > Guardar como**.
2. En **Tipo**, selecciona:
   - **Documento de Word** (`.docx`) — para uso normal.
   - **Plantilla de Word** (`.dotx`) — para formato oficial de plantilla.
3. Guarda el archivo.

Nombre recomendado: `memoria-tecnica-corporativa-v1`

---

## 7) Subir a EdificIA

1. Entra como **Admin**.
2. Ve a **/admin/templates**.
3. Sube el archivo Word (`.docx` o `.dotx`, máx. 10 MB) mediante selección o Drag & Drop.
4. Asigna tipo `MemoriaTecnica`.
5. Activa la plantilla y márcala como predeterminada si es la principal.

### 7.1) ¿Cómo se aplica la plantilla en exportación?

- La exportación aplica automáticamente la plantilla **predeterminada** para `MemoriaTecnica`.
- Si no hay plantilla activa o falla el render con plantilla, se aplica fallback al exportador estándar sin romper la descarga.
- Puedes seleccionar una plantilla concreta desde el modal de exportación en el editor.

---

## 8) Validación automática en subida (backend)

Al subir una plantilla en `/admin/templates`, EdificIA valida automáticamente:

- Formato Word OpenXML compatible (`.docx` o `.dotx`).
- Tamaño máximo: **10 MB**.
- MIME compatible con Word OpenXML.
- Que el archivo se pueda abrir como OpenXML válido.
- Que exista cuerpo principal del documento (`word/document.xml`).

Si alguna validación falla, la API devuelve un error descriptivo (`Template.InvalidFormat`) y no guarda la plantilla.

---

## 9) Prueba rápida (checklist)

- [ ] Se puede exportar un proyecto en `.docx`.
- [ ] Se aplican estilos corporativos (tipos de letra, cabecera, pie de página, etc.).
- [ ] Los placeholders `{{...}}` aparecen sustituidos con datos reales.
- [ ] Los estilos de título (Título 1, Título 2, Título 3) se aplican al árbol de contenido.
- [ ] Si desactivas la plantilla, la exportación sigue funcionando (fallback).

---

## 10) Errores típicos y cómo resolverlos

### Problema: "Un placeholder no se sustituye"

- Comprueba que el texto es exactamente `{{CLAVE_EN_MAYUSCULAS}}` sin espacios dentro de las llaves.
- Verifica que la clave está en el catálogo de parámetros activos (consulta con el administrador).
- Word puede dividir el texto `{{PROJECT_TITLE}}` en varios fragmentos internos si lo editas carácter a carácter. Para evitarlo, escribe el placeholder completo de una vez o usa copiar/pegar.

### Problema: "Se rompe el formato al exportar"

- Usa estilos de párrafo de Word (`Título 1 / 2 / 3`, `Normal`) en lugar de formato manual directo.
- No mezcles estilos de plantilla con formato inline excesivo.

### Problema: "No aparece en exportación"

- Verifica que la plantilla está **activa** en `/admin/templates`.
- Comprueba que no hay otra plantilla marcada como predeterminada que la esté solapando.
- Reexporta el documento.

### Problema: "La subida falla por formato inválido"

- Abre el archivo en Word y vuelve a guardarlo como `.docx` o `.dotx` desde **Archivo > Guardar como**.
- Verifica que el archivo no está corrupto ni protegido con contraseña.

---

## 11) Buenas prácticas recomendadas

- Versiona tus plantillas (`v1`, `v2`, `v3`) — no sobreescribas una plantilla activa.
- Cambia una cosa cada vez y prueba la exportación antes de activar la nueva versión.
- Evita diseños excesivamente complejos en la primera versión.
- Mantén siempre una plantilla "estable" activa como seguridad.
- Usa `{{...}}` en la portada y Content Controls solo si necesitas control preciso sobre secciones específicas.

---

## 12) Plantilla mínima sugerida (MVP)

Para empezar rápido, crea una plantilla con:

1. Portada con estilo corporativo y placeholders:
   - `{{PROJECT_TITLE}}`
   - `{{PROJECT_ADDRESS}}`
   - `{{EXPORT_DATE}}`
2. Cabecera con nombre de empresa / logotipo.
3. Pie de página con número de versión y fecha.
4. Estilos de Título 1 / 2 / 3 con tipografía corporativa.

Con eso ya tienes una base funcional. El árbol de contenidos del proyecto se insertará automáticamente a continuación de la portada, con los estilos de tu plantilla.

---

## Recursos

- [Centro de ayuda](/ayuda)
- [Guía de definición de plantillas](/ayuda/guia-definicion-plantillas-documento)
