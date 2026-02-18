# ============================================================
# EDIFICIA Web — Multi-stage Dockerfile (Production)
# Target: Astro 4 SSR (Node adapter)
# ============================================================

# --- ETAPA 1: DEPENDENCIAS ---
FROM node:20-alpine AS deps
WORKDIR /app
COPY package*.json ./
# --force: skip EBADPLATFORM on optional win32 binaries in lockfile (generated on Windows)
RUN npm ci --omit=dev --force

# --- ETAPA 2: BUILD ---
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
# --force: skip EBADPLATFORM on optional win32 binaries in lockfile (generated on Windows)
RUN npm ci --force
COPY . .
RUN npm run build

# --- ETAPA 3: RUNTIME ---
FROM node:20-alpine AS runtime
WORKDIR /app

# Copiar solo lo necesario para ejecutar el servidor SSR
COPY --from=build /app/dist ./dist
COPY --from=deps  /app/node_modules ./node_modules
COPY --from=build /app/package.json ./package.json

# Usuario no-root
RUN addgroup -S astro && adduser -S astro -G astro
USER astro

ENV HOST=0.0.0.0
ENV PORT=4321
EXPOSE 4321

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:4321/ || exit 1

CMD ["node", "./dist/server/entry.mjs"]