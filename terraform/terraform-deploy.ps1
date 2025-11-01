# Terraform Management Script for CeibaFunds Infrastructure
# Usage: .\terraform-deploy.ps1 [environment] [action]

param(
    [Parameter(Position = 0)]
    [ValidateSet('dev', 'staging', 'prod')]
    [string]$Environment,
    
    [Parameter(Position = 1)]
    [ValidateSet('init', 'validate', 'fmt', 'plan', 'apply', 'destroy', 'output')]
    [string]$Action,
    
    [switch]$AutoApprove,
    [switch]$Help
)

function Write-ColorOutput($Color, $Message) {
    Write-Host $Message -ForegroundColor $Color
}

function Show-Help {
    Write-ColorOutput Cyan "🏗️  CeibaFunds Terraform Management"
    Write-ColorOutput Cyan "=================================="
    Write-Host ""
    Write-Host "Usage: .\terraform-deploy.ps1 [environment] [action]"
    Write-Host ""
    Write-Host "Environments:"
    Write-Host "  - dev"
    Write-Host "  - staging"
    Write-Host "  - prod"
    Write-Host ""
    Write-Host "Actions:"
    Write-Host "  - init      Initialize Terraform"
    Write-Host "  - validate  Validate Terraform configuration"
    Write-Host "  - fmt       Format Terraform files"
    Write-Host "  - plan      Show execution plan"
    Write-Host "  - apply     Apply changes"
    Write-Host "  - destroy   Destroy infrastructure"
    Write-Host "  - output    Show outputs"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\terraform-deploy.ps1 dev init     # Initialize development environment"
    Write-Host "  .\terraform-deploy.ps1 dev plan     # Plan development deployment"
    Write-Host "  .\terraform-deploy.ps1 prod apply   # Deploy to production"
    Write-Host ""
}

function Test-Prerequisites {
    Write-ColorOutput Yellow "🔍 Checking prerequisites..."
    
    # Check Terraform
    try {
        terraform --version | Out-Null
    }
    catch {
        Write-ColorOutput Red "❌ Terraform is not installed"
        Write-Host "Please install Terraform: https://www.terraform.io/downloads.html"
        exit 1
    }
    
    # Check AWS CLI
    try {
        aws --version | Out-Null
    }
    catch {
        Write-ColorOutput Red "❌ AWS CLI is not installed"
        Write-Host "Please install AWS CLI: https://aws.amazon.com/cli/"
        exit 1
    }
    
    # Check AWS credentials
    try {
        aws sts get-caller-identity | Out-Null
    }
    catch {
        Write-ColorOutput Red "❌ AWS credentials not configured"
        Write-Host "Please configure AWS credentials: aws configure"
        exit 1
    }
    
    Write-ColorOutput Green "✅ Prerequisites check passed"
}

function Set-TerraformBackend {
    param([string]$env)
    
    Write-ColorOutput Yellow "🏗️  Setting up Terraform backend for $env..."
    
    $backendConfig = @"
terraform {
  backend "s3" {
    bucket         = "ceibafunds-terraform-state-$env"
    key            = "$env/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "ceibafunds-terraform-locks-$env"
    encrypt        = true
  }
}
"@
    
    $backendConfig | Out-File -FilePath "backend-$env.tf" -Encoding UTF8
}

function Initialize-Terraform {
    param([string]$env)
    
    Write-ColorOutput Blue "🚀 Initializing Terraform for $env environment..."
    
    Set-TerraformBackend $env
    terraform init -reconfigure
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput Green "✅ Terraform initialized successfully"
    }
    else {
        Write-ColorOutput Red "❌ Terraform initialization failed"
        exit 1
    }
}

function Test-TerraformConfig {
    Write-ColorOutput Blue "🔍 Validating Terraform configuration..."
    terraform validate
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput Green "✅ Configuration is valid"
    }
    else {
        Write-ColorOutput Red "❌ Configuration validation failed"
        exit 1
    }
}

function Format-TerraformFiles {
    Write-ColorOutput Blue "📝 Formatting Terraform files..."
    terraform fmt -recursive
    Write-ColorOutput Green "✅ Files formatted"
}

function New-TerraformPlan {
    param([string]$env)
    
    Write-ColorOutput Blue "📋 Planning Terraform changes for $env..."
    
    terraform plan -var-file="terraform.tfvars.$env" -out="$env.tfplan"
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput Green "✅ Plan completed. Review the changes above."
        Write-ColorOutput Yellow "⚠️  Run 'terraform apply $env.tfplan' to apply changes"
    }
    else {
        Write-ColorOutput Red "❌ Planning failed"
        exit 1
    }
}

function Invoke-TerraformApply {
    param([string]$env)
    
    Write-ColorOutput Blue "🚀 Applying Terraform changes for $env..."
    
    if (Test-Path "$env.tfplan") {
        Write-ColorOutput Yellow "📋 Using existing plan file: $env.tfplan"
        terraform apply "$env.tfplan"
        if ($LASTEXITCODE -eq 0) {
            Remove-Item "$env.tfplan" -ErrorAction SilentlyContinue
        }
    }
    else {
        Write-ColorOutput Yellow "📋 No plan file found. Running plan and apply..."
        if ($AutoApprove) {
            terraform apply -var-file="terraform.tfvars.$env" -auto-approve
        }
        else {
            terraform apply -var-file="terraform.tfvars.$env"
        }
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput Green "✅ Infrastructure deployed successfully!"
        Write-Host ""
        Write-ColorOutput Blue "📊 Getting outputs..."
        terraform output
    }
    else {
        Write-ColorOutput Red "❌ Apply failed"
        exit 1
    }
}

function Remove-TerraformInfrastructure {
    param([string]$env)
    
    Write-ColorOutput Red "⚠️  WARNING: This will destroy all infrastructure for $env!"
    
    if (-not $AutoApprove) {
        $confirm = Read-Host "Are you sure you want to continue? Type 'yes' to confirm"
        if ($confirm -ne "yes") {
            Write-Host "Aborted."
            exit 0
        }
    }
    
    Write-ColorOutput Blue "💥 Destroying infrastructure for $env..."
    terraform destroy -var-file="terraform.tfvars.$env" -auto-approve
    
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput Green "✅ Infrastructure destroyed"
    }
    else {
        Write-ColorOutput Red "❌ Destroy failed"
        exit 1
    }
}

function Get-TerraformOutput {
    Write-ColorOutput Blue "📊 Terraform outputs:"
    terraform output -json | ConvertFrom-Json | ConvertTo-Json -Depth 10
}

# Main execution
if ($Help -or (-not $Environment) -or (-not $Action)) {
    Show-Help
    exit 0
}

# Check prerequisites
Test-Prerequisites

# Change to terraform directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Execute action
switch ($Action) {
    'init' { Initialize-Terraform $Environment }
    'validate' { Test-TerraformConfig }
    'fmt' { Format-TerraformFiles }
    'plan' { New-TerraformPlan $Environment }
    'apply' { Invoke-TerraformApply $Environment }
    'destroy' { Remove-TerraformInfrastructure $Environment }
    'output' { Get-TerraformOutput }
    default {
        Write-ColorOutput Red "❌ Unknown action: $Action"
        Show-Help
        exit 1
    }
}