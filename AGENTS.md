# **🤖 EDIFICIA \- Contexto del Sistema y Guías de Desarrollo**

**System Prompt para Agentes AI:** Actúa como un Arquitecto de Software Senior especializado en .NET 8, Clean Architecture y Astro/React. Tu objetivo es mantener la coherencia estricta con las reglas definidas en este documento.

## **1\. Identidad del Proyecto**

* **Nombre:** EDIFICIA (Asistente Generativo de Memorias de Arquitectura).  
* **Propósito:** SaaS para la redacción automatizada y asistida por IA de Memorias de Proyecto de Ejecución en España (CTE/LOE).  
* **Lógica Core:** El sistema discrimina entre "Obra Nueva" y "Reforma" (exención LOE), adaptando dinámicamente el árbol de contenidos normativos.  
* **Repositorio:** https://github.com/jesusjbriceno/edificia  
* **Licencia:** Apache 2.0.

## **2\. Entornos y Dominios**

* **Producción Web:** https://edificia.jesusjbriceno.dev  
* **Producción API:** https://api-edificia.jesusjbriceno.dev  
* **Local Web:** http://localhost:4321  
* **Local API:** http://localhost:5000

## **3\. Stack Tecnológico (Estricto)**

### **🔙 Backend (apps/api)**

* **Framework:** .NET 8 Web API.  
* **Arquitectura:** Clean Architecture \+ CQRS (Mediator).  
* **ORM (Escritura):** Entity Framework Core (PostgreSQL).  
* **ORM (Lectura):** Dapper (Consultas SQL Raw optimizadas).  
* **Validación:** FluentValidation.  
* **Mapeo:** **Manual** (Operadores explícitos). **PROHIBIDO AutoMapper**.  
* **IA:** FluxGatewayService (OAuth2 Client Credentials) en capa Infrastructure.  
* **Testing:** xUnit \+ Moq.

### **🎨 Frontend (apps/web)**

* **Shell:** Astro 4 (SSR).  
* **Interactividad:** React 18 (Islands Architecture).  
* **Estilos:** **Tailwind CSS v4**.  
* **Estado:** Zustand \+ IndexedDB (idb-keyval) para persistencia offline.  
* **Validación:** Zod.  
* **Editor:** TipTap (Headless WYSIWYG).  
* **Testing:** Vitest.

### **☁️ Infraestructura**

* **Base de Datos:** PostgreSQL 16 (Convención snake\_case).  
  * **Nota Crítica:** El contenido de la memoria se almacena en una columna JSONB, no en tablas relacionales por capítulo.  
* **Caché:** Redis (StackExchange.Redis).  
* **Contenedores:** Docker Compose.

## **4\. Mapa del Monorepo**

/  
├── AGENTS.md                   \# Contexto Maestro (Este archivo)  
├── docker-compose.yml          \# Orquestador  
├── apps/  
│   ├── api/                    \# Solución .NET  
│   │   ├── src/  
│   │   │   ├── Edificia.Domain         \# Entidades Puras, ValueObjects  
│   │   │   ├── Edificia.Shared         \# Kernel compartido (Result\<T\>, Utils)  
│   │   │   ├── Edificia.Application    \# CQRS Handlers, Validators, Interfaces  
│   │   │   ├── Edificia.Infrastructure \# EF Context, Dapper, Flux Service  
│   │   │   └── Edificia.API            \# Controllers, Swagger  
│   │   └── tests/                      \# Proyectos xUnit  
│   └── web/                    \# Proyecto Astro  
│       ├── public/normativa/   \# JSONs estáticos (cte\_2024.json)  
│       ├── src/  
│       │   ├── components/ui/  \# Componentes Atómicos (Tailwind)  
│       │   ├── islands/        \# Features React (Editor, Wizard)  
│       │   ├── pages/          \# Rutas Astro  
│       │   └── store/          \# Zustand Stores  
└── docs/                       \# Documentación Funcional y Técnica

## **5\. Guía de Estilo y Patrones**

### **5.1. Reglas Generales**

1. **Dependency Rule:** Domain \-\> Shared. Application \-\> Domain. Infrastructure \-\> Application. API \-\> Todo.  
2. **Idioma:** Código en Inglés (Variables, Métodos). Dominio/Strings en Español (ej: "Memoria Descriptiva").  
3. **Explicit over Implicit:** Evitar "magia". Mapeos y configuraciones explícitas.

### **5.2. Backend (.NET)**

* **Naming:** PascalCase público, \_camelCase privado.  
* **Controladores:** "Thin Controllers". Reciben Request \-\> Mapean a Command \-\> Envían a MediatR \-\> Retornan Result.  
* **Result Pattern:** NO lanzar excepciones para control de flujo. Retornar Result.Success() o Result.Failure().  
* **DTOs:** Sufijos ...Request (entrada) y ...Response (salida).

### **5.3. Frontend (TS/React)**

* **Strict TypeScript:** Prohibido any. Props siempre tipadas.  
* **Tailwind v4:** Usar sintaxis moderna. Orden lógico: Layout \-\> Spacing \-\> Sizing \-\> Visual.  
* **Componentes:** Pequeños y reutilizables.  
  // Ejemplo  
  interface ButtonProps { label: string; onClick: () \=\> void; }  
  export const PrimaryButton \= ({ label, onClick }: ButtonProps) \=\> { ... }

* **Zod:** Todo formulario debe tener un esquema Zod exportado.

## **6\. Flujos Críticos de Desarrollo**

### **6.1. Nueva Feature (End-to-End)**

1. Definir Entidad en Domain.  
2. Definir Request/Response records en Application.  
3. Implementar Command/Query \+ Handler \+ Validator in Application.  
4. Implementar persistencia en Infrastructure.  
5. Exponer en API Controller y documentar en Swagger.  
6. Crear componente React en web/src/islands conectado al store.

### **6.2. Integración IA (Flux Gateway)**

* **Frontend:** NUNCA llama a la IA directamente.  
* **Backend:** Edificia.Infrastructure gestiona el token OAuth2 y la caché.  
* **Privacidad:** Los datos personales se eliminan del prompt antes de enviar a Flux.

### **6.3. Estructura de Datos "Memoria"**

* La memoria del proyecto NO es una tabla. Es un árbol JSON guardado en Projects.ContentTreeJson.  
* Usar PATCH endpoints para actualizaciones parciales y eficientes.