<!-- PROJECT SHIELDS -->
[![License][license-shield]][license-url]
[![Build][build-shield]][build-url]
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


<p align="center">
    <img src="./assets/genocs-library-logo.png" alt="icon">
</p>


# Genocs .NET library

This repo contains a set of libraries to build LOB (Line Of Business) applications. The library is open source and built to be PRODUCTION READY. The library is built on top of .NET8, it is designed and maintained by Genocs. 

Packages are available on [NuGet Genocs](https://www.nuget.org/profiles/gioema_nocco).

## The idea

***Build a software library to be cloud agnostic***

Building a software library to be cloud agnostic has several advantages. First, it allows developers to create applications that can be deployed on any cloud platform without having to rewrite code or make major changes. This makes it easier for developers to quickly deploy their applications across multiple cloud providers. Additionally, it reduces the cost of development and maintenance since developers don’t have to write separate code for each cloud provider. Finally, it increases the scalability of applications since they can be easily deployed on different cloud platforms with minimal effort. 

***Cloud agnostic by use of Containers***

The advantages of using containers are numerous. Containers provide a lightweight, portable, and isolated environment for applications to run in, allowing them to be easily moved between different systems. This makes it easier to deploy applications quickly and reliably across different environments. Additionally, containers can help reduce resource consumption by running multiple applications on the same host, as each container is isolated from the others. This helps to improve efficiency and scalability. Finally, containers provide an additional layer of security, as they are isolated from the underlying operating system and other applications.

## Documentation

You can find a useful documentation about how to use the library. The documentation contains the complete set of libraries, template, CLI that altogether make the *genocs ecosystem* a comprensive set of tools to build enterprise solutions.

Documentation available at [Genocs Blog](https://genocs-blog.netlify.app/library/)

## Infrastructure

In this section you can find the infrastructure components you need to execute the solution. Infrastucture components are the database, the enterprice servise bus, the distributed logging, monitoring, tracing systems along with database and many more.
You can use **Docker compose** to setup the infrastructure components just by running few commands.


``` bash
cd ./containers
# Setup the infrastructure
docker compose -f ./infrastructure-bare.yml --env-file ./.env --project-name genocs up -d
docker compose -f ./infrastructure-monitoring.yml --env-file ./.env --project-name genocs up -d
docker compose -f ./infrastructure-scaling.yml --env-file ./.env --project-name genocs up -d
docker compose -f ./infrastructure-security.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup sqlserver database (no need if you use postgres)
docker compose -f ./infrastructure-sqlserver.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup elk stack
docker compose -f ./infrastructure-elk.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup AI ML components prepared by Genocs
docker compose -f ./infrastructure-ml.yml --env-file ./.env --project-name genocs up -d
```

`infrastructure-bare.yml` allows to install the basic infrastructure components. They are:
- [RabbitMQ](https://rabbitmq.com)
- [Redis](https://redis.io)
- [MongoDB](https://mongodb.com)
- [Postgres](https://www.postgresql.org/).

You can run them locally:

- [RabbitMQ](http://localhost:15672): `localhost:15672`
- Redis: `localhost:6379`
- MongoDB: `localhost:27017`
- Postgres: `localhost:5432`


`infrastructure-monitoring.yml` allows to install the monitoring infrastructure components. They are:
- [Prometheus](https://prometheus.io/)
- [Grafana](https://grafana.com/)
- [InfluxDB](https://www.influxdata.com/)
- [Jaeger](https://www.jaegertracing.io/)
- [Seq](https://datalust.co/seq)


You can run them locally:

- [Prometheus](localhost:9090): `localhost:9090`
- [Grafana](localhost:3000): `localhost:3000`
- [InfluxDB](localhost:8086): `localhost:8086`
- [Jaeger](localhost:16686): `localhost:16686`
- [Seq](localhost:5341): `localhost:5341`


`infrastructure-scaling.yml` allows to install the scaling infrastructure components. They are:
- Fabio
- Consul

`infrastructure-security.yml` allows to install the security infrastructure components.

Inside the file you can find:

- vault (Hashicorp)

The script below allows to setup the infrastructure components. This means that you can find all the containers inside the same network.

The network is called `genocs`.

``` yml 
networks:
  genocs:
    name: genocs-network
    driver: bridge
```

Remember to add the network configuration inside your docker compose file to setup the network, before running the containers.


## ***Kubernetes cluster***

You can setup the application inside a Kubernetes cluster.

Check the repo [enterprise-containers](https://github.com/Genocs/enterprise-containers) to setup a Kubernetes cluster. 
There you can find scripts, configuration files and documentation to setup a cluster from scratch.

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
    "issuer": "genocs-identity-service",
    "validIssuer": "genocs-identity-service",
    "validateAudience": false,
    "validateIssuer": true,
    "validateLifetime": true,
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

## Demo Application
Inside the library there is a simple demo application you can use to test the library. 

Following are the commands to build and run the demo application.

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

Inside **./src/apps** folder you can find a full-fledged application composed by:
- ApiGateway
- Identity Service
- Order Service
- Product Service
- SignalR Service

In that way you can test the entire flow.

**TODO**: Add a architecture diagram to show the components and how they interact with each other.

### How to BUILD & RUN the application

The build and run process can be done by using docker-compose

``` bash
cd src/apps

# Build with docker compose
docker compose -f ./docker-compose.yml -f ./docker-compose.override.yml --env-file ./local.env --project-name genocs-app build

# *** Before running the solution remember to check ***
# *** if the infrastructure services were setup     ***

# Run with docker compose
docker compose -f ./docker-compose.yml --env-file ./local.env --project-name genocs-app up -d

# Clean Docker cache
docker builder prune
```


Following commands are useful to build and push the images one by one

``` bash
cd src/apps

# Build the api gateway
docker build -t genocs/apigateway:1.0.0 -t genocs/apigateway:latest -f ./apigateway.dockerfile .

# Build the identity service
docker build -t genocs/identities:1.0.0 -t genocs/identities:latest -f ./identity-webapi.dockerfile .

# Build the order service
docker build -t genocs/orders:1.0.0 -t genocs/orders:latest -f ./containers/order-webapi.dockerfile .

# Build the product service
docker build -t genocs/products:1.0.0 -t genocs/products:latest -f ./product-webapi.dockerfile .

# Build the signalr service
docker build -t genocs/signalr:1.0.0 -t genocs/signalr:latest -f ./signalr-webapi.dockerfile .


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
  

[![buy-me-a-coffee](https://raw.githubusercontent.com/Genocs/blazor-template/main/assets/buy-me-a-coffee.png "buy-me-a-coffee")](https://www.buymeacoffee.com/genocs)

## Code Contributors

This project exists thanks to all the people who contribute. [Submit your PR and join the team!](CONTRIBUTING.md)

[![genocs contributors](https://contrib.rocks/image?repo=Genocs/blazor-template "genocs contributors")](https://github.com/genocs/blazor-template/graphs/contributors)

## Financial Contributors

Become a financial contributor and help me sustain the project. [Support the Project!](https://opencollective.com/genocs/contribute)

<a href="https://opencollective.com/genocs"><img src="https://opencollective.com/genocs/individuals.svg?width=890"></a>


## Acknowledgements
- [devmentors](https://github.com/devmentors)
- [abp](https://github.com/abpframework)


- simple changes