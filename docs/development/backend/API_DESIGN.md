# **📡 Diseño de API REST y Estrategia de DTOs \- EDIFICIA**

**Versión:** 2.2 (Soporte Prompt Engine & Technical Context)

## **2\. Catálogo de Endpoints (Actualizado)**

### **🏗️ Módulo: Proyectos (Projects)**

Se amplía el modelo de datos para soportar el Motor de Prompts.

#### **Modelo de Entidad Ampliado (Project)**

public class Project   
{  
    public Guid Id { get; set; }  
    public string Title { get; set; }  
      
    // \--- Estrategia \---  
    public InterventionType InterventionType { get; set; }  
    public bool IsLoeRequired { get; set; }  
      
    // \--- Contexto Técnico (Input para IA) \---  
    // Almacena las respuestas de los formularios técnicos (ej: "Cimentación: Zapatas")  
    // Se usa para inyectar datos en el Prompt.  
    public string TechnicalContextJson { get; set; } \= "{}";   
      
    // \--- Contexto Normativo (Input para IA) \---  
    public string MunicipalRegulation { get; set; }  
      
    // \--- Contenido Generado (Output) \---  
    public string ContentTreeJson { get; set; }  
}

#### **DTOs para Prompt Engine**

Cuando el frontend solicita generar texto, puede enviar datos frescos del formulario actual, que se fusionan con el contexto guardado.

**POST /api/v1/projects/{id}/ai/generate**

**Request (GenerateTextRequest):**

public record GenerateTextRequest(  
    string SectionCode,       // "MD.2.1"  
    JsonElement CurrentFormData, // Datos del formulario que el usuario acaba de tocar  
    string? UserInstructions  // "Hazlo más técnico"  
);

**Lógica del Backend (Prompt Engine):**

1. Recupera el Project de BD.  
2. Recupera el User (Autor) para obtener Nombre y Nº Colegiado (Contexto de Autoría).  
3. Fusiona: Project.TechnicalContextJson \+ CurrentFormData.  
4. Construye el Prompt:"Actúa como el Arq. {User.Name}. Redacta el apartado {SectionCode} para una obra de tipo {InterventionType}.  
   Datos técnicos: {MergedData}.  
   Normativa Local: {Project.MunicipalRegulation}."

### **🔐 Módulo: Auth (Actualizado)**

Añadido el endpoint para el cambio de contraseña obligatorio.

| Verbo | Ruta | Request DTO | Response | Descripción |
| :---- | :---- | :---- | :---- | :---- |
| POST | /api/v1/auth/change-password | ChangePasswordRequest | Unit | Permite cambiar la clave si se tiene el token temporal (incluso si está restringido). |

public record ChangePasswordRequest(  
    string CurrentPassword,  
    string NewPassword  
);

## **4\. Próximos Pasos (Feature 5.2)**

1. **Migración EF:** Añadir columna technical\_context\_json a la tabla projects.  
2. **Extensión Identity:** Añadir columna must\_change\_password a asp\_net\_users.  
3. **Prompt Builder:** Implementar la clase PromptBuilderService en Application que orqueste la fusión de estos datos antes de llamar a Flux.