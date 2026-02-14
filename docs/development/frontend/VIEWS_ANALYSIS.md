# **🗺️ Análisis de Vistas y Navegación \- EDIFICIA**

**Versión:** 1.0

**Enfoque:** UX para SaaS de alta productividad.

**Stack:** Astro (Routing/Layouts) \+ React (Islas Interactivas).

## **1\. Mapa de Navegación (Sitemap)**

La aplicación se divide en tres zonas de contexto claramente diferenciadas por su Layout.

graph TD  
    root\[/\] \--\>|Redirección| Auth  
      
    subgraph "Public Zone (AuthLayout)"  
        Auth\[Login / Registro\]  
    end

    Auth \--\>|Success| Dash  
      
    subgraph "Management Zone (DashboardLayout)"  
        Dash\[Dashboard de Proyectos\]  
        Profile\[Perfil de Usuario\]  
        NewProj\[Wizard Nuevo Proyecto\]  
    end

    Dash \--\>|Select Project| Editor  
    NewProj \--\>|Create| Editor

    subgraph "Immersive Zone (EditorLayout)"  
        Editor\[Editor de Memoria\]  
        Editor \--\>|Sub-view| Normativa\[Navegador Normativo\]  
        Editor \--\>|Sub-view| Write\[Área de Redacción\]  
        Editor \--\>|Action| Export\[Exportar DOCX\]  
    end

## **2\. Definición de Layouts (Astro)**

Astro gestionará el marco estático para asegurar un FCP (First Contentful Paint) inmediato.

### **2.1. AuthLayout.astro**

* **Propósito:** Centrar la atención en el formulario de entrada.  
* **Elementos:**  
  * Fondo neutro/corporativo simple.  
  * Card central flotante.  
  * Sin navegación global.

### **2.2. DashboardLayout.astro**

* **Propósito:** Gestión administrativa y listados.  
* **Elementos:**  
  * **Header Global:** Logo EDIFICIA, Avatar Usuario, Breadcrumbs.  
  * **Container:** Ancho limitado (max-w-7xl), centrado.  
  * **Footer:** Enlaces legales y versión.

### **2.3. EditorLayout.astro (El Reto UX)**

* **Propósito:** Maximizar el espacio de escritura. Eliminar distracciones.  
* **Elementos:**  
  * **Full Screen:** Ocupa el 100vh y 100vw.  
  * **Sidebar Izquierdo (React):** Árbol de navegación normativa (Colapsable).  
  * **Top Bar (React):** Título del proyecto, Estado de guardado (Saved/Saving...), Botón Exportar.  
  * **Main Area:** Scroll independiente para el contenido.

## **3\. Catálogo de Vistas (Views)**

### **🟢 V-01: Login (/)**

* **Componentes:** LoginForm (React \+ Zod).  
* **Comportamiento:**  
  * Validación de credenciales contra API.  
  * Si hay token válido en localStorage/cookie, redirige auto a /dashboard.

### **🟢 V-02: Dashboard (/dashboard)**

* **Propósito:** Listado de expedientes recientes.  
* **Componentes:**  
  * ProjectStats: Resumen rápido (ej: "3 Proyectos en curso").  
  * ProjectGrid: Grid de tarjetas con los proyectos.  
    * *Card:* Título, Fecha, Badge de Tipo (Nueva/Reforma), Barra de progreso.  
  * NewProjectButton: CTA (Call to Action) flotante o destacado.

### **🟡 V-03: Wizard de Estrategia (Modal o /projects/new)**

* **Importancia Crítica:** Aquí se define la lógica de negocio del cliente.  
* **Pasos del Wizard (React State):**  
  1. **Datos Básicos:** Título, Emplazamiento (Input texto).  
  2. **Estrategia (El Filtro):**  
     * Selección Visual: \[🏢 Obra Nueva\] vs \[🔨 Reforma/Rehab\].  
     * Check: "¿Aplica LOE Art. 4?" (Con tooltip explicativo del Art 2.2).  
  3. **Contexto Local:** Input para "Referencia Ordenanza Municipal".  
* **Resultado:** Al finalizar, llama a POST /projects y redirige a V-04.

### **🔴 V-04: El Editor (/editor/\[id\])**

* **Arquitectura de UI:** Panel dividido (Split View).  
* **Zona A: Navegador Normativo (Izquierda \- 20%)**  
  * Árbol JSON renderizado recursivamente.  
  * Indicadores visuales: ✅ Completado, ⚠️ Pendiente, 🔒 No aplica (Filtrado).  
  * *Funcionalidad:* Al hacer clic, carga el contenido en la Zona B.  
* **Zona B: Espacio de Trabajo (Derecha \- 80%)**  
  * **Header de Sección:** Título del apartado (ej: "MD.1 Memoria Descriptiva").  
  * **Formulario de Datos:** (Si el apartado requiere inputs estructurados).  
  * **Editor de Texto (TipTap):**  
    * Barra de herramientas flotante (Bold, H1, H2).  
    * **Botón Mágico IA:** "✨ Redactar con EDIFICIA".  
    * *Comportamiento IA:* Abre un modal/popover, muestra "Generando...", inserta texto.

## **4\. Requerimientos de Componentes UI (Design System)**

Para mantener la coherencia con Tailwind v4, definiremos estos componentes base en src/components/ui.

### **Inputs & Forms**

* InputText, TextArea: Con soporte de error y label integrado.  
* SelectBox: Para elegir tipologías.  
* CheckboxCard: Tarjetas seleccionables grandes (para elegir Obra Nueva vs Reforma).

### **Feedback**

* Badge: Pill de colores para estados (Verde=Visado, Gris=Borrador).  
* Spinner: Indicador de carga para llamadas a IA.  
* Toast: Notificación flotante ("Guardado correctamente", "Error de conexión").

### **Layout**

* Sidebar: Contenedor colapsable con transición suave.  
* Modal: Ventana emergente para el Wizard y confirmaciones críticas.

## **5\. Estrategia de Rutas (Astro Pages)**

| Ruta URL | Archivo Astro | Layout | Acceso |
| :---- | :---- | :---- | :---- |
| / | pages/index.astro | AuthLayout | Público |
| /dashboard | pages/dashboard.astro | DashboardLayout | Privado (Auth Guard) |
| /projects/new | pages/projects/new.astro | DashboardLayout | Privado |
| /editor/\[id\] | pages/editor/\[id\].astro | EditorLayout | Privado |
| /404 | pages/404.astro | AuthLayout | Público |

## **6\. Próximos Pasos (Implementación)**

1. Crear los componentes base (Button, Input, Card) con Tailwind v4.  
2. Implementar los 3 Layouts en Astro.  
3. Maquetar la vista Dashboard con datos *mock*.  
4. Construir la vista Editor (la más compleja) integrando TipTap.