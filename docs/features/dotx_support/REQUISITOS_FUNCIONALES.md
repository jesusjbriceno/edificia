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

* **RF-08 | Almacenamiento Agnostic:** La ruta física de guardado de los archivos `.dotx` debe ser inyectada por variables de entorno.  
  * *Local/Dev:* Directorio en la máquina host (Ej. `./local_data/templates`).  
  * *Producción:* Volumen montado en Docker persistente en el VPS.
* **RF-09 | Límite de Tamaño:** Restringir el upload de plantillas a un máximo definido (ej. 10MB) para proteger los recursos del VPS KVM 2.

### Integración y Automatización (n8n)

* **RF-10 | Hook de Validación (n8n):** Al subir una plantilla, el sistema puede disparar un Webhook hacia n8n para tareas asíncronas (notificación de actualización al equipo técnico o extracción de metadatos/etiquetas del XML).
