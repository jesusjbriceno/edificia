# **Análisis Detallado \- EdificIA**

**Versión:** 2.1 (Sistema de Notificaciones, Mejoras de UI/UX del Editor)

**Foco:** Integración Flux Gateway, Lógica de Intervención, Sistema de Notificaciones

## **2\. Análisis Técnico: La Pasarela de IA (Flux Gateway)**

**Requisito:** RF-IA-01

* **Cambio de Paradigma:** No nos conectamos a "Google Gemini". Nos conectamos a "Edificia AI Service" (que por debajo es Flux).  
* **Flujo de Autenticación (OAuth2):**  
  1. El Backend (.NET) solicita token a dashboard-flux.jesusjbriceno.dev/auth/login enviando client\_id y client\_secret.  
  2. Recibe un access\_token (JWT).  
  3. El Backend cachea este token (IMemoryCache) hasta su expiración.  
  4. El Backend llama al endpoint de chat inyectando el header Authorization: Bearer {token}.  
* **Ventaja Arquitectónica:** Si mañana Flux cambia Gemini por GPT-4o Mini o Claude Haiku, **no tocamos ni una línea de código**, solo configuración.

## **3\. Análisis Funcional: El "Filtro de Obra"**

Para cumplir con la visión del cliente ("No pedir hormigón en una reforma de baño"):

* **Lógica del Árbol JSON:**  
  * El frontend (React) recibe el árbol completo del CTE.  
  * Aplica una función recursiva filterTree(nodes, projectConfig).  
  * Si Project.InterventionType \== Reform Y el nodo tiene requiresNewWork: true, el nodo se elimina de la UI.

## **4\. Sistema de Notificaciones**

**Decisión arquitectónica:** Notificaciones persistidas en base de datos (no WebSockets en v1).

* **Entidad Domain:** `Notification` en `Edificia.Domain.Entities` con `AuditableEntity` como base. Métodos de fábrica `Create()` y de comportamiento `MarkAsRead()`.  
* **Patrón CQRS:**  
  * **Query:** `GetNotificationsQuery` — recupera notificaciones del usuario autenticado via Dapper para máximo rendimiento de lectura.  
  * **Commands:** `MarkAsReadCommand` y `MarkAllAsReadCommand` — actualizan estado via EF Core.  
* **API:** `NotificationsController` expone 3 endpoints: `GET /api/notifications`, `POST /api/notifications/{id}/read`, `POST /api/notifications/mark-all-read`.  
* **Frontend:** `NotificationBell` (indicador con contador de no leídas) + `NotificationsList` (dropdown con acciones). El `notificationService` llama directamente a la API real sin persistencia local.

## **5\. Mejoras de UI/UX del Editor (pre-release)**

* **Búsqueda en SidebarNavigation:** Función recursiva `searchTree(nodes, query)` que filtra el árbol de capítulos CTE en tiempo real sin afectar al estado global. Implementada con `useMemo` para optimización de renders.  
* **EditorHeader multi-nivel:** Cabecera con contexto completo del proyecto (nombre + tipo de intervención), botón de retorno al dashboard y separadores visuales.  
* **Portal-based Dropdown:** `ui/Dropdown.tsx` usa `ReactDOM.createPortal` para evitar clipping en tablas (gestión de usuarios) y selectores dentro de contenedores con `overflow: hidden`.  
* **Portal-based MobileSidebar:** `MobileSidebar.tsx` usa `ReactDOM.createPortal` hacia `document.body` para escapar el containing block que crea `backdrop-filter: blur()` en el `<header>` del `DashboardLayout`. Sin el portal, `position: fixed` en WebKit/Blink queda confinado al stacking context del header, ignorando el z-index respecto al resto del viewport.  
* **Catálogo de componentes con Storybook v8:** Los componentes atómicos de `ui/` (`Button`, `Badge`, `Input`, `Modal`, `Skeleton`) están documentados con Storybook 8 usando `@storybook/react-vite`. La integración con Tailwind CSS v4 se realiza mediante `viteFinal` en `.storybook/main.ts`, que inyecta `@tailwindcss/vite` como plugin y registra el alias `@/→src/`. Para resolver la incompatibilidad entre el módulo ESM del proyecto (`"type": "module"` en `package.json`) y el `require()` que usa Storybook internamente, se añade un `.storybook/package.json` con `{ "type": "commonjs" }`. Las stories siguen el formato CSF con `tags: ['autodocs']`, generando documentación automática de props desde los tipos TypeScript sin configuración extra.  
* **Limpieza de datos hardcoded:** Eliminación de usuarios y notificaciones hardcodeados en las vistas admin, dando paso a datos reales desde la API.