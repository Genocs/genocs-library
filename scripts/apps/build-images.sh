#!/bin/bash

# Set environment variables coming from .env file
export $(grep -v '^#' ./local.env | xargs)

# Read service list file
SERVICE_LIST_FILE="./src/apps/scripts/service-list.txt"

if [ ! -f "$SERVICE_LIST_FILE" ]; then
    echo "Error: Service list file not found: $SERVICE_LIST_FILE"
    exit 1
fi

# Function to get dockerfile path and service directory based on service name
get_service_info() {
    local service_name=$1
    case $service_name in
        "apigateway")
            echo "apigateway apigateway"
            ;;
        "identities-webapi")
            echo "identities-webapi identities"
            ;;
        "products-webapi")
            echo "products-webapi products"
            ;;
        "orders-webapi")
            echo "orders-webapi orders"
            ;;
        "notifications-webapi")
            echo "notifications-webapi notifications"
            ;;
        *)
            echo "Unknown service: $service_name" >&2
            return 1
            ;;
    esac
}

# Read each service from the file and build the Docker image
while IFS= read -r service_name || [ -n "$service_name" ]; do
    # Skip empty lines and comments
    if [[ -z "$service_name" || "$service_name" =~ ^[[:space:]]*# ]]; then
        continue
    fi
    
    # Remove any trailing whitespace
    service_name=$(echo "$service_name" | xargs)
    
    # Get service info
    service_info=$(get_service_info "$service_name")
    if [ $? -ne 0 ]; then
        echo "Skipping unknown service: $service_name"
        continue
    fi
    
    # Extract image name and directory name
    read -r image_name dir_name <<< "$service_info"
    
    echo "Building $image_name..."
    docker build -t genocs/$image_name:$IMAGE_VERSION -t genocs/$image_name:latest -f ./src/apps/$dir_name/WebApi/Dockerfile .
    
    if [ $? -ne 0 ]; then
        echo "Error: Failed to build $image_name"
        exit 1
    fi
    
    echo "Successfully built $image_name"
    
done < "$SERVICE_LIST_FILE"

echo "All services built successfully!"
