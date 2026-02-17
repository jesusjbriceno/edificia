# **🚀 Guía de Despliegue y Configuración de Entornos — EDIFICIA**

**Versión:** 2.0  
**Última actualización:** 2026-02-17  
**Enfoque:** Contenerización para Producción (Docker).

**Objetivo:** Publicar la solución en un servidor VPS o Cloud (Linux).

---

## **1. Estrategia de Configuración**

EDIFICIA sigue el principio de **"Configuración en el Entorno"**.

- **Código:** El mismo código viaja desde Desarrollo a Producción.
- **Configuración:** Cambia según dónde se ejecute mediante **Variables de Entorno**.

### **Jerarquía de Carga (.NET)**

El Backend lee la configuración en este orden (el último gana):

1. `appsettings.json` (Base — estructura y valores por defecto).
2. `appsettings.Production.json` (Overrides de producción — niveles de log).
3. **Variables de Entorno del Sistema** (Docker). **← Aquí inyectamos secretos.**

---

## **2. Configuración del Backend (.NET API)**

### **2.1. Archivo `appsettings.json` (Base)**

Define la **estructura completa** de configuración. No contiene secretos reales en el repositorio.

> **Nota:** Las secciones de configuración (`Jwt`, `Email`, `Security`, `FluxGateway`)
> están mapeadas a clases C# con `SectionName` constante en Infrastructure.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=edificia;Username=edificia;Password=edificia_dev"
  },
  "Cors": {
    "AllowedOrigins": ["https://edificia.jesusjbriceno.dev"]
  },
  "FluxGateway": {
    "AuthUrl": "https://dashboard-flux.jesusjbriceno.dev/api/v1/auth/login",
    "ChatUrl": "https://dashboard-flux.jesusjbriceno.dev/api/v1/chat/completions",
    "ClientId": "",
    "ClientSecret": "",
    "Model": null
  },
  "Security": {
    "RootEmail": "admin@edificia.dev",
    "RootInitialPassword": "ChangeMe123!"
  },
  "Jwt": {
    "SecretKey": "REPLACE_WITH_A_SECURE_SECRET_KEY_OF_AT_LEAST_32_CHARS!",
    "Issuer": "https://api-edificia.jesusjbriceno.dev",
    "Audience": "https://edificia.jesusjbriceno.dev",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Email": {
    "Provider": "Smtp",
    "FromAddress": "noreply@edificia.dev",
    "FromName": "EDIFICIA",
    "SmtpHost": "localhost",
    "SmtpPort": 1025,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "SmtpUseSsl": false,
    "BrevoApiKey": "",
    "BrevoApiUrl": "https://api.brevo.com/v3"
  }
}
```

### **2.2. Mapeo Sección → Clase C#**

| Sección JSON      | Clase C#              | Proyecto                 |
|--------------------|-----------------------|--------------------------|
| `Jwt`              | `JwtSettings`         | Infrastructure.Identity  |
| `Security`         | `SecuritySettings`    | Infrastructure.Identity  |
| `Email`            | `EmailSettings`       | Infrastructure.Email     |
| `FluxGateway`      | `FluxGatewaySettings` | Infrastructure.Ai        |
| `Cors`             | (lectura directa)     | API.Configuration        |
| `ConnectionStrings`| (lectura directa)     | Infrastructure           |

### **2.3. Archivo `appsettings.Production.json`**

Solo sobrescribe niveles de log para producción. Los secretos se inyectan vía variables de entorno.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Edificia": "Information"
    }
  }
}
```

### **2.4. Dockerfile de Producción (`apps/api/Dockerfile`)**

Multi-stage build con .NET 10 sobre Alpine para imagen mínima (~100 MB).

```dockerfile
# --- ETAPA 1: COMPILACIÓN ---
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY src/Edificia.Domain/*.csproj            ./Edificia.Domain/
COPY src/Edificia.Shared/*.csproj            ./Edificia.Shared/
COPY src/Edificia.Application/*.csproj       ./Edificia.Application/
COPY src/Edificia.Infrastructure/*.csproj    ./Edificia.Infrastructure/
COPY src/Edificia.API/*.csproj               ./Edificia.API/

RUN dotnet restore ./Edificia.API/Edificia.API.csproj

COPY src/ ./
WORKDIR /src/Edificia.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- ETAPA 2: RUNTIME ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

USER app
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Edificia.API.dll"]
```

**Puntos clave:**
- **`Edificia.Shared`** se incluye en la capa de restore (faltaba en la versión anterior).
- **`USER app`** ejecuta el proceso como usuario no-root.
- **`HEALTHCHECK`** usa el endpoint `/health/live` ya definido en `Program.cs`.
- **Alpine** reduce el tamaño de la imagen final.

---

## **3. Configuración del Frontend (Astro + React)**

### **3.1. Variables de Entorno (`.env`)**

