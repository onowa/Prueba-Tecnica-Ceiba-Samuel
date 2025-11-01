# Health Check Script para CeibaFunds API (PowerShell)
# Este script verifica el estado de salud de todos los servicios

param(
    [switch]$Detailed = $false,
    [switch]$Json = $false
)

# Función para escribir con colores
function Write-ColoredText($ForegroundColor, $Message) {
    Write-Host $Message -ForegroundColor $ForegroundColor
}

# URLs de los servicios
$apiUrl = "http://localhost:5000"
$dynamoUrl = "http://localhost:8000"
$adminUrl = "http://localhost:8001"
$prometheusUrl = "http://localhost:9090"
$grafanaUrl = "http://localhost:3000"

$results = @{
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    services  = @{}
    summary   = @{}
}

Write-ColoredText Yellow "🏥 CeibaFunds Health Check"
Write-ColoredText Yellow "========================="
Write-Output ""

# Función para verificar servicio
function Test-Service {
    param(
        [string]$Name,
        [string]$Url,
        [int]$ExpectedStatus = 200
    )
    
    Write-Host "Verificando $Name... " -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
        if ($response.StatusCode -eq $ExpectedStatus) {
            Write-ColoredText Green "✅ OK"
            $results.services[$Name] = @{ status = "OK"; url = $Url; statusCode = $response.StatusCode }
            return $true
        }
        else {
            Write-ColoredText Red "❌ FAIL (Status: $($response.StatusCode))"
            $results.services[$Name] = @{ status = "FAIL"; url = $Url; statusCode = $response.StatusCode; error = "Unexpected status code" }
            return $false
        }
    }
    catch {
        Write-ColoredText Red "❌ FAIL ($($_.Exception.Message))"
        $results.services[$Name] = @{ status = "FAIL"; url = $Url; error = $_.Exception.Message }
        return $false
    }
}

# Función para verificar endpoint JSON
function Test-JsonEndpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$JsonPath = $null
    )
    
    Write-Host "Verificando $Name... " -NoNewline
    
    try {
        $response = Invoke-RestMethod -Uri $Url -Method Get -TimeoutSec 5 -ErrorAction Stop
        
        if ($JsonPath) {
            $value = Invoke-Expression "`$response.$JsonPath"
            if ($value) {
                Write-ColoredText Green "✅ OK"
                $results.services[$Name] = @{ status = "OK"; url = $Url; data = $response }
                return $true
            }
            else {
                Write-ColoredText Red "❌ FAIL (JSON path not found)"
                $results.services[$Name] = @{ status = "FAIL"; url = $Url; error = "JSON path not found: $JsonPath" }
                return $false
            }
        }
        else {
            Write-ColoredText Green "✅ OK"
            $results.services[$Name] = @{ status = "OK"; url = $Url; data = $response }
            return $true
        }
    }
    catch {
        Write-ColoredText Red "❌ FAIL ($($_.Exception.Message))"
        $results.services[$Name] = @{ status = "FAIL"; url = $Url; error = $_.Exception.Message }
        return $false
    }
}

# Verificar Docker
Write-ColoredText Yellow "🐳 Verificando Docker..."
try {
    $dockerVersion = docker --version 2>$null
    if ($dockerVersion) {
        Write-ColoredText Green "✅ Docker disponible: $dockerVersion"
        $results.docker = @{ status = "OK"; version = $dockerVersion }
    }
    else {
        throw "Docker no encontrado"
    }
}
catch {
    Write-ColoredText Red "❌ Docker no está disponible"
    $results.docker = @{ status = "FAIL"; error = "Docker not available" }
    if (-not $Json) { exit 1 }
}

# Verificar contenedores
Write-ColoredText Yellow "📦 Verificando contenedores..."
try {
    $containers = docker-compose ps -q 2>$null
    if ($containers) {
        $containerCount = ($containers | Measure-Object).Count
        Write-ColoredText Green "✅ $containerCount contenedores ejecutándose"
        $results.containers = @{ status = "OK"; count = $containerCount }
    }
    else {
        Write-ColoredText Red "❌ No hay contenedores ejecutándose"
        $results.containers = @{ status = "FAIL"; error = "No containers running" }
    }
}
catch {
    Write-ColoredText Red "❌ Error al verificar contenedores: $($_.Exception.Message)"
    $results.containers = @{ status = "FAIL"; error = $_.Exception.Message }
}

# Verificar servicios principales
Write-Output ""
Write-ColoredText Yellow "🌐 Verificando servicios web..."

# API Health Check
$apiOk = Test-JsonEndpoint "API Health" "$apiUrl/health" "status"
if ($apiOk) {
    # Verificar endpoints específicos de la API
    Test-Service "API Swagger" "$apiUrl/swagger/index.html"
    Test-Service "API Funds" "$apiUrl/api/funds"
    Test-Service "API Customers" "$apiUrl/api/customers"
}

# DynamoDB
$dbOk = Test-Service "DynamoDB Local" $dynamoUrl

# Servicios opcionales
$adminOk = Test-Service "DynamoDB Admin" $adminUrl
if (-not $adminOk) {
    Write-ColoredText Yellow "⚠️  DynamoDB Admin no disponible"
}

