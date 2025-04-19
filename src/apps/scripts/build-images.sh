#!/bin/bash

cd ./src/apps

# Set environment variables coming from  Read variables from .env file
export $(grep -v '^#' ./local.env | xargs)


# Build the api gateway
docker build -t genocs/apigateway:$IMAGE_VERSION -t genocs/apigateway:latest -f ./apigateway.dockerfile .

# Build the identities service
docker build -t genocs/identities-webapi:$IMAGE_VERSION -t genocs/identities-webapi:latest -f ./identities.dockerfile .

# Build the products service
docker build -t genocs/products-webapi:$IMAGE_VERSION -t genocs/products-webapi:latest -f ./products.dockerfile .

# Build the orders service
docker build -t genocs/orders-webapi:$IMAGE_VERSION -t genocs/orders-webapi:latest -f ./orders.dockerfile .

# Build the notifications service
docker build -t genocs/notifications-webapi:$IMAGE_VERSION -t genocs/notifications-webapi:latest -f ./notifications.dockerfile .

cd ..
cd ..