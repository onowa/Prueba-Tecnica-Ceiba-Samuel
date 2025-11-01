# CeibaFunds AWS Infrastructure with Terraform

## 🏗️ Overview

This directory contains the complete AWS infrastructure configuration for the CeibaFunds API using Terraform. It provides a production-ready setup with multiple environments, monitoring, and security best practices.

## 📋 Infrastructure Components

### Core Services

- **API Gateway** - RESTful API endpoint with throttling and logging
- **Lambda Functions** - Serverless .NET 8 runtime (alternative to ECS)
- **DynamoDB Tables** - NoSQL database with encryption and backup
- **ECR Repository** - Container registry for Docker images
- **CloudWatch** - Logging, monitoring, and alerting
- **IAM Roles** - Secure access management

### Optional Components

- **Custom Domain** - Route 53 and ACM certificate integration
- **VPC Configuration** - Network isolation for Lambda functions
- **SNS Topics** - Alert notifications
- **CloudWatch Dashboard** - Infrastructure monitoring

## 🚀 Quick Start

### Prerequisites

```bash
# Install Terraform
# Download from: https://www.terraform.io/downloads.html

# Install AWS CLI
# Download from: https://aws.amazon.com/cli/

# Configure AWS credentials
aws configure
```

### Deploy Infrastructure

#### Using PowerShell (Windows)

```powershell
# Initialize and deploy development environment
.\terraform-deploy.ps1 dev init
.\terraform-deploy.ps1 dev plan
.\terraform-deploy.ps1 dev apply

# Deploy to production
.\terraform-deploy.ps1 prod init
.\terraform-deploy.ps1 prod apply
```

#### Using Bash (Linux/Mac)

```bash
# Make script executable
chmod +x terraform-deploy.sh

# Initialize and deploy development environment
./terraform-deploy.sh dev init
./terraform-deploy.sh dev plan
./terraform-deploy.sh dev apply

# Deploy to production
./terraform-deploy.sh prod init
./terraform-deploy.sh prod apply
```

#### Using Terraform directly

```bash
# Initialize
terraform init

# Plan deployment
terraform plan -var-file="terraform.tfvars.dev"

# Apply changes
terraform apply -var-file="terraform.tfvars.dev"
```

## 🌍 Environments

### Development (`dev`)

- Minimal monitoring
- No backup enabled
- Lower API throttling limits
- 7-day log retention

### Staging (`staging`)

- Full monitoring enabled
- Backup enabled
- Moderate API limits
- 14-day log retention

### Production (`prod`)

- Full monitoring and alerting
- Backup and encryption enabled
- High API throttling limits
- 30-day log retention
- Custom domain support

## 📊 Configuration

### Environment Variables

Each environment uses specific configuration files:

- `terraform.tfvars.dev` - Development settings
- `terraform.tfvars.staging` - Staging settings
- `terraform.tfvars.prod` - Production settings

### Key Variables

```hcl
# Basic Configuration
environment = "dev"
aws_region  = "us-east-1"
project_name = "ceibafunds"

# Monitoring
enable_monitoring = true
enable_backup = true
log_retention_days = 30

# API Throttling
api_throttle_rate_limit = 200
api_throttle_burst_limit = 500

# Optional: Custom Domain
api_domain = "api.ceibafunds.com"
certificate_arn = "arn:aws:acm:us-east-1:ACCOUNT:certificate/CERT-ID"
```

## 🗄️ DynamoDB Tables

The infrastructure creates four DynamoDB tables with appropriate indexes:

### Tables Created

1. **customers** - Customer information

   - Primary Key: `Id` (String)
   - GSI: `EmailIndex` on `Email`

2. **funds** - Investment funds

   - Primary Key: `Id` (String)
   - GSI: `CategoryIndex` on `Category` + `IsActive`

3. **subscriptions** - Customer fund subscriptions

   - Primary Key: `Id` (String)
   - GSI: `CustomerIdIndex` on `CustomerId` + `Status`
   - GSI: `FundIdIndex` on `FundId` + `Status`

4. **transactions** - Transaction history
   - Primary Key: `Id` (String) + `CreatedAt` (String)
   - GSI: `CustomerIdIndex` on `CustomerId` + `CreatedAt`
   - GSI: `SubscriptionIdIndex` on `SubscriptionId` + `CreatedAt`

### Features

