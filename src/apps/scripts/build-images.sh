#!/bin/bash

# Set environment variables coming from .env file
export $(grep -v '^#' ./local.env | xargs)

# Build the api gateway
docker build -t genocs/apigateway:$IMAGE_VERSION -t genocs/apigateway:latest -f ./src/apps/apigateway/WebApi/Dockerfile .

# Build the identities service
docker build -t genocs/identities-webapi:$IMAGE_VERSION -t genocs/identities-webapi:latest -f ./src/apps/identity/WebApi/Dockerfile .

# Build the products service
docker build -t genocs/products-webapi:$IMAGE_VERSION -t genocs/products-webapi:latest -f ./src/apps/products/WebApi/Dockerfile .

# Build the orders service
docker build -t genocs/orders-webapi:$IMAGE_VERSION -t genocs/orders-webapi:latest -f ./src/apps/orders/WebApi/Dockerfile .

# Build the notifications service
docker build -t genocs/notifications-webapi:$IMAGE_VERSION -t genocs/notifications-webapi:latest -f ./src/apps/notifications/WebApi/Dockerfile .
