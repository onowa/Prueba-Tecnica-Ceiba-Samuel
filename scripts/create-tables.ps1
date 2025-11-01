# Script para crear tablas DynamoDB Local
$endpoint = "http://localhost:8000"

Write-Host "🚀 Creando tablas DynamoDB Local..." -ForegroundColor Green

# Función para crear tabla usando HTTP API
function Create-DynamoTable {
    param(
        [string]$TableName,
        [hashtable]$TableDefinition
    )
    
    # Variables de entorno para DynamoDB Local
    $env:AWS_ACCESS_KEY_ID = "dummy"
    $env:AWS_SECRET_ACCESS_KEY = "dummy"
    $env:AWS_DEFAULT_REGION = "us-east-1"
    
    $headers = @{
        "Content-Type" = "application/x-amz-json-1.0"
        "X-Amz-Target" = "DynamoDB_20120810.CreateTable"
    }
    
    $body = $TableDefinition | ConvertTo-Json -Depth 10
    
    try {
        Write-Host "Creando tabla: $TableName" -ForegroundColor Yellow
        $response = Invoke-RestMethod -Uri $endpoint -Method POST -Headers $headers -Body $body
        Write-Host "✅ Tabla $TableName creada exitosamente" -ForegroundColor Green
        return $true
    } catch {
        if ($_.Exception.Message -like "*ResourceInUseException*") {
            Write-Host "⚠️ Tabla $TableName ya existe" -ForegroundColor Yellow
            return $true
        } else {
            Write-Host "❌ Error creando tabla $TableName : $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
}

# Tabla Funds
$fundsTable = @{
    TableName = "Funds"
    AttributeDefinitions = @(
        @{AttributeName="Id"; AttributeType="S"}
    )
    KeySchema = @(
        @{AttributeName="Id"; KeyType="HASH"}
    )
    ProvisionedThroughput = @{ReadCapacityUnits=5; WriteCapacityUnits=5}
}

# Tabla Customers  
$customersTable = @{
    TableName = "Customers"
    AttributeDefinitions = @(
        @{AttributeName="Id"; AttributeType="S"}
    )
    KeySchema = @(
        @{AttributeName="Id"; KeyType="HASH"}
    )
    ProvisionedThroughput = @{ReadCapacityUnits=5; WriteCapacityUnits=5}
}

# Tabla Subscriptions
$subscriptionsTable = @{
    TableName = "Subscriptions"  
    AttributeDefinitions = @(
        @{AttributeName="Id"; AttributeType="S"}
    )
    KeySchema = @(
        @{AttributeName="Id"; KeyType="HASH"}
    )
    ProvisionedThroughput = @{ReadCapacityUnits=5; WriteCapacityUnits=5}
}

# Tabla Transactions
$transactionsTable = @{
    TableName = "Transactions"
    AttributeDefinitions = @(
        @{AttributeName="Id"; AttributeType="S"}
    )
    KeySchema = @(
        @{AttributeName="Id"; KeyType="HASH"}
    )
    ProvisionedThroughput = @{ReadCapacityUnits=5; WriteCapacityUnits=5}
}

# Crear todas las tablas
$success = $true
$success = $success -and (Create-DynamoTable -TableName "Funds" -TableDefinition $fundsTable)
$success = $success -and (Create-DynamoTable -TableName "Customers" -TableDefinition $customersTable) 
$success = $success -and (Create-DynamoTable -TableName "Subscriptions" -TableDefinition $subscriptionsTable)
$success = $success -and (Create-DynamoTable -TableName "Transactions" -TableDefinition $transactionsTable)

if ($success) {
    Write-Host "🎉 ¡Todas las tablas creadas exitosamente!" -ForegroundColor Green
    Write-Host "Puedes verificar en: http://localhost:8001" -ForegroundColor Cyan
} else {
    Write-Host "⚠️ Algunas tablas no se pudieron crear" -ForegroundColor Yellow
}