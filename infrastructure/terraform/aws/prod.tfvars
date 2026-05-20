# Production Environment Configuration

environment = "prod"
aws_region  = "us-east-1"
project_name = "genocs-ca"

# Tags
tags = {
  Environment = "Production"
  CostCenter  = "Operations"
  Project     = "Clean Architecture"
  Criticality = "High"
}

# VPC Configuration
vpc_cidr              = "10.3.0.0/16"
availability_zones    = 3

# Container Registry - IMMUTABLE for production
ecr_image_tag_mutability = "IMMUTABLE"
ecr_scan_on_push         = true

# ECS Cluster
ecs_cluster_name = "genocs-cluster"

# App Services Configuration
app1_name         = "webapi"
app2_name         = "worker"
app1_docker_image = "nginx:latest"
app2_docker_image = "nginx:latest"
app1_port         = 8080
app2_port         = 8080

# Task Resources - Higher for production
app1_cpu    = 1024
app1_memory = 2048
app2_cpu    = 1024
app2_memory = 2048

# Health Check
health_check_path                = "/health"
health_check_interval            = 30
health_check_timeout             = 5
health_check_healthy_threshold   = 3
health_check_unhealthy_threshold = 2

# Scaling - Higher for production
app1_min_tasks = 3
app1_max_tasks = 10
app1_target_cpu_utilization    = 60
app1_target_memory_utilization = 75

app2_min_tasks = 2
app2_max_tasks = 8
app2_target_cpu_utilization    = 60
app2_target_memory_utilization = 75

scale_up_cooldown   = 300
scale_down_cooldown = 600

# Logging - Longer retention for production
log_retention_days = 90

# Features
enable_container_insights = true
enable_xray_tracing       = true

# Load Balancing
app1_enable_load_balancing = true
app2_enable_load_balancing = false

# Custom App Settings for Production
app1_custom_settings = {
  ASPNETCORE_DETAILEDERRORS = "false"
  Logging__LogLevel__Default = "Warning"
  Logging__LogLevel__Microsoft = "Warning"
  Logging__LogLevel__System = "Warning"
}

app2_custom_settings = {
  ASPNETCORE_DETAILEDERRORS = "false"
  Logging__LogLevel__Default = "Warning"
  Logging__LogLevel__Microsoft = "Warning"
  Logging__LogLevel__System = "Warning"
}
