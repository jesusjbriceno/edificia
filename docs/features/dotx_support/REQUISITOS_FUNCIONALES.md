# Documento de Requisitos Funcionales (RF)

**Módulo:** Sistema de Gestión Multimodelo de Plantillas (.dotx)  
**Versión:** 1.0

## Contexto

El sistema debe permitir la administración, almacenamiento y aplicación dinámica de plantillas de documentos Word (.dotx) para generar entregables (Memorias Técnicas, Reportes, etc.). Si no existe una plantilla definida para un caso de uso, el sistema debe garantizar la continuidad del servicio utilizando el motor de renderizado legado (código duro).

## 1. Requisitos Funcionales (RF)

### Gestión de Plantillas (Administración)

* **RF-01 | Alta de Plantillas:** El administrador debe poder subir archivos `.dotx` a través de la UI (Astro).  
* **RF-02 | Tipificación:** Cada plantilla debe estar categorizada (Ej: `MemoriaTécnica`, `Presupuesto`, `ReporteInspeccion`).  
* **RF-03 | Metadatos:** El sistema debe registrar nombre, descripción, versión, estado (Activo/Inactivo) y usuario creador.  
* **RF-04 | Listado y Edición:** La UI debe ofrecer un CRUD completo (Listar, Activar/Desactivar, Eliminar lógica/físicamente).

### Motor de Renderizado y Fallback

* **RF-05 | Renderizado Pixel-Perfect:** El sistema debe inyectar la información en los *Content Controls* (`SdtElement`) de la plantilla preservando el *Style ID* original del `.dotx`.  
* **RF-06 | Mecanismo de Fallback:** Si un proyecto solicita una exportación y no hay plantillas activas para su tipología, el sistema derivará la petición de forma transparente al motor de generación programática en `.docx` heredado.  
* **RF-07 | Actualización de Índices:** El documento generado debe forzar la actualización automática de campos dinámicos (TOC - Tabla de Contenidos) al ser abierto por el cliente.

### Infraestructura y Parametrización

* **RF-08 | Almacenamiento Agnostic:** El sistema debe abstraer el guardado/lectura de plantillas detrás de un proveedor configurable por entorno (`local` o `n8n`).  
  * *Local/Dev (`local`):* Directorio en la máquina host (Ej. `./local_data/templates`).  
  * *Producción recomendada (`n8n`):* La API delega el guardado/lectura a un webhook de n8n, y el flujo persiste en el backend de storage elegido (Drive, OneDrive, S3, Synology, etc.) devolviendo identificador/ruta canónica.
* **RF-09 | Límite de Tamaño:** Restringir el upload de plantillas a un máximo definido (ej. 10MB) para proteger los recursos del VPS KVM 2.

### Integración y Automatización (n8n)

* **RF-10 | Gestión de Storage vía n8n (recomendado):** Al subir/recuperar una plantilla, la API puede delegar la operación al webhook de n8n y persistir metadatos en DB solo tras confirmación `OK` del flujo.
* **RF-11 | Hook de Validación/Auditoría (n8n):** Tras operaciones de plantilla, el sistema puede disparar un Webhook adicional para tareas asíncronas (notificación técnica, auditoría, extracción de metadatos/etiquetas XML).

### Escalabilidad de tipos y flujo de exportación (evolución)

* **RF-12 | Plantillas disponibles por tipo:** El sistema debe permitir múltiples plantillas disponibles para un mismo tipo documental.
* **RF-13 | Plantilla predeterminada por tipo:** Debe existir, como máximo, una plantilla predeterminada por tipo documental.
* **RF-14 | Selector en exportación:** Al exportar, el usuario debe poder elegir plantilla entre las disponibles para el tipo del documento.
* **RF-15 | Selección automática:** Si solo existe una plantilla disponible para el tipo, el sistema debe preseleccionarla automáticamente.
* **RF-16 | Nombre del documento de salida:** El usuario debe poder editar el nombre final del archivo exportado antes de generar el documento.
* **RF-17 | Catálogo dinámico de tipos:** Admin/SuperAdmin deben poder crear/editar/desactivar tipos de plantilla sin cambios de código.
* **RF-18 | Compatibilidad por tipo:** El sistema debe validar la compatibilidad de `Tag` mínimos según el tipo de plantilla configurado.
