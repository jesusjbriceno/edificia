# **Análisis Detallado \- EDIFICIA**

**Versión:** 2.0

**Foco:** Integración Flux Gateway y Lógica de Intervención

## **2\. Análisis Técnico: La Pasarela de IA (Flux Gateway)**

**Requisito:** RF-IA-01

* **Cambio de Paradigma:** No nos conectamos a "Google Gemini". Nos conectamos a "Edificia AI Service" (que por debajo es Flux).  
* **Flujo de Autenticación (OAuth2):**  
  1. El Backend (.NET) solicita token a dashboard-flux.jesusjbriceno.dev/auth/login enviando client\_id y client\_secret.  
  2. Recibe un access\_token (JWT).  
  3. El Backend cachea este token (IMemoryCache) hasta su expiración.  
  4. El Backend llama al endpoint de chat inyectando el header Authorization: Bearer {token}.  
* **Ventaja Arquitectónica:** Si mañana Flux cambia Gemini por GPT-4o Mini o Claude Haiku, **no tocamos ni una línea de código**, solo configuración.

## **3\. Análisis Funcional: El "Filtro de Obra"**

Para cumplir con la visión del cliente ("No pedir hormigón en una reforma de baño"):

* **Lógica del Árbol JSON:**  
  * El frontend (React) recibe el árbol completo del CTE.  
  * Aplica una función recursiva filterTree(nodes, projectConfig).  
  * Si Project.InterventionType \== Reform Y el nodo tiene requiresNewWork: true, el nodo se elimina de la UI.