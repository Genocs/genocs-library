# Generate a random suffix for unique naming
resource "random_id" "suffix" {
  byte_length = 4
}

# Local values for common configurations
locals {
  resource_suffix = lower("${var.project_name}-${var.environment}-${random_id.suffix.hex}")

  common_tags = merge(
    var.tags,
    {
      Environment = var.environment
      Project     = var.project_name
      ManagedBy   = "Terraform"
      CreatedDate = timestamp()
    }
  )

  # App names
  app1_full_name = "app-${var.app1_name}-${local.resource_suffix}"
  app2_full_name = "app-${var.app2_name}-${local.resource_suffix}"

  # Calculate subnets based on availability zones
  azs = data.aws_availability_zones.available.names
}

# Data source for available AZs
data "aws_availability_zones" "available" {
  state = "available"
}

# VPC
resource "aws_vpc" "main" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = merge(
    local.common_tags,
    {
      Name = "vpc-${local.resource_suffix}"
    }
  )
}

# Internet Gateway
resource "aws_internet_gateway" "main" {
  vpc_id = aws_vpc.main.id

  tags = merge(
    local.common_tags,
    {
      Name = "igw-${local.resource_suffix}"
    }
  )
}

# Public Subnets
resource "aws_subnet" "public" {
  count                   = var.availability_zones
  vpc_id                  = aws_vpc.main.id
  cidr_block              = cidrsubnet(var.vpc_cidr, 4, count.index)
  availability_zone       = local.azs[count.index % length(local.azs)]
  map_public_ip_on_launch = true

  tags = merge(
    local.common_tags,
    {
      Name = "subnet-public-${count.index + 1}-${local.resource_suffix}"
    }
  )
}

# Private Subnets
resource "aws_subnet" "private" {
  count             = var.availability_zones
  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(var.vpc_cidr, 4, count.index + var.availability_zones)
  availability_zone = local.azs[count.index % length(local.azs)]

  tags = merge(
    local.common_tags,
    {
      Name = "subnet-private-${count.index + 1}-${local.resource_suffix}"
    }
  )
}

# Elastic IPs for NAT Gateways
resource "aws_eip" "nat" {
  count  = var.availability_zones
  domain = "vpc"

  depends_on = [aws_internet_gateway.main]

  tags = merge(
    local.common_tags,
    {
      Name = "eip-nat-${count.index + 1}-${local.resource_suffix}"
    }
  )
}

# NAT Gateways
resource "aws_nat_gateway" "main" {
  count         = var.availability_zones
  allocation_id = aws_eip.nat[count.index].id
  subnet_id     = aws_subnet.public[count.index].id

  depends_on = [aws_internet_gateway.main]

  tags = merge(
    local.common_tags,
    {
      Name = "nat-${count.index + 1}-${local.resource_suffix}"
    }
  )
}

# Route Table - Public
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block      = "0.0.0.0/0"
    gateway_id      = aws_internet_gateway.main.id
  }

  tags = merge(
    local.common_tags,
    {
      Name = "rt-public-${local.resource_suffix}"
    }
  )
}

# Route Table Association - Public
resource "aws_route_table_association" "public" {
  count          = var.availability_zones
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

# Route Tables - Private (one per AZ for NAT Gateway)
resource "aws_route_table" "private" {
  count  = var.availability_zones
  vpc_id = aws_vpc.main.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.main[count.index].id
  }

  tags = merge(
    local.common_tags,
    {
      Name = "rt-private-${count.index + 1}-${local.resource_suffix}"
    }
  )
}

# Route Table Association - Private
resource "aws_route_table_association" "private" {
  count          = var.availability_zones
  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = aws_route_table.private[count.index].id
}

# Security Group for ALB
resource "aws_security_group" "alb" {
  name        = "sg-alb-${local.resource_suffix}"
  description = "Security group for ALB"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(
    local.common_tags,
    {
      Name = "sg-alb-${local.resource_suffix}"
    }
  )
}

