# **📏 Guía de Estilo y Estándares de Desarrollo \- EDIFICIA**

**Repositorio:** [https://github.com/jesusjbriceno/edificia](https://github.com/jesusjbriceno/edificia)

**Versión:** 2.2 (Stack Estricto)

Este documento define las reglas innegociables para el código de EDIFICIA.

## **1\. Stack Tecnológico**

### **Backend (.NET 8\)**

* **API:** Web API (Controllers). **OpenAPI/Swagger** obligatorio.  
* **Validación:** **FluentValidation**.  
* **Mapeo:** **Manual** (Operators explicit/implicit).  
* **ORM (Comandos):** **EF Core** (Npgsql).  
* **ORM (Consultas):** **Dapper** (SQL Raw optimizado).  
* **Testing:** **xUnit** \+ **Moq**.

### **Frontend (Astro \+ React)**

* **Core:** Astro 4 (SSR).  
* **Interacción:** React 18 \+ **TypeScript (Strict)**.  
* **Estilos:** **Tailwind CSS v4**.  
* **Validación:** **Zod**.  
* **Estado:** Zustand \+ IndexedDB.  
* **Testing:** **Vitest**.

### **Infraestructura**

* **DB:** **PostgreSQL 16**.  
* **Caché:** **Redis** (StackExchange.Redis).  
* **IA Gateway:** Flux Gateway.

## **2\. Estándares de Backend (.NET)**

### **2.1. Base de Datos (Naming Conventions)**

PostgreSQL usa snake\_case. .NET usa PascalCase.

* **Regla:** Configurar EF Core (EFCore.NamingConventions) para transformar automáticamente.  
* **Tablas:** Plural, snake\_case (ej: projects).  
* **Columnas:** snake\_case (ej: created\_at, owner\_id).

### **2.2. Patrón de Mapeo (DTOs)**

**PROHIBIDO:** Usar librerías de mapeo automático (AutoMapper, Mapster).

**OBLIGATORIO:** Usar operadores de conversión en los DTOs o métodos de extensión.

// Ejemplo: Explicit Operator  
public class ProjectResponse   
{  
    public Guid Id { get; set; }  
    public string Title { get; set; }

    public static explicit operator ProjectResponse(Project entity)   
    {  
        return new ProjectResponse {  
            Id \= entity.Id,  
            Title \= entity.Title  
        };  
    }  
}

### **2.3. Validación**

La validación de entrada NO ocurre en el controlador, sino en el Pipeline de MediatR o mediante filtros de FluentValidation.

public class CreateProjectValidator : AbstractValidator\<CreateProjectCommand\>  
{  
    public CreateProjectValidator()  
    {  
        RuleFor(x \=\> x.Title).NotEmpty().MaximumLength(200);  
        RuleFor(x \=\> x.CadastralRef).Matches(@"^\[0-9A-Z\]{20}$");  
    }  
}

### **2.4. Estrategia de Acceso a Datos (CQRS Híbrido)**

* **Writes (Commands):** Usar DbContext (EF Core) para aprovechar el *Change Tracker* y la consistencia transaccional.  
* **Reads (Queries):** Usar IDbConnection (Dapper) con SQL directo contra vistas materializadas o tablas para máximo rendimiento. Retornar DTOs/Records directamente (Prohibido retornar Entidades de Dominio en Queries).

## **3\. Estándares de Frontend (React/TS)**

### **3.1. Tipado y Zod**

Todo formulario o entrada de datos externa debe tener un esquema Zod.

// definition  
import { z } from 'zod';

export const ProjectSchema \= z.object({  
  title: z.string().min(5, "Título muy corto"),  
  interventionType: z.enum(\['New', 'Reform'\])  
});

export type ProjectForm \= z.infer\<typeof ProjectSchema\>;

### **3.2. Componentes y Tailwind v4**

* Usar la sintaxis de **Tailwind v4**.  
* **Diseño Atómico:** Crear componentes pequeños en /components/ui (Button, Input, Card) que encapsulen los estilos de Tailwind para evitar "sopa de clases" repetida.

### **3.3. Testing (Vitest)**

* Tests unitarios para hooks de lógica de negocio (ej: useProjectFilters).  
* Tests de integración para flujos críticos (ej: rellenar formulario \-\> validación Zod \-\> envío a store).

## **4\. Git Workflow**

* **Main:** Producción.  
* **Develop:** Integración.  
* **Feat:** feature/auth-flux-gateway.  
* **Commits:** feat: implementa validación zod en formulario login (Conventional Commits).