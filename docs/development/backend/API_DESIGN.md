# **📡 Diseño de API REST y Patrón CQRS \- EDIFICIA**

**Versión:** 2.0

**Arquitectura:** RESTful sobre CQRS (Mediator Pattern).

## **1\. Filosofía CQRS en EDIFICIA**

Separamos estrictamente las operaciones de lectura (Queries) de las de escritura (Commands).

### **🔴 Commands (Escritura)**

* **Responsabilidad:** Modificar el estado del sistema (INSERT, UPDATE, DELETE).  
* **Herramienta:** Entity Framework Core (aprovecha Change Tracker y Transacciones).  
* **Retorno:** Generalmente devuelve el ID del recurso creado o Unit (Void).  
* **Naming:** Verbo \+ Entidad \+ "Command" (ej: CreateProjectCommand).

### **🔵 Queries (Lectura)**

* **Responsabilidad:** Leer datos para la UI.  
* **Herramienta:** Dapper (Micro-ORM) con SQL Raw contra Vistas o Tablas.  
* **Retorno:** DTOs planos optimizados para la vista (sin grafos de objetos complejos).  
* **Naming:** Verbo \+ Entidad \+ "Query" (ej: GetProjectDashboardQuery).

## **2\. Flujo de Datos (The Thin Controller)**

Los Controladores API son "tontos". Su único trabajo es recibir el HTTP Request, mapearlo a un Comando/Query de MediatR y devolver la respuesta HTTP adecuada.

// Ejemplo Conceptual  
\[HttpPost\]  
public async Task\<IActionResult\> Create(CreateProjectRequest request)  
{  
    // 1\. Mapear DTO externo \-\> Command interno  
    var command \= new CreateProjectCommand(request.Title, request.InterventionType);  
      
    // 2\. Despachar al Bus en memoria  
    var result \= await \_mediator.Send(command);  
      
    // 3\. Retornar  
    return CreatedAtAction(nameof(Get), new { id \= result }, result);  
}

## **3\. Catálogo de Operaciones (CQRS)**

### **🏗️ Módulo: Projects**

| Tipo | Clase (MediatR) | Request DTO (API) | Response DTO | Herramienta |
| :---- | :---- | :---- | :---- | :---- |
| **Query** | GetProjectsQuery | (QueryString) | Paginated\<ProjectSummary\> | **Dapper** |
| **Query** | GetProjectByIdQuery | \- | ProjectDetail | **Dapper** |
| **Command** | CreateProjectCommand | CreateProjectRequest | Guid (New Id) | **EF Core** |
| **Command** | UpdateProjectSettingsCommand | UpdateSettingsRequest | Unit | **EF Core** |
| **Command** | DeleteProjectCommand | \- | Unit | **EF Core** |

### **📄 Módulo: Sections (Memoria)**

| Tipo | Clase (MediatR) | Request DTO (API) | Response DTO | Herramienta |
| :---- | :---- | :---- | :---- | :---- |
| **Query** | GetProjectTreeQuery | \- | ContentTreeJson | **Dapper** (Select directo jsonb) |
| **Command** | PatchSectionContentCommand | UpdateSectionRequest | Unit | **EF Core** (Optimizado con ExecuteUpdate) |

### **🤖 Módulo: AI (Inferencia)**

| Tipo | Clase (MediatR) | Request DTO (API) | Response DTO | Herramienta |
| :---- | :---- | :---- | :---- | :---- |
| **Command** | GenerateSectionTextCommand | GenerateTextRequest | GeneratedTextResponse | **Flux Gateway Service** |

## **4\. Estructura de Carpetas (Application Layer)**

Para mantener el orden, la capa Edificia.Application reflejará esta división:

/Edificia.Application  
  /Projects  
    /Commands  
      /CreateProject  
        CreateProjectCommand.cs  
        CreateProjectHandler.cs  
        CreateProjectValidator.cs  
    /Queries  
      /GetProjects  
        GetProjectsQuery.cs  
        GetProjectsHandler.cs  
        ProjectSummaryDto.cs

## **5\. Manejo de Respuestas y Errores**

No lanzamos excepciones para control de flujo. Usamos un patrón Result\<T\>.

* **Success:** Result.Success(data) \-\> HTTP 200/201.  
* **Validation Error:** Result.Failure(ValidationError) \-\> HTTP 400\.  
* **Not Found:** Result.NotFound() \-\> HTTP 404\.

El controlador se encarga de traducir este Result al IActionResult correspondiente.