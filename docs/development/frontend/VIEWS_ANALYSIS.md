# **🗺️ Análisis de Vistas y Navegación \- EDIFICIA**

**Versión:** 1.1 (Incluye Admin y Auth Flows)

## **1\. Mapa de Navegación (Sitemap)**

graph TD  
    root\[/\] \--\> Auth  
      
    subgraph "Public Zone"  
        Auth\[Login\]  
        Forgot\[Recuperar Contraseña\]  
        Reset\[Reset Password\]  
    end

    Auth \--\>|User| Dash\[Dashboard Proyectos\]  
    Auth \--\>|Admin| AdminDash\[Panel Administración\]

    subgraph "Admin Zone (DashboardLayout)"  
        AdminDash \--\> Users\[Gestión Usuarios\]  
        AdminDash \--\> Audit\[Auditoría Proyectos\]  
    end

## **3\. Catálogo de Vistas (Views)**

### **🟢 V-Auth-01: Login (/)**

* Formulario estándar (Email/Pass).  
* Enlace "¿Olvidaste tu contraseña?" \-\> Ir a V-Auth-02.

### **🟢 V-Auth-02: Recuperación (/auth/recovery)**

* **Paso 1:** Input Email \-\> Acción: Enviar correo.  
* **Paso 2 (Ruta con Token):** /auth/reset?token=... \-\> Input Nueva Password \+ Confirmación.

### **🟣 V-Admin-01: Gestión de Usuarios (/admin/users)**

* **Acceso:** Solo rol SuperAdmin.  
* **Layout:** DashboardLayout (con menú lateral extendido).  
* **Componentes:**  
  * UserTable: Columnas (Nombre, Email, Rol, Estado, Último Acceso).  
  * Actions: Botones Editar, Bloquear, Eliminar.  
  * CreateUserModal: Formulario para dar de alta nuevos arquitectos o supervisores manualmente.

### **🟣 V-Admin-02: Auditoría (/admin/audit)**

* **Acceso:** Roles SuperAdmin y Supervisor.  
* **Componente:** GlobalProjectGrid.  
* **Diferencia:** Muestra TODOS los proyectos de TODOS los usuarios en modo "Solo Lectura". Al hacer clic, abre el Editor pero sin permisos de escritura.

*(Resto del documento: Dashboard, Wizard y Editor se mantienen...)*