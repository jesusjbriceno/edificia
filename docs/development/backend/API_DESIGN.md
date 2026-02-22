# **📡 Diseño de API REST — EdificIA**

**Versión:** 3.0  
**Última actualización:** Junio 2025  
**Base URL (Local):** `http://localhost:5000/api`  
**Base URL (Producción):** `https://api-edificia.jesusjbriceno.dev/api`

---

## **1. Visión General**

La API de EdificIA expone **21 endpoints** organizados en 5 módulos:

| Módulo | Endpoints | Autenticación | Descripción |
|--------|-----------|---------------|-------------|
| Auth | 6 | Mixta | Autenticación JWT, gestión de tokens y perfil |
| Projects | 6 | ActiveUser | CRUD de proyectos y árbol de contenido |
| Users | 7 | RequireAdmin | Gestión de usuarios (CRUD + activación) |
| AI | 1 | ActiveUser | Generación de texto con IA |
| Export | 1 | ActiveUser | Exportación a DOCX |

### **1.1. Convenciones**

- **Formato:** JSON (`application/json`) para entrada/salida; DOCX para exportación.
- **Errores:** RFC 7807 ProblemDetails en todas las respuestas de error.
- **Paginación:** Query params `page` (≥1) y `pageSize` (1–50).
- **IDs:** UUID v4 (`Guid`).
- **Fechas:** ISO 8601 UTC.

### **1.2. Autenticación**

Basada en **JWT Bearer** con **Refresh Token Rotation**.

```
Authorization: Bearer <access_token>
```

**Políticas de autorización:**

| Política | Descripción | Roles permitidos |
|----------|-------------|------------------|
| `ActiveUser` | Usuario activo autenticado | Root, Admin, Architect, Collaborator |
| `RequireAdmin` | Administración de usuarios | Root, Admin |
| `RequireArchitect` | Operaciones de arquitecto | Root, Admin, Architect |
| `RequireRoot` | Super administrador | Root |

### **1.3. Formato de Error (ProblemDetails)**

```json
{
  "status": 400,
  "title": "Error en la solicitud",
  "detail": "Descripción del error.",
  "code": "Entity.ErrorCode"
}
```

**Mapeo de códigos de error a HTTP:**

| Prefijo del código | HTTP Status | Descripción |
|---------------------|-------------|-------------|
| `NotFound.*` | 404 | Recurso no encontrado |
| `Conflict.*` | 409 | Conflicto (duplicado) |
| `Unauthorized.*` | 401 | No autenticado / credenciales inválidas |
| `Forbidden.*` | 403 | Sin permisos suficientes |
| `Validation.*` | 400 | Error de validación |
| `Failure.*` | 500 | Error interno |

**Errores de validación (FluentValidation):**

```json
{
  "status": 400,
  "title": "Errores de validación",
  "detail": "Se encontraron errores de validación.",
  "errors": [
    { "property": "Email", "error": "'Email' no puede estar vacío." }
  ]
}
```

---

## **2. Módulo: Auth (`/api/auth`)**

### **2.1. Login**

Autentica un usuario y devuelve tokens JWT + Refresh Token.

```
POST /api/auth/login
```

**Autenticación:** Ninguna (AllowAnonymous)

**Request Body:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `email` | string | ✅ | Email válido |
| `password` | string | ✅ | Contraseña |

```json
{
  "email": "arquitecto@edificia.es",
  "password": "MiContraseña123!"
}
```

**Response `200 OK`:**

```json
{
  "accessToken": "eyJhbGciOiJI...",
  "refreshToken": "base64-encoded-64-bytes...",
  "expiresInMinutes": 60,
  "mustChangePassword": false,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "arquitecto@edificia.es",
    "fullName": "María García López",
    "collegiateNumber": "COA-12345",
    "roles": ["Architect"]
  }
}
```

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Auth.InvalidCredentials` | 401 | Email o contraseña incorrectos |
| `Auth.AccountInactive` | 403 | Cuenta desactivada |
| `Auth.AccountLockedOut` | 403 | Cuenta bloqueada temporalmente |

---

### **2.2. Refresh Token**

Rota el refresh token (emite nuevos access + refresh tokens). Implementa **stolen-token detection**: si se reutiliza un token ya revocado, se revocan todos los tokens del usuario.

```
POST /api/auth/refresh
```

**Autenticación:** Ninguna (AllowAnonymous)

**Request Body:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `refreshToken` | string | ✅ | Token de refresco actual |

```json
{
  "refreshToken": "base64-encoded-token..."
}
```

**Response `200 OK`:** Mismo formato que Login (§2.1).

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Auth.InvalidRefreshToken` | 401 | Token no encontrado o ya revocado (stolen-token detection activado) |
| `Auth.RefreshTokenExpired` | 401 | Token expirado |

