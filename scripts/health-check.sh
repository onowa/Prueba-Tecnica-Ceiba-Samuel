#!/bin/bash

# Health Check Script para CeibaFunds API
# Este script verifica el estado de salud de todos los servicios

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# URLs de los servicios
API_URL="http://localhost:5000"
DYNAMODB_URL="http://localhost:8000"
ADMIN_URL="http://localhost:8001"
PROMETHEUS_URL="http://localhost:9090"
GRAFANA_URL="http://localhost:3000"

echo -e "${BLUE}🏥 CeibaFunds Health Check${NC}"
echo -e "${BLUE}=========================${NC}"
echo ""

# Función para verificar servicio
check_service() {
    local name=$1
    local url=$2
    local expected_status=${3:-200}
    
    echo -n "Verificando $name... "
    
    if curl -s -o /dev/null -w "%{http_code}" --max-time 5 "$url" | grep -q "$expected_status"; then
        echo -e "${GREEN}✅ OK${NC}"
        return 0
    else
        echo -e "${RED}❌ FAIL${NC}"
        return 1
    fi
}

# Función para verificar endpoint JSON
check_json_endpoint() {
    local name=$1
    local url=$2
    local json_path=$3
    
    echo -n "Verificando $name... "
    
    response=$(curl -s --max-time 5 "$url" 2>/dev/null)
    if echo "$response" | jq -e "$json_path" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ OK${NC}"
        return 0
    else
        echo -e "${RED}❌ FAIL${NC}"
        return 1
    fi
}

# Verificar Docker
echo -e "${YELLOW}🐳 Verificando Docker...${NC}"
if ! docker --version > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker no está disponible${NC}"
    exit 1
fi

# Verificar contenedores
echo -e "${YELLOW}📦 Verificando contenedores...${NC}"
containers=$(docker-compose ps --services 2>/dev/null || echo "")
if [ -z "$containers" ]; then
    echo -e "${RED}❌ No hay contenedores ejecutándose${NC}"
    exit 1
fi

running_containers=$(docker-compose ps -q 2>/dev/null | wc -l)
echo -e "${GREEN}✅ $running_containers contenedores ejecutándose${NC}"

# Verificar servicios principales
echo ""
echo -e "${YELLOW}🌐 Verificando servicios web...${NC}"

# API Health Check
if check_json_endpoint "API Health" "$API_URL/health" '.status'; then
    # Verificar endpoints específicos de la API
    check_service "API Swagger" "$API_URL/swagger/index.html"
    check_service "API Funds" "$API_URL/api/funds"
    check_service "API Customers" "$API_URL/api/customers"
fi

# DynamoDB
check_service "DynamoDB Local" "$DYNAMODB_URL"

# DynamoDB Admin (opcional)
check_service "DynamoDB Admin" "$ADMIN_URL" || echo -e "${YELLOW}⚠️  DynamoDB Admin no disponible${NC}"

# Prometheus (opcional)
check_service "Prometheus" "$PROMETHEUS_URL" || echo -e "${YELLOW}⚠️  Prometheus no disponible${NC}"

# Grafana (opcional)
check_service "Grafana" "$GRAFANA_URL" || echo -e "${YELLOW}⚠️  Grafana no disponible${NC}"

# Verificar métricas de la API
echo ""
echo -e "${YELLOW}📊 Verificando métricas...${NC}"
if check_service "API Metrics" "$API_URL/metrics"; then
    echo -e "${GREEN}✅ Métricas disponibles${NC}"
fi

# Verificar logs recientes
echo ""
echo -e "${YELLOW}📋 Verificando logs recientes...${NC}"
if [ -d "logs" ] && [ "$(ls -A logs 2>/dev/null)" ]; then
    latest_log=$(ls -t logs/*.log 2>/dev/null | head -1)
    if [ -n "$latest_log" ]; then
        log_size=$(du -h "$latest_log" | cut -f1)
        echo -e "${GREEN}✅ Log más reciente: $latest_log ($log_size)${NC}"
        
        # Verificar errores recientes
        error_count=$(tail -100 "$latest_log" 2>/dev/null | grep -i "error\|exception\|fail" | wc -l)
        if [ "$error_count" -gt 0 ]; then
            echo -e "${YELLOW}⚠️  $error_count errores en las últimas 100 líneas${NC}"
        else
            echo -e "${GREEN}✅ No hay errores recientes${NC}"
        fi
    fi
else
    echo -e "${YELLOW}⚠️  No se encontraron logs${NC}"
fi

# Verificar recursos del sistema
echo ""
echo -e "${YELLOW}🖥️  Verificando recursos del sistema...${NC}"

# Memoria
memory_usage=$(docker stats --no-stream --format "table {{.Container}}\t{{.MemUsage}}" | grep -v CONTAINER | head -5)
if [ -n "$memory_usage" ]; then
    echo -e "${GREEN}✅ Uso de memoria por contenedor:${NC}"
    echo "$memory_usage"
fi

# CPU
cpu_usage=$(docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}" | grep -v CONTAINER | head -5)
if [ -n "$cpu_usage" ]; then
    echo -e "${GREEN}✅ Uso de CPU por contenedor:${NC}"
    echo "$cpu_usage"
fi

# Verificar conectividad de red
echo ""
echo -e "${YELLOW}🌐 Verificando conectividad de red...${NC}"
if ping -c 1 8.8.8.8 > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Conectividad externa disponible${NC}"
else
    echo -e "${YELLOW}⚠️  No hay conectividad externa${NC}"
fi

# Resumen final
echo ""
echo -e "${BLUE}📋 Resumen del Health Check${NC}"
echo -e "${BLUE}===========================${NC}"

# Verificar si todos los servicios críticos están funcionando
api_ok=false
db_ok=false

if curl -s --max-time 5 "$API_URL/health" > /dev/null 2>&1; then
    api_ok=true
fi

if curl -s --max-time 5 "$DYNAMODB_URL" > /dev/null 2>&1; then
    db_ok=true
fi

if [ "$api_ok" = true ] && [ "$db_ok" = true ]; then
    echo -e "${GREEN}✅ Sistema funcionando correctamente${NC}"
    echo -e "${GREEN}🚀 API disponible en: $API_URL/swagger${NC}"
    exit 0
else
    echo -e "${RED}❌ Algunos servicios críticos no están disponibles${NC}"
    if [ "$api_ok" = false ]; then
        echo -e "${RED}   - API no disponible${NC}"
    fi
    if [ "$db_ok" = false ]; then
        echo -e "${RED}   - DynamoDB no disponible${NC}"
    fi
    exit 1
fi