En Astro, las variables que necesita el cliente deben llevar el prefijo `PUBLIC_`.

```env
# URL pública de la API
PUBLIC_API_URL=https://api-edificia.jesusjbriceno.dev

NODE_ENV=production
```

### **3.2. Dockerfile de Producción (`apps/web/Dockerfile`)**

Multi-stage build para SSR con el adaptador Node.js de Astro.

```dockerfile
# --- ETAPA 1: DEPENDENCIAS ---
FROM node:20-alpine AS deps
WORKDIR /app
COPY package*.json ./
RUN npm ci --omit=dev

# --- ETAPA 2: BUILD ---
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

# --- ETAPA 3: RUNTIME ---
FROM node:20-alpine AS runtime
WORKDIR /app

COPY --from=build /app/dist ./dist
COPY --from=deps  /app/node_modules ./node_modules
COPY --from=build /app/package.json ./package.json

RUN addgroup -S astro && adduser -S astro -G astro
USER astro

ENV HOST=0.0.0.0
ENV PORT=4321
EXPOSE 4321

CMD ["node", "./dist/server/entry.mjs"]
```

> **Prerrequisito:** Instalar el adaptador de Node:
> ```bash
> npm install @astrojs/node
> ```
> Y configurarlo en `astro.config.mjs`:
> ```js
> import node from '@astrojs/node';
> export default defineConfig({
>   output: 'server',
>   adapter: node({ mode: 'standalone' }),
> });
> ```

---

## **4. Orquestación**

### **4.1. Desarrollo Local (`docker-compose.yml`)**

Levanta solo la infraestructura (PostgreSQL, Redis, MailHog). La API y el frontend se ejecutan fuera de Docker para hot-reload.

```bash
docker compose up -d
```

| Servicio   | Puerto               | Uso             |
|------------|----------------------|-----------------|
| PostgreSQL | 5432                 | BD principal    |
| Redis      | 6379                 | Caché           |
| MailHog    | 1025 (SMTP) / 8025 (Web UI) | Captura de emails |

### **4.2. Producción — Stack Completo (`docker-compose.prod.yml`)**

Despliega **todo** (BD + Redis + API + Web) en Docker. Ideal para VPS nuevos o entornos aislados.

```bash
cp .env.example .env
nano .env

docker compose -f docker-compose.prod.yml --env-file .env up -d
```

### **4.3. Producción — Solo Apps (`docker-compose.apps.yml`)**

Despliega **solo API + Web**. Usa cuando PostgreSQL y Redis ya existen en el servidor (o son servicios gestionados).

```bash
cp .env.example .env
nano .env  # Rellenar DB_HOST, DB_PORT, REDIS_HOST, REDIS_PORT con los valores reales

docker compose -f docker-compose.apps.yml --env-file .env up -d
```

> **Variables adicionales para BD/Redis externos:**
>
> | Variable     | Descripción          | Ejemplo            |
> |--------------|----------------------|--------------------|
> | `DB_HOST`    | Host de PostgreSQL   | `192.168.1.50`     |
> | `DB_PORT`    | Puerto de PostgreSQL | `5432`             |
> | `DB_NAME`    | Nombre de la BD      | `edificia_db`      |
> | `REDIS_HOST` | Host de Redis        | `192.168.1.50`     |
> | `REDIS_PORT` | Puerto de Redis      | `6379`             |

#### **Variables de Entorno requeridas en `.env`:**

| Variable           | Descripción                            | Ejemplo                     |
|--------------------|----------------------------------------|-----------------------------|
| `DB_USER`          | Usuario PostgreSQL                     | `edificia`                  |
| `DB_PASSWORD`      | Contraseña PostgreSQL                  | (generada)                  |
| `REDIS_PASSWORD`   | Contraseña Redis                       | (generada)                  |
| `JWT_SECRET`       | Clave JWT (mín. 32 chars)              | `openssl rand -base64 64`   |
| `FLUX_CLIENT_ID`   | Client ID de Flux Gateway              | —                           |
| `FLUX_CLIENT_SECRET`| Client Secret de Flux Gateway         | —                           |
| `EMAIL_PROVIDER`   | `Smtp` o `Brevo`                       | `Smtp`                      |
| `BREVO_API_KEY`    | API Key de Brevo (si Provider=Brevo)   | —                           |
| `SMTP_HOST`        | Servidor SMTP                          | `smtp.example.com`          |
| `SMTP_PORT`        | Puerto SMTP                            | `587`                       |
| `SMTP_USERNAME`    | Usuario SMTP                           | —                           |
| `SMTP_PASSWORD`    | Contraseña SMTP                        | —                           |
| `SMTP_USE_SSL`     | TLS/SSL para SMTP                      | `true`                      |
| `ROOT_EMAIL`       | Email del admin root (seed)            | `admin@edificia.dev`        |
| `ROOT_PASSWORD`    | Contraseña inicial root                | (generada)                  |