---

### **2.3. Revoke Token**

Revoca un refresh token (logout). Operación idempotente.

```
POST /api/auth/revoke
```

**Autenticación:** Bearer JWT (Authorize)

**Request Body:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `refreshToken` | string | ✅ | Token a revocar |

```json
{
  "refreshToken": "base64-encoded-token..."
}
```

**Response `200 OK`:** Sin cuerpo.

---

### **2.4. Change Password**

Cambia la contraseña del usuario autenticado. Si el usuario tiene `mustChangePassword = true`, el flag se desactiva automáticamente.

```
POST /api/auth/change-password
```

**Autenticación:** Bearer JWT (Authorize)

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `currentPassword` | string | ✅ | No vacío |
| `newPassword` | string | ✅ | Min 8 chars, 1 mayúscula, 1 minúscula, 1 dígito, 1 especial, ≠ currentPassword |

```json
{
  "currentPassword": "MiContraseña123!",
  "newPassword": "NuevaContraseña456@"
}
```

**Response `200 OK`:** Sin cuerpo.

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Auth.InvalidCurrentPassword` | 401 | Contraseña actual incorrecta |
| `Auth.PasswordChangeFailed` | 500 | Error al cambiar la contraseña |

---

### **2.5. Get Current User (Me)**

Obtiene la información del usuario autenticado.

```
GET /api/auth/me
```

**Autenticación:** ActiveUser

**Response `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "arquitecto@edificia.es",
  "fullName": "María García López",
  "roles": ["Architect"]
}
```

---

### **2.6. Update Profile**

Actualiza el perfil del usuario autenticado (nombre y número de colegiado).

```
PUT /api/auth/profile
```

**Autenticación:** ActiveUser

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `fullName` | string | ✅ | No vacío, max 200 chars |
| `collegiateNumber` | string? | ❌ | Max 50 chars |

```json
{
  "fullName": "María García López",
  "collegiateNumber": "COA-12345"
}
```

**Response `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "arquitecto@edificia.es",
  "fullName": "María García López",
  "collegiateNumber": "COA-12345"
}
```

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Auth.ProfileUpdateFailed` | 500 | Error al actualizar el perfil |

---

## **3. Módulo: Projects (`/api/projects`)**

### **3.1. List Projects**

Lista paginada de proyectos con filtros opcionales.

```
GET /api/projects?page=1&pageSize=10&status=Draft&search=casa
```

**Autenticación:** ActiveUser

**Query Parameters:**

| Parámetro | Tipo | Default | Validación | Descripción |
|-----------|------|---------|------------|-------------|
| `page` | int | 1 | ≥ 1 | Número de página |
| `pageSize` | int | 10 | 1–50 | Elementos por página |
| `status` | string? | — | Draft, InProgress, Completed, Archived | Filtro por estado |
| `search` | string? | — | — | Búsqueda ILIKE en título |

**Response `200 OK`:**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Reforma integral vivienda unifamiliar",
      "description": "Reforma completa de vivienda en Madrid",
      "address": "Calle Mayor 123, Madrid",
      "interventionType": "Reform",
      "isLoeRequired": false,
      "cadastralReference": "1234567AB1234C0001XY",
      "localRegulations": "PGOU Madrid 2024",
      "status": "Draft",
      "createdAt": "2025-01-15T10:30:00Z",
      "updatedAt": null
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

### **3.2. Get Project**

Obtiene un proyecto por ID.

```
GET /api/projects/{id}
```

**Autenticación:** ActiveUser

**Path Parameters:**

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Guid | ID del proyecto |

**Response `200 OK`:** Mismo formato que un elemento de la lista (§3.1).

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Project.NotFound` | 404 | Proyecto no encontrado |

---

### **3.3. Create Project**

Crea un nuevo proyecto.

```
POST /api/projects
```

**Autenticación:** ActiveUser

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `title` | string | ✅ | No vacío, max 300 chars |
| `interventionType` | enum | ✅ | `NewConstruction` (0), `Reform` (1), `Extension` (2) |
| `isLoeRequired` | bool | ✅ | — |
| `description` | string? | ❌ | Max 2000 chars |
| `address` | string? | ❌ | Max 500 chars |
| `cadastralReference` | string? | ❌ | Max 100 chars |
| `localRegulations` | string? | ❌ | Max 5000 chars |

```json
{
  "title": "Reforma integral vivienda unifamiliar",
  "interventionType": 1,
  "isLoeRequired": false,
  "description": "Reforma completa de vivienda en Madrid",
  "address": "Calle Mayor 123, Madrid",
  "cadastralReference": "1234567AB1234C0001XY",
  "localRegulations": "PGOU Madrid 2024"
}
```

**Response `201 Created`:**

```
Location: /api/projects/{id}
```

```json
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

