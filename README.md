# **🏗️ EDIFICIA**

**Plataforma SaaS para la Redacción Automatizada de Memorias de Arquitectura (CTE/LOE).**

## **📖 Visión del Producto**

**EDIFICIA** es un **Asistente Estratégico de Visado** diseñado para arquitectos españoles.

A diferencia de soluciones genéricas, EDIFICIA entiende la escala de la obra:

* **Discrimina** inteligentemente entre Obra Nueva y Rehabilitación.  
* **Gestiona** la exención de LOE (Art 2.2) para obras menores automáticamente.  
* **Redacta** descripciones técnicas y justifica normativa usando IA soberana (vía Flux Gateway).  
* **Garantiza** la persistencia offline ("Modo Túnel") para trabajar en obra.

## **🌍 Entornos**

| Entorno | Aplicación | URL |
| :---- | :---- | :---- |
| **Producción** | Web (Frontend) | [https://edificia.jesusjbriceno.dev](https://www.google.com/search?q=https://edificia.jesusjbriceno.dev) |
|  | API (Swagger) | [https://api-edificia.jesusjbriceno.dev/swagger](https://www.google.com/search?q=https://api-edificia.jesusjbriceno.dev/swagger) |
| **Local** | Web | http://localhost:4321 |
|  | API | http://localhost:5000 |

## **🚀 Quick Start (Despliegue Local)**

Todo el entorno está contenerizado.

### **Prerrequisitos**

* Docker & Docker Compose.  
* Git.

### **Instalación**

1. **Clonar el repositorio**  
   git clone \[https://github.com/jesusjbriceno/edificia.git\](https://github.com/jesusjbriceno/edificia.git)  
   cd edificia

2. **Configurar Variables de Entorno**  
   cp .env.example .env  
   \# Edita .env y añade tus credenciales de Flux Gateway (Client ID / Secret)

3. **Levantar Infraestructura**  
   docker-compose up \-d

## **📂 Estructura del Monorepo**

/  
├── apps/  
│   ├── api/            \# Backend .NET 8 (Clean Architecture)  
│   └── web/            \# Frontend Astro \+ React  
├── docs/               \# Documentación de Análisis y Diseño  
└── docker-compose.yml  \# Orquestador de Servicios

## **🛠️ Stack Tecnológico**

* **Backend:** .NET 8, EF Core (Writes), Dapper (Reads).  
* **Frontend:** Astro 4, React 18, Tailwind CSS v4, Zustand.  
* **Base de Datos:** PostgreSQL 16 (Híbrida Relacional/JSONB).  
* **IA:** Flux Gateway (OAuth2).  
* **Validación:** FluentValidation (Back) / Zod (Front).

## **🤝 Contribución**

Por favor, consulta [AGENTS.md](https://www.google.com/search?q=./AGENTS.md) para entender las reglas de arquitectura y estándares de código antes de contribuir.

## **📄 Licencia**

Este proyecto está bajo la licencia **Apache 2.0**. Consulta el archivo LICENSE para más detalles.