#!/bin/bash

# Build with docker compose
docker compose -f ./src/apps/docker-compose.yml -f ./src/apps/docker-compose.override.yml --env-file ./local.env --project-name genocs build

# Set environment variables coming from  Read variables from .env file
export $(grep -v '^#' ./local.env | xargs)

# Push on Dockerhub

# create a loop to push all images
for image in apigateway identities-webapi products-webapi orders-webapi notifications-webapi
do
    docker push genocs/$image:$IMAGE_VERSION
    docker tag genocs/$image:$IMAGE_VERSION genocs/$image:latest
    docker push genocs/$image:latest
done