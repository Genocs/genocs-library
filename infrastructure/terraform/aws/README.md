# AWS Terraform Configuration for Genocs Library

This directory contains Terraform configuration for deploying Genocs Library microservices to AWS using ECS Fargate.

## Architecture Overview

The AWS setup mirrors the Azure architecture with these key components:

- **VPC & Networking**: Multi-AZ VPC with public and private subnets, NAT Gateways, and Internet Gateway
- **Container Registry**: Elastic Container Registry (ECR) for Docker images
- **Container Orchestration**: ECS Fargate for serverless container deployment
- **Load Balancing**: Application Load Balancer (ALB) for traffic distribution
- **Monitoring**: CloudWatch for logs and metrics, optional X-Ray for distributed tracing
- **Auto Scaling**: ECS Service auto-scaling based on CPU and memory utilization
- **IAM**: Proper role-based access control with least privilege policies

## File Structure

```
aws/
├── providers.tf          # AWS provider configuration
├── variables.tf          # Input variable definitions
├── main.tf              # Main resource definitions (VPC, ECS, ECR, IAM, etc.)
├── outputs.tf           # Output values
├── backend.tf           # Remote state configuration (S3 + DynamoDB)
├── dev.tfvars           # Development environment variables
├── test.tfvars          # Test environment variables
├── stage.tfvars         # Staging environment variables
├── prod.tfvars          # Production environment variables
└── README.md            # This file
```

## Prerequisites

1. **AWS Account**: Active AWS account with appropriate IAM permissions
2. **Terraform**: Version 1.5.0 or higher
3. **AWS CLI**: For state backend setup (optional but recommended)
4. **Docker**: For building and pushing images to ECR (when deploying apps)

## Quick Start

### 1. Initialize Terraform

```bash
cd infrastructure/terraform/aws

# For local state (development only)
terraform init

# For remote S3 backend (recommended for team/production)
# See backend.tf for S3 setup instructions
terraform init -backend-config="bucket=terraform-state-xxx" \
               -backend-config="key=genocs-ca/dev.tfstate" \
               -backend-config="region=us-east-1"
```

### 2. Deploy to Development

```bash
terraform plan -var-file=dev.tfvars
terraform apply -var-file=dev.tfvars
```

### 3. Deploy to Other Environments

```bash
# Test environment
terraform plan -var-file=test.tfvars
terraform apply -var-file=test.tfvars

# Staging environment
terraform plan -var-file=stage.tfvars
terraform apply -var-file=stage.tfvars

# Production environment
terraform plan -var-file=prod.tfvars
terraform apply -var-file=prod.tfvars
```

## Configuration

### Environment Variables

Each environment has a `.tfvars` file with specific configurations:

- **dev.tfvars**: Minimal resources, debug logging, no X-Ray tracing
- **test.tfvars**: Medium resources, information logging
- **stage.tfvars**: Production-like resources, X-Ray enabled
- **prod.tfvars**: Maximum resources, warning logging, strict health checks

### Key Variables

#### Resource Sizing

```hcl
app1_cpu    = 512          # 256, 512, 1024, 2048, 4096
app1_memory = 1024         # Must be compatible with CPU
app2_cpu    = 512
app2_memory = 1024
```

#### Scaling

```hcl
app1_min_tasks = 1
app1_max_tasks = 3
app1_target_cpu_utilization = 70        # Percentage
app1_target_memory_utilization = 80     # Percentage
```

#### Networking

```hcl
vpc_cidr           = "10.0.0.0/16"
availability_zones = 2
```

#### Features

```hcl
enable_container_insights = true   # CloudWatch Container Insights
enable_xray_tracing        = false  # AWS X-Ray distributed tracing
app1_enable_load_balancing = true   # ALB for app1
```

## Deploying Applications

### 1. Build and Push Docker Image to ECR

```bash
# Get ECR repository URL from outputs
ECR_REPO=$(terraform output -raw ecr_app1_repository_url)

# Build image
docker build -t $ECR_REPO:latest .

# Get authentication token
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin $ECR_REPO

# Push image
docker push $ECR_REPO:latest
```

### 2. Update ECS Service

Once the image is pushed, ECS will automatically pull the latest image on next deployment:

```bash
# Force new deployment
aws ecs update-service \
  --cluster $(terraform output -raw ecs_cluster_name) \
  --service app-webapi-xxx \
  --force-new-deployment \
  --region us-east-1
```

## Monitoring & Logging

### CloudWatch Logs

View logs from ECS tasks:

```bash
# Get log group names
terraform output app1_log_group_name
terraform output app2_log_group_name

# View logs
aws logs tail /ecs/app-webapi-xxx --follow
aws logs tail /ecs/app-worker-xxx --follow
```

### Container Insights Metrics

CloudWatch Container Insights provides:
- CPU and memory utilization
- Task count and deployment status
- Network I/O metrics
- Integrated logs and metrics

Access via AWS Console → CloudWatch → Container Insights

### X-Ray Tracing (if enabled)

Distributed tracing for service-to-service calls. Enable in `.tfvars`:

```hcl
enable_xray_tracing = true
```

## Auto Scaling

