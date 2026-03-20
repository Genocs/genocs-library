#!/bin/bash

# Print a message to the console
echo "Building and running Web Apps Docker images with Docker Compose..."

cd ./infrastructure/containers/apps
# Build with docker compose
docker compose -f ./docker-compose.override.yml -f ./docker-compose.yml --env-file ./.env --project-name genocs build

# Run with docker compose
docker compose -f ./docker-compose.yml --env-file ./.env --project-name genocs up -d

# Go back to the root directory
cd ../../../

echo "Web App Docker images built and running successfully."