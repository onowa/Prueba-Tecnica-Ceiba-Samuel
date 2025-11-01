#!/bin/bash
# Script de gestión de Docker para CeibaFunds API

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Función de ayuda
show_help() {
    echo -e "${BLUE}🐳 CeibaFunds Docker Management Script${NC}"
    echo -e "${BLUE}=====================================\n${NC}"
    echo "Uso: ./docker-manage.sh [COMANDO] [OPCIONES]"
    echo ""
    echo "Comandos disponibles:"
    echo "  dev          - Iniciar entorno de desarrollo completo"
    echo "  prod         - Iniciar entorno de producción"
    echo "  build        - Construir imágenes Docker"
    echo "  test         - Ejecutar tests en contenedor"
    echo "  logs         - Mostrar logs de los servicios"
    echo "  stop         - Detener todos los servicios"
    echo "  clean        - Limpiar contenedores, imágenes y volúmenes"
    echo "  status       - Mostrar estado de los contenedores"
    echo "  shell        - Abrir shell en el contenedor de la API"
    echo "  db-init      - Inicializar tablas de DynamoDB"
    echo "  monitoring   - Iniciar con Prometheus y Grafana"
    echo ""
    echo "Ejemplos:"
    echo "  ./docker-manage.sh dev              # Iniciar desarrollo"
    echo "  ./docker-manage.sh logs api         # Ver logs de la API"
    echo "  ./docker-manage.sh prod             # Iniciar producción"
    echo "  ./docker-manage.sh monitoring       # Iniciar con monitoreo"
}

# Función para mostrar estado
show_status() {
    echo -e "${BLUE}📊 Estado de los contenedores:${NC}"
    docker-compose ps
    echo ""
    echo -e "${BLUE}🌐 URLs disponibles:${NC}"
    echo "  • API Swagger: http://localhost:5000/swagger"
    echo "  • API Health: http://localhost:5000/health"
    echo "  • DynamoDB Admin: http://localhost:8001"
    echo "  • Prometheus: http://localhost:9090 (si está habilitado)"
    echo "  • Grafana: http://localhost:3000 (si está habilitado)"
}

# Función para desarrollo
start_dev() {
    echo -e "${GREEN}🚀 Iniciando entorno de desarrollo...${NC}"
    
    # Crear directorios necesarios
    mkdir -p logs data/dynamodb
    
    # Construir y ejecutar
    docker-compose up --build -d
    
    echo -e "${GREEN}⏳ Esperando que los servicios estén listos...${NC}"
    sleep 10
    
    show_status
    
    echo -e "${GREEN}✅ Entorno de desarrollo listo!${NC}"
}

# Función para producción
start_prod() {
    echo -e "${GREEN}🚀 Iniciando entorno de producción...${NC}"
    
    # Crear directorios necesarios
    mkdir -p logs
    
    # Ejecutar con configuración de producción
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
    
    echo -e "${GREEN}⏳ Esperando que los servicios estén listos...${NC}"
    sleep 15
    
    show_status
    
    echo -e "${GREEN}✅ Entorno de producción listo!${NC}"
}

# Función para construir imágenes
build_images() {
    echo -e "${BLUE}🔨 Construyendo imágenes Docker...${NC}"
    docker-compose build --no-cache
    echo -e "${GREEN}✅ Imágenes construidas exitosamente!${NC}"
}

# Función para ejecutar tests
run_tests() {
    echo -e "${BLUE}🧪 Ejecutando tests...${NC}"
    docker-compose run --rm ceibafunds-api dotnet test --verbosity normal
    echo -e "${GREEN}✅ Tests completados!${NC}"
}

# Función para mostrar logs
show_logs() {
    local service=${2:-""}
    if [ -n "$service" ]; then
        echo -e "${BLUE}📋 Logs del servicio: $service${NC}"
        docker-compose logs -f "$service"
    else
        echo -e "${BLUE}📋 Logs de todos los servicios:${NC}"
        docker-compose logs -f
    fi
}

# Función para detener servicios
stop_services() {
    echo -e "${YELLOW}⏸️  Deteniendo servicios...${NC}"
    docker-compose down
    echo -e "${GREEN}✅ Servicios detenidos!${NC}"
}

# Función para limpiar
clean_all() {
    echo -e "${YELLOW}🧹 Limpiando contenedores, imágenes y volúmenes...${NC}"
    
    # Confirmar acción
    read -p "¿Estás seguro? Esto eliminará todos los datos locales (y/N): " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Cancelado."
        exit 1
    fi
    
    docker-compose down -v --remove-orphans
    docker system prune -f --volumes
    docker image prune -f
    
    # Limpiar directorios locales
    rm -rf logs/* data/*
    
    echo -e "${GREEN}✅ Limpieza completada!${NC}"
}

# Función para abrir shell
open_shell() {
    echo -e "${BLUE}💻 Abriendo shell en el contenedor de la API...${NC}"
    docker-compose exec ceibafunds-api /bin/bash
}

# Función para inicializar base de datos
init_db() {
    echo -e "${BLUE}🗄️  Inicializando tablas de DynamoDB...${NC}"
    docker-compose run --rm dynamodb-initializer
    echo -e "${GREEN}✅ Tablas inicializadas!${NC}"
}

# Función para monitoring
start_monitoring() {
    echo -e "${GREEN}📊 Iniciando con monitoreo (Prometheus + Grafana)...${NC}"
    
    # Crear directorios necesarios
    mkdir -p logs data/dynamodb
    
    # Ejecutar con perfil de monitoring
    docker-compose --profile monitoring up --build -d
    
    echo -e "${GREEN}⏳ Esperando que los servicios estén listos...${NC}"
    sleep 15
    
    show_status
    
    echo -e "${GREEN}✅ Entorno con monitoreo listo!${NC}"
    echo -e "${BLUE}📊 Grafana está disponible en: http://localhost:3000${NC}"
    echo -e "${BLUE}   Usuario: admin, Password: admin123${NC}"
}

# Función principal
main() {
    case "${1:-help}" in
        "dev")
            start_dev
            ;;
        "prod")
            start_prod
            ;;
        "build")
            build_images
            ;;
        "test")
            run_tests
            ;;
        "logs")
            show_logs "$@"
            ;;
        "stop")
            stop_services
            ;;
        "clean")
            clean_all
            ;;
        "status")
            show_status
            ;;
        "shell")
            open_shell
            ;;
        "db-init")
            init_db
            ;;
        "monitoring")
            start_monitoring
            ;;
        "help"|*)
            show_help
            ;;
    esac
}

# Verificar que Docker está ejecutándose
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker no está ejecutándose. Por favor inicia Docker e intenta nuevamente.${NC}"
    exit 1
fi

# Verificar que docker-compose está disponible
if ! command -v docker-compose > /dev/null 2>&1; then
    echo -e "${RED}❌ docker-compose no está instalado. Por favor instálalo e intenta nuevamente.${NC}"
    exit 1
fi

# Ejecutar función principal
main "$@"
