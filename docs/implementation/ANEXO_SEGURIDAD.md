# **🛡️ Anexo de Seguridad y Gestión de Usuarios — EdificIA**

**Versión:** 2.0 (Completo — incluye CRUD de Usuarios, Refresh Tokens y Perfil)

**Referencia:** Complementa al Roadmap de Implementación.

**Stack:** ASP.NET Core Identity, JWT, RBAC.

---

## **1. Definición de Roles y Políticas (RBAC)**

### **1.1. Roles del Sistema**

| Rol | Constante | Descripción | Capacidades |
| :--- | :--- | :--- | :--- |
| **Root** | `AppRoles.Root` | Super-administrador del sistema. Solo uno. Se crea por seeding. | Todo. Gestión de Admins. |
| **Admin** | `AppRoles.Admin` | Administrador de la organización. | Gestión de Architects y Collaborators. Acceso completo a proyectos. |
| **Architect** | `AppRoles.Architect` | Arquitecto colegiado que redacta memorias. | CRUD de proyectos propios. Generación IA. Exportar. |
| **Collaborator** | `AppRoles.Collaborator` | Colaborador con acceso de solo lectura. | Ver proyectos asignados. Sin edición ni generación IA. |

### **1.2. Políticas de Autorización**

| Política | Constante | Requisito |
| :--- | :--- | :--- |
| **ActiveUser** | `AppPolicies.ActiveUser` | Usuario autenticado + `IsActive = true` + NO tiene claim `pwd_change_required`. |
| **RequireRoot** | `AppPolicies.RequireRoot` | Rol = Root. |
| **RequireAdmin** | `AppPolicies.RequireAdmin` | Rol = Root o Admin. |
| **RequireArchitect** | `AppPolicies.RequireArchitect` | Rol = Root, Admin o Architect. |

### **1.3. Claims Personalizados**

| Claim | Constante | Uso |
| :--- | :--- | :--- |
| `amr` | `AppClaims.Amr` | Método de autenticación. Valor `pwd_change_required` si debe cambiar contraseña. |
| `full_name` | `AppClaims.FullName` | Nombre completo del usuario (informativo). |
| `collegiate_number` | `AppClaims.CollegiateNumber` | Número de colegiado (informativo, solo Architects). |

---

## **2. Modelo de Datos: ApplicationUser**

Extiende `IdentityUser<Guid>` con los siguientes campos adicionales:

| Campo | Tipo | Descripción |
| :--- | :--- | :--- |
| `FullName` | `string` | Nombre completo del usuario (requerido). |
| `CollegiateNumber` | `string?` | Número de colegiado (solo Architects). |
| `MustChangePassword` | `bool` | Si `true`, el JWT emitido tiene claim restringido. |
| `IsActive` | `bool` | Si `false`, login rechazado con error `AccountDisabled`. |
| `CreatedAt` | `DateTime` | Timestamp de creación (auto-set por `SaveChangesAsync`). |
| `UpdatedAt` | `DateTime?` | Timestamp de última modificación. |

**Estado actual:** ✅ Implementado en `Edificia.Domain/Entities/ApplicationUser.cs`.

---

## **3. Política de Contraseñas**

Configurada en Identity Options y reforzada con FluentValidation:

| Regla | Valor |
| :--- | :--- |
| Longitud mínima | 8 caracteres |
| Requiere mayúscula | Sí |
| Requiere minúscula | Sí |
| Requiere dígito | Sí |
| Requiere carácter especial | Sí |
| Contraseña ≠ contraseña actual | Sí (validación en ChangePassword) |

**Bloqueo por intentos fallidos:**

| Parámetro | Valor |
| :--- | :--- |
| `MaxFailedAccessAttempts` | 5 |
| `DefaultLockoutTimeSpan` | 15 minutos |
| `AllowedForNewUsers` | `true` |

**Estado actual:** ✅ Implementado en `Infrastructure/DependencyInjection.cs`.

---

## **4. Autenticación JWT**

| Parámetro | Valor | Fuente |
| :--- | :--- | :--- |
| Algoritmo | HMAC-SHA256 | `JwtTokenService` |
| Expiración access token | 60 minutos | `Jwt:ExpirationMinutes` |
| Expiración refresh token | 7 días | `Jwt:RefreshTokenExpirationDays` (⚠️ **no implementado aún**) |
| Issuer/Audience | Configurables | `appsettings.json` |

### **4.1. Claims incluidos en el JWT**

