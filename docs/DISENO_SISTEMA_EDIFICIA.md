# **Diseño de Sistema (SDD) \- EDIFICIA**

**Versión:** 2.0

## **1\. Arquitectura Lógica**

**Namespace Base:** Edificia.\*

graph TD  
    UI\[Frontend: Astro/React\] \--\>|JSON| API\[Edificia.API\]  
    API \--\>|Ef Core| DB\[(PostgreSQL)\]  
    API \--\>|OAuth2 \+ JSON| Flux\[Flux Gateway\]  
    Flux \-.-\>|Proxy| Models\[LLM Providers\]

## **2\. Configuración de Infraestructura (.NET)**

El appsettings.json gestionará la identidad de la aplicación frente a la pasarela.

"FluxGateway": {  
  "AuthUrl": "\[https://dashboard-flux.jesusjbriceno.dev/api/v1/auth/login\](https://dashboard-flux.jesusjbriceno.dev/api/v1/auth/login)",  
  "ChatUrl": "\[https://dashboard-flux.jesusjbriceno.dev/api/v1/chat/completions\](https://dashboard-flux.jesusjbriceno.dev/api/v1/chat/completions)",  
  "ClientId": "EDIFICIA\_PROD\_01",  
  "ClientSecret": "\[SECRET\_VAULT\]"  
}

## **3\. Servicio de Infraestructura (FluxAiService)**

Este servicio en Edificia.Infrastructure debe implementar:

1. **Token Management:** Comprobar si existe token válido en caché. Si no, hacer Login.  
2. **Resilience:** Usar Polly para reintentos si el Gateway devuelve 503 o 429\.  
3. **DTO Mapping:** Adaptar el formato de respuesta de Flux al formato interno de Edificia.

## **4\. Modelo de Datos (Core)**

La entidad Project es el corazón del sistema.

public class Project {  
    public Guid Id { get; set; }  
    public string Title { get; set; }  
    // Estrategia  
    public InterventionType Intervention { get; set; } // Obra Nueva, Reforma...  
    public bool LoeExemption { get; set; } // Art 2.2  
    // Datos  
    public string ContentTreeJson { get; set; } // JSONB  
}  
