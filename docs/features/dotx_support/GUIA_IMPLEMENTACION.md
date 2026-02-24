# Guía Técnica de Implementación (C# .NET 10 & Astro 4)

Esta guía detalla el orden lógico de ejecución (Least-To-Most) para implementar el Sistema de Plantillas en la arquitectura existente desplegada en Coolify.

## Fase 1: Infraestructura y Configuraciones Core

### 1. Variables de Entorno (appsettings.json y Coolify)

Define la ubicación lógica del almacenamiento.

En `.env` (local):

```bash
Storage__TemplatesBasePath=./local_data/templates
```

En Docker Compose / Coolify (appsettings.Production.json o ENV var):

```bash
Storage__TemplatesBasePath=/app/storage/templates
```

**(IMPORTANTE):** Modifica el `docker-compose.apps.yml` para asegurar que el volumen persiste:

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

### 1. Implementar `IFileStorageService`

Crea el servicio que escribirá físicamente el `IFormFile` (convertido a stream) en el disco.

```csharp
// Asegurar que el directorio exista en el constructor
Directory.CreateDirectory(_basePath);
```

### 2. Estrategia de Caché L1 (`IMemoryCache`)

Dado que operamos en un nodo único (VPS KVM 2), evitaremos usar Redis para guardar binarios pesados en RAM. En su lugar, usaremos `IMemoryCache` de .NET.

* **Lectura:** Antes de ir al disco (NVMe), el `DocumentExportService` busca la plantilla en caché.  
* **Invalidación:** Cuando el Administrador sube una nueva versión de la plantilla, el `TemplateService` hace un `_cache.Remove($"Template_{templateType}")` para forzar la lectura del disco en la próxima petición.

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

**Integración n8n (Opcional en esta fase):**

En el handler del `POST`, inyectar un `IHttpClientFactory` o usar un servicio de `n8nNotifier` para enviar un HTTP POST asíncrono y de fuego-y-olvidar (fire-and-forget) al webhook de n8n.

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