- `sub` / `NameIdentifier` — User ID (Guid)
- `email` — Email del usuario
- `role` — Rol(es) asignados
- `full_name` — Nombre completo
- `collegiate_number` — Número de colegiado (si aplica)
- `amr` — `pwd_change_required` si `MustChangePassword = true`

**Estado actual:** ✅ Access token y refresh token implementados.

---

## **5. Estrategia de Usuario Root (Bootstrapping)**

El despliegue inicial del sistema debe garantizar la existencia de un administrador sin intervención manual en la base de datos.

### **5.1. Parametrización (Environment Variables)**

El contenedor de la API leerá las credenciales iniciales de las variables de entorno. **Nunca** se hardcodean en el código.

```yaml
# docker-compose.yml
environment:
  - Security__RootEmail=${ROOT_EMAIL:-admin@edificia.dev}
  - Security__RootInitialPassword=${ROOT_PASSWORD:-ChangeMe123!}
```

### **5.2. Proceso de Seeding Automático**

Implementado como `IHostedService` (`IdentityDataInitializer`) que se ejecuta al arrancar la API:

1. **Roles:** Crea los 4 roles si no existen (Root, Admin, Architect, Collaborator).
2. **Root User:** Si no existe ningún usuario con rol Root:
   - Crea el usuario con `Security__RootEmail`.
   - Asigna la password `Security__RootInitialPassword`.
   - Asigna el rol Root.
   - **CRÍTICO:** `MustChangePassword = true`.

### **5.3. Flujo "First Login"**

1. **Login:** El Root se loguea con la clave temporal → recibe JWT con claim `amr: pwd_change_required`.
2. **Bloqueo:** La política `ActiveUser` rechaza todas las peticiones excepto `POST /auth/change-password`.
3. **Cambio:** El Root cambia su password → se limpia `MustChangePassword` → obtiene JWT completo.
4. **Frontend:** Detecta `MustChangePassword` en la respuesta de login y redirige a pantalla de cambio.

**Estado actual:** ✅ Completamente implementado.

---

## **6. Gestión de Usuarios (CRUD) — ⚠️ PENDIENTE**

### **6.1. Operaciones requeridas**

Los usuarios con rol Root o Admin deben poder gestionar usuarios mediante la API. Los usuarios gestionados serán de rol Architect o Collaborator (Root sólo gestiona Admins).

| Operación | Endpoint | Política | Descripción |
| :--- | :--- | :--- | :--- |
| **Crear** | `POST /api/users` | RequireAdmin | Crea un usuario con password temporal. `MustChangePassword = true`. Envía email de bienvenida. |
| **Listar** | `GET /api/users` | RequireAdmin | Lista paginada con filtros por rol, estado y búsqueda. |
| **Obtener** | `GET /api/users/{id}` | RequireAdmin | Detalle de un usuario por ID. |
| **Actualizar** | `PUT /api/users/{id}` | RequireAdmin | Actualiza FullName, CollegiateNumber, Role. |
| **Desactivar** | `POST /api/users/{id}/deactivate` | RequireAdmin | Pone `IsActive = false`. Login bloqueado. |
| **Reactivar** | `POST /api/users/{id}/activate` | RequireAdmin | Pone `IsActive = true`. |
| **Reset password** | `POST /api/users/{id}/reset-password` | RequireAdmin | Genera password temporal. `MustChangePassword = true`. Envía email. |

### **6.2. Reglas de negocio**

- Un Admin **no puede** crear/modificar/desactivar usuarios Root ni otros Admins.
- Solo Root puede gestionar Admins.
- No se puede desactivar al propio usuario autenticado.
- No se puede eliminar usuarios (soft-delete vía `IsActive`).
- Al crear un usuario, se valida `RequireUniqueEmail`.
- El email de bienvenida incluye la password temporal y enlace a la plataforma.

### **6.3. Arquitectura (Application Layer)**

