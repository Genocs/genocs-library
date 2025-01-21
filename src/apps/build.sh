#!/bin/bash

cd ./src/apps

# Build with docker compose
docker compose -f ./docker-compose.yml -f ./docker-compose.override.yml --env-file ./local.env --project-name genocs build

# Set environment variables
# read variables from .env file
export $(grep -v '^#' ./local.env | xargs)

export IMAGE_VERSION=1.0.4


# Push on Dockerhub
docker push genocs/apigateway:$IMAGE_VERSION
docker tag genocs/apigateway:$IMAGE_VERSION genocs/apigateway:latest
docker push genocs/apigateway:latest
docker push genocs/identities-webapi:$IMAGE_VERSION
docker tag genocs/identities-webapi:$IMAGE_VERSION genocs/identities-webapi:latest
docker push genocs/identities-webapi:latest
docker push genocs/products-webapi:$IMAGE_VERSION
docker tag genocs/products-webapi:$IMAGE_VERSION genocs/products-webapi:latest
docker push genocs/products-webapi:latest
docker push genocs/orders-webapi:$IMAGE_VERSION
docker tag genocs/orders-webapi:$IMAGE_VERSION genocs/orders-webapi:latest
docker push genocs/orders-webapi:latest
docker push genocs/notifications-webapi:$IMAGE_VERSION
docker tag genocs/notifications-webapi:$IMAGE_VERSION genocs/notifications-webapi:latest
docker push genocs/notifications-webapi:latest
