# Plantilla base para plantilla `.dotx` — EdificIA

> Este documento es un `Markdown` estilizado para pegar en Microsoft Word y convertirlo en una plantilla `.dotx` siguiendo las instrucciones más abajo.

---

## Instrucciones rápidas (pegar → convertir)

1. Abre Microsoft Word y crea un documento nuevo.
2. Copia todo el contenido de este fichero Markdown y pégalo en el documento.
3. Ajusta estilos: selecciona los títulos y aplica los estilos de Word (`Heading 1`, `Heading 2`, `Heading 3`) según convenga.
4. Inserta los **Content Controls** (Programador → Controles) donde veas las etiquetas entre `<<...>>` y configura su **Tag** con el valor indicado en la tabla de mapeo.
5. Guarda como **Plantilla**: `Archivo → Guardar como → Tipo: Plantilla de Word (*.dotx)`.

---

# Portada

**Título del Proyecto:**

<<ProjectTitle>>

**Subtítulo / Descripción breve:**

<<ProjectSubtitle>>

**Datos del proyecto**

- **Dirección:** <<ProjectAddress>>
- **Promotor:** <<ProjectClient>>
- **Tipo de intervención:** <<InterventionType>>
- **Fecha:** <<ProjectDate>>

---

# Resumen Ejecutivo

<<MD.01>>

---

# 1. Memoria Descriptiva

## 1.1 Descripción general

<<MD.02>>

## 1.2 Agentes intervinientes

| Rol | Nombre / Empresa | Contacto |
|---|---:|---|
| Promotor | <<Agent.Promoter>> | <<Agent.PromoterContact>> |
| Redactor | <<Agent.Author>> | <<Agent.AuthorContact>> |

---

# 2. Memoria Constructiva

## 2.1 Sistema estructural

<<MC.01>>

## 2.2 Acabados y soluciones constructivas

<<MC.02>>

---

# 3. Cálculos y Anexos

> Ejemplos útiles para pruebas: tabla, enlace y fórmula.

### Tabla de ejemplo

| Concepto | Valor | Unidad |
|---|---:|---:|
| Longitud | 12.50 | m |
| Área | 45.00 | m² |

### Enlace de normativa

[CTE - Documento 1](https://www.example.org/normativa/cte)

### Fórmula de ejemplo (inline y bloque)

Fórmula inline: $E = m \cdot c^2$

Fórmula bloque:

$$
\sigma = \frac{F}{A}
$$

> Nota: Word no interpreta LaTeX nativamente; las fórmulas se conservan como texto y se podrán reemplazar por OMML desde el exportador si se desea. Si prefieres, sustituye estas fórmulas por campos OMML tras pegar en Word.

---

# 4. Observaciones finales

<<MD.99>>

---

## Mapeo recomendado de `Tag`s (Contract)

- `ProjectTitle` → `Project.Title`
- `ProjectSubtitle` → `Project.Subtitle`
- `ProjectAddress` → `Project.Address`
- `ProjectClient` → `Project.Client` 
- `InterventionType` → `Project.InterventionType`
- `ProjectDate` → `Project.Date`
- `MD.01` → `Section: Memoria Descriptiva - Resumen`
- `MD.02` → `Section: Memoria Descriptiva - Descripción general`
- `MC.01` → `Section: Memoria Constructiva - Estructura`
- `MC.02` → `Section: Memoria Constructiva - Acabados`
- `MD.99` → `Notas finales`
- `Agent.*` → campos de agentes (usar `Agent.Promoter`, `Agent.Author`, etc.)

> Regla: la **Etiqueta (Tag)** dentro del Content Control debe coincidir exactamente con la clave que espere el backend.

---

## Checklist antes de guardar como `.dotx`

- [ ] Todos los títulos principales usan `Heading 1`.
- [ ] Subtítulos usan `Heading 2` o `Heading 3` según jerarquía.
- [ ] Cada placeholder `<<...>>` tiene un Content Control con `Tag` configurado.
- [ ] Tablas de ejemplo están correctamente formateadas.
- [ ] Enlaces usan el esquema `https://` o `mailto:` (evitar javascript: u otros protocolos).
- [ ] Guardar versión numerada: `memoria-tecnica-v1.dotx`.

---

## Consejos para fidelidad en la exportación

- Usa tipos de letra estándar (Calibri, Cambria, etc.) para máxima compatibilidad.
- Para fórmulas críticas, considera crear objetos OMML en Word o dejar un marcador `<<Formula:ID>>` para sustitución por el exportador backend.
- Evita layouts excesivamente complejos en la primera versión: portada, índice automático y bloques dinámicos son suficientes.

---

## Ejemplo mínimo (MVP)

1. Portada con `<<ProjectTitle>>` y `<<ProjectSubtitle>>`.
2. Bloque `<<MD.01>>` para resumen.
3. Bloque `<<MC.01>>` para constructiva.
4. Tabla de datos básicos y enlace a normativa.

---

*Generado para EdificIA — sigue la [guía de definición de plantilla](GUIA_DEFINICION_PLANTILLA_DOTX.md) para detalles avanzados.*
