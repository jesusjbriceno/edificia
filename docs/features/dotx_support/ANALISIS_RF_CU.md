# Análisis de Requisitos y Casos de Uso (CU)

## 1. Análisis de Impacto

* **Rendimiento (VPS KVM 2):** Alto riesgo de fragmentación del LOH (Large Object Heap) en C# al cargar archivos binarios concurrentemente. Se mitiga implementando RecyclableMemoryStream.  
* **Transaccionalidad:** Separación estricta entre los metadatos de la plantilla (PostgreSQL) y el binario físico (File System / Volumen Docker).  
* **Frontend (Astro 4):** Necesidad de crear nuevas rutas en el dashboard de administrador `/admin/templates` con formularios `multipart/form-data` manejados eficientemente sin colapsar el hilo de Node.js (Astro).

## 2. Actores del Sistema

1. **Administrador:** Usuario con rol Admin que sube, clasifica y gestiona el ciclo de vida de las plantillas `.dotx`.  
2. **Usuario Técnico (Arquitecto/Ingeniero):** Solicita la exportación de un proyecto. No elige la plantilla directamente; el sistema resuelve cuál aplicar según los metadatos del proyecto.  
3. **Sistema Orquestador (n8n):** Recibe eventos (Webhooks) desde la API para flujos de trabajo paralelos.

## 3. Catálogo de Casos de Uso

### CU-01: Administrar Plantilla Documental

* **Actor:** Administrador  
* **Flujo Principal:**  
  1. El Admin accede a Configuración > Plantillas.  
  2. Hace click en "Nueva Plantilla".  
  3. Completa: Nombre, Tipo (Select: Memoria/Reporte), Descripción, Archivo `.dotx`.  
  4. El frontend envía un `multipart/form-data` a la API.  
  5. La API guarda el archivo en el `StorageProvider` (ruta parametrizada) y crea un registro en PostgreSQL con la ruta relativa y metadata.  
  6. (Opcional) La API envía un evento HTTP POST a n8n informando de una nueva versión disponible.

### CU-02: Generar Exportación de Proyecto (Con Fallback)

* **Actor:** Usuario Técnico  
* **Flujo Principal:**  
  1. Usuario solicita descargar la "Memoria" del Proyecto X.  
  2. La API consulta en base de datos: ¿Existe una plantilla activa con Tipo = `MemoriaTécnica`?  
  3. **Alternativa A (Existe):**  
     * Se lee el `.dotx` del Volumen (usando caché en RAM si es posible).  
     * Se instancia OpenXML, se inyectan los datos (Content Controls).  
     * Se devuelve el binario.  
  4. **Alternativa B (No Existe):**  
     * Interviene el *Fallback* (Motor Legado).  
     * Se genera el documento desde cero programáticamente.  
  5. Se entrega el `.docx` al navegador del usuario.

### CU-03: Automatización de Entrega vía n8n (Post-Generación)

* **Actor:** Orquestador (n8n)  
* **Flujo:** Tras el CU-02, si el usuario marca "Enviar al cliente", la API envía un Webhook a n8n con el ID del Documento/Proyecto. n8n se encarga de descargar el documento, formatear el correo (posible uso de IA para redactar el cuerpo del correo) y enviarlo vía SMTP/Brevo, liberando a la API principal de esta carga.