---

### **3.4. Get Content Tree**

Obtiene el árbol de contenido normativo del proyecto (estructura JSONB).

```
GET /api/projects/{id}/tree
```

**Autenticación:** ActiveUser

**Response `200 OK`:**

```json
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "interventionType": "Reform",
  "isLoeRequired": false,
  "contentTreeJson": "{\"chapters\":[{\"id\":\"MD\",\"title\":\"Memoria Descriptiva\",...}]}"
}
```

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Project.NotFound` | 404 | Proyecto no encontrado |

---

### **3.5. Update Content Tree**

Reemplaza completamente el árbol de contenido del proyecto.

```
PUT /api/projects/{id}/tree
```

**Autenticación:** ActiveUser

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `contentTreeJson` | string | ✅ | No vacío, max 1 MB |

```json
{
  "contentTreeJson": "{\"chapters\":[{\"id\":\"MD\",\"title\":\"Memoria Descriptiva\",\"sections\":[]}]}"
}
```

**Response `204 No Content`**

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Project.NotFound` | 404 | Proyecto no encontrado |

---

### **3.6. Patch Section Content**

Actualiza el contenido de una sección específica dentro del árbol (actualización parcial JSONB).

```
PATCH /api/projects/{id}/sections/{sectionId}
```

**Autenticación:** ActiveUser

**Path Parameters:**

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Guid | ID del proyecto |
| `sectionId` | string | ID de la sección dentro del árbol (max 200 chars) |

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `content` | string | ✅ | No nulo, max 512 KB |

```json
{
  "content": "<p>El presente proyecto se refiere a la reforma integral...</p>"
}
```

**Response `204 No Content`**

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Project.NotFound` | 404 | Proyecto no encontrado |
| `Section.NotFound` | 404 | Sección no encontrada en el árbol |
| `Project.NoContentTree` | 400 | El proyecto no tiene árbol de contenido |

---

## **4. Módulo: Users (`/api/users`)**

> Todos los endpoints requieren política **RequireAdmin** (roles Root o Admin).

### **4.1. List Users**

Lista paginada de usuarios con filtros opcionales.

```
GET /api/users?page=1&pageSize=10&role=Architect&isActive=true&search=garcía
```

**Query Parameters:**

| Parámetro | Tipo | Default | Validación | Descripción |
|-----------|------|---------|------------|-------------|
| `page` | int | 1 | ≥ 1 | Número de página |
| `pageSize` | int | 10 | 1–50 | Elementos por página |
| `role` | string? | — | Root, Admin, Architect, Collaborator | Filtro por rol |
| `isActive` | bool? | — | — | Filtro por estado activo |
| `search` | string? | — | — | Búsqueda ILIKE en nombre o email |

**Response `200 OK`:**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "arquitecto@edificia.es",
      "fullName": "María García López",
      "collegiateNumber": "COA-12345",
      "isActive": true,
      "mustChangePassword": false,
      "role": "Architect",
      "createdAt": "2025-01-10T08:00:00Z",
      "updatedAt": "2025-01-15T14:30:00Z"
    }
  ],
  "totalCount": 12,
  "page": 1,
  "pageSize": 10,
  "totalPages": 2,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

### **4.2. Get User**

Obtiene un usuario por ID.

```
GET /api/users/{id}
```

**Response `200 OK`:** Mismo formato que un elemento de la lista (§4.1).

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `User.NotFound` | 404 | El usuario no existe |

---

### **4.3. Create User**

Crea un nuevo usuario. Se genera una contraseña temporal y se envía por email. El usuario debe cambiarla en el primer login (`mustChangePassword = true`).

```
POST /api/users
```

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `email` | string | ✅ | Email válido |
| `fullName` | string | ✅ | No vacío, max 200 chars |
| `role` | string | ✅ | `Admin`, `Architect`, `Collaborator` |
| `collegiateNumber` | string? | ❌ | Max 50 chars |

```json
{
  "email": "nuevo@edificia.es",
  "fullName": "Carlos Rodríguez Pérez",
  "role": "Architect",
  "collegiateNumber": "COA-67890"
}
```

**Response `201 Created`:**

```
Location: /api/users/{id}
```

```json
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `User.EmailAlreadyExists` | 409 | Ya existe un usuario con ese email |
| `User.InvalidRole` | 400 | Rol no válido |
| `User.CannotModifyHigherRole` | 403 | No tiene permisos para asignar ese rol |
| `User.CreationFailed` | 500 | Error al crear el usuario |