# Security Group for ECS Tasks
resource "aws_security_group" "ecs_tasks" {
  name        = "sg-ecs-${local.resource_suffix}"
  description = "Security group for ECS tasks"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port       = 0
    to_port         = 65535
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(
    local.common_tags,
    {
      Name = "sg-ecs-${local.resource_suffix}"
    }
  )
}

# Application Load Balancer
resource "aws_lb" "main" {
  name               = "alb-${local.resource_suffix}"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = aws_subnet.public[*].id

  enable_deletion_protection = false

  tags = merge(
    local.common_tags,
    {
      Name = "alb-${local.resource_suffix}"
    }
  )
}

# CloudWatch Log Group for ECS
resource "aws_cloudwatch_log_group" "ecs" {
  name              = "/ecs/${local.resource_suffix}"
  retention_in_days = var.log_retention_days

  tags = merge(
    local.common_tags,
    {
      Name = "log-group-ecs-${local.resource_suffix}"
    }
  )
}

# CloudWatch Log Groups for Applications
resource "aws_cloudwatch_log_group" "app1" {
  name              = "/ecs/${local.app1_full_name}"
  retention_in_days = var.log_retention_days

  tags = merge(
    local.common_tags,
    {
      Name = "log-group-app1-${local.resource_suffix}"
    }
  )
}

resource "aws_cloudwatch_log_group" "app2" {
  name              = "/ecs/${local.app2_full_name}"
  retention_in_days = var.log_retention_days

  tags = merge(
    local.common_tags,
    {
      Name = "log-group-app2-${local.resource_suffix}"
    }
  )
}

# ECR Repository for App 1
resource "aws_ecr_repository" "app1" {
  name                 = "ecr-${var.app1_name}-${local.resource_suffix}"
  image_tag_mutability = var.ecr_image_tag_mutability

  image_scanning_configuration {
    scan_on_push = var.ecr_scan_on_push
  }

  tags = merge(
    local.common_tags,
    {
      Name = "ecr-${var.app1_name}-${local.resource_suffix}"
    }
  )
}

# ECR Repository for App 2
resource "aws_ecr_repository" "app2" {
  name                 = "ecr-${var.app2_name}-${local.resource_suffix}"
  image_tag_mutability = var.ecr_image_tag_mutability

  image_scanning_configuration {
    scan_on_push = var.ecr_scan_on_push
  }

  tags = merge(
    local.common_tags,
    {
      Name = "ecr-${var.app2_name}-${local.resource_suffix}"
    }
  )
}

# ECS Cluster
resource "aws_ecs_cluster" "main" {
  name = "${var.ecs_cluster_name}-${local.resource_suffix}"

  setting {
    name  = "containerInsights"
    value = var.enable_container_insights ? "enabled" : "disabled"
  }

  tags = merge(
    local.common_tags,
    {
      Name = "ecs-cluster-${local.resource_suffix}"
    }
  )
}

# ECS Cluster Capacity Providers
resource "aws_ecs_cluster_capacity_providers" "main" {
  cluster_name = aws_ecs_cluster.main.name

  capacity_providers = ["FARGATE", "FARGATE_SPOT"]

  default_capacity_provider_strategy {
    base              = 1
    weight            = 100
    capacity_provider = "FARGATE"
  }
}

# IAM Role for ECS Task Execution
resource "aws_iam_role" "ecs_task_execution_role" {
  name = "iam-ecs-task-exec-${local.resource_suffix}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = local.common_tags
}

# Attach execution role policy
resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_policy" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# IAM Role for ECS Task (application role)
resource "aws_iam_role" "ecs_task_role" {
  name = "iam-ecs-task-${local.resource_suffix}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = local.common_tags
}

# Allow tasks to push logs to CloudWatch
resource "aws_iam_role_policy" "ecs_task_cloudwatch_logs" {
  name = "policy-ecs-logs-${local.resource_suffix}"
  role = aws_iam_role.ecs_task_execution_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = [
          "${aws_cloudwatch_log_group.app1.arn}:*",
          "${aws_cloudwatch_log_group.app2.arn}:*"
        ]
      }
    ]
  })
}

