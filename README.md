# .NET Core libraries 



| Badge                    | Description                    |
| ------------------------ | ------------------------------ |
| Travis CI/CD             | [![Travis](https://travis-ci.org/Genocs/genocs-library.svg?branch=master)](https://travis-ci.org/Genocs/genocs-library)       |
| Github Actions           | [![.NET](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml/badge.svg)](https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml)     |
| Azure CI/CD              | work in progress |
| NuGet package version    | [![NuGet](https://img.shields.io/badge/nuget-v1.0.2-blue)](https://www.nuget.org/packages/Genocs.Core) |
| NuGet package download   | [![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core) |
| Discord community        | ![Discord](https://dcbadge.vercel.app/api/shield/461057072054927361?style=flat-square)  |


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
## Core

Core project contains general purpose functionalities to be used on DDD service

## Persistence MongoDb

Persistente  MongoDb is the library that allows to persist data on MongoDb

## ServiceBusAzure
The ServiceBusAzure allows to publish/send messages to Azure Service Bus

- [service-bus-quickstart-cli](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-cli)


## Support

api-workbench.rest

Use this file inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin 

