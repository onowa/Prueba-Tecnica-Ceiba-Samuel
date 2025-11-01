# Primary outputs
output "api_gateway_url" {
  description = "Base URL for API Gateway stage"
  value       = aws_api_gateway_deployment.ceibafunds_deployment.invoke_url
}

output "api_gateway_stage_url" {
  description = "Full URL for API Gateway stage"
  value       = "${aws_api_gateway_deployment.ceibafunds_deployment.invoke_url}${aws_api_gateway_stage.ceibafunds_stage.stage_name}"
}

output "custom_domain_url" {
  description = "Custom domain URL (if configured)"
  value       = var.api_domain != "" ? "https://${var.api_domain}" : null
}

# DynamoDB Table Names
output "dynamodb_table_names" {
  description = "Names of all DynamoDB tables"
  value = {
    customers     = aws_dynamodb_table.customers.name
    funds        = aws_dynamodb_table.funds.name
    subscriptions = aws_dynamodb_table.subscriptions.name
    transactions  = aws_dynamodb_table.transactions.name
  }
}

output "dynamodb_table_arns" {
  description = "ARNs of all DynamoDB tables"
  value = {
    customers     = aws_dynamodb_table.customers.arn
    funds        = aws_dynamodb_table.funds.arn
    subscriptions = aws_dynamodb_table.subscriptions.arn
    transactions  = aws_dynamodb_table.transactions.arn
  }
}

# ECR Repository
output "ecr_repository_url" {
  description = "URL of the ECR repository"
  value       = aws_ecr_repository.ceibafunds_api.repository_url
}

# Lambda Function
output "lambda_function_name" {
  description = "Name of the Lambda function"
  value       = aws_lambda_function.ceibafunds_api.function_name
}

output "lambda_function_arn" {
  description = "ARN of the Lambda function"
  value       = aws_lambda_function.ceibafunds_api.arn
}

# IAM Roles
output "lambda_execution_role_arn" {
  description = "ARN of the Lambda execution role"
  value       = aws_iam_role.lambda_execution_role.arn
}

output "ecs_task_role_arn" {
  description = "ARN of the ECS task role"
  value       = aws_iam_role.ecs_task_role.arn
}

output "ecs_execution_role_arn" {
  description = "ARN of the ECS execution role"
  value       = aws_iam_role.ecs_execution_role.arn
}

# Monitoring
output "cloudwatch_dashboard_url" {
  description = "URL to the CloudWatch dashboard"
  value       = var.enable_monitoring ? "https://${var.aws_region}.console.aws.amazon.com/cloudwatch/home?region=${var.aws_region}#dashboards:name=${aws_cloudwatch_dashboard.ceibafunds_dashboard[0].dashboard_name}" : null
}

output "sns_topic_arn" {
  description = "ARN of the SNS topic for alerts"
  value       = var.enable_monitoring ? aws_sns_topic.alerts[0].arn : null
}

# Environment Configuration
output "environment_variables" {
  description = "Environment variables for application deployment"
  value = {
    ASPNETCORE_ENVIRONMENT = var.environment
    AWS_REGION            = var.aws_region
    DYNAMODB_CUSTOMERS_TABLE = aws_dynamodb_table.customers.name
    DYNAMODB_FUNDS_TABLE     = aws_dynamodb_table.funds.name
    DYNAMODB_SUBSCRIPTIONS_TABLE = aws_dynamodb_table.subscriptions.name
    DYNAMODB_TRANSACTIONS_TABLE = aws_dynamodb_table.transactions.name
  }
  sensitive = false
}

# Security Information
output "security_group_id" {
  description = "ID of the Lambda security group (if VPC is used)"
  value       = length(var.subnet_ids) > 0 ? aws_security_group.lambda_sg[0].id : null
}

# Resource Summary
output "resource_summary" {
  description = "Summary of created resources"
  value = {
    api_gateway = {
      name = aws_api_gateway_rest_api.ceibafunds_api.name
      id   = aws_api_gateway_rest_api.ceibafunds_api.id
      url  = aws_api_gateway_deployment.ceibafunds_deployment.invoke_url
    }
    lambda = {
      name = aws_lambda_function.ceibafunds_api.function_name
      arn  = aws_lambda_function.ceibafunds_api.arn
    }
    dynamodb_tables = {
      customers     = aws_dynamodb_table.customers.name
      funds        = aws_dynamodb_table.funds.name
      subscriptions = aws_dynamodb_table.subscriptions.name
      transactions  = aws_dynamodb_table.transactions.name
    }
    ecr_repository = aws_ecr_repository.ceibafunds_api.repository_url
    monitoring_enabled = var.enable_monitoring
  }
}