# Allow tasks to pull images from ECR
resource "aws_iam_role_policy" "ecs_task_ecr_pull" {
  name = "policy-ecr-pull-${local.resource_suffix}"
  role = aws_iam_role.ecs_task_execution_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ecr:GetAuthorizationToken",
          "ecr:BatchGetImage",
          "ecr:GetDownloadUrlForLayer",
          "ecr:BatchCheckLayerAvailability"
        ]
        Resource = "*"
      }
    ]
  })
}

# ECS Task Definition for App 1 (WebAPI)
resource "aws_ecs_task_definition" "app1" {
  family                   = local.app1_full_name
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.app1_cpu
  memory                   = var.app1_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([
    {
      name      = var.app1_name
      image     = var.app1_docker_image
      essential = true
      portMappings = [
        {
          containerPort = var.app1_port
          hostPort      = var.app1_port
          protocol      = "tcp"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.app1.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      environment = [
        for key, value in merge(
          {
            "ASPNETCORE_ENVIRONMENT" = title(var.environment)
            "ENVIRONMENT"            = var.environment
          },
          var.app1_custom_settings
        ) : {
          name  = key
          value = value
        }
      ]

      healthCheck = {
        command     = ["CMD-SHELL", "curl -f http://localhost:${var.app1_port}${var.health_check_path} || exit 1"]
        interval    = var.health_check_interval
        timeout     = var.health_check_timeout
        retries     = 3
        startPeriod = 60
      }

      xrayTracingConfig = var.enable_xray_tracing ? {
        mode = "active"
      } : null
    }
  ])

  tags = merge(
    local.common_tags,
    {
      Name = "task-def-${var.app1_name}-${local.resource_suffix}"
    }
  )

  depends_on = [
    aws_iam_role_policy.ecs_task_cloudwatch_logs,
    aws_iam_role_policy.ecs_task_ecr_pull
  ]
}

# ECS Task Definition for App 2 (Worker)
resource "aws_ecs_task_definition" "app2" {
  family                   = local.app2_full_name
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.app2_cpu
  memory                   = var.app2_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([
    {
      name      = var.app2_name
      image     = var.app2_docker_image
      essential = true

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.app2.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "ecs"
        }
      }

      environment = [
        for key, value in merge(
          {
            "ASPNETCORE_ENVIRONMENT" = title(var.environment)
            "ENVIRONMENT"            = var.environment
          },
          var.app2_custom_settings
        ) : {
          name  = key
          value = value
        }
      ]

      xrayTracingConfig = var.enable_xray_tracing ? {
        mode = "active"
      } : null
    }
  ])

  tags = merge(
    local.common_tags,
    {
      Name = "task-def-${var.app2_name}-${local.resource_suffix}"
    }
  )

  depends_on = [
    aws_iam_role_policy.ecs_task_cloudwatch_logs,
    aws_iam_role_policy.ecs_task_ecr_pull
  ]
}

# ALB Target Group for App 1
resource "aws_lb_target_group" "app1" {
  count            = var.app1_enable_load_balancing ? 1 : 0
  name             = "tg-${var.app1_name}-${local.resource_suffix}"
  port             = var.app1_port
  protocol         = "HTTP"
  vpc_id           = aws_vpc.main.id
  target_type      = "ip"
  health_check {
    healthy_threshold   = var.health_check_healthy_threshold
    unhealthy_threshold = var.health_check_unhealthy_threshold
    timeout             = var.health_check_timeout
    interval            = var.health_check_interval
    path                = var.health_check_path
    matcher             = "200"
  }

  tags = merge(
    local.common_tags,
    {
      Name = "tg-${var.app1_name}-${local.resource_suffix}"
    }
  )
}

# ALB Listener for App 1
resource "aws_lb_listener" "app1" {
  count            = var.app1_enable_load_balancing ? 1 : 0
  load_balancer_arn = aws_lb.main.arn
  port             = 80
  protocol         = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.app1[0].arn
  }
}