**Jerarquía de roles (restricción):**

| Actor | Puede crear |
|-------|-------------|
| Root | Admin, Architect, Collaborator |
| Admin | Architect, Collaborator |

---

### **4.4. Update User**

Actualiza los datos y rol de un usuario existente.

```
PUT /api/users/{id}
```

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `fullName` | string | ✅ | No vacío, max 200 chars |
| `role` | string | ✅ | `Admin`, `Architect`, `Collaborator` |
| `collegiateNumber` | string? | ❌ | Max 50 chars |

```json
{
  "fullName": "Carlos Rodríguez Pérez",
  "role": "Admin",
  "collegiateNumber": null
}
```

**Response `204 No Content`**

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `User.NotFound` | 404 | El usuario no existe |
| `User.CannotModifyHigherRole` | 403 | No tiene permisos para ese rol |
| `User.UpdateFailed` | 500 | Error al actualizar |
| `User.RoleChangeFailed` | 500 | Error al cambiar el rol |

---

### **4.5. Deactivate User**

Desactiva un usuario (soft delete). El usuario no podrá autenticarse.

```
POST /api/users/{id}/deactivate
```

**Response `200 OK`:** Sin cuerpo.

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `User.NotFound` | 404 | El usuario no existe |
| `User.CannotDeactivateSelf` | 403 | No puede desactivar su propia cuenta |
| `User.CannotModifyHigherRole` | 403 | No tiene permisos sobre ese usuario |

---

### **4.6. Activate User**

Reactiva un usuario previamente desactivado.

```
POST /api/users/{id}/activate
```

**Response `200 OK`:** Sin cuerpo.

**Errores:** Mismos que §4.5 (excepto `CannotDeactivateSelf`).

---

### **4.7. Reset User Password**

Restablece la contraseña de un usuario. Se genera una contraseña temporal y se envía por email. El usuario debe cambiarla en el siguiente login.

```
POST /api/users/{id}/reset-password
```

**Response `200 OK`:** Sin cuerpo.

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `User.NotFound` | 404 | El usuario no existe |
| `User.CannotModifyHigherRole` | 403 | No tiene permisos sobre ese usuario |
| `User.PasswordResetFailed` | 500 | Error al restablecer la contraseña |

---

## **5. Módulo: AI (`/api/projects/{id}/ai`)**

### **5.1. Generate Section Text**

Genera texto para una sección del proyecto usando IA (Flux Gateway con OAuth2).

```
POST /api/projects/{id}/ai/generate
```

**Autenticación:** ActiveUser

**Path Parameters:**

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Guid | ID del proyecto |

**Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `sectionId` | string | ✅ | No vacío, max 200 chars |
| `prompt` | string | ✅ | No vacío, max 10.000 chars |
| `context` | string? | ❌ | Max 50.000 chars |

```json
{
  "sectionId": "MD.01",
  "prompt": "Describe los agentes intervinientes en el proyecto",
  "context": "Información adicional de contexto..."
}
```

**Response `200 OK`:**

```json
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sectionId": "MD.01",
  "generatedText": "<p>Los agentes intervinientes en el presente proyecto son...</p>"
}
```

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Project.NotFound` | 404 | Proyecto no encontrado |
| `Ai.GenerationFailed` | 500 | Error en la generación de texto |

**Nota:** El backend construye internamente un prompt enriquecido con metadatos del proyecto (título, tipo de intervención, LOE, dirección, normativa local, contenido existente de la sección).

---

## **6. Módulo: Export (`/api/projects/{id}/export`)**

### **6.1. Export to DOCX**

Exporta el proyecto completo como documento Word (.docx).

```
GET /api/projects/{id}/export
```

**Autenticación:** ActiveUser

**Path Parameters:**

| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | Guid | ID del proyecto |

**Response `200 OK`:**

```
Content-Type: application/vnd.openxmlformats-officedocument.wordprocessingml.document
Content-Disposition: attachment; filename="Memoria_Titulo_Proyecto.docx"
```

Cuerpo: Archivo binario DOCX.

**Errores:**

| Código | HTTP | Descripción |
|--------|------|-------------|
| `Project.NotFound` | 404 | Proyecto no encontrado |
| `Export.NoContent` | 400 | El proyecto no tiene contenido para exportar |

---

## **7. Health Checks**

Endpoints de monitorización (sin autenticación).

| Endpoint | Descripción |
|----------|-------------|
| `GET /health/live` | Liveness probe — verifica que la app está corriendo |
| `GET /health/ready` | Readiness probe — verifica conexión a PostgreSQL |

---

## **8. Arquitectura Interna**

### **8.1. Pipeline de Request**

```
HTTP Request
  → Controller (thin: mapea DTO → Command/Query)
    → MediatR Pipeline
      → ValidationBehavior (FluentValidation)
      → LoggingBehavior (Serilog + CorrelationId)
      → Handler (lógica de negocio)
    → Result<T> / Result
  → Controller (HandleResult / HandleError)
    → HTTP Response (ProblemDetails si error)
