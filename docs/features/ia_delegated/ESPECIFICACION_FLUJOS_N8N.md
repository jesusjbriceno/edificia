# **🧠 Especificación de Flujos n8n \- EDIFICIA**

Para que EDIFICIA funcione, n8n debe exponer un **Webhook POST** que cumpla estrictamente este contrato.

## **1\. Seguridad (Autenticación)**

El nodo **Webhook** en n8n debe validar la cabecera:

* **Header Name:** x-edificia-auth  
* **Value:** Debe coincidir con la variable AI\_\_ApiSecret del Backend.

*Si no coincide, el flujo debe responder 403 Forbidden inmediatamente.*

## **2\. Estructura de Entrada (Body)**

El nodo Webhook recibirá este JSON:

{  
  "sectionCode": "MD.2.1.Cimentacion",  
  "projectType": "NewConstruction",  
  "municipalRegulation": "PGOU Madrid 1997",  
  "technicalContext": {  
    "sistema": "Zapatas Aisladas",  
    "material": "Hormigón HA-25",  
    "terreno": "Arcillas Expansivas"  
  },  
  "userInstructions": "Haz énfasis en la impermeabilización."  
}

## **3\. Lógica del Flujo (Sugerida)**

### **Opción A: Conexión Directa a Modelo (Gemini/OpenAI)**

1. **Webhook:** Recibe datos.  
2. **Prompt Construction (Code Node):**  
   * Crea un System Prompt: *"Eres un arquitecto experto en CTE España..."*  
   * Crea un User Prompt combinando technicalContext y userInstructions.  
3. **AI Node (Google Gemini / OpenAI):** Envía el prompt.  
4. **Output Cleaning:** Elimina comillas markdown (\`\`\`json ...) si el modelo es verboso.  
5. **Response:** Devuelve el JSON final.

### **Opción B: Integración vía Flux Gateway**

1. **Webhook:** Recibe datos.  
2. **HTTP Request (Auth):** POST https://flux.../api/v1/auth/app con `{ clientId, clientSecret }` (credenciales guardadas en n8n Credentials).  
3. **HTTP Request (Chat):** POST https://flux.../api/v1/chat con el Bearer Token obtenido en el paso anterior.  
4. **Response:** Devuelve el JSON final.

## **4\. Estructura de Salida (Response)**

El último nodo del flujo (**Respond to Webhook**) debe devolver **siempre** un JSON plano. No devolver texto crudo.

{  
  "generatedText": "TEXTO\_FINAL\_EN\_HTML\_O\_MARKDOWN",  
  "usage": {  
    "model": "gemini-1.5-pro",  
    "tokens": 450  
  }  
}

**Nota Importante:** El campo generatedText es el único obligatorio que el Backend leerá.