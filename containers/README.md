# Genocs .NET library

In this section you can find the infrastructure components to setup the environment.

## Infrastructure

You will use ***Docker compose*** to setup the infrastructure components.

Remember to install [Docker Desktop](https://www.docker.com/products/docker-desktop) on your machine.

Setup the root folder this folder before move on.

```bash
# Setup the infrastructure
docker-compose -f ./infrastructure-bare.yml --env-file ./.env --project-name genocs up -d
docker-compose -f ./infrastructure-monitoring.yml --env-file ./.env --project-name genocs up -d
docker-compose -f ./infrastructure-scaling.yml --env-file ./.env --project-name genocs up -d
docker-compose -f ./infrastructure-security.yml --env-file ./.env --project-name genocs up -d

# Use this file only in case you want to setup sqlserver database (no need if you use postgres)
docker-compose -f ./infrastructure-sqlserver.yml --project-name genocs up -d

# Use this file only in case you want to setup elk stack
docker-compose -f ./infrastructure-elk.yml --project-name genocs up -d

# Use this file only in case you want to setup AI ML components
docker-compose -f ./infrastructure-ml.yml --project-name genocs up -d

# To stop the application
docker compose -f ./infrastructure-bare.yml --env-file ./.env --project-name genocs stop
```

`infrastructure-bare.yml` allows to install the basic infrastructure components. Basic components are the [RabbitMQ](https://rabbitmq.com), [Redis](https://redis.io), [Mongo](https://mongodb.com), [Postgres](https://www.postgresql.org/).

- [rabbitmq](http://localhost:15672/)
- Redis
- MongoDb
- PostgreSQL

`infrastructure-monitoring.yml` allows to install the monitoring infrastructure components.

Inside the file you can find:

- Prometheus
- Grafana
- influxdb
- jaeger
- seq

`infrastructure-scaling.yml` allows to install the scaling infrastructure components.

Inside the file you can find:

- Fabio
- consul

`infrastructure-security.yml` allows to install the security infrastructure components.

Inside the file you can find:

- vault (Hashicorp)

The script below allows to setup the infrastructure components. This means that you can find all the containers inside the same network.

The network is called `genocs`.

```yml
networks:
  genocs:
    name: genocs-network
    driver: bridge
```

Remember to add the network configuration inside your docker compose file to setup the network, before running the containers.

```yml
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

## Usefull info

Connect to the private Azure Docker registry

### How getting info about docker environment

```bash
docker image ls
docker container ls
docker volume ls
docker network ls
```

### How clear docker environment

**WARNING**: This command will clear everything within your docker environment.

```bash
docker stop $(docker ps -a -q)
docker rm $(docker ps -a -q)
docker rmi $(docker images -q)
docker rmi $(docker images -q) -f

docker container prune
docker image prune
docker volume prune
docker network prune

docker image ls
docker container ls
docker volume ls
docker network ls
```

## **Libraries**
You can find a more on:
[**Documentation**](https://genocs-blog.netlify.app/library/)
