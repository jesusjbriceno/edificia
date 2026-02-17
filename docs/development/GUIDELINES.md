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

### **3.1. Validación con Zod + react-hook-form**

Todo formulario debe tener un esquema Zod resuelto con `@hookform/resolvers/zod`.

```typescript
import { z } from 'zod';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

const projectSchema = z.object({
  title: z.string().min(1, 'El título es obligatorio'),
  status: z.enum(['Active', 'OnHold', 'Completed']),
});

type ProjectFormData = z.infer<typeof projectSchema>;

const { register, handleSubmit, formState: { errors } } = useForm<ProjectFormData>({
  resolver: zodResolver(projectSchema),
});
```

**Regla de orden en Zod:** Colocar `.min(1)` antes de `.email()` para que los mensajes de campo obligatorio tengan prioridad sobre los de formato.

### **3.2. Componentes y Tailwind v4**

* Usar la sintaxis de **Tailwind v4** con tema oscuro premium (`bg-dark-bg`, `text-white`, `border-white/5`).
* **Diseño Atómico:** Los componentes base residen en `components/ui/` (Button, Input, Card, Badge).
* **Editor Enriquecido:** El editor de memorias usa **TipTap** con extensiones (StarterKit, Underline). La barra de herramientas (`EditorToolbar`) se renderiza como componente independiente.

### **3.3. Testing (Vitest)**

* **Ubicación centralizada:** Todos los archivos `.test.tsx` / `.test.ts` residen en `src/tests/`.
* **Imports absolutos:** Los tests usan el alias `@/` (resuelto a `src/`) para importar componentes.
* **Configuración:** `vitest.config.ts` apunta a `./src/tests/setup.ts`.
* Tests unitarios para stores de estado (ej: `useAuthStore`).
* Tests de integración para flujos críticos (ej: rellenar formulario → validación Zod → envío).

## **4. Git Workflow (Git Flow \ Estricto)**

### **4.1. Ramas**

| Rama | Propósito | Se crea desde | Se mergea a |
| :---- | :---- | :---- | :---- |
| **main** | Producción estable | — | — |
| **develop** | Integración continua | main (una vez) | main (release) |
| **feature/\*** | Una feature del Roadmap | develop | develop (vía PR) |
| **hotfix/\*** | Corrección urgente | main | main + develop |

### **4.2. Reglas Innegociables**

1. **PROHIBIDO** commitear directamente en `main` o `develop`.
2. Todo cambio entra vía **Pull Request** desde una rama `feature/*` o `hotfix/*`.
3. Cada PR debe referenciar el **ID del Roadmap** (ej: "Fase 1.1 - Shared Kernel").
4. Antes de crear la PR, el código debe **compilar sin errores** (`dotnet build` / `npm run build`).
5. Los tests deben estar en verde antes de solicitar merge.

### **4.3. Flujo de Trabajo**

```
1. git checkout develop && git pull
2. git checkout -b feature/<nombre>
3. # ... desarrollo + commits ...
4. git push -u origin feature/<nombre>
5. gh pr create --base develop --title "feat: ..." --body "..."
6. # Revisión → Aprobación → Merge
7. git checkout develop && git pull
```

### **4.4. Conventional Commits**

Todos los mensajes de commit siguen el estándar [Conventional Commits](https://www.conventionalcommits.org/):

* `feat:` Nueva funcionalidad.
* `fix:` Corrección de bugs.
* `docs:` Solo documentación.
* `refactor:` Refactorización sin cambio funcional.
* `test:` Añadir o corregir tests.
* `chore:` Tareas de mantenimiento.

### **4.5. Naming de Ramas (Ejemplos)**

* `feature/shared-kernel`
* `feature/infra-persistence`
* `feature/api-swagger`
* `feature/project-domain`
* `hotfix/fix-db-connection`