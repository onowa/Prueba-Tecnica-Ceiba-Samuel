#!/bin/bash

# Terraform Management Script for CeibaFunds Infrastructure
# Usage: ./terraform-deploy.sh [environment] [action]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
ENVIRONMENTS=("dev" "staging" "prod")
ACTIONS=("plan" "apply" "destroy" "init" "validate" "fmt" "output")

show_help() {
    echo -e "${BLUE}🏗️  CeibaFunds Terraform Management${NC}"
    echo -e "${BLUE}==================================${NC}"
    echo ""
    echo "Usage: $0 [environment] [action]"
    echo ""
    echo "Environments:"
    for env in "${ENVIRONMENTS[@]}"; do
        echo "  - $env"
    done
    echo ""
    echo "Actions:"
    echo "  - init      Initialize Terraform"
    echo "  - validate  Validate Terraform configuration"
    echo "  - fmt       Format Terraform files"
    echo "  - plan      Show execution plan"
    echo "  - apply     Apply changes"
    echo "  - destroy   Destroy infrastructure"
    echo "  - output    Show outputs"
    echo ""
    echo "Examples:"
    echo "  $0 dev init     # Initialize development environment"
    echo "  $0 dev plan     # Plan development deployment"
    echo "  $0 prod apply   # Deploy to production"
    echo ""
}

validate_environment() {
    local env=$1
    if [[ ! " ${ENVIRONMENTS[@]} " =~ " ${env} " ]]; then
        echo -e "${RED}❌ Invalid environment: $env${NC}"
        echo "Valid environments: ${ENVIRONMENTS[*]}"
        exit 1
    fi
}

validate_action() {
    local action=$1
    if [[ ! " ${ACTIONS[@]} " =~ " ${action} " ]]; then
        echo -e "${RED}❌ Invalid action: $action${NC}"
        echo "Valid actions: ${ACTIONS[*]}"
        exit 1
    fi
}

check_prerequisites() {
    echo -e "${YELLOW}🔍 Checking prerequisites...${NC}"
    
    # Check Terraform
    if ! command -v terraform &> /dev/null; then
        echo -e "${RED}❌ Terraform is not installed${NC}"
        echo "Please install Terraform: https://www.terraform.io/downloads.html"
        exit 1
    fi
    
    # Check AWS CLI
    if ! command -v aws &> /dev/null; then
        echo -e "${RED}❌ AWS CLI is not installed${NC}"
        echo "Please install AWS CLI: https://aws.amazon.com/cli/"
        exit 1
    fi
    
    # Check AWS credentials
    if ! aws sts get-caller-identity &> /dev/null; then
        echo -e "${RED}❌ AWS credentials not configured${NC}"
        echo "Please configure AWS credentials: aws configure"
        exit 1
    fi
    
    echo -e "${GREEN}✅ Prerequisites check passed${NC}"
}

setup_backend() {
    local env=$1
    echo -e "${YELLOW}🏗️  Setting up Terraform backend for $env...${NC}"
    
    # Create backend configuration
    cat > backend-$env.tf <<EOF
terraform {
  backend "s3" {
    bucket         = "ceibafunds-terraform-state-$env"
    key            = "$env/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "ceibafunds-terraform-locks-$env"
    encrypt        = true
  }
}
EOF
}

terraform_init() {
    local env=$1
    echo -e "${BLUE}🚀 Initializing Terraform for $env environment...${NC}"
    
    setup_backend $env
    terraform init -reconfigure
    
    echo -e "${GREEN}✅ Terraform initialized successfully${NC}"
}

terraform_validate() {
    echo -e "${BLUE}🔍 Validating Terraform configuration...${NC}"
    terraform validate
    echo -e "${GREEN}✅ Configuration is valid${NC}"
}

terraform_fmt() {
    echo -e "${BLUE}📝 Formatting Terraform files...${NC}"
    terraform fmt -recursive
    echo -e "${GREEN}✅ Files formatted${NC}"
}

terraform_plan() {
    local env=$1
    echo -e "${BLUE}📋 Planning Terraform changes for $env...${NC}"
    
    terraform plan -var-file="terraform.tfvars.$env" -out="$env.tfplan"
    
    echo -e "${GREEN}✅ Plan completed. Review the changes above.${NC}"
    echo -e "${YELLOW}⚠️  Run 'terraform apply $env.tfplan' to apply changes${NC}"
}

terraform_apply() {
    local env=$1
    echo -e "${BLUE}🚀 Applying Terraform changes for $env...${NC}"
    
    # Check if plan exists
    if [[ -f "$env.tfplan" ]]; then
        echo -e "${YELLOW}📋 Using existing plan file: $env.tfplan${NC}"
        terraform apply "$env.tfplan"
        rm -f "$env.tfplan"
    else
        echo -e "${YELLOW}📋 No plan file found. Running plan and apply...${NC}"
        terraform apply -var-file="terraform.tfvars.$env" -auto-approve
    fi
    
    echo -e "${GREEN}✅ Infrastructure deployed successfully!${NC}"
    echo ""
    echo -e "${BLUE}📊 Getting outputs...${NC}"
    terraform output
}

terraform_destroy() {
    local env=$1
    echo -e "${RED}⚠️  WARNING: This will destroy all infrastructure for $env!${NC}"
    read -p "Are you sure you want to continue? Type 'yes' to confirm: " confirm
    
    if [[ $confirm != "yes" ]]; then
        echo "Aborted."
        exit 0
    fi
    
    echo -e "${BLUE}💥 Destroying infrastructure for $env...${NC}"
    terraform destroy -var-file="terraform.tfvars.$env" -auto-approve
    
    echo -e "${GREEN}✅ Infrastructure destroyed${NC}"
}

terraform_output() {
    echo -e "${BLUE}📊 Terraform outputs:${NC}"
    terraform output -json | jq .
}

main() {
    local environment=$1
    local action=$2
    
    # Show help if no arguments
    if [[ $# -eq 0 ]]; then
        show_help
        exit 0
    fi
    
    # Validate arguments
    validate_environment "$environment"
    validate_action "$action"
    
    # Check prerequisites
    check_prerequisites
    
    # Change to terraform directory
    cd "$(dirname "$0")"
    
    # Execute action
    case $action in
        init)
            terraform_init "$environment"
            ;;
        validate)
            terraform_validate
            ;;
        fmt)
            terraform_fmt
            ;;
        plan)
            terraform_plan "$environment"
            ;;
        apply)
            terraform_apply "$environment"
            ;;
        destroy)
            terraform_destroy "$environment"
            ;;
        output)
            terraform_output
            ;;
        *)
            echo -e "${RED}❌ Unknown action: $action${NC}"
            show_help
            exit 1
            ;;
    esac
}

main "$@"
