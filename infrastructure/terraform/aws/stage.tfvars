# Staging Environment Configuration

environment = "stage"
aws_region  = "us-east-1"
project_name = "genocs-ca"

# Tags
tags = {
  Environment = "Staging"
  CostCenter  = "Operations"
  Project     = "Clean Architecture"
}

# VPC Configuration
vpc_cidr              = "10.2.0.0/16"
availability_zones    = 2

# Container Registry
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

# Task Resources
app1_cpu    = 512
app1_memory = 1024
app2_cpu    = 512
app2_memory = 1024

# Health Check
health_check_path                = "/health"
health_check_interval            = 30
health_check_timeout             = 5
health_check_healthy_threshold   = 2
health_check_unhealthy_threshold = 3

# Scaling
app1_min_tasks = 2
app1_max_tasks = 5
app1_target_cpu_utilization    = 70
app1_target_memory_utilization = 80

app2_min_tasks = 2
app2_max_tasks = 4
app2_target_cpu_utilization    = 70
app2_target_memory_utilization = 80

scale_up_cooldown   = 300
scale_down_cooldown = 300

# Logging
log_retention_days = 30

# Features
enable_container_insights = true
enable_xray_tracing       = true

# Load Balancing
app1_enable_load_balancing = true
app2_enable_load_balancing = false

# Custom App Settings for Staging
app1_custom_settings = {
  ASPNETCORE_DETAILEDERRORS = "false"
  Logging__LogLevel__Default = "Information"
}

app2_custom_settings = {
  ASPNETCORE_DETAILEDERRORS = "false"
  Logging__LogLevel__Default = "Information"
}
