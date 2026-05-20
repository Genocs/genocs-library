# Backend configuration for Terraform state management
# This uses AWS S3 for remote state storage
# Uncomment and configure this block after creating the backend storage

# terraform {
#   backend "s3" {
#     bucket         = "terraform-state-<account-id>-<region>"
#     key            = "genocs-ca/dev.tfstate"
#     region         = "us-east-1"
#     encrypt        = true
#     dynamodb_table = "terraform-locks"
#   }
# }

# Instructions for setting up the backend:
# 1. Create an S3 bucket for Terraform state:
#    aws s3api create-bucket --bucket terraform-state-<account-id>-<region> --region us-east-1
#    aws s3api put-bucket-versioning --bucket terraform-state-<account-id>-<region> --versioning-configuration Status=Enabled
#    aws s3api put-bucket-encryption --bucket terraform-state-<account-id>-<region> --server-side-encryption-configuration '{
#      "Rules": [{
#        "ApplyServerSideEncryptionByDefault": {
#          "SSEAlgorithm": "AES256"
#        }
#      }]
#    }'
#
# 2. Create a DynamoDB table for state locking:
#    aws dynamodb create-table \
#      --table-name terraform-locks \
#      --attribute-definitions AttributeName=LockID,AttributeType=S \
#      --key-schema AttributeName=LockID,KeyType=HASH \
#      --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5
#
# 3. Uncomment the backend block above and replace placeholders with your values
#
# 4. Initialize Terraform with the backend:
#    terraform init -backend-config="key=genocs-ca/dev.tfstate"
#
# 5. For each environment, use a different key:
#    - dev:   genocs-ca/dev.tfstate
#    - test:  genocs-ca/test.tfstate
#    - stage: genocs-ca/stage.tfstate
#    - prod:  genocs-ca/prod.tfstate

# Alternative: Use local backend for development (default if backend block is commented)
# State files will be stored locally in terraform.tfstate
