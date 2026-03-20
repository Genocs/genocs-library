#!/bin/bash

# Build with docker compose
docker compose -f ./src/apps/docker-compose.yml -f ./src/apps/docker-compose.override.yml --env-file ./local.env --project-name genocs build

# Set environment variables coming from .env file
export $(grep -v '^#' ./local.env | xargs)

# Read service list file
SERVICE_LIST_FILE="./src/apps/scripts/service-list.txt"

if [[ ! -f "$SERVICE_LIST_FILE" ]]; then
    echo "Error: Service list file not found: $SERVICE_LIST_FILE"
    exit 1
fi

# Push on Dockerhub
echo "Starting to push Docker images..."

# Read each service from the file and push the Docker image
while IFS= read -r service_name || [[ -n "$service_name" ]]; do
    # Skip empty lines and comments
    if [[ -z "$service_name" || "$service_name" =~ ^[[:space:]]*# ]]; then
        continue
    fi
    
    # Remove any trailing whitespace
    service_name=$(echo "$service_name" | xargs)    
    echo "Pushing $image_name..."
    
    # Push versioned image
    docker push genocs/$image_name:$IMAGE_VERSION
    if [[ $? -ne 0 ]]; then
        echo "Error: Failed to push genocs/$image_name:$IMAGE_VERSION"
        exit 1
    fi
    
    # Tag and push latest
    docker tag genocs/$image_name:$IMAGE_VERSION genocs/$image_name:latest
    docker push genocs/$image_name:latest
    if [[ $? -ne 0 ]]; then
        echo "Error: Failed to push genocs/$image_name:latest"
        exit 1
    fi
    
    echo "Successfully pushed $image_name"
    
done < "$SERVICE_LIST_FILE"

echo "All images pushed successfully!"