- **Encryption**: Server-side encryption enabled
- **Streams**: DynamoDB Streams for change tracking
- **Backup**: Point-in-time recovery (production)
- **Billing**: Pay-per-request pricing model

## 🔐 Security

### IAM Roles

- **Lambda Execution Role** - DynamoDB access and logging
- **ECS Task Role** - Application permissions
- **API Gateway Role** - CloudWatch logging

### Security Features

- Encrypted DynamoDB tables
- VPC isolation for Lambda (optional)
- Security groups for network access
- IAM least privilege access
- ECR vulnerability scanning

## 📈 Monitoring

### CloudWatch Alarms

- API Gateway 4XX/5XX errors
- High latency alerts
- DynamoDB throttling
- Custom application metrics

### Dashboard

- API Gateway metrics
- DynamoDB performance
- Lambda function stats
- Cost monitoring

### Alerting

- SNS topics for notifications
- Email/SMS alert integration
- Automated incident response

## 🏃‍♂️ Usage Examples

### Deploy Development Environment

```powershell
# Initialize Terraform
.\terraform-deploy.ps1 dev init

# Review planned changes
.\terraform-deploy.ps1 dev plan

# Apply infrastructure
.\terraform-deploy.ps1 dev apply

# Get deployment URLs
.\terraform-deploy.ps1 dev output
```

### Deploy Production Environment

```powershell
# Initialize with production settings
.\terraform-deploy.ps1 prod init

# Plan with production variables
.\terraform-deploy.ps1 prod plan

# Apply to production (requires confirmation)
.\terraform-deploy.ps1 prod apply

# Monitor deployment
.\terraform-deploy.ps1 prod output
```

### Destroy Environment

```powershell
# Destroy development environment
.\terraform-deploy.ps1 dev destroy

# Destroy production (with confirmation)
.\terraform-deploy.ps1 prod destroy
```

## 📝 Outputs

After successful deployment, Terraform provides:

- **API Gateway URL** - Base API endpoint
- **Custom Domain URL** - Friendly domain (if configured)
- **DynamoDB Table Names** - For application configuration
- **ECR Repository URL** - For Docker image pushes
- **CloudWatch Dashboard URL** - Monitoring interface

## 🔄 CI/CD Integration

### GitHub Actions

The infrastructure integrates with the CI/CD pipeline:

```yaml
# Deploy infrastructure
- name: Deploy Infrastructure
  run: |
    cd terraform
    ./terraform-deploy.sh ${{ github.event.inputs.environment }} apply

# Get outputs for deployment
- name: Get Infrastructure Outputs
  run: |
    cd terraform
    terraform output -json > ../outputs.json
```

### Environment Variables

The outputs provide environment variables for application deployment:

```bash
export DYNAMODB_CUSTOMERS_TABLE=$(terraform output -raw customers_table_name)
export DYNAMODB_FUNDS_TABLE=$(terraform output -raw funds_table_name)
export API_GATEWAY_URL=$(terraform output -raw api_gateway_url)
```

## 🧹 Cleanup

### Remove All Resources

```powershell
# Development
.\terraform-deploy.ps1 dev destroy

# Staging
.\terraform-deploy.ps1 staging destroy

# Production (requires manual confirmation)
.\terraform-deploy.ps1 prod destroy
```

### Partial Cleanup

```bash
# Remove specific resources
terraform destroy -target=aws_lambda_function.ceibafunds_api

# Clean up state files
rm -rf .terraform/
rm terraform.tfstate*
```

## 🛠️ Troubleshooting

### Common Issues

#### AWS Credentials

```bash
# Check credentials
aws sts get-caller-identity

# Configure if needed
aws configure
```

#### Terraform State

```bash
# Reset state if corrupted
terraform init -reconfigure

# Import existing resources
terraform import aws_dynamodb_table.customers table-name
```

#### Permissions

```bash
# Required AWS permissions:
- dynamodb:*
- lambda:*
- apigateway:*
- iam:*
- logs:*
- ecr:*
```

### Debugging

```bash
# Enable detailed logging
export TF_LOG=DEBUG

# Validate configuration
terraform validate

# Check formatting
terraform fmt -check
```

## 📚 Additional Resources

- [AWS Provider Documentation](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [Terraform Best Practices](https://www.terraform.io/docs/cloud/guides/recommended-practices/index.html)
- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
- [DynamoDB Best Practices](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/best-practices.html)

---

**Infrastructure as Code for CeibaFunds API - Built with ❤️ and Terraform**
