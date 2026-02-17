# **🗺️ Análisis de Vistas y Navegación — EDIFICIA**

**Versión:** 2.0 (Actualizado con Editor Premium, Admin Projects y Tests Centralizados)

## **1. Mapa de Navegación (Sitemap)**

```mermaid
graph TD
    root[/] --> Auth

    subgraph "Public Zone"
        Auth[Login /]
        Forgot[Recuperar Contraseña /forgot-password]
    end

    Auth -->|User| Dash[Dashboard Proyectos /dashboard]
    Auth -->|Admin| AdminDash[Panel Administración]
    Auth -->|User| Profile[Perfil /profile]

    subgraph "Admin Zone"
        AdminDash --> Users[Gestión Usuarios /admin/users]
        AdminDash --> Projects[Gestión Proyectos /admin/projects]
    end

    Dash --> Editor[Editor de Memoria /projects/:id]
```

## **2. Catálogo de Vistas (Views)**

### **🟢 V-Auth-01: Login (`/`)**

* Formulario estándar (Email/Pass) con fondo arquitectónico premium.
* Enlace "¿Olvidaste tu contraseña?" → Ir a V-Auth-02.
* **Componentes:** `LoginForm`, `AuthGuard`.

### **🟢 V-Auth-02: Recuperación (`/forgot-password`)**

* Input de Email → Acción: Enviar correo de recuperación.
* **Componentes:** `ForgotPassword`.

### **🔵 V-Dash-01: Dashboard de Proyectos (`/dashboard`)**

* Grid de tarjetas de proyectos activos del usuario.
* Botón "Nuevo Proyecto" que lanza el Wizard.
* **Componentes:** `ProjectCard`, `ProjectWizard`, `AuthGuard`.

### **🟡 V-Profile-01: Perfil de Usuario (`/profile`)**

* Vista de información personal del usuario.
* **Componentes:** `ProfileView`.

### **🟣 V-Admin-01: Gestión de Usuarios (`/admin/users`)**

* **Acceso:** Solo rol SuperAdmin.
* **Layout:** Con Sidebar de navegación y menú lateral.
* **Componentes:**
  * `UserTable`: Columnas (Nombre, Email, Rol, Estado, Último Acceso).
  * `UserRow`: Fila extraída con acciones Editar/Bloquear.
  * `UserForm`: Formulario validado con Zod (modal/in-page) para alta y edición.

### **🟣 V-Admin-02: Gestión de Proyectos (`/admin/projects`)**

* **Acceso:** Rol Admin o SuperAdmin.
* **Componentes:**
  * `ProjectManagement`: Orquestador (listado + creación).
  * `ProjectRow`: Fila premium con estado visual (En Ejecución / En Espera / Finalizado).
  * `ProjectForm`: Formulario validado con Zod (Título, Descripción, Estado, Presupuesto).
* **Funcionalidades:** Búsqueda por título/descripción, filtrado, creación inline.

### **🔵 V-Editor-01: Editor de Memoria Técnica (`/projects/:id`)**

* **Layout:** Sidebar (Capítulos) + Editor central (TipTap).
* **Componentes:**
  * `SidebarNavigation`: Árbol recursivo de capítulos CTE con enlace a zona Admin.
  * `EditorShell`: Contenedor principal con Header de estado y botones de acción.
  * `EditorToolbar`: Barra de herramientas de formato (Negrita, Cursiva, H1-H3, Listas, Citas, Undo/Redo).
* **Estado:** Zustand (`useEditorStore`) con persistencia IndexedDB vía `idb-keyval`.

## **3. Inventario de Componentes UI**

| Componente | Ubicación | Propósito |
| :--- | :--- | :--- |
| `Button` | `ui/Button.tsx` | Botón reutilizable con variantes y estados |
| `Input` | `ui/Input.tsx` | Campo de entrada estilizado |
| `Card` | `ui/Card.tsx` | Tarjeta contenedora |
| `Badge` | `ui/Badge.tsx` | Etiqueta de estado |

## **4. Stores (Zustand)**

| Store | Ubicación | Datos Gestionados |
| :--- | :--- | :--- |
| `useAuthStore` | `store/useAuthStore.ts` | Token, usuario, login/logout, isAuthenticated |
| `useEditorStore` | `store/useEditorStore.ts` | Sección activa, contenido por sección, estado de guardado |