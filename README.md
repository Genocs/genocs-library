# Genocs .NET libraries

| Badge                       | Description                    |
| --------------------------- | ------------------------------ |
| Travis CI/CD                | [![Travis](https://travis-ci.org/Genocs/genocs-library.svg?branch=master)](https://travis-ci.org/Genocs/genocs-library)       |
| Github Actions              | [![.NET](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml)     |
| Azure CI/CD                 | work in progress |
| NuGet package latestversion | [![NuGet](https://img.shields.io/badge/nuget-v2.4.0-blue)](https://www.nuget.org/packages/Genocs.Core) |
| NuGet package downloads     | [![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core) |
| Discord community           | ![Discord](https://dcbadge.vercel.app/api/shield/461057072054927361?style=flat-square)  |

---

This repo contains a set of libraries designed by Genocs. The libraries are built using .NET standard 2.1 or .NET7. 

Packages are availables on [nuget](https://www.nuget.org/packages).


***Infrastructure***
---
This section will shows how to setup the infrastructure components

## Docker compose
If you want to use docker compose to run infrastructure and application you can run the commands below

``` bash
# Setup the infrastructure  
docker-compose -f ./containers/infrastructure-bare.yml -f ./containers/infrastructure-monitoring.yml --project-name genocs-application up -d
```


## Kubernetes cluster

***TBW***

***Application***
---

Some useful commands
``` bash
# Build the solution 
dotnet build

# Pack the projects
dotnet pack

# Run project with console
dotnet run --project ./src/Genocs.Core.Demo.WebApi
dotnet run --project ./src/Genocs.Core.Demo.Worker
```

``` bash
# To pack the project with nuspec file
cd src/Genocs.Core
dotnet pack -p:NuspecFile=./Genocs.Core.nuspec --no-restore -o .

# To push on nuget
dotnet nuget push
dotnet nuget push *.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
```

## How to build Docker Demo images

``` bash
# Build webapi
docker build -t genocs/demo-webapi:2.0.0 -t genocs/demo-webapi:latest -f ./demo-webapi.dockerfile .

# Push on Dockerhub
docker push genocs/demo-webapi:2.0.0
docker push genocs/demo-webapi:latest

# Build worker
docker build -t genocs/demo-worker:2.0.0 -t genocs/demo-worker:latest -f ./demo-worker.dockerfile .

# Push on Dockerhub
docker push genocs/demo-worker:2.0.0
docker push genocs/demo-worker:latest
```

A second option is to run docker-compose

``` bash
# Build with docker compose
docker-compose -f ./demo-docker-compose.yml build

# Run with docker compose
docker-compose -f ./demo-docker-compose.yml up [-d]
```

## Auth

Auth service defines the functionalities to build Auth system that relay on OAUTH JWT.

## Common

This project contains common functionalities that do not relay on runtime or other libraries. This project contains interfaces and base classes used as placeholders and to be used across without dependencies.

## Core

Core project contains general purpose functionalities to be used on DDD service.

## HTTP

This library implements HTTP client functionalities.

## Logging

This library implements logging functionalities.

## MessageBrokers

TBW

## MessageBrokers Outbox

TBW

## MessageBrokers MongoDB

TBW


## MessageBrokers RabbitMQ

TBW

## Metrics

TBW

## Persistence MongoDB

Persistence MongoDB is the library that allows to persist data on MongoDB.

## Persistence Redis

Persistence Redis is the library that allows to persist data on Redis database.

## Query Builder

TBW

## Secrets Vault

TBW

## Security

TBW

## ServiceBusAzure

The ServiceBusAzure allows to publish/send messages to Azure Service Bus

- [service-bus-quickstart-cli](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-cli)

``` PS
# Create Azure Resource group
az group create -n rg-genocs --location "West Europe"

# Create WebApp
# https://learn.microsoft.com/en-us/cli/azure/webapp?view=azure-cli-latest#az-webapp-create

# Create the Azure Servicebus namespace
az servicebus namespace create --resource-group rg-genocs --name asb-genocs --location "West Europe"

# Create the queue
az servicebus queue create --resource-group rg-genocs --namespace-name asb-genocs --name queue_1

# Get the connection string
az servicebus namespace authorization-rule keys list --resource-group rg-genocs --namespace-name asb-genocs --name RootManageSharedAccessKey --query primaryConnectionString --output tsv 

```

## Tracing

TBW

## WebApi

TBW

## WebApi CQRS

TBW

## WebApi Security

TBW


## WebApi Swagger

TBW




## Support

api-workbench.rest

Use this file inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin 

## Configuration

``` json
   "app": {
    "name": "Demo WebApi",
    "service": "demo-service",
    "instance": "01",
    "version": "v1.0",
    "displayBanner": false,
    "displayVersion": false
  },
  "consul": {
    "enabled": false,
    "url": "http://localhost:8500",
    "service": "demo-service",
    "address": "docker.for.win.localhost",
    "port": "5070",
    "pingEnabled": true,
    "pingEndpoint": "health",
    "pingInterval": 3,
    "removeAfterInterval": 3
  },
  "fabio": {
    "enabled": false,
    "url": "http://localhost:9999",
    "service": "demo-service"
  },
  "httpClient": {
    "type": "fabio",
    "retries": 3,
    "services": {},
    "requestMasking": {
      "enabled": true,
      "maskTemplate": "*****"
    },
    "correlationIdHeader": "x-correlation-id"
  },
  "logger": {
    "level": "information",
    "excludePaths": [ "/", "/ping", "/metrics" ],
    "excludeProperties": [
      "api_key",
      "access_key",
      "ApiKey",
      "ApiSecret",
      "ClientId",
      "ClientSecret",
      "ConnectionString",
      "Password",
      "Email",
      "Login",
      "Secret",
      "Token"
    ],
    "console": {
      "enabled": false
    },
    "elk": {
      "enabled": false,
      "url": "http://localhost:9200"
    },
    "file": {
      "enabled": false,
      "path": "logs/logs.txt",
      "interval": "day"
    },
    "seq": {
      "enabled": false,
      "url": "http://localhost:5341",
      "apiKey": "secret"
    },
    "azure": {
      "enabled": false,
      "connectionString": ""
    },
    "tags": {}
  },
  "jaeger": {
    "enabled": false,
    "serviceName": "users",
    "udpHost": "localhost",
    "udpPort": 6831,
    "maxPacketSize": 65000,
    "sampler": "const",
    "excludePaths": [ "/", "/ping", "/metrics" ]
  },
  "jwt": {
    "certificate": {
      "location": "certs/localhost.pfx",
      "password": "test",
      "rawData": ""
    },
    "issuer": "demo",
    "validIssuer": "demo",
    "validateAudience": false,
    "validateIssuer": false,
    "validateLifetime": false,
    "expiry": "01:00:00"
  },
  "metrics": {
    "enabled": false,
    "influxEnabled": false,
    "prometheusEnabled": false,
    "influxUrl": "http://localhost:8086",
    "database": "test",
    "env": "local",
    "interval": 5
  },
  "prometheus": {
    "enabled": false,
    "endpoint": "/metrics"
  },
  "mongo": {
    "connectionString": "mongodb://localhost:27017",
    "database": "genocs-users-service",
    "seed": false
  },
  "outbox": {
    "enabled": false,
    "type": "sequential",
    "expiry": 3600,
    "intervalMilliseconds": 2000,
    "inboxCollection": "inbox",
    "outboxCollection": "outbox",
    "disableTransactions": false
  },
  "rabbitMq": {
    "connectionName": "users-service",
    "retries": 3,
    "retryInterval": 2,
    "conventionsCasing": "snakeCase",
    "logger": {
      "enabled": false
    },
    "username": "guest",
    "password": "guest",
    "virtualHost": "/",
    "port": 5672,
    "hostnames": [
      "localhost"
    ],
    "requestedConnectionTimeout": "00:00:30",
    "requestedHeartbeat": "00:01:00",
    "socketReadTimeout": "00:00:30",
    "socketWriteTimeout": "00:00:30",
    "continuationTimeout": "00:00:20",
    "handshakeContinuationTimeout": "00:00:10",
    "networkRecoveryInterval": "00:00:05",
    "exchange": {
      "declare": true,
      "durable": true,
      "autoDelete": false,
      "type": "topic",
      "name": "users"
    },
    "queue": {
      "declare": true,
      "durable": true,
      "exclusive": false,
      "autoDelete": false,
      "template": "users-service/{{exchange}}.{{message}}"
    },
    "context": {
      "enabled": true,
      "header": "message_context"
    },
    "spanContextHeader": "span_context"
  },
  "redis": {
    "connectionString": "localhost",
    "instance": "users-service:",
    "database": 0
  },
  "swagger": {
    "enabled": false,
    "reDocEnabled": false,
    "name": "v1",
    "title": "API",
    "version": "v1",
    "routePrefix": "swagger",
    "includeSecurity": true
  },
  "security": {
    "certificate": {
      "header": "Certificate"
    }
  },
  "vault": {
    "enabled": false,
    "url": "http://localhost:8200",
    "authType": "token",
    "token": "secret",
    "username": "user",
    "password": "secret",
    "kv": {
      "enabled": true,
      "engineVersion": 2,
      "mountPoint": "kv",
      "path": "users-service/settings"
    },
    "pki": {
      "enabled": true,
      "roleName": "users-service",
      "commonName": "users-service.demo.io"
    },
    "lease": {
      "mongo": {
        "type": "database",
        "roleName": "users-service",
        "enabled": true,
        "autoRenewal": true,
        "templates": {
          "connectionString": "mongodb://{{username}}:{{password}}@localhost:27017"
        }
      }
    }
  }
  ```