#### **Mapeo de variables → appsettings (.NET)**

Docker traduce `__` (doble guión bajo) a `:` para la jerarquía de configuración:

| Variable de Entorno                      | Sección en appsettings              |
|------------------------------------------|-------------------------------------|
| `ConnectionStrings__DefaultConnection`   | `ConnectionStrings:DefaultConnection` |
| `Jwt__SecretKey`                         | `Jwt:SecretKey`                     |
| `Jwt__Issuer`                            | `Jwt:Issuer`                        |
| `Jwt__Audience`                          | `Jwt:Audience`                      |
| `FluxGateway__ClientId`                  | `FluxGateway:ClientId`              |
| `FluxGateway__ClientSecret`              | `FluxGateway:ClientSecret`          |
| `Email__Provider`                        | `Email:Provider`                    |
| `Email__BrevoApiKey`                     | `Email:BrevoApiKey`                 |
| `Email__SmtpHost`                        | `Email:SmtpHost`                    |
| `Email__SmtpPort`                        | `Email:SmtpPort`                    |
| `Email__SmtpUsername`                     | `Email:SmtpUsername`                |
| `Email__SmtpPassword`                    | `Email:SmtpPassword`               |
| `Email__SmtpUseSsl`                      | `Email:SmtpUseSsl`                  |
| `Security__RootEmail`                    | `Security:RootEmail`                |
| `Security__RootInitialPassword`          | `Security:RootInitialPassword`      |
| `Cors__AllowedOrigins__0`                | `Cors:AllowedOrigins[0]`            |

---

## **5. Proxy Inverso (Nginx / Caddy)**

### **5.1. Ejemplo con Caddy (Recomendado — SSL automático)**

```caddyfile
api-edificia.jesusjbriceno.dev {
    reverse_proxy localhost:5000
}

edificia.jesusjbriceno.dev {
    reverse_proxy localhost:4321
}
```

### **5.2. Ejemplo con Nginx**

```nginx
server {
    listen 443 ssl http2;
    server_name api-edificia.jesusjbriceno.dev;

    ssl_certificate     /etc/letsencrypt/live/api-edificia.jesusjbriceno.dev/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api-edificia.jesusjbriceno.dev/privkey.pem;

    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 443 ssl http2;
    server_name edificia.jesusjbriceno.dev;

    ssl_certificate     /etc/letsencrypt/live/edificia.jesusjbriceno.dev/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/edificia.jesusjbriceno.dev/privkey.pem;

    location / {
        proxy_pass http://localhost:4321;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## **6. Checklist de Pase a Producción**

### **6.1. Antes del Despliegue**

- [ ] **Archivo `.env` creado** a partir de `.env.example` con valores reales.
- [ ] **`DB_PASSWORD`** — Contraseña compleja generada.
- [ ] **`JWT_SECRET`** — Mínimo 32 caracteres aleatorios (`openssl rand -base64 64`).
- [ ] **`FLUX_CLIENT_SECRET`** — Credenciales de la pasarela IA.
- [ ] **`ROOT_PASSWORD`** — Contraseña fuerte para el admin inicial.
- [ ] **`REDIS_PASSWORD`** — Contraseña para Redis.

### **6.2. Base de Datos**

- [ ] Las migraciones se aplican al arranque si están configuradas, o manualmente:
  ```bash
  docker compose -f docker-compose.prod.yml exec api \
      dotnet ef database update --project /app
  ```

### **6.3. SSL / Proxy Inverso**

- [ ] Certificados SSL configurados (Let's Encrypt / Caddy automático).
- [ ] `api-edificia.jesusjbriceno.dev` → `localhost:5000`.
- [ ] `edificia.jesusjbriceno.dev` → `localhost:4321`.

### **6.4. CORS**

- [ ] `Cors__AllowedOrigins__0` coincide exactamente con el dominio del frontend (sin `/` al final).

### **6.5. Verificación Post-Despliegue**

```bash
# Health check del API
curl -f https://api-edificia.jesusjbriceno.dev/health/live

# Health check (readiness - incluye BD)
curl -f https://api-edificia.jesusjbriceno.dev/health/ready

# Swagger (solo disponible en Development — no expuesto en producción)
# curl https://api-edificia.jesusjbriceno.dev/swagger
```

---

## **7. Comandos Útiles**

```bash
# Construir imágenes
docker compose -f docker-compose.prod.yml build

# Levantar en background
docker compose -f docker-compose.prod.yml --env-file .env up -d

# Ver logs del API
docker compose -f docker-compose.prod.yml logs -f api

# Reiniciar un servicio
docker compose -f docker-compose.prod.yml restart api

# Parar todo
docker compose -f docker-compose.prod.yml down

# Parar y eliminar volúmenes (⚠️ BORRA DATOS)
docker compose -f docker-compose.prod.yml down -v
```