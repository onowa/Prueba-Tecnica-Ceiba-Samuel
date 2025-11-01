# Guía de Despliegue AWS CloudFormation

## 🎯 Objetivo

Esta guía te explica cómo desplegar la **CeibaFunds API** en AWS usando CloudFormation.

## 📋 Prerrequisitos

### 1. AWS CLI Instalado

```bash
# Verificar instalación
aws --version

# Si no está instalado, descargar de:
# https://aws.amazon.com/cli/
```

### 2. Credenciales AWS Configuradas

```bash
# Configurar credenciales
aws configure

# Verificar conexión
aws sts get-caller-identity
```

### 3. Permisos Necesarios

Tu usuario AWS necesita permisos para:

- CloudFormation (crear/actualizar stacks)
- DynamoDB (crear tablas)
- Lambda (crear funciones)
- API Gateway (crear APIs)
- IAM (crear roles)
- S3 (crear buckets)

## 🚀 Despliegue Automático

### Opción A: Script PowerShell (Recomendado)

```powershell
# Despliegue en desarrollo
..\scripts\deploy-aws.ps1 -Environment dev

# Despliegue en producción
..\scripts\deploy-aws.ps1 -Environment prod -Region us-west-2
```

### Opción B: Comandos Manuales

#### 1. Crear Stack de Infraestructura

```bash
aws cloudformation create-stack \
  --stack-name ceibafunds-api-dev \
  --template-body file://cloudformation/infrastructure.yaml \
  --parameters ParameterKey=Environment,ParameterValue=dev \
  --capabilities CAPABILITY_IAM \
  --region us-east-1
```

#### 2. Esperar que Complete

```bash
aws cloudformation wait stack-create-complete \
  --stack-name ceibafunds-api-dev \
  --region us-east-1
```

#### 3. Verificar Recursos Creados

```bash
aws cloudformation describe-stacks \
  --stack-name ceibafunds-api-dev \
  --region us-east-1 \
  --query "Stacks[0].Outputs"
```

## 📦 Desplegar el Código de la API

### 1. Compilar la Aplicación

```bash
dotnet publish src/CeibaFunds.API -c Release -o publish
```

### 2. Crear Package de Deployment

```powershell
# Windows
Compress-Archive -Path publish/* -DestinationPath ceibafunds-api.zip

# Linux/Mac
zip -r ceibafunds-api.zip publish/
```

### 3. Subir a S3

```bash
# Obtener nombre del bucket (creado por CloudFormation)
BUCKET_NAME=$(aws cloudformation describe-stacks \
  --stack-name ceibafunds-api-dev \
  --query "Stacks[0].Outputs[?OutputKey=='DeploymentBucket'].OutputValue" \
  --output text)

# Subir ZIP
aws s3 cp ceibafunds-api.zip s3://$BUCKET_NAME/ceibafunds-api.zip
```

### 4. Actualizar Función Lambda

```bash
aws lambda update-function-code \
  --function-name ceibafunds-api-dev \
  --s3-bucket $BUCKET_NAME \
  --s3-key ceibafunds-api.zip
```

## 🌐 Recursos Creados

El CloudFormation crea los siguientes recursos:

### DynamoDB Tables

- `Customers-dev` - Tabla de clientes
- `Funds-dev` - Tabla de fondos
- `Subscriptions-dev` - Tabla de suscripciones
- `Transactions-dev` - Tabla de transacciones

### Lambda Function

- `ceibafunds-api-dev` - Función que ejecuta tu .NET API

### API Gateway

- REST API con endpoint público
- Integración con Lambda
- URL: `https://xxxxxxxx.execute-api.us-east-1.amazonaws.com/dev`

### IAM Roles

- `LambdaExecutionRole` - Permisos para Lambda acceder a DynamoDB

### S3 Bucket

- `ceibafunds-deployments-dev-ACCOUNT_ID` - Para deployments

## 🧪 Verificar Despliegue

### 1. Health Check

```bash
# Obtener URL de la API
API_URL=$(aws cloudformation describe-stacks \
  --stack-name ceibafunds-api-dev \
  --query "Stacks[0].Outputs[?OutputKey=='ApiEndpoint'].OutputValue" \
  --output text)

# Probar health endpoint
curl $API_URL/health
```

### 2. Probar Endpoints

```bash
# Listar fondos
curl $API_URL/api/funds

# Crear cliente (ejemplo)
curl -X POST $API_URL/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan.perez@email.com",
    "phoneNumber": "+57301234567",
    "dateOfBirth": "1990-01-15"
  }'
```

### 3. Ver Swagger UI

Abrir en navegador: `$API_URL/swagger`

## 🔧 Troubleshooting

### Error: Stack ya existe

```bash
# Actualizar en lugar de crear
aws cloudformation update-stack \
  --stack-name ceibafunds-api-dev \
  --template-body file://cloudformation/infrastructure.yaml \
  --parameters ParameterKey=Environment,ParameterValue=dev \
  --capabilities CAPABILITY_IAM
```

### Error: Permisos insuficientes

- Verificar que tu usuario tenga los permisos listados en prerrequisitos
- Contactar administrador AWS para otorgar permisos

### Error: Función Lambda no responde

- Verificar logs en CloudWatch: `/aws/lambda/ceibafunds-api-dev`
- Verificar que el código se subió correctamente al S3

### Error: DynamoDB Access Denied

- El CloudFormation crea automáticamente los permisos IAM necesarios
- Si hay error, verificar que el rol `LambdaExecutionRole` tenga las políticas correctas

## 🗑️ Limpiar Recursos

Para eliminar todo lo creado:

```bash
# Eliminar stack (borra todos los recursos)
aws cloudformation delete-stack --stack-name ceibafunds-api-dev

# Esperar que termine
aws cloudformation wait stack-delete-complete --stack-name ceibafunds-api-dev
```

⚠️ **Nota**: Esto eliminará TODOS los datos de DynamoDB. Asegúrate de hacer backup si es necesario.

## 💡 Tips de Producción

### 1. Usar diferentes ambientes

```bash
# Desarrollo
..\scripts\deploy-aws.ps1 -Environment dev

# Staging
..\scripts\deploy-aws.ps1 -Environment staging

# Producción
..\scripts\deploy-aws.ps1 -Environment prod
```

### 2. Monitoreo

- Los logs se guardan automáticamente en CloudWatch
- Configurar alarmas para errores y latencia
- API Gateway incluye métricas automáticas

### 3. Seguridad

- El template incluye permisos mínimos necesarios
- Considerar agregar WAF para protección adicional
- Usar HTTPS (incluido automáticamente en API Gateway)

### 4. Escalabilidad

- DynamoDB configurado como PAY_PER_REQUEST (escala automáticamente)
- Lambda escala automáticamente hasta 1000 instancias concurrentes
- API Gateway maneja hasta 10,000 requests/segundo por defecto

---

**¡Listo!** Tu API de CeibaFunds estará funcionando en AWS con infraestructura profesional y escalable. 🎉
