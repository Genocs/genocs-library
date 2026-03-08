# General Configuration
variable "environment" {
  description = "Environment name (dev, test, stage, prod)"
  type        = string
  validation {
    condition     = contains(["dev", "test", "stage", "prod"], var.environment)
    error_message = "Environment must be one of: dev, test, stage, prod."
  }
}

variable "aws_region" {
  description = "AWS region for resources"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "genocs-ca"
}

variable "tags" {
  description = "Common tags to be applied to all resources"
  type        = map(string)
  default     = {}
}

# VPC Configuration
variable "vpc_cidr" {
  description = "CIDR block for VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "Number of availability zones to use"
  type        = number
  default     = 2
}

# Container Registry Configuration
variable "ecr_image_tag_mutability" {
  description = "Image tag mutability for ECR"
  type        = string
  default     = "MUTABLE"
  validation {
    condition     = contains(["MUTABLE", "IMMUTABLE"], var.ecr_image_tag_mutability)
    error_message = "ECR image tag mutability must be MUTABLE or IMMUTABLE."
  }
}

variable "ecr_scan_on_push" {
  description = "Enable image scanning on push to ECR"
  type        = bool
  default     = true
}

# ECS Cluster Configuration
variable "ecs_cluster_name" {
  description = "Name of the ECS cluster"
  type        = string
  default     = "genocs-cluster"
}

# App Service Configuration
variable "app1_name" {
  description = "Name for the first app service"
  type        = string
  default     = "webapi"
}

variable "app2_name" {
  description = "Name for the second app service"
  type        = string
  default     = "worker"
}

variable "app1_docker_image" {
  description = "Docker image for the first app service"
  type        = string
  default     = "nginx:latest"
}

variable "app2_docker_image" {
  description = "Docker image for the second app service"
  type        = string
  default     = "nginx:latest"
}

variable "app1_port" {
  description = "Port for the first app service"
  type        = number
  default     = 8080
}

variable "app2_port" {
  description = "Port for the second app service"
  type        = number
  default     = 8080
}

variable "app1_cpu" {
  description = "CPU units for the first app service task (256, 512, 1024, 2048, 4096)"
  type        = number
  default     = 512
}

variable "app1_memory" {
  description = "Memory in MB for the first app service task"
  type        = number
  default     = 1024
}

variable "app2_cpu" {
  description = "CPU units for the second app service task"
  type        = number
  default     = 512
}

variable "app2_memory" {
  description = "Memory in MB for the second app service task"
  type        = number
  default     = 1024
}

variable "health_check_path" {
  description = "Health check path for load balancer"
  type        = string
  default     = "/health"
}

variable "health_check_interval" {
  description = "Health check interval in seconds"
  type        = number
  default     = 30
}

variable "health_check_timeout" {
  description = "Health check timeout in seconds"
  type        = number
  default     = 5
}

variable "health_check_healthy_threshold" {
  description = "Health check healthy threshold"
  type        = number
  default     = 2
}

variable "health_check_unhealthy_threshold" {
  description = "Health check unhealthy threshold"
  type        = number
  default     = 3
}

# Scaling Configuration
variable "app1_min_tasks" {
  description = "Minimum number of tasks for first app"
  type        = number
  default     = 1
}

variable "app1_max_tasks" {
  description = "Maximum number of tasks for first app"
  type        = number
  default     = 3
}

variable "app1_target_cpu_utilization" {
  description = "Target CPU utilization percentage for first app"
  type        = number
  default     = 70
}

variable "app1_target_memory_utilization" {
  description = "Target memory utilization percentage for first app"
  type        = number
  default     = 80
}

variable "app2_min_tasks" {
  description = "Minimum number of tasks for second app"
  type        = number
  default     = 1
}

variable "app2_max_tasks" {
  description = "Maximum number of tasks for second app"
  type        = number
  default     = 3
}

variable "app2_target_cpu_utilization" {
  description = "Target CPU utilization percentage for second app"
  type        = number
  default     = 70
}

variable "app2_target_memory_utilization" {
  description = "Target memory utilization percentage for second app"
  type        = number
  default     = 80
}

variable "scale_up_cooldown" {
  description = "Cooldown period in seconds for scale up"
  type        = number
  default     = 300
}

variable "scale_down_cooldown" {
  description = "Cooldown period in seconds for scale down"
  type        = number
  default     = 300
}

# Logging Configuration
variable "log_retention_days" {
  description = "Log retention period in days"
  type        = number
  default     = 7
}

# Custom App Settings
variable "app1_custom_settings" {
  description = "Custom environment variables for the first app"
  type        = map(string)
  default     = {}
}

variable "app2_custom_settings" {
  description = "Custom environment variables for the second app"
  type        = map(string)
  default     = {}
}

variable "enable_container_insights" {
  description = "Enable CloudWatch Container Insights for ECS"
  type        = bool
  default     = true
}

variable "enable_xray_tracing" {
  description = "Enable X-Ray tracing for services"
  type        = bool
  default     = false
}

variable "app1_enable_load_balancing" {
  description = "Enable load balancer for first app"
  type        = bool
  default     = true
}

variable "app2_enable_load_balancing" {
  description = "Enable load balancer for second app"
  type        = bool
  default     = false
}
