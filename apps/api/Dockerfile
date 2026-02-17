# ============================================================
# EDIFICIA API — Multi-stage Dockerfile (Production)
# Target: .NET 10 | Architecture: Clean Architecture
# ============================================================

# --- ETAPA 1: COMPILACIÓN ---
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copiar csproj y restaurar (capa de caché de NuGet)
COPY src/Edificia.Domain/*.csproj            ./Edificia.Domain/
COPY src/Edificia.Shared/*.csproj            ./Edificia.Shared/
COPY src/Edificia.Application/*.csproj       ./Edificia.Application/
COPY src/Edificia.Infrastructure/*.csproj    ./Edificia.Infrastructure/
COPY src/Edificia.API/*.csproj               ./Edificia.API/

RUN dotnet restore ./Edificia.API/Edificia.API.csproj

# Copiar el código fuente y compilar
COPY src/ ./
WORKDIR /src/Edificia.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- ETAPA 2: RUNTIME (Imagen ligera) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# Metadatos OCI
LABEL org.opencontainers.image.title="EDIFICIA API" \
      org.opencontainers.image.description="API para memorias de arquitectura (CTE/LOE)" \
      org.opencontainers.image.source="https://github.com/jesusjbriceno/edificia" \
      org.opencontainers.image.licenses="Apache-2.0"

WORKDIR /app

# Instalar icu-libs para globalización (necesario en alpine)
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

# Usuario no-root por seguridad
USER app

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Edificia.API.dll"]