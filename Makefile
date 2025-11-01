# Makefile para CeibaFunds API
# Uso: make [target]

.PHONY: help dev prod build test logs stop clean status shell db-init monitoring

# Variables
COMPOSE_FILE = docker-compose.yml
COMPOSE_DEV = docker-compose.yml -f docker-compose.override.yml
COMPOSE_PROD = docker-compose.yml -f docker-compose.prod.yml
COMPOSE_MONITORING = docker-compose.yml --profile monitoring

# Colores para output
GREEN = \033[0;32m
BLUE = \033[0;34m
YELLOW = \033[1;33m
NC = \033[0m # No Color

# Target por defecto
help: ## Mostrar esta ayuda
	@echo "$(BLUE)🐳 CeibaFunds Docker Management$(NC)"
	@echo "$(BLUE)==============================$(NC)"
	@echo ""
	@echo "Targets disponibles:"
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  $(GREEN)%-15s$(NC) %s\n", $$1, $$2}' $(MAKEFILE_LIST)

dev: ## Iniciar entorno de desarrollo
	@echo "$(GREEN)🚀 Iniciando entorno de desarrollo...$(NC)"
	@mkdir -p logs data/dynamodb
	@docker-compose $(COMPOSE_DEV) up --build -d
	@echo "$(GREEN)⏳ Esperando que los servicios estén listos...$(NC)"
	@sleep 10
	@make status
	@echo "$(GREEN)✅ Entorno de desarrollo listo!$(NC)"

prod: ## Iniciar entorno de producción
	@echo "$(GREEN)🚀 Iniciando entorno de producción...$(NC)"
	@mkdir -p logs
	@docker-compose $(COMPOSE_PROD) up --build -d
	@echo "$(GREEN)⏳ Esperando que los servicios estén listos...$(NC)"
	@sleep 15
	@make status
	@echo "$(GREEN)✅ Entorno de producción listo!$(NC)"

build: ## Construir imágenes Docker
	@echo "$(BLUE)🔨 Construyendo imágenes Docker...$(NC)"
	@docker-compose build --no-cache
	@echo "$(GREEN)✅ Imágenes construidas exitosamente!$(NC)"

test: ## Ejecutar tests
	@echo "$(BLUE)🧪 Ejecutando tests...$(NC)"
	@docker-compose run --rm ceibafunds-api dotnet test --verbosity normal
	@echo "$(GREEN)✅ Tests completados!$(NC)"

logs: ## Mostrar logs de todos los servicios
	@echo "$(BLUE)📋 Logs de todos los servicios:$(NC)"
	@docker-compose logs -f

logs-api: ## Mostrar logs de la API
	@echo "$(BLUE)📋 Logs de la API:$(NC)"
	@docker-compose logs -f ceibafunds-api

logs-db: ## Mostrar logs de DynamoDB
	@echo "$(BLUE)📋 Logs de DynamoDB:$(NC)"
	@docker-compose logs -f dynamodb-local

stop: ## Detener todos los servicios
	@echo "$(YELLOW)⏸️  Deteniendo servicios...$(NC)"
	@docker-compose down
	@echo "$(GREEN)✅ Servicios detenidos!$(NC)"

clean: ## Limpiar contenedores, imágenes y volúmenes
	@echo "$(YELLOW)🧹 Limpiando contenedores, imágenes y volúmenes...$(NC)"
	@echo "$(YELLOW)⚠️  Esto eliminará todos los datos locales!$(NC)"
	@read -p "¿Continuar? (y/N): " confirm; \
	if [ "$$confirm" = "y" ] || [ "$$confirm" = "Y" ]; then \
		docker-compose down -v --remove-orphans; \
		docker system prune -f --volumes; \
		docker image prune -f; \
		rm -rf logs/* data/*; \
		echo "$(GREEN)✅ Limpieza completada!$(NC)"; \
	else \
		echo "Cancelado."; \
	fi

status: ## Mostrar estado de los contenedores
	@echo "$(BLUE)📊 Estado de los contenedores:$(NC)"
	@docker-compose ps
	@echo ""
	@echo "$(BLUE)🌐 URLs disponibles:$(NC)"
	@echo "  • API Swagger: http://localhost:5000/swagger"
	@echo "  • API Health: http://localhost:5000/health"
	@echo "  • DynamoDB Admin: http://localhost:8001"
	@echo "  • Prometheus: http://localhost:9090 (si está habilitado)"
	@echo "  • Grafana: http://localhost:3000 (si está habilitado)"

shell: ## Abrir shell en el contenedor de la API
	@echo "$(BLUE)💻 Abriendo shell en el contenedor de la API...$(NC)"
	@docker-compose exec ceibafunds-api /bin/bash

shell-db: ## Abrir shell en DynamoDB
	@echo "$(BLUE)💻 Abriendo shell en DynamoDB...$(NC)"
	@docker-compose exec dynamodb-local /bin/bash

db-init: ## Inicializar tablas de DynamoDB
	@echo "$(BLUE)🗄️  Inicializando tablas de DynamoDB...$(NC)"
	@docker-compose run --rm dynamodb-initializer
	@echo "$(GREEN)✅ Tablas inicializadas!$(NC)"

monitoring: ## Iniciar con Prometheus y Grafana
	@echo "$(GREEN)📊 Iniciando con monitoreo...$(NC)"
	@mkdir -p logs data/dynamodb
	@docker-compose $(COMPOSE_MONITORING) up --build -d
	@echo "$(GREEN)⏳ Esperando que los servicios estén listos...$(NC)"
	@sleep 15
	@make status
	@echo "$(GREEN)✅ Entorno con monitoreo listo!$(NC)"
	@echo "$(BLUE)📊 Grafana: http://localhost:3000 (admin/admin123)$(NC)"

health: ## Verificar health de los servicios
	@echo "$(BLUE)🏥 Verificando salud de los servicios...$(NC)"
	@echo "API Health:"
	@curl -s http://localhost:5000/health | jq . || echo "API no disponible"
	@echo ""
	@echo "DynamoDB Local:"
	@curl -s http://localhost:8000/ || echo "DynamoDB no disponible"

ps: ## Alias para status
	@make status

up: ## Alias para dev
	@make dev

down: ## Alias para stop
	@make stop

restart: ## Reiniciar servicios
	@echo "$(YELLOW)🔄 Reiniciando servicios...$(NC)"
	@make stop
	@sleep 2
	@make dev

# Targets para testing específicos
test-unit: ## Ejecutar solo tests unitarios
	@docker-compose run --rm ceibafunds-api dotnet test tests/CeibaFunds.UnitTests/CeibaFunds.UnitTests.csproj --verbosity normal

test-integration: ## Ejecutar solo tests de integración
	@docker-compose run --rm ceibafunds-api dotnet test tests/CeibaFunds.IntegrationTests/CeibaFunds.IntegrationTests.csproj --verbosity normal

# Targets para desarrollo
watch: ## Iniciar con hot reload (requiere dotnet watch)
	@echo "$(BLUE)👀 Iniciando en modo watch...$(NC)"
	@docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build -d dynamodb-local dynamodb-admin
	@sleep 5
	@cd src/CeibaFunds.API && dotnet watch run

# Targets de utilidad
version: ## Mostrar versiones de Docker
	@echo "$(BLUE)📋 Versiones instaladas:$(NC)"
	@docker --version
	@docker-compose --version

pull: ## Actualizar imágenes base
	@echo "$(BLUE)📦 Actualizando imágenes base...$(NC)"
	@docker pull mcr.microsoft.com/dotnet/sdk:8.0
	@docker pull mcr.microsoft.com/dotnet/aspnet:8.0
	@docker pull amazon/dynamodb-local:latest
	@docker pull aaronshaf/dynamodb-admin:latest
