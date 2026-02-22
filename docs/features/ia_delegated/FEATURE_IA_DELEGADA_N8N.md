# **🚀 Feature: Migración a IA Delegada (n8n)**

**Fecha:** 17/02/2026

**Estado:** En Progreso

**Impacto:** Arquitectura de Backend, Configuración de Infraestructura.

## **1\. Contexto y Motivación**

En la arquitectura original (v1.0 \- v2.1), **EdificIA** se integraba directamente con proveedores de IA (Google Gemini) o pasarelas intermedias (Flux Gateway) mediante código C\# en el Backend (Edificia.Infrastructure).

### **Problema Detectado**

Esta aproximación genera un **acoplamiento fuerte**.

1. **Rigidez:** Cambiar de proveedor (ej: de Gemini a OpenAI) requiere recompilar y redesplegar la API.  
2. **Complejidad:** La lógica de "Pre-procesamiento" (buscar normativa actualizada, limpiar el prompt) ensucia el código de negocio.  
3. **Mantenimiento:** Gestionar tokens de autenticación (email/password login) y reintentos dentro de la API consume recursos de desarrollo.

### **Solución Propuesta (v2.2)**

**Delegar la inteligencia en n8n.**

El Backend de EdificIA se vuelve "agnóstico". Solo sabe enviar un contexto técnico a un Webhook y esperar un texto. La decisión de qué modelo usar, cómo autenticarse o si hacer búsquedas previas, recae en el flujo visual de n8n.

## **2\. Alcance del Cambio**

### **❌ Lo que eliminamos**

* Dependencia de Flux Gateway en el Backend (`FluxAiService`, DTOs, Settings).  
* Gestión de Tokens (email/password login) y caché de credenciales de IA.  
* Construcción de prompts en el Backend (`PromptTemplateService`) — se delega a n8n.  
* Configuraciones complejas de proveedores en appsettings.json (`FluxGateway` section).

### **✅ Lo que implementamos**

* **Cliente HTTP Ligero:** Un único servicio N8nAiService que hace POST a un Webhook.  
* **Seguridad Simple:** Autenticación mediante Header X-Edificia-Auth compartido.  
* **Normalización:** Contrato estricto de JSON (Input/Output) que n8n debe respetar.

## **3\. Beneficios**

1. **Hot-Swap:** Podemos cambiar de Gemini 1.5 a GPT-5 en n8n sin tocar el servidor de EdificIA.  
2. **Lógica Híbrida:** n8n permite flujos como *"Si es Reforma \-\> Usa Modelo A; Si es Obra Nueva \-\> Usa Modelo B"*.  
3. **Observabilidad:** n8n ofrece historial visual de ejecuciones para depurar prompts fallidos.