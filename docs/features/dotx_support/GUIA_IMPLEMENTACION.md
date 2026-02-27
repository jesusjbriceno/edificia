# Guía Técnica de Implementación (C# .NET 10 & Astro 4)

Esta guía detalla el orden lógico de ejecución (Least-To-Most) para implementar el Sistema de Plantillas en la arquitectura existente desplegada en Coolify.

> Contrato del webhook de storage: ver `docs/features/dotx_support/ESPECIFICACION_TEMPLATE_STORAGE_N8N.md`.
> Roadmap de ejecución de la feature: ver `docs/features/dotx_support/ROADMAP_IMPLEMENTACION_DOTX_N8N.md`.

## Fase 1: Infraestructura y Configuraciones Core

### 1. Variables de Entorno (appsettings.json y Coolify)

Define proveedor y configuración de almacenamiento de plantillas.

#### Opción recomendada (producción): proveedor `n8n`

En `.env`:

```bash
TemplateStorage__Provider=n8n
TemplateStorage__N8nWebhookUrl=https://n8n.tudominio.com/webhook/template-storage
TemplateStorage__N8nApiSecret=REEMPLAZAR_SECRETO
TemplateStorage__TimeoutSeconds=60
```

#### Opción local/dev: proveedor `local`

En `.env` (local):

```bash
TemplateStorage__Provider=local
TemplateStorage__BasePath=./local_data/templates
```

En Docker Compose / Coolify (si se usa proveedor local en producción):

```bash
TemplateStorage__BasePath=/app/storage/templates
```

**(IMPORTANTE, solo proveedor `local`):** Modifica el `docker-compose.apps.yml` para asegurar que el volumen persiste:

```yaml
services:
  api:
    volumes:
      - edificia_storage:/app/storage
volumes:
  edificia_storage:
```

### 2. Dominio y Migraciones de Base de Datos

1. Añadir entidad `AppTemplate` en `Edificia.Domain/Entities`.  
2. Añadir `DbSet<AppTemplate> Templates` en `EdificiaDbContext`.  
3. Ejecutar migración:

```bash
dotnet ef migrations add AddTemplateSystem -p src/Edificia.Infrastructure -s src/Edificia.API
```

4. Aplicar migración.

## Fase 2: Servicios Base (Capa de Infraestructura)

### 1. Implementar `IFileStorageService` + estrategias

Implementar dos estrategias:

- `LocalFileStorageService` (desarrollo/local).
- `N8nTemplateStorageService` (recomendada en producción).

La estrategia `n8n` debe invocar un webhook síncrono y devolver `storageKey`/identificador para persistir en DB.

```csharp
// Asegurar que el directorio exista en el constructor
Directory.CreateDirectory(_basePath);
```

### 2. Estrategia de Caché L1 (`IMemoryCache`)

Dado que operamos en un nodo único (VPS KVM 2), evitaremos usar Redis para guardar binarios pesados en RAM. En su lugar, usaremos `IMemoryCache` de .NET.

* **Lectura:** Antes de ir al proveedor (`n8n` o local), el `DocumentExportService` busca la plantilla en caché.  
* **Invalidación:** Cuando el Administrador sube una nueva versión de la plantilla, el `TemplateService` hace un `_cache.Remove($"Template_{templateType}")` para forzar refresco en la próxima petición.

### 3. El Motor OpenXML (`TemplateDocxGenerator`)

Implementa el motor sugerido previamente. **Regla de oro:** Utilizar siempre RecyclableMemoryStream para la manipulación en RAM, evitando fugas de memoria y fragmentación del LOH en el VPS.

### 4. Refactor del `ExportProjectHandler` (Application Layer)

Aplica la lógica de **Fallback** combinada con la Caché:

```csharp
var template = await _templateRepository.GetActiveTemplateAsync(ProjectTypes.Memoria, cancellationToken);

byte[] documentBytes;
if (template != null)
{
    // Intenta obtener de MemoryCache primero, si no, lee del Storage y guarda en caché
    var cacheKey = $"Template_{template.TemplateType}";
    if (!_memoryCache.TryGetValue(cacheKey, out byte[] templateFile))
    {
        templateFile = await _storageService.GetFileAsync(template.StoragePath);
        _memoryCache.Set(cacheKey, templateFile, TimeSpan.FromHours(12)); // Cachear por 12 horas
    }

    documentBytes = await _templateGenerator.GenerateAsync(templateFile, projectData);
}
else
{
    _logger.LogWarning("No active template found. Using legacy programmatic generator.");
    documentBytes = await _legacyGenerator.GenerateAsync(projectData);
}
```

## Fase 3: Exposición de APIs (Controladores)

Crear `TemplatesController.cs` con acceso restringido `[Authorize(Roles = AppRoles.Admin)]`:

* `POST /api/templates` (Recibe `[FromForm]` múltiples datos y el Archivo). **Nota:** Debe incluir la lógica de limpiar la caché (`IMemoryCache.Remove`).  
* `GET /api/templates` (Paginado).  
* `PUT /api/templates/{id}/toggle-status`.

### Validación recomendada en `POST /api/templates`

Antes de persistir en storage, validar binario `.dotx` con OpenXML:

- Documento OpenXML legible (`WordprocessingDocument.Open`).
- Existencia de `MainDocumentPart.Document.Body`.
- Existencia de controles de contenido con `Tag`.
- Reglas por tipo de plantilla (ej. `MemoriaTecnica`: `ProjectTitle`, `MD.01`, `MC.01`).

Si falla, devolver error de dominio de validación (`Template.InvalidFormat`) y **no** guardar archivo.

**Integración n8n (recomendada en esta fase):**

En el handler del `POST`, inyectar un cliente HTTP para invocar de forma síncrona el webhook de storage de `n8n`. Persistir metadatos en DB solo cuando la respuesta sea exitosa.

## Fase 4: Frontend (Astro 4 / React)

1. **Crear UI Administrativa:** `/apps/web/src/pages/admin/templates.astro`.  
2. **Componente React `TemplateForm.tsx`:** Debe manejar la subida de archivos utilizando un objeto `FormData` nativo del navegador.

```javascript
const formData = new FormData();
formData.append('name', name);
formData.append('templateType', type);
formData.append('file', selectedFile);

// axios o fetch con headers apropiados (evitar setear Content-Type manual para que el browser genere el boundary).
```

3. **UI de Exportación:** La vista de exportación (ProjectDetailPage) no cambia. La magia del *fallback* ocurre transparentemente en el backend.

4. **UI Admin de Plantillas:** Mostrar en la misma vista una tarjeta de reglas de subida (formato, tamaño, tags obligatorios y enlace a guía) para reducir errores de usuario antes del submit.
