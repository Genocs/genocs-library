# VPC Outputs
output "vpc_id" {
  description = "ID of the VPC"
  value       = aws_vpc.main.id
}

output "vpc_cidr" {
  description = "CIDR block of the VPC"
  value       = aws_vpc.main.cidr_block
}

# Subnet Outputs
output "public_subnet_ids" {
  description = "IDs of public subnets"
  value       = aws_subnet.public[*].id
}

output "private_subnet_ids" {
  description = "IDs of private subnets"
  value       = aws_subnet.private[*].id
}

# Security Group Outputs
output "alb_security_group_id" {
  description = "ID of ALB security group"
  value       = aws_security_group.alb.id
}

output "ecs_tasks_security_group_id" {
  description = "ID of ECS tasks security group"
  value       = aws_security_group.ecs_tasks.id
}

# Load Balancer Outputs
output "alb_dns_name" {
  description = "DNS name of the load balancer"
  value       = aws_lb.main.dns_name
}

output "alb_arn" {
  description = "ARN of the load balancer"
  value       = aws_lb.main.arn
}

output "alb_zone_id" {
  description = "Zone ID of the load balancer"
  value       = aws_lb.main.zone_id
}

# ECR Repository Outputs
output "ecr_app1_repository_url" {
  description = "URL of the ECR repository for app1"
  value       = aws_ecr_repository.app1.repository_url
}

output "ecr_app1_registry_id" {
  description = "Registry ID of the ECR repository for app1"
  value       = aws_ecr_repository.app1.registry_id
}

output "ecr_app2_repository_url" {
  description = "URL of the ECR repository for app2"
  value       = aws_ecr_repository.app2.repository_url
}

output "ecr_app2_registry_id" {
  description = "Registry ID of the ECR repository for app2"
  value       = aws_ecr_repository.app2.registry_id
}

# ECS Cluster Outputs
output "ecs_cluster_name" {
  description = "Name of the ECS cluster"
  value       = aws_ecs_cluster.main.name
}

output "ecs_cluster_id" {
  description = "ID of the ECS cluster"
  value       = aws_ecs_cluster.main.id
}

output "ecs_cluster_arn" {
  description = "ARN of the ECS cluster"
  value       = aws_ecs_cluster.main.arn
}

# ECS Service Outputs
output "ecs_service_app1_name" {
  description = "Name of the first ECS service"
  value       = aws_ecs_service.app1.name
}

output "ecs_service_app1_id" {
  description = "ID of the first ECS service"
  value       = aws_ecs_service.app1.id
}

output "ecs_service_app2_name" {
  description = "Name of the second ECS service"
  value       = aws_ecs_service.app2.name
}

output "ecs_service_app2_id" {
  description = "ID of the second ECS service"
  value       = aws_ecs_service.app2.id
}

# ECS Task Definition Outputs
output "ecs_task_definition_app1_arn" {
  description = "ARN of the first task definition"
  value       = aws_ecs_task_definition.app1.arn
}

output "ecs_task_definition_app1_revision" {
  description = "Revision of the first task definition"
  value       = aws_ecs_task_definition.app1.revision
}

output "ecs_task_definition_app2_arn" {
  description = "ARN of the second task definition"
  value       = aws_ecs_task_definition.app2.arn
}

output "ecs_task_definition_app2_revision" {
  description = "Revision of the second task definition"
  value       = aws_ecs_task_definition.app2.revision
}

# CloudWatch Log Group Outputs
output "ecs_log_group_name" {
  description = "Name of the ECS CloudWatch log group"
  value       = aws_cloudwatch_log_group.ecs.name
}

output "app1_log_group_name" {
  description = "Name of the app1 CloudWatch log group"
  value       = aws_cloudwatch_log_group.app1.name
}

output "app2_log_group_name" {
  description = "Name of the app2 CloudWatch log group"
  value       = aws_cloudwatch_log_group.app2.name
}

# IAM Role Outputs
output "ecs_task_execution_role_arn" {
  description = "ARN of the ECS task execution role"
  value       = aws_iam_role.ecs_task_execution_role.arn
}

output "ecs_task_role_arn" {
  description = "ARN of the ECS task role"
  value       = aws_iam_role.ecs_task_role.arn
}

# Target Group Outputs
output "alb_target_group_app1_arn" {
  description = "ARN of the ALB target group for app1"
  value       = try(aws_lb_target_group.app1[0].arn, "")
}

output "alb_target_group_app1_name" {
  description = "Name of the ALB target group for app1"
  value       = try(aws_lb_target_group.app1[0].name, "")
}

# Auto Scaling Outputs
output "app1_autoscaling_target_id" {
  description = "ID of the app1 autoscaling target"
  value       = aws_appautoscaling_target.app1.resource_id
}

output "app2_autoscaling_target_id" {
  description = "ID of the app2 autoscaling target"
  value       = aws_appautoscaling_target.app2.resource_id
}

# Summary Output
output "deployment_summary" {
  description = "Summary of the deployment"
  value = {
    environment             = var.environment
    aws_region              = var.aws_region
    vpc_id                  = aws_vpc.main.id
    vpc_cidr                = aws_vpc.main.cidr_block
    alb_dns_name            = aws_lb.main.dns_name
    ecs_cluster_name        = aws_ecs_cluster.main.name
    ecr_app1_repository_url = aws_ecr_repository.app1.repository_url
    ecr_app2_repository_url = aws_ecr_repository.app2.repository_url
    ecs_service_app1_name   = aws_ecs_service.app1.name
    ecs_service_app2_name   = aws_ecs_service.app2.name
    app1_log_group_name     = aws_cloudwatch_log_group.app1.name
    app2_log_group_name     = aws_cloudwatch_log_group.app2.name
  }
}
