# .NET Core libraries 



| Badge                      | Description                    |
| -------------------------- | ------------------------------ |
| Travis CI/CD               | [![Travis](https://travis-ci.org/Genocs/genocs-library.svg?branch=master)](https://travis-ci.org/Genocs/genocs-library)       |
| Github Actions             | [![.NET](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml)     |
| Azure CI/CD                | work in progress |
| NuGet package latesversion | [![NuGet](https://img.shields.io/badge/nuget-v2.4.0-blue)](https://www.nuget.org/packages/Genocs.Core) |
| NuGet package downloads    | [![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core) |
| Discord community          | ![Discord](https://dcbadge.vercel.app/api/shield/461057072054927361?style=flat-square)  |


----

This repo contains a set of basic libraries designed by Genocs. The libraries are built using .NET standard 2.1. The package version is hosted on [nuget](https://www.nuget.org/packages).


To build the project type following command
``` bash
dotnet build .\src
```

To pack the project type following command
``` bash
dotnet pack .\src

cd src/Genocs.Core
dotnet pack -p:NuspecFile=./Genocs.Core.nuspec --no-restore -o .
```


To push the project type following command
``` bash
dotnet nuget push
dotnet nuget push *.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
```

## How to build Docker Demo images

Following how to build docker images
``` bash
# **** webapi ****
# Build
docker build -t genocs/demo-webapi:1.0.0 -t genocs/demo-webapi:latest -f .\webapi.dockerfile .

# Add tags
docker tag genocs/demo-webapi genocs/demo-webapi:1.0.0
docker tag genocs/demo-webapi genocs/demo-webapi:latest

# Push on Dockerhub
docker push genocs/demo-webapi:1.0.0
docker push genocs/demo-webapi:latest

# **** worker ****
# Build
docker build -t genocs/demo-worker:1.0.0 -t genocs/demo-worker:latest -f .\worker.dockerfile .

# Add tags
docker tag genocs/demo-worker genocs/demo-worker:1.0.0
docker tag genocs/demo-worker genocs/demo-worker:latest

# Push on Dockerhub
docker push genocs/demo-worker:1.0.0
docker push genocs/demo-worker:latest
```

A second option is to run docker-compose
``` bash
# Build with docker compose
docker-compose -f .\docker-compose.yml build

# Run with docker compose
docker-compose -f .\docker-compose.yml up [-d]
```

## Core

Core project contains general purpose functionalities to be used on DDD service

## Persistence MongoDb

Persistente  MongoDb is the library that allows to persist data on MongoDb

## ServiceBusAzure
The ServiceBusAzure allows to publish/send messages to Azure Service Bus

- [service-bus-quickstart-cli](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-cli)


``` PS
# Create the namespace
az servicebus namespace create --resource-group rg-genocs --name asb-genocs --location "West Europe"

# Create the queue
az servicebus queue create --resource-group rg-genocs --namespace-name asb-genocs --name queue_1

# Get the connection string
az servicebus namespace authorization-rule keys list --resource-group rg-genocs --namespace-name asb-genocs --name RootManageSharedAccessKey --query primaryConnectionString --output tsv 

```


## Support

api-workbench.rest

Use this file inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin 

