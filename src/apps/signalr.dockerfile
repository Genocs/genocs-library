#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.


# FROM mcr.microsoft.com/dotnet/core/aspnet:3.0
# FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
# FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
# FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine
# FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
# FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app
EXPOSE 80
EXPOSE 443

# FROM mcr.microsoft.com/dotnet/core/sdk:3.0  AS build-env
# FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build-env
# FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
# FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build-env
# FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
# FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env

WORKDIR /src
COPY ["signalr/Genocs.SignalR.WebApi", "Genocs.SignalR.WebApi/"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["NuGet.config", "NuGet.config"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["stylecop.json", "stylecop.json"]

WORKDIR "/src/Genocs.SignalR.WebApi"

FROM build-env AS publish
RUN dotnet build "Genocs.SignalR.WebApi.csproj" -c Release -o /app/build
# RUN dotnet publish "Genocs.SignalR.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/build .
ENTRYPOINT ["dotnet", "Genocs.SignalR.WebApi.dll"]
