# Random suffix for unique resource names
resource "random_id" "suffix" {
  byte_length = 4
}

locals {
  name_prefix = "${var.project_name}-${var.environment}"
  common_tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# DynamoDB Tables
resource "aws_dynamodb_table" "customers" {
  name           = "${local.name_prefix}-customers-${random_id.suffix.hex}"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "Id"
  stream_enabled = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "Email"
    type = "S"
  }

  global_secondary_index {
    name     = "EmailIndex"
    hash_key = "Email"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = var.enable_backup
  }

  server_side_encryption {
    enabled = true
  }

  tags = merge(local.common_tags, {
    Name = "CeibaFunds Customers Table"
  })
}

resource "aws_dynamodb_table" "funds" {
  name           = "${local.name_prefix}-funds-${random_id.suffix.hex}"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "Id"
  stream_enabled = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "Category"
    type = "S"
  }

  attribute {
    name = "IsActive"
    type = "S"
  }

  global_secondary_index {
    name     = "CategoryIndex"
    hash_key = "Category"
    range_key = "IsActive"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = var.enable_backup
  }

  server_side_encryption {
    enabled = true
  }

  tags = merge(local.common_tags, {
    Name = "CeibaFunds Funds Table"
  })
}

resource "aws_dynamodb_table" "subscriptions" {
  name           = "${local.name_prefix}-subscriptions-${random_id.suffix.hex}"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "Id"
  stream_enabled = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "CustomerId"
    type = "S"
  }

  attribute {
    name = "FundId"
    type = "S"
  }

  attribute {
    name = "Status"
    type = "S"
  }

  global_secondary_index {
    name     = "CustomerIdIndex"
    hash_key = "CustomerId"
    range_key = "Status"
    projection_type = "ALL"
  }

  global_secondary_index {
    name     = "FundIdIndex"
    hash_key = "FundId"
    range_key = "Status"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = var.enable_backup
  }

  server_side_encryption {
    enabled = true
  }

  tags = merge(local.common_tags, {
    Name = "CeibaFunds Subscriptions Table"
  })
}

resource "aws_dynamodb_table" "transactions" {
  name           = "${local.name_prefix}-transactions-${random_id.suffix.hex}"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "Id"
  range_key      = "CreatedAt"
  stream_enabled = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "CreatedAt"
    type = "S"
  }

  attribute {
    name = "CustomerId"
    type = "S"
  }

  attribute {
    name = "SubscriptionId"
    type = "S"
  }

  global_secondary_index {
    name     = "CustomerIdIndex"
    hash_key = "CustomerId"
    range_key = "CreatedAt"
    projection_type = "ALL"
  }

  global_secondary_index {
    name     = "SubscriptionIdIndex"
    hash_key = "SubscriptionId"
    range_key = "CreatedAt"
    projection_type = "ALL"
  }

  point_in_time_recovery {
    enabled = var.enable_backup
  }

  server_side_encryption {
    enabled = true
  }

  tags = merge(local.common_tags, {
    Name = "CeibaFunds Transactions Table"
  })
}