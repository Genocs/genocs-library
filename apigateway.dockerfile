#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


#FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
#FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
#FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

#FROM mcr.microsoft.com/dotnet/core/sdk:3.0  AS build-env
#FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build-env
# FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /src

COPY ["src/Genocs.Persistence.MongoDb", "src/Genocs.Persistence.MongoDb/"]
COPY ["src/apps/api-gateway/Genocs.APIGateway", "src/Genocs.APIGateway/"]

WORKDIR "/src/src/Genocs.APIGateway"

#RUN dotnet restore "Genocs.APIGateway.csproj"

RUN dotnet build "Genocs.APIGateway.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "Genocs.APIGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.APIGateway.dll"]