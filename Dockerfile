# Dockerfile para CeibaFunds API
# Multi-stage build para optimizar tamaño y seguridad

# ============================================
# Build Stage - Compilación y tests
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar archivos de proyecto para restaurar dependencias
COPY *.sln ./
COPY src/CeibaFunds.API/*.csproj ./src/CeibaFunds.API/
COPY src/CeibaFunds.Application/*.csproj ./src/CeibaFunds.Application/
COPY src/CeibaFunds.Domain/*.csproj ./src/CeibaFunds.Domain/
COPY src/CeibaFunds.Infrastructure/*.csproj ./src/CeibaFunds.Infrastructure/
COPY tests/CeibaFunds.UnitTests/*.csproj ./tests/CeibaFunds.UnitTests/
COPY tests/CeibaFunds.IntegrationTests/*.csproj ./tests/CeibaFunds.IntegrationTests/

# Restaurar dependencias
RUN dotnet restore

# Copiar todo el código fuente
COPY . ./

# Ejecutar tests unitarios
RUN dotnet test tests/CeibaFunds.UnitTests/CeibaFunds.UnitTests.csproj --no-restore --verbosity normal

# Compilar la aplicación
RUN dotnet publish src/CeibaFunds.API/CeibaFunds.API.csproj -c Release -o out --no-restore

# ============================================
# Runtime Stage - Imagen de producción
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Copiar archivos compilados desde build stage
COPY --from=build-env /app/out .

# Configurar variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Exponer puerto
EXPOSE 8080

# Configurar health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "CeibaFunds.API.dll"]