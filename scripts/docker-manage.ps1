# Docker Management Script para CeibaFunds API
# Uso: .\docker-manage.ps1 [comando] [opciones]

param(
    [Parameter(Position = 0)]
    [ValidateSet('dev', 'prod', 'build', 'test', 'logs', 'stop', 'clean', 'status', 'shell', 'db-init', 'monitoring', 'help')]
    [string]$Command = 'help',
    
    [string]$Service = '',
    [switch]$NoCache
)

# Configuracion de colores
$ErrorActionPreference = 'Stop'

function Write-ColoredMessage($Color, $Message) {
    Write-Host $Message -ForegroundColor $Color
}

function Show-Help {
    Write-ColoredMessage Cyan "Docker CeibaFunds Management"
    Write-ColoredMessage Cyan "=========================="
    Write-Host ""
    Write-Host "COMANDOS DISPONIBLES:"
    Write-Host ""
    Write-ColoredMessage Green "  dev           Iniciar entorno de desarrollo"
    Write-ColoredMessage Green "  prod          Iniciar entorno de produccion"
    Write-ColoredMessage Green "  build         Construir imagenes Docker"
    Write-ColoredMessage Green "  test          Ejecutar tests en contenedores"
    Write-ColoredMessage Green "  logs          Mostrar logs (usar -Service para servicio especifico)"
    Write-ColoredMessage Green "  stop          Detener todos los servicios"
    Write-ColoredMessage Green "  clean         Limpiar contenedores, imagenes y volumenes"
    Write-ColoredMessage Green "  status        Mostrar estado de contenedores y URLs"
    Write-ColoredMessage Green "  shell         Abrir shell en contenedor API"
    Write-ColoredMessage Green "  db-init       Inicializar tablas DynamoDB"
    Write-ColoredMessage Green "  monitoring    Iniciar con Prometheus y Grafana"
    Write-Host ""
    Write-Host "EJEMPLOS:"
    Write-Host "  .\docker-manage.ps1 dev                 # Iniciar desarrollo"
    Write-Host "  .\docker-manage.ps1 logs -Service api   # Ver logs de API"
    Write-Host "  .\docker-manage.ps1 build -NoCache      # Build sin cache"
    Write-Host ""
}

function Start-Dev {
    Write-ColoredMessage Green "Iniciando entorno de desarrollo..."
    
    # Crear directorios necesarios
    if (!(Test-Path "logs")) { New-Item -ItemType Directory -Path "logs" }
    if (!(Test-Path "data\dynamodb")) { New-Item -ItemType Directory -Path "data\dynamodb" -Force }
    
    docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build -d
    
    Write-ColoredMessage Yellow "Esperando que los servicios esten listos..."
    Start-Sleep -Seconds 10
    
    Show-Status
    Write-ColoredMessage Green "Entorno de desarrollo listo!"
}

function Start-Prod {
    Write-ColoredMessage Green "Iniciando entorno de produccion..."
    
    if (!(Test-Path "logs")) { New-Item -ItemType Directory -Path "logs" }
    
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
    
    Write-ColoredMessage Yellow "Esperando que los servicios esten listos..."
    Start-Sleep -Seconds 15
    
    Show-Status
    Write-ColoredMessage Green "Entorno de produccion listo!"
}

function Invoke-ImageBuild {
    Write-ColoredMessage Cyan "Construyendo imagenes Docker..."
    
    if ($NoCache) {
        docker-compose build --no-cache
    }
    else {
        docker-compose build
    }
    
    Write-ColoredMessage Green "Imagenes construidas exitosamente!"
}

function Invoke-Tests {
    Write-ColoredMessage Cyan "Ejecutando tests..."
    
    # Asegurar que DynamoDB esta corriendo para tests de integracion
    docker-compose up -d dynamodb-local
    Start-Sleep -Seconds 5
    
    docker-compose run --rm ceibafunds-api dotnet test --verbosity normal
    
    Write-ColoredMessage Green "Tests completados!"
}