```

### **8.2. CQRS: Escritura vs Lectura**

| Operación | ORM | Patrón |
|-----------|-----|--------|
| **Escritura** (Commands) | Entity Framework Core | Repository → SaveChanges |
| **Lectura** (Queries) | Dapper (SQL raw) | IDbConnectionFactory → SQL optimizado |

### **8.3. Modelo de Datos "Memoria"**

El contenido del proyecto se almacena como **JSONB** en la columna `content_tree_json`, NO en tablas relacionales por capítulo:

```json
{
  "chapters": [
    {
      "id": "MD",
      "title": "Memoria Descriptiva",
      "content": null,
      "sections": [
        {
          "id": "MD.01",
          "title": "Agentes",
          "content": "<p>Texto generado...</p>",
          "sections": []
        }
      ]
    }
  ]
}
```

### **8.4. Refresh Token Rotation**

1. Login emite `accessToken` (JWT, 60 min) + `refreshToken` (64 bytes, 7 días).
2. `/api/auth/refresh` rota: revoca token actual → emite nuevo par.
3. **Stolen-token detection:** Si se reutiliza un token ya revocado, se revocan TODOS los tokens del usuario (protección contra replay attacks).

---

## **9. Resumen de DTOs**

### **Request DTOs**

| DTO | Módulo | Endpoint |
|-----|--------|----------|
| `LoginRequest` | Auth | POST /api/auth/login |
| `RefreshTokenRequest` | Auth | POST /api/auth/refresh |
| `RevokeTokenRequest` | Auth | POST /api/auth/revoke |
| `ChangePasswordRequest` | Auth | POST /api/auth/change-password |
| `UpdateProfileRequest` | Auth | PUT /api/auth/profile |
| `CreateProjectRequest` | Projects | POST /api/projects |
| `UpdateProjectTreeRequest` | Projects | PUT /api/projects/{id}/tree |
| `UpdateSectionRequest` | Projects | PATCH /api/projects/{id}/sections/{sectionId} |
| `GenerateTextRequest` | AI | POST /api/projects/{id}/ai/generate |
| `CreateUserRequest` | Users | POST /api/users |
| `UpdateUserRequest` | Users | PUT /api/users/{id} |

### **Response DTOs**

| DTO | Módulo | Descripción |
|-----|--------|-------------|
| `LoginResponse` | Auth | JWT + RefreshToken + UserInfo |
| `UpdateProfileResponse` | Auth | Perfil actualizado |
| `ProjectResponse` | Projects | Datos del proyecto (sin árbol) |
| `ContentTreeResponse` | Projects | Árbol de contenido JSONB |
| `PagedResponse<T>` | Common | Lista paginada genérica |
| `UserResponse` | Users | Datos del usuario con rol |
| `GeneratedTextResponse` | AI | Texto generado por IA |
| `ExportDocumentResponse` | Export | Archivo DOCX (binario) |

---

## **10. Enumeraciones**

### **InterventionType**

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | `NewConstruction` | Obra nueva |
| 1 | `Reform` | Reforma |
| 2 | `Extension` | Ampliación |

### **ProjectStatus**

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | `Draft` | Borrador |
| 1 | `InProgress` | En redacción |
| 2 | `Completed` | Completado |
| 3 | `Archived` | Archivado |

### **Roles**

| Rol | Descripción |
|-----|-------------|
| `Root` | Super administrador del sistema |
| `Admin` | Administrador de organización |
| `Architect` | Arquitecto redactor |
| `Collaborator` | Colaborador con acceso limitado |