$prometheusOk = Test-Service "Prometheus" $prometheusUrl
if (-not $prometheusOk) {
    Write-ColoredText Yellow "⚠️  Prometheus no disponible"
}

$grafanaOk = Test-Service "Grafana" $grafanaUrl
if (-not $grafanaOk) {
    Write-ColoredText Yellow "⚠️  Grafana no disponible"
}

# Verificar métricas de la API
Write-Output ""
Write-ColoredText Yellow "📊 Verificando métricas..."
$metricsOk = Test-Service "API Metrics" "$apiUrl/metrics"
if (-not $metricsOk) {
    Write-ColoredText Yellow "⚠️  Métricas no disponibles"
}

# Verificar logs recientes
Write-Output ""
Write-ColoredText Yellow "📋 Verificando logs recientes..."
if (Test-Path "logs") {
    $logFiles = Get-ChildItem -Path "logs" -Filter "*.log" | Sort-Object LastWriteTime -Descending
    if ($logFiles) {
        $latestLog = $logFiles[0]
        $logSize = [math]::Round($latestLog.Length / 1KB, 2)
        Write-ColoredText Green "✅ Log más reciente: $($latestLog.Name) ($logSize KB)"
        
        # Verificar errores recientes
        $errorCount = (Get-Content $latestLog.FullName -Tail 100 | Select-String -Pattern "error|exception|fail" -CaseSensitive:$false).Count
        if ($errorCount -gt 0) {
            Write-ColoredText Yellow "⚠️  $errorCount errores en las últimas 100 líneas"
            $results.logs = @{ status = "WARNING"; latestLog = $latestLog.Name; errors = $errorCount }
        }
        else {
            Write-ColoredText Green "✅ No hay errores recientes"
            $results.logs = @{ status = "OK"; latestLog = $latestLog.Name; errors = 0 }
        }
    }
    else {
        Write-ColoredText Yellow "⚠️  No se encontraron archivos de log"
        $results.logs = @{ status = "WARNING"; error = "No log files found" }
    }
}
else {
    Write-ColoredText Yellow "⚠️  Directorio de logs no encontrado"
    $results.logs = @{ status = "WARNING"; error = "Logs directory not found" }
}

# Verificar recursos del sistema (solo si está disponible)
if ($Detailed) {
    Write-Output ""
    Write-ColoredText Yellow "🖥️  Verificando recursos del sistema..."
    
    try {
        $dockerStats = docker stats --no-stream --format "table {{.Container}}\t{{.MemUsage}}\t{{.CPUPerc}}" 2>$null
        if ($dockerStats) {
            Write-ColoredText Green "✅ Estadísticas de contenedores:"
            $dockerStats | ForEach-Object { Write-Output "   $_" }
            $results.systemResources = @{ status = "OK"; stats = $dockerStats }
        }
    }
    catch {
        Write-ColoredText Yellow "⚠️  No se pudieron obtener estadísticas de contenedores"
        $results.systemResources = @{ status = "WARNING"; error = $_.Exception.Message }
    }
}

# Verificar conectividad de red
Write-Output ""
Write-ColoredText Yellow "🌐 Verificando conectividad de red..."
try {
    $ping = Test-NetConnection -ComputerName "8.8.8.8" -Port 53 -InformationLevel Quiet
    if ($ping) {
        Write-ColoredText Green "✅ Conectividad externa disponible"
        $results.network = @{ status = "OK" }
    }
    else {
        Write-ColoredText Yellow "⚠️  No hay conectividad externa"
        $results.network = @{ status = "WARNING"; error = "No external connectivity" }
    }
}
catch {
    Write-ColoredText Yellow "⚠️  No se pudo verificar conectividad de red"
    $results.network = @{ status = "WARNING"; error = $_.Exception.Message }
}

# Resumen final
Write-Output ""
Write-ColoredText Blue "📋 Resumen del Health Check"
Write-ColoredText Blue "==========================="

$results.summary.apiOk = $apiOk
$results.summary.dbOk = $dbOk
$results.summary.overallStatus = if ($apiOk -and $dbOk) { "HEALTHY" } else { "UNHEALTHY" }

if ($apiOk -and $dbOk) {
    Write-ColoredText Green "✅ Sistema funcionando correctamente"
    Write-ColoredText Green "🚀 API disponible en: $apiUrl/swagger"
    $results.summary.message = "System is healthy"
    $exitCode = 0
}
else {
    Write-ColoredText Red "❌ Algunos servicios críticos no están disponibles"
    if (-not $apiOk) {
        Write-ColoredText Red "   - API no disponible"
    }
    if (-not $dbOk) {
        Write-ColoredText Red "   - DynamoDB no disponible"
    }
    $results.summary.message = "Some critical services are unavailable"
    $exitCode = 1
}

# Output JSON si se solicita
if ($Json) {
    $results | ConvertTo-Json -Depth 3
}
else {
    Write-Output ""
    Write-ColoredText Blue "Para obtener output en formato JSON, use el parámetro -Json"
    Write-ColoredText Blue "Para información detallada, use el parámetro -Detailed"
}

exit $exitCode
