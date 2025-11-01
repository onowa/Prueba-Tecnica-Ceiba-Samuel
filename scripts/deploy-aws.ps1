# Script de despliegue para AWS CloudFormation
param(
    [string]$Environment = "dev",
    [string]$Region = "us-east-1",
    [string]$StackName = "ceibafunds-api"
)

Write-Host "🚀 Iniciando despliegue de CeibaFunds API en AWS" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Region: $Region" -ForegroundColor Yellow
Write-Host "Stack: $StackName-$Environment" -ForegroundColor Yellow

# Verificar AWS CLI
try {
    $awsVersion = aws --version
    Write-Host "✅ AWS CLI encontrado: $awsVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ AWS CLI no está instalado. Instalalo primero:" -ForegroundColor Red
    Write-Host "   https://aws.amazon.com/cli/" -ForegroundColor Yellow
    exit 1
}

# Verificar credenciales AWS
try {
    $account = aws sts get-caller-identity --query "Account" --output text
    Write-Host "✅ Conectado a AWS Account: $account" -ForegroundColor Green
} catch {
    Write-Host "❌ No hay credenciales AWS configuradas. Ejecuta:" -ForegroundColor Red
    Write-Host "   aws configure" -ForegroundColor Yellow
    exit 1
}

# Paso 1: Crear/Actualizar la infraestructura
Write-Host "`n📦 Paso 1: Desplegando infraestructura..." -ForegroundColor Cyan

$stackExists = aws cloudformation describe-stacks --stack-name "$StackName-$Environment" --region $Region 2>$null
if ($stackExists) {
    Write-Host "🔄 Actualizando stack existente..." -ForegroundColor Yellow
    $command = "update-stack"
} else {
    Write-Host "🆕 Creando nuevo stack..." -ForegroundColor Yellow
    $command = "create-stack"
}

aws cloudformation $command `
    --stack-name "$StackName-$Environment" `
    --template-body file://cloudformation/infrastructure.yaml `
    --parameters ParameterKey=Environment,ParameterValue=$Environment `
    --capabilities CAPABILITY_IAM `
    --region $Region

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Comando CloudFormation ejecutado correctamente" -ForegroundColor Green
    
    # Esperar a que termine
    Write-Host "⏳ Esperando que termine el despliegue..." -ForegroundColor Yellow
    aws cloudformation wait stack-$command-complete --stack-name "$StackName-$Environment" --region $Region
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "🎉 ¡Stack desplegado exitosamente!" -ForegroundColor Green
    } else {
        Write-Host "❌ Error durante el despliegue" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "❌ Error ejecutando CloudFormation" -ForegroundColor Red
    exit 1
}

# Paso 2: Obtener outputs del stack
Write-Host "`n📊 Información del despliegue:" -ForegroundColor Cyan

$outputs = aws cloudformation describe-stacks --stack-name "$StackName-$Environment" --region $Region --query "Stacks[0].Outputs" --output table

if ($outputs) {
    Write-Host $outputs
    
    # Obtener URL de la API
    $apiUrl = aws cloudformation describe-stacks --stack-name "$StackName-$Environment" --region $Region --query "Stacks[0].Outputs[?OutputKey=='ApiEndpoint'].OutputValue" --output text
    
    if ($apiUrl) {
        Write-Host "`n🌐 Tu API está disponible en:" -ForegroundColor Green
        Write-Host "   $apiUrl" -ForegroundColor White
        Write-Host "`n📝 Ejemplos de uso:" -ForegroundColor Cyan
        Write-Host "   Health Check: $apiUrl/health" -ForegroundColor White
        Write-Host "   Swagger UI: $apiUrl/swagger" -ForegroundColor White
        Write-Host "   Customers: $apiUrl/api/customers" -ForegroundColor White
    }
}

# Paso 3: Instrucciones para desplegar el código
Write-Host "`n📤 Para desplegar el código de la API:" -ForegroundColor Cyan
Write-Host "1. Compilar la aplicación:" -ForegroundColor Yellow
Write-Host "   dotnet publish src/CeibaFunds.API -c Release -o publish" -ForegroundColor White
Write-Host "`n2. Crear ZIP del deployment:" -ForegroundColor Yellow
Write-Host "   Compress-Archive -Path publish/* -DestinationPath ceibafunds-api.zip" -ForegroundColor White
Write-Host "`n3. Subir a S3:" -ForegroundColor Yellow
Write-Host "   aws s3 cp ceibafunds-api.zip s3://ceibafunds-deployments-$Environment-$account/ceibafunds-api.zip" -ForegroundColor White
Write-Host "`n4. Actualizar función Lambda:" -ForegroundColor Yellow
Write-Host "   aws lambda update-function-code --function-name ceibafunds-api-$Environment --s3-bucket ceibafunds-deployments-$Environment-$account --s3-key ceibafunds-api.zip" -ForegroundColor White

Write-Host "`n✨ ¡Despliegue completado!" -ForegroundColor Green