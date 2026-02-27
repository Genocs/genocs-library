<!-- PROJECT SHIELDS -->

[![License][license-shield]][license-url]
[![Build][build-shield]][build-url]
[![CodeQL][codeql-shield]][codeql-url]
[![Downloads][downloads-shield]][downloads-url]
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Discord][discord-shield]][discord-url]
[![Gitter][gitter-shield]][gitter-url]
[![Twitter][twitter-shield]][twitter-url]
[![Twitterx][twitterx-shield]][twitterx-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

[license-shield]: https://img.shields.io/github/license/Genocs/genocs-library?color=2da44e&style=flat-square
[license-url]: https://github.com/Genocs/genocs-library/blob/main/LICENSE
[build-shield]: https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml/badge.svg?branch=main
[build-url]: https://github.com/Genocs/genocs-library/actions/workflows/build_and_test.yml
[downloads-shield]: https://img.shields.io/nuget/dt/Genocs.Microservice.Template.svg?color=2da44e&label=downloads&logo=nuget
[downloads-url]: https://www.nuget.org/packages/Genocs.Microservice.Template
[contributors-shield]: https://img.shields.io/github/contributors/Genocs/genocs-library.svg?style=flat-square
[contributors-url]: https://github.com/Genocs/genocs-library/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Genocs/genocs-library?style=flat-square
[forks-url]: https://github.com/Genocs/genocs-library/network/members
[stars-shield]: https://img.shields.io/github/stars/Genocs/genocs-library.svg?style=flat-square
[stars-url]: https://img.shields.io/github/stars/Genocs/genocs-library?style=flat-square
[issues-shield]: https://img.shields.io/github/issues/Genocs/genocs-library?style=flat-square
[issues-url]: https://github.com/Genocs/genocs-library/issues
[discord-shield]: https://img.shields.io/discord/1106846706512953385?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat-square
[discord-url]: https://discord.com/invite/fWwArnkV
[gitter-shield]: https://img.shields.io/badge/chat-on%20gitter-blue.svg
[gitter-url]: https://gitter.im/genocs/
[twitter-shield]: https://img.shields.io/twitter/follow/genocs?color=1DA1F2&label=Twitter&logo=Twitter&style=flat-square
[twitter-url]: https://twitter.com/genocs
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=flat-square&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/giovanni-emanuele-nocco-b31a5169/
[twitterx-shield]: https://img.shields.io/twitter/url/https/twitter.com/genocs.svg?style=social
[twitterx-url]: https://twitter.com/genocs
[codeql-shield]: https://github.com/Genocs/genocs-library/actions/workflows/codeql-analysis.yml/badge.svg
[codeql-url]: https://github.com/Genocs/genocs-library/actions/workflows/codeql-analysis.yml

<!-- PROJECT LOGO -->
<p align="center">
  <a href="https://github.com/genocs/genocs-library">
    <img src="https://raw.githubusercontent.com/genocs/genocs-library/main/assets/genocs-library-banner.png" alt=".NET Microservice Template">
  </a>
  <h3 align="center">.NET CLI</h3>
  <p align="center">
    Open Source Library For .NET10 Microservice
    <br />
    <a href="https://learn.fiscanner.net"><strong>Read the Documentation</strong></a>
    <br />
    <br />
    <a href="https://github.com/genocs/genocs-library/issues">Report Bug</a>
    ·
    <a href="https://github.com/genocs/genocs-library/issues">Request Feature</a>
    .
    <a href="https://github.com/genocs/genocs-library/issues">Request Documentation</a>
  </p>
</p>

# Genocs .NET library

This repo contains a set of libraries to build LOB _Line Of Business_ applications. The library is open source and built to be PRODUCTION READY. The library is built on top of .NET10, ant it is designed and maintained by Genocs.

Packages are available on [NuGet Genocs](https://www.nuget.org/profiles/gioema_nocco).

## The idea

**_Build a software library to be cloud agnostic_**

Building a software library to be cloud agnostic has several advantages. First, it allows developers to create applications that can be deployed on any cloud platform without having to rewrite code or make major changes. This makes it easier for developers to quickly deploy their applications across multiple cloud providers. Additionally, it reduces the cost of development and maintenance since developers don’t have to write separate code for each cloud provider. Finally, it increases the scalability of applications since they can be easily deployed on different cloud platforms with minimal effort.

**_Cloud agnostic by use of Containers_**

The advantages of using containers are numerous. Containers provide a lightweight, portable, and isolated environment for applications to run in, allowing them to be easily moved between different systems. This makes it easier to deploy applications quickly and reliably across different environments. Additionally, containers can help reduce resource consumption by running multiple applications on the same host, as each container is isolated from the others. This helps to improve efficiency and scalability. Finally, containers provide an additional layer of security, as they are isolated from the underlying operating system and other applications.

## Documentation

You can find a useful documentation about how to use the library. The documentation contains the complete set of libraries, template, CLI that altogether make the _genocs ecosystem_ a comprensive set of tools to build enterprise solutions.

Documentation available at [Genocs Blog](https://learn.fiscanner.net/library/)

## Table of Contents

- [Quick Start](#quick-start)
- [Features](#features)
- [Folders structure](#folders-structure)
- [Infrastructure](#infrastructure)
- [Kubernetes cluster](#kubernetes-cluster)
- [Aspire Integration](#aspire-integration)
- [Support](#support)
- [Configuration](#configuration)
- [Demo Application](#demo-application)
- [Enterprise Application](#enterprise-application)
- [Development Tools](#development-tools)
- [Cloud Deployment](#cloud-deployment)
- [License](#license)
- [Changelog](#changelog)
- [Community](#community)
- [Support](#support-1)
- [Code Contributors](#code-contributors)
- [Financial Contributors](#financial-contributors)
- [Acknowledgements](#acknowledgements)

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)
- [Visual Studio Code](https://code.visualstudio.com/) with the following extensions:
  - [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
  - [Docker](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker)
  - [YAML](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-yaml)
  - [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)

### Setup


```bash
# 1. Clone the repository
git clone https://github.com/Genocs/genocs-library.git
cd genocs-library

# 2. Build the solution
dotnet build

#3. Pack the projects
dotnet pack

# 4. To pack the project with nuspec file
cd src/Genocs.Core
dotnet pack -p:NuspecFile=./Genocs.Core.nuspec --no-restore -o .

# 5. To push on nuget
dotnet nuget push
dotnet nuget push *.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
```


## Features

- **Cloud Agnostic:** Build applications that can run on any cloud platform without code changes.
- **Containerized:** Lightweight and portable applications using container technology.
- **Microservices Ready:** Easily create and manage microservices architectures.
- **Enterprise Integration:** Built-in support for enterprise services like databases, messaging, and caching.
- **Monitoring and Logging:** Integrated solutions for application monitoring and logging.
- **Security:** Robust security features including API management and access control.
- **Scalability:** Effortlessly scale applications horizontally or vertically.
- **Flexibility:** Supports multiple programming models and architectures.
- **Developer Friendly:** Easy to use and well-documented libraries and tools.
- **Open Source:** Fully open source with a permissive license.

## Folders structure

The repository is structured as follows:

```plaintext
root-project/
├── .azure/
│   ├── pipelines/
│   │   ├── ci-build.yml
│   │   ├── ci-release.yml
│   │   └── ...
├── .cursor/
│   ├── rules/
│   │   ├── solution_architect.mdc
│   │   └── ...
├── .devcontainer/
│   └── ...
├── .git/
│   └── ...
├── .github/
│   ├── agents/
│   │   ├── Code Reviewer.agent.md
│   │   ├── Documentation Writer.agent.md
│   │   └── ...
│   ├── copilot-instructions.md
│   ├── workflows/
│   │   ├── ...
├── .vscode/
│   └── mcp.json
├── devops/
│   ├── azure
│   │   ├── ci-publish_on_acr.yml
│   │   ├── ci-publish_on_nuget.yml
│   │   └── ...
├── docs/
│   └── ...
├── infrastructure/
│   ├── bicep/
│   │   └── ...
│   ├── docker/
│   │   └── ...
│   ├── helm/
│   │   └── ...
│   ├── k8s/
│   │   └── ...
│   ├── terraform/
│   │   └── ...
│   ├── ...
├── scripts/
│   └── ...
├── src/
│   └── ...
├── .dockerignore
├── .editconfig
├── .env
├── .gitattributes
├── .gitignore
├── Directory.Build.props
├── Directory.Build.targets
├── dotnet.ruleset
├── global.json
├── icon.png
├── stylecop.json
├── nuget.config
├── [project].slnx
├── README.md
├── LICENSE
├── CHANGELOG.md
├── CONTRIBUTING.md
├── api-workbench.rest
└── ...
```

## Infrastructure

In this section you can find the infrastructure components you need to execute the solution. Infrastructure components are the database, the enterprise service bus, the distributed logging, monitoring, tracing systems along with database and many more.
You can use **Docker compose** to setup the infrastructure components just by running few commands.

```bash
cd ./infrastructure/docker

# Setup the infrastructure.
# Use this file to setup the basic infrastructure components (RabbitMQ, MongoDB)
docker compose -f ./infrastructure.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup Redis and PostgreSQL (no need if you use MongoDB)
docker compose -f ./infrastructure-db.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup monitoring infrastructure components (Prometheus, Grafana, InfluxDB, Jaeger, Seq)
docker compose -f ./infrastructure-monitoring.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup scaling infrastructure components (Fabio, Consul)
docker compose -f ./infrastructure-scaling.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup security infrastructure components (Vault)
docker compose -f ./infrastructure-security.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup sqlserver database (no need if you use PostgreSQL)
docker compose -f ./infrastructure-sqlserver.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup mySql database (no need if you use PostgreSQL)
docker compose -f ./infrastructure-mysql.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup oracle database (no need if you use PostgreSQL)
docker compose -f ./infrastructure-oracle.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup elk stack
docker compose -f ./infrastructure-elk.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup AI ML components prepared by Genocs
docker compose -f ./infrastructure-ml.yml --env-file ./.env --project-name genocs up -d
```

> **NOTE**:
>
> The folder contains a .env.example file with the environment variables used by the docker compose files, remember to update the values based on your needs before running the commands.
>
> Pay attention to do not push the .env file to a public repository with sensitive data, consider to use secret management tools to manage the secrets in a secure way. 

`infrastructure.yml` allows to install the basic infrastructure components. They are:

- [RabbitMQ](https://rabbitmq.com)
- [MongoDB](https://mongodb.com)

`infrastructure-db.yml` allows to install Redis and PostgreSQL

- [Redis](https://redis.io)
- [Postgres](https://www.postgresql.org/)

You can check them locally:

- [RabbitMQ](http://localhost:15672): `localhost:15672`
- Redis: `TCP:localhost:6379`
- MongoDB: `TCP:localhost:27017`
- Postgres: `TCP:localhost:5432`

`infrastructure-monitoring.yml` allows to install the monitoring infrastructure components. They are:

- [Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Prometheus](https://prometheus.io/)
- [Grafana](https://grafana.com/)
- [InfluxDB](https://www.influxdata.com/)
- [Jaeger](https://www.jaegertracing.io/)
- [Seq](https://datalust.co/seq)

You can find the console locally at:

- [Aspire](localhost:18888): `localhost:18888`
- [Prometheus](localhost:9090): `localhost:9090`
- [Grafana](localhost:3000): `localhost:3000`
- [InfluxDB](localhost:8086): `localhost:8086`
- [Jaeger](localhost:16686): `localhost:16686`
- [Seq](localhost:5341): `localhost:5341`

`infrastructure-scaling.yml` allows to install the scaling infrastructure components composed by a Fabio (Loadbalancer) Service Discovery (Consul) components. They are:

- [Fabio](https://fabiolb.net/)
- [Consul](https://www.consul.io/)

`infrastructure-security.yml` allows to install the security infrastructure components.

Inside the file you can find:

- vault (Hashicorp)

> **NOTE**
>
> The commands above allows to setup infrastructure components, this means you can find all the containers inside the same network `genocs`.
>
> Whenever possible the data are persisted on the host machine by means of volumens, so you can restart the containers without losing data.

```yml
networks:
  genocs:
    name: genocs-network
    driver: bridge

volumes:
  rabbitmq-data:
  mongo-data:
  redis-data:
  postgres-data:
  influx-data:
  grafana-data:
  jaeger-data:
  seq-data:
  vault-data:
  elk-data:
  fabio-data:
  consul-data:
  prometheus-data:
  ml-data:
```

Remember to add the network configuration inside your docker compose file to setup the network, before running the containers.

## **_Kubernetes cluster_**

You can setup the application inside a Kubernetes cluster.

Check the repo [enterprise-containers](https://github.com/Genocs/enterprise-containers) to setup a Kubernetes cluster.
There you can find scripts, configuration files and documentation to setup a cluster from scratch.

## **_Aspire Integration_**

SOON :rocket:

## Support

Use [**api-workbench**](./api-workbench.rest) inside Visual Studio code with [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) plugin

## Configuration

```json
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
    "address": "docker.for.mac.localhost",
    "port": "5070",
    "pingEnabled": true,
    "pingEndpoint": "healthz",
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
    "excludePaths": [
      "/",
      "/healthz",
      "/alive",
      "/metrics"
    ],
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
      "enableTracing": false,
      "enableMetrics": false,
      "enableLogging": false,
      "connectionString": "InstrumentationKey=<<your_istrumentation_key>>;IngestionEndpoint=https://<<your_region>>.in.applicationinsights.azure.com/;LiveEndpoint=https://<<your_region>>.livediagnostics.monitor.azure.com/;ApplicationId=<<your_application_id>>"
    },
    "tags": {}
  },
  "jwt": {
    "certificate": {
      "location": "certs/localhost.pfx",
      "password": "test",
      "rawData": ""
    },
    "issuer": "genocs-identities-service",
    "validIssuer": "genocs-identities-service",
    "validateAudience": false,
    "validateIssuer": true,
    "validateLifetime": true,
    "expiry": "01:00:00"
  },
  "prometheus": {
    "enabled": false,
    "endpoint": "/metrics"
  },
  "mongodb": {
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
  "rabbitmq": {
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
  "openapi": {
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
  "azureKeyVault": {
    "enabled": false,
    "name": "gnx-keyvault",
    "managedIdentityId": "secret",
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

## **_Demo Application_**

Inside the repo there is a simple demo application you can use to test the library. The demo application is composed by a WebApi and a Worker service, they are both using the library to show you how to use it in a real application.

### How to BUILD & RUN the application
Following are the commands to build and run the demo application.

```bash
# Build the solution
dotnet build

# Pack the projects
dotnet pack

# Run project with console
dotnet run --project ./src/demo/WebApi
dotnet run --project ./src/demo/Worker
dotnet run --project ./src/demo/HelloWorld.WebApi
```

### Build and push the Docker images to Dockerhub

```bash
# Build webapi Docker image
docker build -t genocs/demo-webapi:2.0.0 -t genocs/demo-webapi:latest -f ./src/demo/WebApi/Dockerfile .

# Push webapi Docker image to Dockerhub
docker push genocs/demo-webapi:2.0.0
docker push genocs/demo-webapi:latest

# Build WORKER Docker image
docker build -t genocs/demo-worker:2.0.0 -t genocs/demo-worker:latest -f ./src/demo/Worker/Dockerfile .

# Push WORKER Docker image to Dockerhub
docker push genocs/demo-worker:2.0.0
docker push genocs/demo-worker:latest
```

---

## **_Enterprise Application_**

### Application Components

Inside **./src/apps** folder you can find a full-fledged application composed by:

- ApiGateway
- Identity Service
- Order Service
- Product Service
- Notification Service

In that way you can test the entire flow.

| Component            | Description                     | Container Port | Visibility                  |
| -------------------- | ------------------------------- | -------------- | --------------------------- |
| ApiGateway           | Handles API requests            | :5500          | Public                      |
| Identity Service     | Manages user identities         | :5510\*        | Private through API Gateway |
| Product Service      | Manages product information     | :5520\*        | Private through API Gateway |
| Order Service        | Processes orders                | :5530\*        | Private through API Gateway |
| Notification Service | Handles real-time communication | :5540\*        | Private through API Gateway |

![Architecture](./assets/architecture_01.png)

### How to BUILD & RUN the application

The build and run process can be done by using docker-compose.

Pre-requisites:
- Docker

```bash

# Build with docker compose
./src/apps/scripts/build-images-docker-compose.sh

# *** Before running the solution remember to check ***
# *** if the infrastructure services were setup     ***

# Run with docker compose
docker compose -f ./src/apps/docker-compose.yml --env-file ./.env --project-name genocs up -d

# Clean Docker cache (optional)
docker builder prune
```

Some useful commands to manage the application:

```bash
# Build docker image one by one
./src/apps/scripts/build-images.sh

# Build the images with docker compose and push to Dockerhub
./src/apps/scripts/build-and-push-images.sh
```

## **_Kubernetes_**

### KubeCtl

You can deploy the application on Kubernetes cluster by using kubectl.

```bash
# Build images
./src/apps/scripts/deploy-k8s.sh
```

### Helm chart

You can deploy the application on Kubernetes cluster by using Helm chart.

Inside the folder **./src/apps/k8s/helm** you can find the Helm chart to set a K8s cluster and deploy the application.

The command below allows to install the Helm chart by using MicroK8s.

```bash
cd ./src/apps/k8s/helm
microk8s helm install genocs ./gnxchart
```

## **_makefile_**

Prerequisites:

- make

To install make on MacOS you can use brew

```bash
brew install make
```

To install make on Ubuntu you can use apt

```bash
sudo apt install make
```

To install make on Windows you can use [choco](https://chocolatey.org/)

```bash
choco install make
```

Upon installation you can use the makefile to build, run, deploy the application.

```bash
# Build the solution
make build
```

## **_Test Suite_**

The test suite is composed by unit tests, integration tests and end to end tests.

Test are important to ensure the quality of the code and to avoid regressions, they are also useful to document the code and to show how to use the library in a real application.

The repo use following testing libraries:
- [xUnit](https://xunit.net/)
- [Moq](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [Shouldly](https://github.com/shouldly/shouldly)
- [NSubstitute](https://nsubstitute.github.io/)
- [Testcontainers](https://github.com/testcontainers/testcontainers-dotnet)
- [Reqnroll](https://docs.reqnroll.net/)

Why use multiple libraries for testing? Each library has its own strengths and weaknesses, by using multiple libraries we can leverage the strengths of each library and have a more comprehensive test suite. Take a look at the documentation to understand which library is best for your use case.

| Library | Tag | Description |
| --- | --- | --- |
| **xUnit** | Unit Testing | A free, open-source, community-focused unit testing tool for the .NET Framework. |
| **Moq** | Mocking | A popular mocking library for .NET that allows you to create mock objects for unit testing. |
| **FluentAssertions** | Assertions | A library that provides a fluent syntax for writing assertions in unit tests. |
| **Shouldly** | Assertions | A library that provides a more readable syntax for writing assertions in unit tests. |
| **NSubstitute** | Mocking | A friendly substitute for .NET mocking libraries that allows you to create mock objects for unit testing. |
| **Testcontainers** | Integration Testing | A library that provides a way to use Docker containers for integration testing in .NET. |
| **Reqnroll** | End-to-End Testing | A library that provides a way to write end-to-end tests for REST APIs in .NET. |



Under evalutation to be added:
- [Respawn](https://github.com/jbogard/Respawn)

| Library | Tag | Description |
| --- | --- | --- |
| **Respawn** | Integration Testing | A library that provides a way to reset the state of a database for integration testing in .NET. |


To run the tests you can use the command below:

```bash
# Run all the tests
make test
# Run unit tests
make test-unit
# Run integration tests
make test-integration
# Run end to end tests
make test-e2e
```

> NOTE:
> If you need to make available internal methods to the test projects, you can use the attribute `InternalsVisibleTo` inside the test project you want to use and specify the name of the project you want to test as parameter.

Add the following line inside the project you want to test:
```csharp
[assembly: InternalsVisibleTo("Genocs.UsersService.Tests.Unit")]
```
or you can update the csproj file by adding the following property:

Add the following lines inside the csproj file of the project you want to test:
```xml
  <!-- InternalsVisibleTo for unit tests -->
  <ItemGroup>
      <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
          <_Parameter1>Genocs.Saga.UnitTests</_Parameter1>
      </AssemblyAttribute>
  </ItemGroup>
```



## **_MCP Servers_**

This repository contains support for MCP servers to be used along with VS Code.
You can find the MCP servers inside the folder `./.vscode/mcp.json`.

This approach allows to have a set of configured servers to be used along with VS Code and to share them with the team.

As you can immagine the drawback of this approach is that in case to be used across multiple repositories you need to copy the file in each repository. Alternative approach copy the file in a common location and configure VS Code to use that file.

For windows you can create a file named `mcp.json` inside the folder `%USERPROFILE%/.vscode/` and add the content of the file.

> NOTE:
>
> PLEASE DOUBLE CHECK THE PATH ON MACOS AND LINUX AS IT MAY CHANGE BASED ON THE INSTALLATION TYPE
>
> For MacOS and Linux you can create a file named `mcp.json` inside the folder `~/.config/Code/User/` and add the content of the file.

### Deploy in a cloud instance

You can deploy Demo Application with one click in Heroku, Microsoft Azure, or Google Cloud Platform:

[<img src="https://www.herokucdn.com/deploy/button.svg" height="30px">](https://heroku.com/deploy?template=https://github.com/heartexlabs/label-studio/tree/heroku-persistent-pg)
[<img src="https://aka.ms/deploytoazurebutton" height="30px">](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fheartexlabs%2Flabel-studio%2Fmaster%2Fazuredeploy.json)
[<img src="https://deploy.cloud.run/button.svg" height="30px">](https://deploy.cloud.run)

## License

This project is licensed with the [MIT license](LICENSE).

## Changelog

View Complete [Changelog](https://github.com/Genocs/microservice-template/blob/main/CHANGELOG.md).

## Community

- Discord [@genocs](https://discord.com/invite/fWwArnkV)
- Facebook Page [@genocs](https://facebook.com/Genocs)
- Youtube Channel [@genocs](https://youtube.com/c/genocs)

## Support

Has this Project helped you learn something New? or Helped you at work?
Here are a few ways by which you can support.

- ⭐ Leave a star!
- 🥇 Recommend this project to your colleagues.
- 🦸 Do consider endorsing me on LinkedIn for ASP.NET Core - [Connect via LinkedIn](https://www.linkedin.com/in/giovanni-emanuele-nocco-b31a5169/)
- ☕ If you want to support this project in the long run, [consider buying me a coffee](https://www.buymeacoffee.com/genocs)!

[![buy-me-a-coffee](https://raw.githubusercontent.com/Genocs/genocs-library/main/assets/buy-me-a-coffee.png "buy-me-a-coffee")](https://www.buymeacoffee.com/genocs)

## Code Contributors

This project exists thanks to all the people who contribute. [Submit your PR and join the team!](CONTRIBUTING.md)

[![genocs contributors](https://contrib.rocks/image?repo=Genocs/genocs-library "genocs contributors")](https://github.com/genocs/genocs-library/graphs/contributors)

## Financial Contributors

Become a financial contributor and help me sustain the project. [Support the Project!](https://opencollective.com/genocs/contribute)

<a href="https://opencollective.com/genocs"><img src="https://opencollective.com/genocs/individuals.svg?width=890"></a>

## Acknowledgements
