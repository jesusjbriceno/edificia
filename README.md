# **🏗️ EdificIA**

**Plataforma SaaS para la Redacción Automatizada de Memorias de Arquitectura (CTE/LOE).**

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Astro](https://img.shields.io/badge/Astro-4.0-orange.svg)](https://astro.build/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)](https://www.postgresql.org/)

---

## **Tabla de Contenidos**

- [Visión del Producto](#-visión-del-producto)
- [Entornos](#-entornos)
- [Quick Start](#-quick-start-despliegue-local)
- [Estructura del Monorepo](#-estructura-del-monorepo)
- [Stack Tecnológico](#-stack-tecnológico)
- [Arquitectura](#-arquitectura)
- [Integración IA](#-integración-ia)
- [Documentación](#-documentación)
- [Contribución](#-contribución)
- [Licencia](#-licencia)

---

## **📖 Visión del Producto**

**EdificIA** es un **Asistente Estratégico de Visado** diseñado para arquitectos españoles. A diferencia de soluciones genéricas, EdificIA incorpora **Inteligencia Constructiva**: comprende el contexto de cada proyecto para adaptar dinámicamente el contenido de la memoria y la normativa aplicable.

| Capacidad | Descripción |
|:----------|:------------|
| **Discriminación Normativa** | Diferencia inteligentemente entre Obra Nueva, Reforma y Ampliación |
| **Gestión LOE** | Aplica automáticamente la exención del Art. 2.2 para obras menores |
| **Redacción IA** | Genera descripciones técnicas y justificaciones normativas vía IA delegada (n8n → Flux Gateway / Google Gemini) |
| **Modo Túnel** | Persistencia offline en IndexedDB para trabajar sin conexión en obra |
| **Exportación DOCX** | Genera la memoria técnica completa en formato Word |
| **Plantillas `.dotx`** | Administración con disponibilidad/predeterminada por tipo, selector en exportación, validación de formato/tags y fallback automático |

> **Contexto académico:** Este proyecto constituye el Trabajo Fin de Máster (TFM) del Máster en Desarrollo de Aplicaciones con IA. Consulta la [Memoria del TFM](docs/TFM/MEMORIA_TFM_EdificIA.md) para el detalle completo.

---

## **🌍 Entornos**

| Entorno | Aplicación | URL |
|:--------|:-----------|:----|
| **Producción** | Web (Frontend) | https://edificia.jesusjbriceno.dev |
|  | API (Swagger) | https://api-edificia.jesusjbriceno.dev/swagger |
| **Local** | Web | http://localhost:4321 |
|  | API | http://localhost:5000 |

---

## **🧪 Acceso temporal para pruebas**

> ⚠️ **Temporal:** este apartado está destinado únicamente a pruebas y debe retirarse cuando deje de ser necesario.

### **1) Usuario administrador**

- ~**Email:** support@jesusjbriceno.dev~
- ~**Pass:** lUEGo#g&3%dR~

### **2) Usuario pruebas**

- ~**Email:** demo@jesusjbriceno.dev~
- ~**Pass:** Huwy9V*v!!4N~

---

## **🚀 Quick Start (Despliegue Local)**

Todo el entorno está contenerizado con Docker.

### **Prerrequisitos**

- [Docker](https://docs.docker.com/get-docker/) & Docker Compose
- [Git](https://git-scm.com/)

### **Instalación**

```bash
# 1. Clonar el repositorio
git clone https://github.com/jesusjbriceno/edificia.git
cd edificia

# 2. Configurar variables de entorno
cp .env.example .env
# Edita .env y añade tus credenciales (DB, JWT, Flux Gateway, etc.)

# 3. Levantar infraestructura + aplicaciones
docker-compose up -d
```

> Para configuración avanzada de entornos y despliegue en producción con Coolify, consulta la [Guía de Despliegue](docs/deployment/GUIA_DESPLIEGUE.md).

---

## **📂 Estructura del Monorepo**

```
/
├── AGENTS.md                        # Contexto maestro para agentes AI
├── docker-compose.yml               # Orquestador (infra local)
├── docker-compose.apps.yml          # Orquestador (apps — Coolify prod)
│
├── apps/
│   ├── api/                         # Backend .NET 10
│   │   ├── src/
│   │   │   ├── Edificia.Domain/     #   Entidades, ValueObjects, Reglas
│   │   │   ├── Edificia.Shared/     #   Kernel: Result<T>, Utils
│   │   │   ├── Edificia.Application/#   CQRS Handlers, Validators
│   │   │   ├── Edificia.Infrastructure/ # EF Core, Dapper, n8n Service
│   │   │   └── Edificia.API/        #   Controllers, Swagger, Middleware
│   │   └── tests/                   #   xUnit + Moq
│   │
│   ├── web/                         # Frontend Astro 4 + React 18
│   │   └── src/
│   │       ├── components/          #   Admin, Editor, Profile, auth, ui
│   │       ├── pages/               #   Rutas Astro (login, dashboard, admin/*)
│   │       ├── store/               #   Zustand (auth, editor, notifications)
│   │       ├── tests/               #   Vitest + Testing Library
│   │       └── lib/                 #   Utilidades (cn, helpers)
│   │
│   └── n8n/                         # Workflows IA
│       ├── workflow-flux.json       #   Proveedor: Flux Gateway (OAuth2)
│       ├── workflow-gemini.json     #   Proveedor: Google Gemini
│       └── workflow-tfm.json        #   Generador de Memoria TFM (Google Drive → Gemini → Slides)
│
└── docs/                            # Documentación completa (ver índice abajo)
```

---

## **🛠️ Stack Tecnológico**

### Backend (`apps/api`)

| Capa | Tecnología |
|:-----|:-----------|
| Framework | .NET 10 Web API |
| Arquitectura | Clean Architecture + CQRS (MediatR) |
| ORM (Escritura) | Entity Framework Core → PostgreSQL |
| ORM (Lectura) | Dapper (SQL Raw optimizado) |
| Validación | FluentValidation |
| Mapeo | Manual (operadores explícitos) — **prohibido AutoMapper** |
| Autenticación | JWT Bearer + Refresh Token Rotation + RBAC |
| Caché | Redis (StackExchange.Redis) |
| Testing | xUnit + Moq |

### Frontend (`apps/web`)

| Capa | Tecnología |
|:-----|:-----------|
| Shell | Astro 4 (SSR, Islands Architecture) |
| Interactividad | React 18 |
| Estilos | Tailwind CSS v4 |
| Estado | Zustand + IndexedDB (idb-keyval) |
| Editor | TipTap (Headless WYSIWYG) |
| Formularios | react-hook-form + Zod |
| Testing | Vitest + Testing Library + Storybook v8 |

### Infraestructura

| Servicio | Tecnología |
|:---------|:-----------|
| Base de Datos | PostgreSQL 16 (snake_case, JSONB) |
| Caché | Redis |
| Contenedores | Docker + Docker Compose |
| PaaS | Coolify v4 (Traefik + TLS automático) |
| IA | n8n → Flux Gateway / Google Gemini |

---

## **🏛️ Arquitectura**

```
┌────────────┐     JSON/REST      ┌──────────────────────────────────────────┐
│            │ ──────────────────► │  Edificia.API (.NET 10)                  │
│  Frontend  │                    │  ┌──────────────────────────────────────┐ │
│  Astro 4   │ ◄────────────────  │  │ Application (CQRS Handlers)         │ │
│  React 18  │                    │  │   ▼ Domain (Entities, Rules)         │ │
│            │                    │  │   ▼ Infrastructure (EF, Dapper, n8n) │ │
│  IndexedDB │                    │  └──────────────────────────────────────┘ │
│  (offline) │                    │     │              │            │         │
└────────────┘                    └─────┼──────────────┼────────────┼─────────┘
                                        │              │            │
                                   PostgreSQL 16    Redis       n8n (webhook)
                                   (JSONB tree)    (cache)     ┌─────────────┐
                                                               │ Flux Gateway│
                                                               │ o Gemini    │
                                                               └─────────────┘
```

> **Dependency Rule estricta:** Domain → Shared. Application → Domain. Infrastructure → Application. API → Todo.

---

## **🤖 Integración IA**

EdificIA **no se acopla a ningún proveedor de IA**. La generación de contenido se delega a workflows n8n mediante la variable de entorno `N8N_WEBHOOK_URL`, lo que permite intercambiar proveedores sin modificar código:

```
Backend (.NET) ──webhook POST──► n8n ──► Flux Gateway (OAuth2)
                                    └──► Google Gemini
                                    └──► Otros proveedores (futuro: Ollama / LM Studio)
```

> ⚠️ **Ollama / LM Studio** son proveedores futuros documentados en el Roadmap §9.3. Actualmente los workflows disponibles son `workflow-flux.json` y `workflow-gemini.json`.

Los workflows se encuentran en `apps/n8n/`. Consulta la [Guía de Workflows n8n](docs/features/ia_delegated/GUIA_WORKFLOWS_N8N.md) para configuración y despliegue.

---

## **📚 Documentación**

Toda la documentación del proyecto se organiza en `docs/`. A continuación el índice completo:

### Trabajo Fin de Máster (TFM)

| Documento | Descripción |
|:----------|:------------|
| [**Memoria del TFM**](docs/TFM/MEMORIA_TFM_EdificIA.md) | Memoria académica completa: resumen, objetivos, metodología, desarrollo técnico y resultados |
| [Contexto TFM](docs/TFM/CONTEXTO_TFM.md) | Información auxiliar del proyecto para el flujo de generación automática |
| [Flujo n8n TFM](apps/n8n/workflow-tfm.json) | Workflow que genera automáticamente la Memoria TFM y las diapositivas desde Google Drive |

### Análisis y Diseño

| Documento | Descripción |
|:----------|:------------|
| [Especificación de Requisitos (ERS)](docs/ERS_EDIFICIA_Lite.md) | Requisitos funcionales y no funcionales del sistema |
| [Análisis Detallado](docs/ANALISIS_DETALLADO.md) | Análisis técnico: pasarela IA, lógica de intervención, notificaciones |
| [Diseño de Sistema (SDD)](docs/DISENO_SISTEMA_EDIFICIA.md) | Arquitectura lógica, diagramas Mermaid, capas del sistema |
| [Manual de Cliente](docs/MANUAL_CLIENTE_EDIFICIA.md) | Guía de usuario orientada al equipo de arquitectura |

### Desarrollo

| Documento | Descripción |
|:----------|:------------|
| [Guía de Estilo y Estándares](docs/development/GUIDELINES.md) | Stack estricto, convenciones de código, patrones obligatorios |
| [Diseño de API REST](docs/development/backend/API_DESIGN.md) | 24 endpoints, contratos request/response, autenticación, paginación y módulo de plantillas |
| [Análisis de Vistas](docs/development/frontend/VIEWS_ANALYSIS.md) | Mapa de navegación y análisis de cada vista del frontend |
| [OpenAPI Spec](docs/openapi.yaml) | Especificación OpenAPI/Swagger de la API |

### Implementación

| Documento | Descripción |
|:----------|:------------|
| [Roadmap Detallado](docs/implementation/ROADMAP_DETALLADO.md) | Plan de implementación por fases con estado de progreso |
| [Anexo de Seguridad](docs/implementation/ANEXO_SEGURIDAD.md) | RBAC, JWT, Refresh Tokens, detección de replay, CRUD de usuarios |

### Features: IA Delegada (n8n)

| Documento | Descripción |
|:----------|:------------|
| [Feature IA Delegada](docs/features/ia_delegated/FEATURE_IA_DELEGADA_N8N.md) | Motivación y diseño de la migración a IA delegada vía n8n |
| [Guía de Workflows n8n](docs/features/ia_delegated/GUIA_WORKFLOWS_N8N.md) | Configuración y arquitectura de los workflows Flux y Gemini |
| [Guía de Implementación n8n](docs/features/ia_delegated/GUIA_IMPLEMENTACION_N8N.md) | Pasos técnicos para la integración backend ↔ n8n |
| [Especificación de Flujos n8n](docs/features/ia_delegated/ESPECIFICACION_FLUJOS_N8N.md) | Contrato del webhook: autenticación, entrada, salida |
| [Integración Flux Gateway](docs/features/ia_delegated/FLUX_INTEGRATION.md) | Endpoints y credenciales para el proveedor Flux |
| [Flux OpenAPI](docs/features/ia_delegated/flux-openapi.json) | Especificación OpenAPI del Flux Gateway |

### Features: Otros

| Documento | Descripción |
|:----------|:------------|
| [Mejora Email con n8n](docs/features/MEJORA_EMAIL_N8N.md) | Propuesta (backlog) de delegación del envío de emails a n8n |
| [Guía definición plantilla .dotx](docs/features/dotx_support/GUIA_DEFINICION_PLANTILLA_DOTX.md) | Paso a paso sencillo para crear plantillas Word `.dotx` con Content Controls compatibles con EdificIA |
| [Plantilla base Markdown](docs/features/dotx_support/TEMPLATE_BASE_MARKDOWN.md) | Base editable para preparar la estructura de plantilla antes de convertir a `.dotx` |
| [Roadmap de implementación .dotx](docs/features/dotx_support/ROADMAP_IMPLEMENTACION_DOTX_N8N.md) | Plan incremental de evolución de la feature `.dotx` (incluye selector de export y tipos dinámicos) |
| [Informe de evolución .dotx (2026-02)](docs/features/dotx_support/INFORME_CAMBIOS_EVOLUCION_DOTX_2026-02.md) | Estado real actual, colisiones detectadas y cambios propuestos para la siguiente iteración |

### Validación de plantillas `.dotx` en subida

- Endpoint de gestión: `/api/templates` (rol Admin/Root).
- Validaciones automáticas: extensión `.dotx`, tamaño máximo 10MB, OpenXML válido, `Content Controls` con `Tag`.
- Para `MemoriaTecnica`, tags mínimos requeridos: `ProjectTitle`, `MD.01`, `MC.01`.
- La UI de `/admin/templates` muestra reglas previas y mensajes guiados si faltan tags obligatorios.

### Despliegue

| Documento | Descripción |
|:----------|:------------|
| [Guía de Despliegue](docs/deployment/GUIA_DESPLIEGUE.md) | Docker, Coolify v4, Traefik, TLS, variables de entorno |

### Contexto para Agentes AI

| Documento | Descripción |
|:----------|:------------|
| [AGENTS.md](AGENTS.md) | Contexto maestro: stack, arquitectura, reglas, Git Flow, patrones |

---

## **🤝 Contribución**

1. Lee [AGENTS.md](AGENTS.md) para entender las reglas de arquitectura y los estándares de código.
2. Consulta la [Guía de Estilo](docs/development/GUIDELINES.md) para convenciones estrictas.
3. Sigue el flujo **Git Flow** (`feature/...` → `develop` → `main`) con **Conventional Commits**.
4. Nunca hagas commits directos en `main` ni `develop` — siempre vía Pull Request.

---

## **📄 Licencia**

Este proyecto está bajo la licencia [**Apache 2.0**](LICENSE).
