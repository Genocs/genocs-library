#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


#FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
#FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
#FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

#FROM mcr.microsoft.com/dotnet/core/sdk:3.0  AS build-env
#FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build-env
# FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
#FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /src
COPY ["src/Genocs.Core", "src/Genocs.Core/"]
COPY ["src/Genocs.Common", "src/Genocs.Common/"]
COPY ["src/Genocs.Persistence.MongoDb", "src/Genocs.Persistence.MongoDb/"]
COPY ["src/Genocs.Core.Demo.Domain", "src/Genocs.Core.Demo.Domain/"]
COPY ["src/Genocs.Logging", "src/Genocs.Logging/"]
COPY ["src/Genocs.Core.Demo.WebApi", "src/Genocs.Core.Demo.WebApi/"]
COPY ["src/Genocs.Core.Demo.Contracts", "src/Genocs.Core.Demo.Contracts/"]
COPY ["src/Genocs.Monitoring", "src/Genocs.Monitoring/"]
COPY ["src/Genocs.ServiceBusAzure", "src/Genocs.ServiceBusAzure/"]

COPY ["LICENSE", "LICENSE"]
COPY ["icon.png", "icon.png"]

WORKDIR "/src/src/Genocs.Core.Demo.WebApi"

RUN dotnet restore "Genocs.Core.Demo.WebApi.csproj"

RUN dotnet build "Genocs.Core.Demo.WebApi.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "Genocs.Core.Demo.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Core.Demo.WebApi.dll"]