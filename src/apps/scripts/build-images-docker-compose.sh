#!/bin/bash

# Build with docker compose
docker compose -f ./src/apps/docker-compose.yml -f ./src/apps/docker-compose.override.yml --env-file ./.env --project-name genocs build
