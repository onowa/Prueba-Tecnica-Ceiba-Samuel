# API Gateway REST API
resource "aws_api_gateway_rest_api" "ceibafunds_api" {
  name        = "${local.name_prefix}-api"
  description = "CeibaFunds Investment API"

  endpoint_configuration {
    types = ["REGIONAL"]
  }

  binary_media_types = ["application/octet-stream"]

  tags = local.common_tags
}

# API Gateway Deployment
resource "aws_api_gateway_deployment" "ceibafunds_deployment" {
  rest_api_id = aws_api_gateway_rest_api.ceibafunds_api.id

  triggers = {
    redeployment = sha1(jsonencode([
      aws_api_gateway_resource.proxy.id,
      aws_api_gateway_method.proxy_method.id,
      aws_api_gateway_integration.lambda_integration.id,
    ]))
  }

  lifecycle {
    create_before_destroy = true
  }

  depends_on = [
    aws_api_gateway_method.proxy_method,
    aws_api_gateway_integration.lambda_integration,
  ]
}

# API Gateway Stage
resource "aws_api_gateway_stage" "ceibafunds_stage" {
  deployment_id = aws_api_gateway_deployment.ceibafunds_deployment.id
  rest_api_id   = aws_api_gateway_rest_api.ceibafunds_api.id
  stage_name    = var.environment

  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.api_logs.arn
    format = jsonencode({
      requestId      = "$context.requestId"
      ip             = "$context.identity.sourceIp"
      caller         = "$context.identity.caller"
      user           = "$context.identity.user"
      requestTime    = "$context.requestTime"
      httpMethod     = "$context.httpMethod"
      resourcePath   = "$context.resourcePath"
      status         = "$context.status"
      protocol       = "$context.protocol"
      responseLength = "$context.responseLength"
      error          = "$context.error.message"
      integrationError = "$context.integration.error"
    })
  }

  tags = local.common_tags
}

# Throttling settings
resource "aws_api_gateway_method_settings" "settings" {
  rest_api_id = aws_api_gateway_rest_api.ceibafunds_api.id
  stage_name  = aws_api_gateway_stage.ceibafunds_stage.stage_name
  method_path = "*/*"

  settings {
    metrics_enabled = true
    logging_level   = "INFO"
    
    throttling_rate_limit  = var.api_throttle_rate_limit
    throttling_burst_limit = var.api_throttle_burst_limit
  }
}

# API Gateway Resource (Proxy)
resource "aws_api_gateway_resource" "proxy" {
  rest_api_id = aws_api_gateway_rest_api.ceibafunds_api.id
  parent_id   = aws_api_gateway_rest_api.ceibafunds_api.root_resource_id
  path_part   = "{proxy+}"
}

# API Gateway Method
resource "aws_api_gateway_method" "proxy_method" {
  rest_api_id   = aws_api_gateway_rest_api.ceibafunds_api.id
  resource_id   = aws_api_gateway_resource.proxy.id
  http_method   = "ANY"
  authorization = "NONE"
}

# API Gateway Integration
resource "aws_api_gateway_integration" "lambda_integration" {
  rest_api_id = aws_api_gateway_rest_api.ceibafunds_api.id
  resource_id = aws_api_gateway_method.proxy_method.resource_id
  http_method = aws_api_gateway_method.proxy_method.http_method

  integration_http_method = "POST"
  type                   = "AWS_PROXY"
  uri                    = aws_lambda_function.ceibafunds_api.invoke_arn
}

# API Gateway Method for root
resource "aws_api_gateway_method" "proxy_root" {
  rest_api_id   = aws_api_gateway_rest_api.ceibafunds_api.id
  resource_id   = aws_api_gateway_rest_api.ceibafunds_api.root_resource_id
  http_method   = "ANY"
  authorization = "NONE"
}

resource "aws_api_gateway_integration" "lambda_root" {
  rest_api_id = aws_api_gateway_rest_api.ceibafunds_api.id
  resource_id = aws_api_gateway_method.proxy_root.resource_id
  http_method = aws_api_gateway_method.proxy_root.http_method

  integration_http_method = "POST"
  type                   = "AWS_PROXY"
  uri                    = aws_lambda_function.ceibafunds_api.invoke_arn
}

# Lambda permission for API Gateway
resource "aws_lambda_permission" "api_gw" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.ceibafunds_api.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_rest_api.ceibafunds_api.execution_arn}/*/*"
}

# Custom Domain (optional)
resource "aws_api_gateway_domain_name" "ceibafunds_domain" {
  count           = var.api_domain != "" ? 1 : 0
  domain_name     = var.api_domain
  certificate_arn = var.certificate_arn
  security_policy = "TLS_1_2"

  endpoint_configuration {
    types = ["REGIONAL"]
  }

  tags = local.common_tags
}

resource "aws_api_gateway_base_path_mapping" "ceibafunds_mapping" {
  count       = var.api_domain != "" ? 1 : 0
  api_id      = aws_api_gateway_rest_api.ceibafunds_api.id
  stage_name  = aws_api_gateway_stage.ceibafunds_stage.stage_name
  domain_name = aws_api_gateway_domain_name.ceibafunds_domain[0].domain_name
}