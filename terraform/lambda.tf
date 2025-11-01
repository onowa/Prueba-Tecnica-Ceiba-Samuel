# ECR Repository for Docker images
resource "aws_ecr_repository" "ceibafunds_api" {
  name                 = "${local.name_prefix}-api"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = local.common_tags
}

# ECR Lifecycle Policy
resource "aws_ecr_lifecycle_policy" "ceibafunds_api_policy" {
  repository = aws_ecr_repository.ceibafunds_api.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Keep last 10 production images"
        selection = {
          tagStatus     = "tagged"
          tagPrefixList = ["prod"]
          countType     = "imageCountMoreThan"
          countNumber   = 10
        }
        action = {
          type = "expire"
        }
      },
      {
        rulePriority = 2
        description  = "Keep last 5 staging images"
        selection = {
          tagStatus     = "tagged"
          tagPrefixList = ["staging"]
          countType     = "imageCountMoreThan"
          countNumber   = 5
        }
        action = {
          type = "expire"
        }
      },
      {
        rulePriority = 3
        description  = "Keep last 3 dev images"
        selection = {
          tagStatus     = "tagged"
          tagPrefixList = ["dev"]
          countType     = "imageCountMoreThan"
          countNumber   = 3
        }
        action = {
          type = "expire"
        }
      },
      {
        rulePriority = 4
        description  = "Delete untagged images older than 1 day"
        selection = {
          tagStatus   = "untagged"
          countType   = "sinceImagePushed"
          countUnit   = "days"
          countNumber = 1
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}

# Lambda function (for serverless deployment option)
resource "aws_lambda_function" "ceibafunds_api" {
  filename         = "placeholder.zip"
  function_name    = "${local.name_prefix}-api"
  role            = aws_iam_role.lambda_execution_role.arn
  handler         = "bootstrap"
  source_code_hash = data.archive_file.lambda_placeholder.output_base64sha256
  runtime         = "provided.al2"
  timeout         = 30
  memory_size     = 512

  dynamic "vpc_config" {
    for_each = length(var.subnet_ids) > 0 ? [1] : []
    content {
      subnet_ids         = var.subnet_ids
      security_group_ids = [aws_security_group.lambda_sg[0].id]
    }
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = var.environment
      AWS_REGION            = var.aws_region
      DYNAMODB_CUSTOMERS_TABLE = aws_dynamodb_table.customers.name
      DYNAMODB_FUNDS_TABLE     = aws_dynamodb_table.funds.name
      DYNAMODB_SUBSCRIPTIONS_TABLE = aws_dynamodb_table.subscriptions.name
      DYNAMODB_TRANSACTIONS_TABLE = aws_dynamodb_table.transactions.name
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.lambda_basic_execution,
    aws_cloudwatch_log_group.lambda_logs,
  ]

  tags = local.common_tags
}

# Placeholder zip file for Lambda (will be replaced by CI/CD)
data "archive_file" "lambda_placeholder" {
  type        = "zip"
  output_path = "placeholder.zip"
  source {
    content  = "placeholder"
    filename = "bootstrap"
  }
}

# Security Group for Lambda (if VPC is used)
resource "aws_security_group" "lambda_sg" {
  count       = length(var.subnet_ids) > 0 ? 1 : 0
  name        = "${local.name_prefix}-lambda-sg"
  description = "Security group for Lambda function"
  vpc_id      = var.vpc_id

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-lambda-sg"
  })
}