# **Especificación de Requisitos de Software (ERS) \- Proyecto EDIFICIA**

**Versión:** 2.0 (Rebranding \+ Flux Gateway)

**Fecha:** 03/02/2026

**Autor:** Arquitectura de Software EDIFICIA

**Estado:** Aprobado para Desarrollo

## **1\. Introducción y Propósito**

**EDIFICIA** es una plataforma SaaS diseñada para la redacción automatizada y asistida por IA de Memorias de Proyecto de Ejecución en España.

El objetivo es proporcionar una herramienta robusta que discrimine entre **Obra Nueva** y **Rehabilitación**, adaptando la estructura normativa (CTE/LOE) y utilizando una pasarela de IA soberana para la generación de contenidos.

## **3\. Requisitos Funcionales (RF)**

### **3.2. Gestión Estratégica del Proyecto**

| ID | Requisito | Prioridad |
| :---- | :---- | :---- |
| **RF-PROJ-01** | **Estrategia de Intervención:** El sistema debe clasificar el proyecto (Nueva/Reforma) y el ámbito LOE (Art 4\. vs Exención Art 2.2). | **MUST** |
| **RF-PROJ-02** | **Contexto Local:** Capacidad de definir normativa municipal específica (Ordenanzas) que alimentará el contexto de la IA. | **MUST** |

### **3.5. Inteligencia Artificial y Redacción**

| ID | Requisito | Prioridad |
| :---- | :---- | :---- |
| **RF-IA-01** | **Pasarela Unificada:** El sistema debe conectarse a través de **Flux Gateway** (OAuth2) para gestionar la inferencia, permitiendo la rotación transparente de modelos subyacentes. | **MUST** |
| **RF-IA-02** | **Prompt Contextual:** La generación de texto debe considerar la tipología de obra (ej: usar verbos de "sustitución" en reformas). | **MUST** |

## **4\. Requisitos No Funcionales (RNF)**

| ID | Requisito | Contexto |
| :---- | :---- | :---- |
| **RNF-SEC-03** | **Seguridad IA:** La autenticación con la IA debe usar **Client Credentials Flow** (ClientId/Secret) y no exponer claves en el cliente. | Seguridad |
| **RNF-ARCH-01** | **Clean Architecture:** Namespaces base Edificia.Domain, Edificia.Application, etc. | Mantenibilidad |

