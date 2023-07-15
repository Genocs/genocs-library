# Genocs .NET libraries

| Badge                       | Description                    |
| --------------------------- | ------------------------------ |
| Travis CI/CD                | [![Travis](https://travis-ci.org/Genocs/genocs-library.svg?branch=master)](https://travis-ci.org/Genocs/genocs-library)       |
| GitHub Actions              | [![.NET](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml)     |
| Azure CI/CD                 | work in progress |
| Discord community           | ![Discord](https://dcbadge.vercel.app/api/shield/461057072054927361?style=flat-square)  |
| NuGet package Version       | [![NuGet](https://img.shields.io/badge/nuget-v2.4.0-blue)](https://www.nuget.org/packages/Genocs.Core) |
| NuGet package Downloads     | [![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core) |

---

This repo contains a set of libraries designed by Genocs. The libraries are built using .NET standard 2.1 or .NET7. 

Packages are available on [NuGet Genocs](https://www.nuget.org/profiles/gioema_nocco).

---

## The idea

***Build a software library to be cloud agnostic***

Building a software library to be cloud agnostic has several advantages. First, it allows developers to create applications that can be deployed on any cloud platform without having to rewrite code or make major changes. This makes it easier for developers to quickly deploy their applications across multiple cloud providers. Additionally, it reduces the cost of development and maintenance since developers don’t have to write separate code for each cloud provider. Finally, it increases the scalability of applications since they can be easily deployed on different cloud platforms with minimal effort. 

***Cloud agnostic by use of Containers***

The advantages of using containers are numerous. Containers provide a lightweight, portable, and isolated environment for applications to run in, allowing them to be easily moved between different systems. This makes it easier to deploy applications quickly and reliably across different environments. Additionally, containers can help reduce resource consumption by running multiple applications on the same host, as each container is isolated from the others. This helps to improve efficiency and scalability. Finally, containers provide an additional layer of security, as they are isolated from the underlying operating system and other applications.

![Overview](./assets/Genocs-Library-Overview.drawio.png)


---

## Infrastructure

This section will show how to setup the infrastructure components.

***Docker compose***

If you want to use docker compose to run infrastructure you can run the command below.

``` bash
# Setup the infrastructure  
docker-compose -f ./containers/infrastructure-bare.yml -f ./containers/infrastructure-monitoring.yml -f ./containers/infrastructure-scaling.yml -f ./containers/infrastructure-security.yml --project-name genocs-infrastructure up -d
```

***Kubernetes cluster***

Second option is to setup the application inside a Kubernetes cluster.

To setup a Kubernetes cluster that can be used for production I suggest to looking at the repo: 
- [enterprise-containers](https://github.com/Genocs/enterprise-containers)

There you can find everything needed to setup a cluster from scratch.

---
## **Libraries**
The following section describes the full set of libraries.

You can find a full documentation on:
[**Documentation**](https://majestic-wisp-d90424.netlify.app/library/)


## Common

This project contains common functionalities do not relied on runtime or other libraries. This project contains interfaces and base classes used as placeholders and to be used across the platform without dependencies.

## Core

Core project contains general purpose functionalities to be used on DDD service.

## HTTP

This library implements WebAPI client HTTP functionalities.

## Logging

This library implements logging functionalities.

## MessageBrokers

This library implements the domain message broker for commands and events.

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


  ---

## Demo Application
Inside the library there is a simple demo application you can use to test the library. 

Some commands
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

### How to build Docker Demo images

``` bash
# Build webapi
docker build -t genocs/demo-webapi:2.0.0 -t genocs/demo-webapi:latest -f ./demo-webapi.dockerfile .
docker build -t genocs/apigateway:7.0.0 -f ./src/apps/containers/apigateway.dockerfile ../../.

# Push on Dockerhub
docker push genocs/demo-webapi:2.0.0
docker push genocs/demo-webapi:latest

# Build worker
docker build -t genocs/demo-worker:2.0.0 -t genocs/demo-worker:latest -f ./demo-worker.dockerfile .

# Push on Dockerhub
docker push genocs/demo-worker:2.0.0
docker push genocs/demo-worker:latest
```


 ---

## Enterprise Application


Take a look inside **./src/apps** folder. There you can find a full-fledged application composed by:
- ApiGateway
- Identity Service
- Order service
- Product Service
- SignalR Service

In that way you can test the entire flow.



### **How to BUILD & RUN the application**

The build and run process can be done by using docker-compose

``` bash
# Build with docker compose
docker-compose -f ./src/apps/application-docker-compose.yml --project-name genocs-app build

# *** Before running the solution remember to check ***
# *** if the infrastructure services were setup     ***

# Run with docker compose
docker-compose -f ./src/apps/application-docker-compose.yml --project-name genocs-app up -d

# Clean Docker cache
docker builder prune
```

---



## **Third party libraries**
- masstransit
- fabio
- consul


---

## **acknowledgments**

- [devmentors](https://github.com/devmentors)
- [abp](https://github.com/abpframework)