# ECS Service for App 1 (WebAPI)
resource "aws_ecs_service" "app1" {
  name            = local.app1_full_name
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app1.arn
  desired_count   = var.app1_min_tasks
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.private[*].id
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = false
  }

  dynamic "load_balancer" {
    for_each = var.app1_enable_load_balancing ? [1] : []
    content {
      target_group_arn = aws_lb_target_group.app1[0].arn
      container_name   = var.app1_name
      container_port   = var.app1_port
    }
  }

  depends_on = [
    aws_lb_listener.app1
  ]

  tags = merge(
    local.common_tags,
    {
      Name = "svc-${var.app1_name}-${local.resource_suffix}"
    }
  )
}

# ECS Service for App 2 (Worker)
resource "aws_ecs_service" "app2" {
  name            = local.app2_full_name
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.app2.arn
  desired_count   = var.app2_min_tasks
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = aws_subnet.private[*].id
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = false
  }

  tags = merge(
    local.common_tags,
    {
      Name = "svc-${var.app2_name}-${local.resource_suffix}"
    }
  )
}

# Auto Scaling Target for App 1
resource "aws_appautoscaling_target" "app1" {
  max_capacity       = var.app1_max_tasks
  min_capacity       = var.app1_min_tasks
  resource_id        = "service/${aws_ecs_cluster.main.name}/${aws_ecs_service.app1.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

# Auto Scaling Policy - CPU for App 1
resource "aws_appautoscaling_policy" "app1_cpu" {
  name               = "policy-cpu-${var.app1_name}-${local.resource_suffix}"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.app1.resource_id
  scalable_dimension = aws_appautoscaling_target.app1.scalable_dimension
  service_namespace  = aws_appautoscaling_target.app1.service_namespace

  target_tracking_scaling_policy_configuration {
    target_value = var.app1_target_cpu_utilization

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }

    scale_out_cooldown = var.scale_up_cooldown
    scale_in_cooldown  = var.scale_down_cooldown
  }
}

# Auto Scaling Policy - Memory for App 1
resource "aws_appautoscaling_policy" "app1_memory" {
  name               = "policy-mem-${var.app1_name}-${local.resource_suffix}"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.app1.resource_id
  scalable_dimension = aws_appautoscaling_target.app1.scalable_dimension
  service_namespace  = aws_appautoscaling_target.app1.service_namespace

  target_tracking_scaling_policy_configuration {
    target_value = var.app1_target_memory_utilization

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageMemoryUtilization"
    }

    scale_out_cooldown = var.scale_up_cooldown
    scale_in_cooldown  = var.scale_down_cooldown
  }
}

# Auto Scaling Target for App 2
resource "aws_appautoscaling_target" "app2" {
  max_capacity       = var.app2_max_tasks
  min_capacity       = var.app2_min_tasks
  resource_id        = "service/${aws_ecs_cluster.main.name}/${aws_ecs_service.app2.name}"
  scalable_dimension = "ecs:service:DesiredCount"
  service_namespace  = "ecs"
}

# Auto Scaling Policy - CPU for App 2
resource "aws_appautoscaling_policy" "app2_cpu" {
  name               = "policy-cpu-${var.app2_name}-${local.resource_suffix}"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.app2.resource_id
  scalable_dimension = aws_appautoscaling_target.app2.scalable_dimension
  service_namespace  = aws_appautoscaling_target.app2.service_namespace

  target_tracking_scaling_policy_configuration {
    target_value = var.app2_target_cpu_utilization

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageCPUUtilization"
    }

    scale_out_cooldown = var.scale_up_cooldown
    scale_in_cooldown  = var.scale_down_cooldown
  }
}

# Auto Scaling Policy - Memory for App 2
resource "aws_appautoscaling_policy" "app2_memory" {
  name               = "policy-mem-${var.app2_name}-${local.resource_suffix}"
  policy_type        = "TargetTrackingScaling"
  resource_id        = aws_appautoscaling_target.app2.resource_id
  scalable_dimension = aws_appautoscaling_target.app2.scalable_dimension
  service_namespace  = aws_appautoscaling_target.app2.service_namespace

  target_tracking_scaling_policy_configuration {
    target_value = var.app2_target_memory_utilization

    predefined_metric_specification {
      predefined_metric_type = "ECSServiceAverageMemoryUtilization"
    }

    scale_out_cooldown = var.scale_up_cooldown
    scale_in_cooldown  = var.scale_down_cooldown
  }
}
