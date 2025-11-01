#!/bin/bash
# Inicializador de tablas DynamoDB para desarrollo local

set -e

echo "Iniciando configuración de DynamoDB Local..."

# Configurar endpoint de DynamoDB Local
DYNAMODB_ENDPOINT="http://dynamodb-local:8000"

# Función para crear tabla si no existe
create_table_if_not_exists() {
    local table_name=$1
    local table_definition=$2
    
    echo "Verificando tabla: $table_name"
    
    # Verificar si la tabla existe
    if aws dynamodb describe-table --table-name "$table_name" --endpoint-url "$DYNAMODB_ENDPOINT" >/dev/null 2>&1; then
        echo "Tabla $table_name ya existe"
    else
        echo "Creando tabla: $table_name"
        aws dynamodb create-table \
            --endpoint-url "$DYNAMODB_ENDPOINT" \
            $table_definition
        
        # Esperar a que la tabla esté activa
        echo "⏳ Esperando que la tabla $table_name esté activa..."
        aws dynamodb wait table-exists \
            --table-name "$table_name" \
            --endpoint-url "$DYNAMODB_ENDPOINT"
        
        echo "Tabla $table_name creada exitosamente"
    fi
}

echo "Creando tablas de CeibaFunds..."

# Tabla de Customers
create_table_if_not_exists "Customers" \
    "--table-name Customers \
     --attribute-definitions \
         AttributeName=Id,AttributeType=S \
         AttributeName=Email,AttributeType=S \
     --key-schema \
         AttributeName=Id,KeyType=HASH \
     --global-secondary-indexes \
         IndexName=EmailIndex,KeySchema=[{AttributeName=Email,KeyType=HASH}],Projection={ProjectionType=ALL},ProvisionedThroughput={ReadCapacityUnits=5,WriteCapacityUnits=5} \
     --provisioned-throughput \
         ReadCapacityUnits=5,WriteCapacityUnits=5"

# Tabla de Funds
create_table_if_not_exists "Funds" \
    "--table-name Funds \
     --attribute-definitions \
         AttributeName=Id,AttributeType=S \
         AttributeName=Category,AttributeType=S \
     --key-schema \
         AttributeName=Id,KeyType=HASH \
     --global-secondary-indexes \
         IndexName=CategoryIndex,KeySchema=[{AttributeName=Category,KeyType=HASH}],Projection={ProjectionType=ALL},ProvisionedThroughput={ReadCapacityUnits=5,WriteCapacityUnits=5} \
     --provisioned-throughput \
         ReadCapacityUnits=5,WriteCapacityUnits=5"

# Tabla de Subscriptions
create_table_if_not_exists "Subscriptions" \
    "--table-name Subscriptions \
     --attribute-definitions \
         AttributeName=Id,AttributeType=S \
         AttributeName=CustomerId,AttributeType=S \
         AttributeName=FundId,AttributeType=S \
     --key-schema \
         AttributeName=Id,KeyType=HASH \
     --global-secondary-indexes \
         IndexName=CustomerIndex,KeySchema=[{AttributeName=CustomerId,KeyType=HASH}],Projection={ProjectionType=ALL},ProvisionedThroughput={ReadCapacityUnits=5,WriteCapacityUnits=5} \
         IndexName=FundIndex,KeySchema=[{AttributeName=FundId,KeyType=HASH}],Projection={ProjectionType=ALL},ProvisionedThroughput={ReadCapacityUnits=5,WriteCapacityUnits=5} \
     --provisioned-throughput \
         ReadCapacityUnits=5,WriteCapacityUnits=5"

# Tabla de Transactions
create_table_if_not_exists "Transactions" \
    "--table-name Transactions \
     --attribute-definitions \
         AttributeName=Id,AttributeType=S \
         AttributeName=CustomerId,AttributeType=S \
         AttributeName=CreatedAt,AttributeType=S \
     --key-schema \
         AttributeName=Id,KeyType=HASH \
     --global-secondary-indexes \
         IndexName=CustomerDateIndex,KeySchema=[{AttributeName=CustomerId,KeyType=HASH},{AttributeName=CreatedAt,KeyType=RANGE}],Projection={ProjectionType=ALL},ProvisionedThroughput={ReadCapacityUnits=5,WriteCapacityUnits=5} \
     --provisioned-throughput \
         ReadCapacityUnits=5,WriteCapacityUnits=5"

echo ""
echo "¡Configuración de DynamoDB completada exitosamente!"
echo "Tablas disponibles:"
aws dynamodb list-tables --endpoint-url "$DYNAMODB_ENDPOINT"

echo ""
echo "URLs útiles:"
echo "   • API: http://localhost:5000"
echo "   • DynamoDB Local: http://localhost:8000"
echo "   • DynamoDB Admin: http://localhost:8001"
echo "   • Swagger UI: http://localhost:5000/swagger"
echo ""
echo "¡Todo listo para desarrollo!"
