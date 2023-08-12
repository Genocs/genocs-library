# Genocs .NET library

[![GitHub](https://img.shields.io/github/license/Genocs/genocs-library?color=2da44e&style=flat-square)](https://github.com/Genocs/genocs-library/blob/main/LICENSE)
[![.NET build and test](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml)
[![NuGet](https://img.shields.io/badge/nuget-v.5.0.0-blue)](https://www.nuget.org/packages/Genocs.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core)
[![Discord](https://img.shields.io/discord/1106846706512953385?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat-square)](https://discord.com/invite/fWwArnkV)
[![Twitter](https://img.shields.io/twitter/follow/genocs?color=1DA1F2&label=Twitter&logo=Twitter&style=flat-square)](https://twitter.com/genocs)


---
This repo contains a set of libraries designed by Genocs. The libraries are built using .NET standard 2.1 or .NET7. 

Packages are available on [NuGet Genocs](https://www.nuget.org/profiles/gioema_nocco).

## The idea

***Build a software library to be cloud agnostic***

Building a software library to be cloud agnostic has several advantages. First, it allows developers to create applications that can be deployed on any cloud platform without having to rewrite code or make major changes. This makes it easier for developers to quickly deploy their applications across multiple cloud providers. Additionally, it reduces the cost of development and maintenance since developers don’t have to write separate code for each cloud provider. Finally, it increases the scalability of applications since they can be easily deployed on different cloud platforms with minimal effort. 

***Cloud agnostic by use of Containers***

The advantages of using containers are numerous. Containers provide a lightweight, portable, and isolated environment for applications to run in, allowing them to be easily moved between different systems. This makes it easier to deploy applications quickly and reliably across different environments. Additionally, containers can help reduce resource consumption by running multiple applications on the same host, as each container is isolated from the others. This helps to improve efficiency and scalability. Finally, containers provide an additional layer of security, as they are isolated from the underlying operating system and other applications.

## Infrastructure

This section will show how to setup the infrastructure components.

***Docker compose***

Use the command below to use docker compose.


``` bash
# Setup the infrastructure
docker-compose -f ./containers/infrastructure-bare.yml --project-name genocs-infrastructure up -d
docker-compose -f ./containers/infrastructure-monitoring.yml --project-name genocs-infrastructure up -d
docker-compose -f ./containers/infrastructure-scaling.yml --project-name genocs-infrastructure up -d
docker-compose -f ./containers/infrastructure-security.yml --project-name genocs-infrastructure up -d
docker-compose -f ./containers/infrastructure-sqlserver.yml --project-name genocs-infrastructure up -d
docker-compose -f ./containers/infrastructure-elk.yml --project-name genocs-infrastructure up -d
docker-compose -f ./containers/infrastructure-ml.yml --project-name genocs-infrastructure up -d
```

`infrastructure-bare.yml` allows to install the basic infrastructure components.

Inside the file you can find:

- rabbitmq
- redis
- mongo
- postgresql

`infrastructure-monitoring.yml` allows to install the monitoring infrastructure components.

Inside the file you can find:

- prometheus
- grafana
- influxdb
- jaeager
- seq

`infrastructure-scaling.yml` allows to install the scaling infrastructure components.

Inside the file you can find:

- fabio
- consul

`infrastructure-security.yml` allows to install the security infrastructure components.

Inside the file you can find:

- vault (hashicorp)

The script below allows to setup the infrastructure components. This means that you can find all the containers inside the same network.

The network is called `genocs`.

``` yml 
networks:
  genocs:
    name: genocs-network
    driver: bridge
```

Remember to add the network configuration inside your docker compose file to setup the network, before running the containers.

``` yml
networks:
  genocs:
    name: genocs-network
    external: true
    driver: bridge
```

## ***Kubernetes cluster***

You can setup the application inside a Kubernetes cluster.

Check the repo [enterprise-containers](https://github.com/Genocs/enterprise-containers) to setup a Kubernetes cluster.

Inside the repo you can find scripts, configuration files and documentation to setup a cluster from scratch.

## **Libraries**
You can find a full documentation on:
[**Documentation**](https://genocs-blog.netlify.app/library/)

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



## Support



Use [**api-workbench**](./api-workbench.rest) inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin 

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
- Order Service
- Product Service
- SignalR Service

In that way you can test the entire flow.



### How to BUILD & RUN the application

The build and run process can be done by using docker-compose

``` bash
cd src/apps

# Build with docker compose
docker compose -f ./application-docker-compose.yml -f ./application-docker-compose.override.yml --env-file ./local.env --project-name genocs-library build

# *** Before running the solution remember to check ***
# *** if the infrastructure services were setup     ***

# Run with docker compose
docker compose -f ./src/apps/application-docker-compose.yml --env-file ./local.env --project-name genocs-library up -d

# Clean Docker cache
docker builder prune
```


Following commands are useful to build and push the images one by one

``` bash
# Build the api gateway
docker build -t genocs/apigateway:1.0.0 -f ./src/apps/containers/apigateway.dockerfile ../../.

# Build the identities service
docker build -t genocs/identities:1.0.0 -f ./src/apps/containers/identity-webapi.dockerfile ../../.

# Build the order service
docker build -t genocs/orders:1.0.0 -f ./src/apps/containers/order-webapi.dockerfile ../../.

# Build the product service
docker build -t genocs/products:1.0.0 -f ./src/apps/containers/product-webapi.dockerfile ../../.

# Build the signalr service
docker build -t genocs/signalr:1.0.0 -f ./src/apps/containers/signalr-webapi.dockerfile ../../.


# Push on Dockerhub
docker push genocs/apigateway:1.0.0
docker push genocs/apigateway:latest
docker push genocs/identities:1.0.0
docker push genocs/identities:latest
docker push genocs/orders:1.0.0
docker push genocs/orders:latest
docker push genocs/products:1.0.0
docker push genocs/products:latest
docker push genocs/signalr:1.0.0
docker push genocs/signalr:latest
```


### Deploy in a cloud instance

You can deploy Demo Application with one click in Heroku, Microsoft Azure, or Google Cloud Platform: 

[<img src="https://www.herokucdn.com/deploy/button.svg" height="30px">](https://heroku.com/deploy?template=https://github.com/heartexlabs/label-studio/tree/heroku-persistent-pg)
[<img src="https://aka.ms/deploytoazurebutton" height="30px">](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fheartexlabs%2Flabel-studio%2Fmaster%2Fazuredeploy.json)
[<img src="https://deploy.cloud.run/button.svg" height="30px">](https://deploy.cloud.run)


## Community

- Discord [@genocs](https://discord.com/invite/fWwArnkV)
- Facebook Page [@genocs](https://facebook.com/Genocs)
- Youtube Channel [@genocs](https://youtube.com/c/genocs)

## Contributors

Submit your PR and join the list!

<a href="https://github.com/Genocs/genocs-library/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=Genocs/genocs-library" />
</a>

## License

This project is licensed with the [MIT license](LICENSE).

## Support :star:

Has this project helped you learn something New? or Helped you at work?
Here are a few ways by which you can support.

-   Leave a star! :star:
-   Recommend this awesome project to your colleagues. 🥇
-   Do consider endorsing me on LinkedIn for ASP.NET Core - [Connect via LinkedIn](https://www.linkedin.com/in/giovanni-emanuele-nocco-b31a5169/) 🦸
-   Or, If you want to support this project in the long run, [consider buying me a coffee](https://www.buymeacoffee.com/genocs)! ☕

<br>
    
<a href="https://www.buymeacoffee.com/genocs"><img width="250" alt="black-button" src="https://user-images.githubusercontent.com/31455818/138557309-27587d91-7b82-4cab-96bb-90f4f4e600f1.png" ></a>

## **acknowledgments**

- [devmentors](https://github.com/devmentors)
- [abp](https://github.com/abpframework)