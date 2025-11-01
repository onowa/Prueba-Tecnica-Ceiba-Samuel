# 🏦 CeibaFunds API - Sistema de Gestión de Fondos BTG Pactual

> **API REST completa para gestión de fondos de inversión desarrollada en .NET 8**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Tests](https://img.shields.io/badge/Tests-40+-brightgreen)](#testing-y-calidad)
[![AWS Ready](https://img.shields.io/badge/AWS-Ready-orange)](cloudformation/)

---

## 📋 **ENTREGABLES BTG PACTUAL - PRUEBA TÉCNICA**

> **👥 Evaluadores BTG:** Enlaces directos a todas las respuestas de la prueba técnica

### 🎯 **PARTE 1 (80%) - Sistema de Fondos**

| Entregable                        | Status | 📁 Ubicación                                         |
| --------------------------------- | ------ | ---------------------------------------------------- |
| **1a) Justificación tecnológica** | ✅     | **[📖 Ver Respuesta](docs/tecnologias-solucion.md)** |
| **1b) Modelo de datos NoSQL**     | ✅     | **[📖 Ver Respuesta](docs/modelo-datos-nosql.md)**   |
| **1c) API REST funcional**        | ✅     | **[⚡ Ver Código + 40 Tests](#quick-start)**         |

### 🗄️ **PARTE 2 (20%) - Consulta SQL**

| Entregable               | Status | 📁 Ubicación                                          |
| ------------------------ | ------ | ----------------------------------------------------- |
| **Query SQL Optimizada** | ✅     | **[📄 Ver Query](sql-queries/Query%20Parte%202.sql)** |

### 🚀 **EXTRAS - Deployment Production**

| Entregable                   | Status | 📁 Ubicación                            |
| ---------------------------- | ------ | --------------------------------------- |
| **CloudFormation AWS**       | ✅     | **[☁️ Ver Templates](cloudformation/)** |
| **Terraform Infrastructure** | ✅     | **[🏗️ Ver Código](terraform/)**         |
| **Docker Deployment**        | ✅     | **[🐳 Ver Setup](docker-compose.yml)**  |

---

## 🎯 Resumen Ejecutivo

**CeibaFunds API** es la solución completa para el sistema de fondos BTG Pactual que permite:

### ✅ **Funcionalidades Implementadas**

- **Suscripción a fondos** con validaciones de saldo y montos mínimos
- **Cancelación de suscripciones** con reembolso automático al balance
- **Gestión de clientes** con validaciones robustas (email único, edad mínima)
- **Historial de transacciones** completo y auditable
- **Notificaciones automáticas** por email y SMS en cada operación
- **API REST documentada** con Swagger/OpenAPI

### 🏗️ **Arquitectura Empresarial**

- **Clean Architecture** con separación clara de responsabilidades
- **CQRS Pattern** para separar operaciones de lectura/escritura
- **40+ Tests unitarios** con cobertura completa de reglas de negocio
- **DynamoDB** para escalabilidad automática
- **CloudFormation** para despliegue automatizado en AWS

### ✅ **Entregables BTG Pactual**

| Requerimiento                           | Status  | Ubicación                       |
| --------------------------------------- | ------- | ------------------------------- |
| **Parte 1a - Tecnologías justificadas** | ✅ 100% | `docs/tecnologias-solucion.md`  |
| **Parte 1b - Modelo datos NoSQL**       | ✅ 100% | `docs/modelo-datos-nosql.md`    |
| **Parte 1c - API REST funcional**       | ✅ 100% | Código completo + 40+ tests     |
| **Parte 2 - Consulta SQL**              | ✅ 100% | `sql-queries/Query Parte 2.sql` |
| **CloudFormation + Documentación**      | ✅ 100% | `cloudformation/`               |

---

## Stack Tecnológico

| Componente           | Tecnología       | Versión | Propósito                |
| -------------------- | ---------------- | ------- | ------------------------ |
| **Framework**        | .NET             | 8.0     | Runtime principal        |
| **Lenguaje**         | C#               | 12.0    | Desarrollo               |
| **Base de Datos**    | AWS DynamoDB     | -       | Persistencia NoSQL       |
| **Patrones**         | MediatR          | 12.x    | CQRS Implementation      |
| **Mapeo**            | AutoMapper       | 12.x    | Object-to-Object Mapping |
| **Validación**       | FluentValidation | 11.x    | Input Validation         |
| **Logging**          | Serilog          | 3.x     | Structured Logging       |
| **Testing**          | xUnit + Moq      | -       | Unit & Integration Tests |
| **Containerización** | Docker           | -       | Deployment               |
| **IaC**              | Terraform        | -       | Infrastructure as Code   |

---

## 🚀 Inicio Rápido (Para Evaluadores BTG)

### ⚡ Ejecución Inmediata

**¿Prisa? Solo necesitas esto:**

```bash
# 1. Clonar e instalar
git clone <repository-url> && cd CeibaFundsAPI
dotnet restore

# 2. Iniciar todo (DynamoDB + API)
docker-compose up -d dynamodb-local
dotnet run --project src/CeibaFunds.API

# 3. Probar: http://localhost:5144/swagger
```

### 📋 Prerrequisitos

- ✅ **.NET 8 SDK** ([Descargar](https://dotnet.microsoft.com/download/dotnet/8.0))
- ✅ **Docker Desktop** ([Descargar](https://www.docker.com/products/docker-desktop/))
- 💡 **VS Code** (opcional, pero recomendado)

### 🎯 Verificación Rápida

```bash
# ✅ Health check
curl http://localhost:5144/health
# Respuesta: {"status":"healthy"}

# ✅ Probar endpoints
curl http://localhost:5144/api/funds
# Respuesta: Lista de fondos disponibles

# ✅ Documentación interactiva
# Navegador: http://localhost:5144/swagger
```

### 🧪 Tests (Verificar Calidad)

```bash
# Ejecutar todos los tests (40+ unitarios)
dotnet test --verbosity normal

# Solo tests unitarios
dotnet test tests/CeibaFunds.UnitTests

# Tests con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### 🆘 ¿Problemas?

**Puerto ocupado?** Cambiar puerto:

```bash
$env:ASPNETCORE_URLS="http://localhost:5000"
dotnet run --project src/CeibaFunds.API
```

**Docker no funciona?** Usar DynamoDB mock en memoria (solo para testing).

---

## Estructura del Proyecto

```
CeibaFundsAPI/
├── src/
│   ├── CeibaFunds.API/              # Web API Layer
│   │   ├── Controllers/             # REST Controllers
│   │   ├── Program.cs               # Application Entry Point
│   │   └── appsettings.json         # Configuration
│   │
│   ├── CeibaFunds.Application/      # Application Layer
│   │   ├── Commands/                # CQRS Commands
│   │   ├── Queries/                 # CQRS Queries
│   │   ├── Handlers/                # MediatR Handlers
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   ├── Validators/              # FluentValidation Rules
│   │   └── Mappings/                # AutoMapper Profiles
│   │
│   ├── CeibaFunds.Domain/           # Domain Layer
│   │   ├── Entities/                # Business Entities
│   │   ├── ValueObjects/            # Domain Value Objects
│   │   ├── Enums/                   # Domain Enumerations
│   │   └── Interfaces/              # Repository Contracts
│   │
│   └── CeibaFunds.Infrastructure/   # Infrastructure Layer
│       ├── Repositories/            # Data Access Layer
│       ├── Services/                # External Services
│       └── Configuration/           # DI Container Setup
│
├── tests/
│   ├── CeibaFunds.UnitTests/        # Unit Tests (40+ tests)
│   └── CeibaFunds.IntegrationTests/ # Integration Tests
│
├── terraform/                       # AWS Infrastructure
│   ├── main.tf                      # Main Configuration
│   ├── dynamodb.tf                  # DynamoDB Tables
│   └── lambda.tf                    # Lambda Functions
│
└── docker/                          # Docker Configuration
    └── docker-compose.yml           # Multi-container Setup
```

---

## Endpoints de la API

### Gestión de Clientes

| Método | Endpoint              | Descripción        | Códigos            |
| ------ | --------------------- | ------------------ | ------------------ |
| `GET`  | `/api/customers`      | Listar clientes    | 200, 500           |
| `GET`  | `/api/customers/{id}` | Obtener cliente    | 200, 404, 500      |
| `POST` | `/api/customers`      | Crear cliente      | 201, 400, 409, 500 |
| `PUT`  | `/api/customers/{id}` | Actualizar cliente | 200, 400, 404, 500 |

### Gestión de Fondos

| Método | Endpoint          | Descripción               | Códigos       |
| ------ | ----------------- | ------------------------- | ------------- |
| `GET`  | `/api/funds`      | Listar fondos disponibles | 200, 500      |
| `GET`  | `/api/funds/{id}` | Obtener fondo específico  | 200, 404, 500 |

### Gestión de Suscripciones

| Método | Endpoint                                   | Descripción               | Códigos            |
| ------ | ------------------------------------------ | ------------------------- | ------------------ |
| `GET`  | `/api/subscriptions/customer/{customerId}` | Suscripciones del cliente | 200, 404, 500      |
| `POST` | `/api/subscriptions/subscribe`             | Suscribir a fondo         | 201, 400, 409, 500 |
| `POST` | `/api/subscriptions/cancel`                | Cancelar suscripción      | 200, 400, 404, 500 |

### Monitoreo

| Método | Endpoint  | Descripción             | Respuesta              |
| ------ | --------- | ----------------------- | ---------------------- |
| `GET`  | `/health` | Estado de la aplicación | `{"status":"healthy"}` |

---

## Casos de Uso Principales

### Registrar Nuevo Cliente

```http
POST /api/customers
Content-Type: application/json

{
  "firstName": "María",
  "lastName": "González",
  "email": "maria.gonzalez@email.com",
  "phoneNumber": "+57301234567",
  "dateOfBirth": "1990-03-15"
}
```

### Suscribirse a un Fondo

```http
POST /api/subscriptions/subscribe
Content-Type: application/json

{
  "customerId": "123e4567-e89b-12d3-a456-426614174000",
  "fundId": "987fcdeb-51a2-43d1-9876-543210987654",
  "amount": 1500000,
  "email": "maria.gonzalez@email.com"
}
```

### Consultar Suscripciones Activas

```http
GET /api/subscriptions/customer/123e4567-e89b-12d3-a456-426614174000
```

---

## Testing y Calidad

### Ejecutar Suite de Tests

```bash
# Todos los tests (40+ tests unitarios)
dotnet test

# Solo tests unitarios
dotnet test tests/CeibaFunds.UnitTests --logger "console;verbosity=detailed"

# Solo tests de integración
dotnet test tests/CeibaFunds.IntegrationTests

# Test con cobertura
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

### Métricas de Calidad

- **40+ Tests Unitarios**: Cobertura completa de handlers y entities
- **Tests de Integración**: Validación de endpoints completos
- **Validación de Reglas de Negocio**: FluentValidation en todas las entradas
- **Logging Estructurado**: Trazabilidad completa con Serilog
- **Health Checks**: Monitoreo automático de estado

---

## Despliegue con Docker

### Desarrollo Local

```bash
# Iniciar todos los servicios
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f api

# Parar servicios
docker-compose down
```

### Producción

```bash
# Build para producción
docker-compose -f docker-compose.yml -f docker-compose.prod.yml build

# Deploy completo
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

---

## Despliegue en AWS

### Con Terraform

```bash
cd terraform

# Configurar credenciales AWS
aws configure

# Inicializar Terraform
terraform init

# Planificar despliegue
terraform plan -var-file="terraform.tfvars.prod"

# Aplicar infraestructura
terraform apply -auto-approve -var-file="terraform.tfvars.prod"
```

### Recursos Creados

- **DynamoDB Tables**: Customers, Funds, Subscriptions, Transactions
- **Lambda Functions**: API handlers serverless
- **API Gateway**: Endpoint público con throttling
- **CloudWatch**: Logs y métricas
- **IAM Roles**: Permisos mínimos requeridos

---

## Configuración de Entorno

### Desarrollo Local

```json
{
  "AWS": {
    "ServiceURL": "http://localhost:8000",
    "AccessKey": "dummy",
    "SecretKey": "dummy",
    "Region": "us-east-1"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "CeibaFunds": "Debug"
    }
  }
}
```

### Variables de Entorno - Producción

```bash
ASPNETCORE_ENVIRONMENT=Production
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=<your-key>
AWS_SECRET_ACCESS_KEY=<your-secret>
```

---

## Seguridad y Validaciones

### Validaciones Implementadas

- **Email**: Formato RFC válido y unicidad
- **Teléfono**: Formato internacional (+57...)
- **Fechas**: Validación de edad mínima (18 años)
- **Montos**: Mínimos de inversión por fondo
- **Enums**: Validación de estados y categorías

### Reglas de Negocio

- **Cliente único por email**
- **Monto mínimo de inversión: $500.000**
- **Balance suficiente para suscripciones**
- **Notificaciones automáticas por transacciones**
- **Una suscripción activa por fondo por cliente**

---

## Monitoreo y Observabilidad

### Logging Estructurado

```json
{
  "@timestamp": "2024-11-01T15:27:07.329Z",
  "@level": "Information",
  "@messageTemplate": "Customer {CustomerId} subscribed to fund {FundId}",
  "CustomerId": "123e4567-e89b-12d3-a456-426614174000",
  "FundId": "987fcdeb-51a2-43d1-9876-543210987654",
  "Amount": 1500000,
  "RequestId": "req-abc123",
  "CorrelationId": "corr-def456"
}
```

### Health Checks

```json
{
  "status": "healthy",
  "timestamp": "2024-11-01T15:27:07.329Z",
  "version": "1.0.0",
  "checks": {
    "database": "healthy",
    "dependencies": "healthy"
  }
}
```

---

## Documentación Adicional

- **[Manual Completo de Usuario](MANUAL-USUARIO.md)** - Guía detallada con ejemplos
- **[Resumen del Proyecto](PROJECT-SUMMARY.md)** - Overview técnico
- **[Guía Docker](DOCKER-GUIDE.md)** - Containerización completa
- **[Deploy en AWS](terraform/README.md)** - Infraestructura como código

---

## Comandos de Desarrollo

### Setup Inicial

```bash
# Compilar solución completa
dotnet build CeibaFunds.sln

# Restaurar paquetes NuGet
dotnet restore CeibaFunds.sln

# Limpiar outputs de build
dotnet clean CeibaFunds.sln
```

### Testing

```bash
# Tests unitarios únicamente
dotnet test tests/CeibaFunds.UnitTests

# Tests de integración
dotnet test tests/CeibaFunds.IntegrationTests

# Todos los tests con reporte detallado
dotnet test --verbosity normal --logger "trx"
```

### Desarrollo Local

```bash
# Modo watch (recarga automática)
dotnet watch run --project src/CeibaFunds.API

# Modo production local
dotnet run --project src/CeibaFunds.API --configuration Release

# Con profile específico
dotnet run --project src/CeibaFunds.API --launch-profile Production
```

---

## Estándares de Desarrollo

### Principios Aplicados

- **Clean Architecture**: Separación de capas y dependencias
- **SOLID Principles**: Single Responsibility, Open/Closed, etc.
- **Domain-Driven Design**: Modelado rico del dominio
- **Test-Driven Development**: Tests como documentación viva
- **CQRS Pattern**: Separación comando/query

### Convenciones de Código

- **Naming**: PascalCase para clases, camelCase para variables
- **Documentation**: XML docs en APIs públicas
- **Testing**: Arrange-Act-Assert pattern
- **Dependencies**: Inyección explícita, no service locator
- **Async/Await**: Task-based asynchrony en I/O operations

---

## 🛠️ Scripts Útiles

| Script                      | Propósito                           | Uso                                       |
| --------------------------- | ----------------------------------- | ----------------------------------------- |
| `scripts/deploy-aws.ps1`    | Desplegar en AWS con CloudFormation | `scripts\deploy-aws.ps1 -Environment dev` |
| `scripts/create-tables.ps1` | Crear tablas DynamoDB local         | `scripts\create-tables.ps1`               |
| `scripts/docker-manage.ps1` | Gestionar contenedores Docker       | `scripts\docker-manage.ps1 start`         |
| `scripts/health-check.ps1`  | Verificar estado de la API          | `scripts\health-check.ps1`                |

---


### **Parte 1 - Sistema de Fondos (80%)**

#### **1a) Justificación Tecnológica** ✅ [`docs/tecnologias-solucion.md`](docs/tecnologias-solucion.md)

- [x] **.NET 8** - Framework principal justificado
- [x] **DynamoDB** - Base de datos NoSQL justificada
- [x] **Clean Architecture** - Patrón arquitectónico justificado
- [x] **CQRS + MediatR** - Patrones de diseño justificados
- [x] **Docker + AWS** - DevOps y deployment justificados

#### **1b) Modelo de Datos NoSQL** ✅ [`docs/modelo-datos-nosql.md`](docs/modelo-datos-nosql.md)

- [x] **Esquema DynamoDB** diseñado y documentado
- [x] **Partition Keys** y Sort Keys definidos
- [x] **Índices GSI** para consultas eficientes
- [x] **Patrones de acceso** identificados y optimizados
- [x] **Modelado de relaciones** en NoSQL

#### **1c) API REST Funcional** ✅ [Ver código completo](src/)

- [x] **Suscripción a fondos** - Endpoint implementado y probado
- [x] **Cancelación de suscripciones** - Endpoint implementado y probado
- [x] **Gestión de clientes** - CRUD completo
- [x] **Historial de transacciones** - Consultas y reportes
- [x] **40+ Tests unitarios** - Cobertura completa [`tests/`](tests/)
- [x] **Documentación OpenAPI** - Swagger UI disponible
- [x] **Manejo de errores** - Respuestas HTTP estándar
- [x] **Validaciones** - FluentValidation implementado

### **Parte 2 - Consulta SQL (20%)**

#### **Query Optimizada** ✅ [`sql-queries/Query Parte 2.sql`](sql-queries/Query%20Parte%202.sql)

- [x] **Consulta correcta** - Obtiene clientes con productos de 3 tipos
- [x] **Optimización** - Uso de EXISTS para performance
- [x] **Sintaxis correcta** - SQL estándar válido
- [x] **Documentación** - Query explicada y comentada

### **Extras - Valor Agregado**

#### **Deployment Production** ✅

- [x] **CloudFormation** - Templates AWS [`cloudformation/`](cloudformation/)
- [x] **Terraform** - Infrastructure as Code [`terraform/`](terraform/)
- [x] **Docker** - Containerización completa
- [x] **CI/CD** - GitHub Actions workflows [`.github/workflows/`](.github/workflows/)

#### **Calidad de Código** ✅

- [x] **Clean Code** - Principios SOLID aplicados
- [x] **Testing** - 40+ pruebas unitarias
- [x] **Documentation** - README profesional
- [x] **Git** - Historial de commits limpio

---

## Licencia

Este proyecto está bajo la **Licencia MIT** - ver el archivo [LICENSE](LICENSE) para detalles.

---

## Información del Desarrollador

> **Proyecto desarrollado utilizando las mejores prácticas de .NET 8, Clean Architecture y principios SOLID.**

**Tecnologías Utilizadas**: .NET 8 • C# 12 • Clean Architecture • CQRS • DynamoDB • Docker • AWS • Terraform

**Patrones Implementados**: Repository • CQRS • Command • Specification • Builder • Factory

---

_Para soporte técnico completo, consultar el [Manual de Usuario](MANUAL-USUARIO.md) o la [documentación de API](http://localhost:5144/swagger) cuando la aplicación esté ejecutándose._