```
Application/Users/
├── Commands/
│   ├── CreateUser/
│   │   ├── CreateUserCommand.cs
│   │   ├── CreateUserHandler.cs
│   │   ├── CreateUserValidator.cs
│   │   └── CreateUserRequest.cs
│   ├── UpdateUser/
│   │   ├── UpdateUserCommand.cs
│   │   ├── UpdateUserHandler.cs
│   │   ├── UpdateUserValidator.cs
│   │   └── UpdateUserRequest.cs
│   ├── ToggleUserStatus/
│   │   ├── ToggleUserStatusCommand.cs
│   │   └── ToggleUserStatusHandler.cs
│   └── ResetUserPassword/
│       ├── ResetUserPasswordCommand.cs
│       └── ResetUserPasswordHandler.cs
├── Queries/
│   ├── GetUsers/
│   │   ├── GetUsersQuery.cs
│   │   ├── GetUsersHandler.cs
│   │   └── GetUsersValidator.cs
│   ├── GetUserById/
│   │   ├── GetUserByIdQuery.cs
│   │   └── GetUserByIdHandler.cs
│   └── UserResponse.cs
└── UserSqlQueries.cs
```

### **6.4. API Controller**

```
API/Controllers/UsersController.cs
  [Authorize(Policy = AppPolicies.RequireAdmin)]
```

---

## **7. Perfil de Usuario (Self-Service) — ✅ IMPLEMENTADO**

El usuario autenticado debe poder gestionar su propio perfil sin permisos de admin.

| Operación | Endpoint | Política | Descripción |
| :--- | :--- | :--- | :--- |
| **Ver perfil** | `GET /auth/me` | ActiveUser | Ya implementado. |
| **Actualizar perfil** | `PUT /auth/profile` | ActiveUser | Actualiza FullName y CollegiateNumber propios. |

### **7.1. Arquitectura**

```
Application/Auth/Commands/UpdateProfile/
├── UpdateProfileCommand.cs
├── UpdateProfileHandler.cs
├── UpdateProfileValidator.cs
└── UpdateProfileRequest.cs
```

---

## **8. Refresh Tokens — ✅ IMPLEMENTADO**

El sistema emite access tokens con 60 min de vida y refresh tokens con rotación para evitar re-login constante.

### **8.1. Modelo de datos**

| Campo | Tipo | Descripción |
| :--- | :--- | :--- |
| `Id` | `Guid` | PK del refresh token. |
| `UserId` | `Guid` | FK → ApplicationUser. |
| `Token` | `string` | Token opaco (64 bytes, Base64). |
| `ExpiresAt` | `DateTime` | Fecha de expiración (7 días). |
| `CreatedAt` | `DateTime` | Fecha de creación. |
| `RevokedAt` | `DateTime?` | Si no es null, el token fue revocado. |
| `ReplacedByTokenId` | `Guid?` | Referencia al token de reemplazo (rotación). |

### **8.2. Endpoints**

| Operación | Endpoint | Descripción |
| :--- | :--- | :--- |
| **Refresh** | `POST /auth/refresh` | Recibe refresh token → devuelve nuevo access + refresh (rotación). |
| **Revoke** | `POST /auth/revoke` | Revoca el refresh token actual (logout). |

### **8.3. Flujo de rotación**

1. Cliente envía refresh token expirado/próximo a expirar.
2. Backend valida el token, comprueba que no está revocado.
3. Genera nuevo par (access token + refresh token).
4. Marca el refresh token anterior como revocado + `ReplacedByTokenId`.
5. Si se detecta reutilización de un token ya revocado → revocar toda la familia (posible robo).

### **8.4. Arquitectura**

```
Domain/Entities/RefreshToken.cs

Application/Auth/Commands/RefreshToken/
├── RefreshTokenCommand.cs
├── RefreshTokenHandler.cs
└── RefreshTokenRequest.cs

Application/Auth/Commands/RevokeToken/
├── RevokeTokenCommand.cs
└── RevokeTokenHandler.cs

Infrastructure/Identity/RefreshTokenRepository.cs (o EF via DbContext)
```

---

## **9. Plan de Implementación**

### **Estado de fases completadas**

| Fase | Feature | Estado |
| :--- | :--- | :--- |
| **S.1** | `feature/identity-core` | ✅ Completada (PR #15) |
| **S.2** | `feature/auth-jwt` | ✅ Completada (PR #16) |

### **Fases pendientes**

| Fase | Feature Branch | Tareas Backend (.NET) | Dependencia |
| :--- | :--- | :--- | :--- |
| **S.3** | `feature/user-management` | CRUD de usuarios, gestión de roles, emails de bienvenida/reset. | S.2 |
| **S.4** | `feature/refresh-tokens` | ✅ Implementado. Rotación de tokens, detección de reutilización, endpoints refresh/revoke. | S.2 |
| **S.5** | `feature/user-profile` | ✅ Implementado. `UpdateProfileCommand`, endpoint `PUT /auth/profile`. | S.2 |