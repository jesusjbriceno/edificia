# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias (Capas separadas para caché)
COPY src/Edificia.Domain/*.csproj ./Edificia.Domain/
COPY src/Edificia.Application/*.csproj ./Edificia.Application/
COPY src/Edificia.Infrastructure/*.csproj ./Edificia.Infrastructure/
COPY src/Edificia.API/*.csproj ./Edificia.API/

RUN dotnet restore ./Edificia.API/Edificia.API.csproj

# Copiar todo el código y compilar
COPY src/ ./
WORKDIR /src/Edificia.API
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Edificia.API.dll"]