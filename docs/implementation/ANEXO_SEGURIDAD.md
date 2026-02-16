# **🛡️ Anexo de Seguridad y Gestión de Usuarios \- EDIFICIA**

**Versión:** 1.1 (Incluye Root Bootstrapping)

**Referencia:** Complementa al Roadmap de Implementación.

**Stack:** ASP.NET Core Identity, JWT, RBAC.

## **1\. Definición de Roles y Políticas (RBAC)**

*(Sin cambios respecto a v1.0)*

## **5\. Estrategia de Usuario Root (Bootstrapping)**

El despliegue inicial del sistema debe garantizar la existencia de un administrador sin intervención manual en la base de datos.

### **5.1. Parametrización (Environment Variables)**

El contenedor de la API leerá las credenciales iniciales de las variables de entorno. **Nunca** se hardcodean en el código.

\# docker-compose.yml  
environment:  
  \- Security\_\_RootEmail=${ROOT\_EMAIL:-admin@edificia.dev}  
  \- Security\_\_RootInitialPassword=${ROOT\_PASSWORD:-ChangeMe123\!}

### **5.2. Proceso de Seeding Automático**

Se implementará un IHostedService (IdentityDataInitializer) que se ejecuta al arrancar la API:

1. **Check:** ¿Existe algún usuario con Rol SuperAdmin?  
2. **Si NO existe:**  
   * Crea el usuario usando Security\_\_RootEmail.  
   * Asigna la password Security\_\_RootInitialPassword.  
   * Asigna el rol Role.Root.  
   * **CRÍTICO:** Establece un flag en base de datos: MustChangePassword \= true.

### **5.3. Flujo "First Login" (Contraseña Maestra)**

Para garantizar la seguridad, el usuario Root recién creado no es funcional al 100% hasta que cambia su clave.

1. **Login:** El usuario Root se loguea con la clave temporal.  
2. **Token Claim:** El sistema detecta MustChangePassword \= true y emite un JWT con un claim especial amr: "pwd\_change\_required".  
3. **Bloqueo:** Una Política de Seguridad global (Policy.ActiveUser) rechaza cualquier petición a la API (excepto /auth/change-password) si el token tiene ese claim.  
4. **Frontend:** Detecta el claim y redirige forzosamente a la pantalla /admin/setup-password.

## **6\. Plan de Implementación Actualizado**

| Fase | Feature | Tareas Backend (.NET) | Tareas Frontend (React) |
| :---- | :---- | :---- | :---- |
| **S.1** | feature/identity-core | • IdentityDbContext \+ Entidad ApplicationUser (con flag MustChangePassword). • IdentityDataInitializer (Seeder). | N/A |
| **S.2** | feature/auth-jwt | • Login Endpoint con chequeo de MustChangePassword. • Middleware/Policy para bloquear usuarios pendientes de setup. | • Interceptor Axios para detectar error 403 o Claim específico y redirigir a Setup. |

*(Resto de fases S.3, S.4, S.5 se mantienen)*