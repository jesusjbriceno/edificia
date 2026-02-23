# **🗺️ Análisis de Vistas y Navegación — EdificIA**

**Versión:** 2.1 (Sistema de Notificaciones, Búsqueda en Editor, Cabecera multi-nivel)

## **1. Mapa de Navegación (Sitemap)**

```mermaid
graph TD
    root[/] --> Auth

    subgraph "Public Zone"
        Auth[Login /]
        Forgot[Recuperar Contraseña /forgot-password]
    end

    Auth -->|User| Dash[Inicio /dashboard]
    Auth -->|Admin| AdminDash[Panel Administración]
    Auth -->|User| Profile[Perfil /profile]

    subgraph "Admin Zone"
        AdminDash --> Users[Gestión Usuarios /admin/users]
        AdminDash --> Projects[Gestión Proyectos /admin/projects]
        AdminDash --> Notifications[Gestión Notificaciones /admin/notifications]
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

### **🔵 V-Dash-01: Inicio (`/dashboard`)**

* Grid de tarjetas de proyectos activos del usuario.
* Botón "Nuevo Proyecto" que lanza el Wizard.
* **Sidebar:** Inicio (activo), Proyectos, Usuarios, Ajustes (deshabilitado), Cerrar Sesión.
* **Header:** Barra de búsqueda, notificaciones, avatar con dropdown (Mi Perfil, Cerrar Sesión).
* **Componentes:** `ProjectCard`, `ProjectWizard`, `AuthGuard`, `SidebarLogout`, `HeaderUser`.

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

### **� V-Admin-03: Gestión de Notificaciones (`/admin/notifications`)**

* **Acceso:** Rol Admin o SuperAdmin.
* **Componentes:**
  * `NotificationsList`: Lista de notificaciones con estado leído/no leído.
  * `NotificationBell`: Icono de campana con contador de notificaciones no leídas.
* **Funcionalidades:** Marcar como leída, marcar todas como leídas.

### **�🔵 V-Editor-01: Editor de Memoria Técnica (`/projects/:id`)**

* **Layout:** Sidebar (Capítulos) + Editor central (TipTap).
* **Componentes:**
  * `SidebarNavigation`: Árbol recursivo de capítulos CTE con enlace a zona Admin y **búsqueda en tiempo real** (`searchTree`) que filtra recursivamente en el árbol de secciones.
  * `EditorHeader`: Cabecera multi-nivel con nombre del proyecto, tipo de intervención (Obra Nueva / Reforma) en contexto y botón de retorno al dashboard.
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
| `Dropdown` | `ui/Dropdown.tsx` | Dropdown portal-based (evita clipping en tablas/layouts) |
| `Select` | `ui/Select.tsx` | Selector estilizado con soporte de opciones tipadas |
| `Modal` | `ui/Modal.tsx` | Modal accesible con portal |
| `MobileSidebar` | `MobileSidebar.tsx` | Menú lateral móvil con portal (`createPortal → document.body`) para escapar el stacking context generado por `backdrop-filter` en el header |
| `HeaderUser` | `HeaderUser.tsx` | Avatar con dropdown (Mi Perfil, Cerrar Sesión) |
| `SidebarLogout` | `SidebarLogout.tsx` | Botón de logout en el sidebar con limpieza de sesión |
| `NotificationBell` | `Admin/NotificationBell.tsx` | Icono de campana con contador de no leídas |
| `NotificationsList` | `Admin/NotificationsList.tsx` | Lista de notificaciones con acciones de lectura |

## **4. Stores (Zustand)**

| Store | Ubicación | Datos Gestionados |
| :--- | :--- | :--- |
| `useAuthStore` | `store/useAuthStore.ts` | Token, usuario, login/logout, isAuthenticated |
| `useEditorStore` | `store/useEditorStore.ts` | Sección activa, contenido por sección, estado de guardado |