### How It Works

Each ECS service has two scaling policies:

1. **CPU Utilization Scaling**: Scales based on average CPU usage
2. **Memory Utilization Scaling**: Scales based on average memory usage

Configuration:

```hcl
app1_target_cpu_utilization    = 70   # Scale up when > 70%
app1_target_memory_utilization = 80   # Scale up when > 80%
scale_up_cooldown              = 300  # 5 minutes
scale_down_cooldown            = 300  # 5 minutes
```

### Monitoring Auto Scaling

```bash
# View scaling activities
aws application-autoscaling describe-scaling-activities \
  --service-namespace ecs \
  --resource-id service/genocs-cluster-xxx/app-webapi-xxx
```

## Load Balancing

### Application Load Balancer

- Distributes traffic across healthy ECS tasks
- Health checks every 30 seconds
- Marks tasks unhealthy after 3 failed checks
- Configurable via `health_check_*` variables

### Accessing Services

```bash
# Get ALB DNS name
ALB_DNS=$(terraform output -raw alb_dns_name)

# Access webapi service
curl http://$ALB_DNS/health
```

## Security Considerations

### IAM Roles & Policies

- **Task Execution Role**: Allows tasks to pull images from ECR and write logs to CloudWatch
- **Task Role**: Application-level permissions (customize as needed)

### Network Security

- ECS tasks run in private subnets (no direct internet access)
- NAT Gateways provide outbound internet access
- Security groups restrict traffic at multiple levels
- Only ALB is publicly accessible (port 80/443)

### Recommended Enhancements

1. **Enable HTTPS**: Add SSL/TLS certificate to ALB listener
2. **Private ECR**: Restrict ECR access via policies
3. **Secrets Management**: Use AWS Secrets Manager for sensitive data
4. **VPC Endpoints**: For accessing AWS services without NAT
5. **Network ACLs**: Additional network-level security

## Troubleshooting

### Tasks Won't Start

```bash
# Check task logs
aws ecs describe-tasks \
  --cluster genocs-cluster-xxx \
  --tasks <task-arn> \
  --region us-east-1

# Check task definition
aws ecs describe-task-definition \
  --task-definition app-webapi-xxx
```

### Health Check Failing

- Verify health endpoint: `curl -i http://localhost:8080/health`
- Check application logs: `aws logs tail /ecs/app-webapi-xxx --follow`
- Verify security groups allow traffic between ALB and tasks

### ECR Image Pull Errors

```bash
# Verify image exists
aws ecr describe-images --repository-name ecr-webapi-xxx

# Check task execution role permissions
aws iam get-role-policy --role-name iam-ecs-task-exec-xxx --policy-name policy-ecr-pull-xxx
```

## State Management

### Local State (Development Only)

State is stored in `terraform.tfstate` in the working directory.

```bash
# View state
terraform show
terraform state list
```

### Remote State (Recommended)

For team environments, use S3 + DynamoDB backend. See `backend.tf` for setup.

```bash
# Migrate from local to remote
terraform init -migrate-state
```

## Cost Optimization

### Ways to Reduce Costs

1. **Use Fargate Spot**: Set `FARGATE_SPOT` as primary capacity provider
2. **Reduce Task Resources**: Adjust `cpu` and `memory` variables
3. **Auto Scaling**: Properly tune min/max task counts
4. **Log Retention**: Adjust `log_retention_days` (default: 7)
5. **NAT Gateway**: Consider NAT instance for dev/test

### Cost Estimation

```bash
terraform plan -var-file=prod.tfvars | grep aws_
```

## Updating Infrastructure

```bash
# Update variables
vim prod.tfvars

# Preview changes
terraform plan -var-file=prod.tfvars

# Apply changes
terraform apply -var-file=prod.tfvars
```

## Cleanup

```bash
# Remove all resources
terraform destroy -var-file=prod.tfvars

# Remove specific resources
terraform destroy -var-file=prod.tfvars -target=aws_ecs_service.app1
```

## Resource Mapping: Azure → AWS

| Azure | AWS |
|-------|-----|
| Resource Group | AWS Account + Region |
| App Service Plan | ECS Cluster + Fargate |
| App Service (Linux) | ECS Service + Task Definition |
| Log Analytics | CloudWatch Log Groups |
| Application Insights | CloudWatch Metrics + X-Ray |
| Container Registry (ACR) | ECR (Elastic Container Registry) |
| Managed Identity | IAM Roles |
| Auto Scale Settings | Application Auto Scaling |
| Health Check | ALB Health Check |
| Virtual Network | VPC |
| Load Balancer | Application Load Balancer (ALB) |

## Additional Resources

- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [ECS Fargate Best Practices](https://docs.aws.amazon.com/AmazonECS/latest/developerguide/task-cpu-memory-error.html)
- [Terraform Best Practices](https://www.terraform.io/docs/cloud/guides/recommended-practices.html)

## Support

For issues with Terraform configuration, check:
1. AWS credentials: `aws sts get-caller-identity`
2. Terraform syntax: `terraform validate`
3. AWS service quotas: Check AWS console for limits
4. CloudTrail logs: Debug API calls