function Show-Logs {
    param([string]$ServiceName)
    
    Write-ColoredMessage Cyan "Mostrando logs..."
    
    if ($ServiceName) {
        docker-compose logs -f $ServiceName
    }
    else {
        docker-compose logs -f
    }
}

function Stop-Services {
    Write-ColoredMessage Yellow "Deteniendo servicios..."
    docker-compose down
    Write-ColoredMessage Green "Servicios detenidos!"
}

function Remove-AllResources {
    Write-ColoredMessage Yellow "Limpiando contenedores, imagenes y volumenes..."
    Write-ColoredMessage Red "Esto eliminara todos los datos locales!"
    
    $confirm = Read-Host "Continuar? (y/N)"
    if ($confirm -eq 'y' -or $confirm -eq 'Y') {
        docker-compose down -v --remove-orphans
        docker system prune -f --volumes
        docker image prune -f
        
        if (Test-Path "logs") { Remove-Item -Path "logs\*" -Force -ErrorAction SilentlyContinue }
        if (Test-Path "data") { Remove-Item -Path "data\*" -Recurse -Force -ErrorAction SilentlyContinue }
        
        Write-ColoredMessage Green "Limpieza completada!"
    }
    else {
        Write-ColoredMessage Yellow "Cancelado."
    }
}

function Show-Status {
    Write-ColoredMessage Cyan "Estado de los contenedores:"
    docker-compose ps
    Write-Host ""
    
    Write-ColoredMessage Cyan "URLs disponibles:"
    Write-Host "  • API Swagger: http://localhost:5000/swagger"
    Write-Host "  • API Health: http://localhost:5000/health"
    Write-Host "  • DynamoDB Admin: http://localhost:8001"
    Write-Host "  • Prometheus: http://localhost:9090 (si esta habilitado)"
    Write-Host "  • Grafana: http://localhost:3000 (si esta habilitado)"
}

function Open-Shell {
    Write-ColoredMessage Cyan "Abriendo shell en el contenedor de la API..."
    docker-compose exec ceibafunds-api /bin/bash
}

function Initialize-Database {
    Write-ColoredMessage Cyan "Inicializando tablas de DynamoDB..."
    docker-compose run --rm dynamodb-initializer
    Write-ColoredMessage Green "Tablas inicializadas!"
}

function Start-Monitoring {
    Write-ColoredMessage Green "Iniciando con monitoreo..."
    
    if (!(Test-Path "logs")) { New-Item -ItemType Directory -Path "logs" }
    if (!(Test-Path "data\dynamodb")) { New-Item -ItemType Directory -Path "data\dynamodb" -Force }
    
    docker-compose --profile monitoring up --build -d
    
    Write-ColoredMessage Yellow "Esperando que los servicios esten listos..."
    Start-Sleep -Seconds 15
    
    Show-Status
    Write-ColoredMessage Green "Entorno con monitoreo listo!"
    Write-ColoredMessage Cyan "Grafana: http://localhost:3000 (admin/admin123)"
}

# Verificar que Docker esta disponible
try {
    docker --version | Out-Null
}
catch {
    Write-ColoredMessage Red "Docker no esta disponible. Por favor instalalo e intenta nuevamente."
    exit 1
}

# Verificar que Docker esta ejecutandose
try {
    docker info | Out-Null
}
catch {
    Write-ColoredMessage Red "Docker no esta ejecutandose. Por favor inicia Docker e intenta nuevamente."
    exit 1
}

# Verificar que docker-compose esta disponible
try {
    docker-compose version | Out-Null
}
catch {
    Write-ColoredMessage Red "docker-compose no esta instalado. Por favor instalalo e intenta nuevamente."
    exit 1
}

# Ejecutar comando principal
switch ($Command) {
    'dev' { Start-Dev }
    'prod' { Start-Prod }
    'build' { Invoke-ImageBuild }
    'test' { Invoke-Tests }
    'logs' { Show-Logs -ServiceName $Service }
    'stop' { Stop-Services }
    'clean' { Remove-AllResources }
    'status' { Show-Status }
    'shell' { Open-Shell }
    'db-init' { Initialize-Database }
    'monitoring' { Start-Monitoring }
    'help' { Show-Help }
    default { Show-Help }
}
