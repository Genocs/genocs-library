#!/bin/bash

cd ./infrastructure/containers

# Run base components (MongoDB and RabbitMQ)
# you can Remove the element '-f /infrastructure-monitoring.wsl2.yml' if you are using Linux or Mac OS
docker compose -f ./infrastructure.yml --env-file ./.env --project-name genocs up -d

sleep 5
docker compose -f ./infrastructure-monitoring.yml -f ./infrastructure-monitoring.wsl2.yml --env-file ./.env --project-name genocs up -d
