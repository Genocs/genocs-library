# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the official .NET ASP.NET runtime as base image (includes ASP.NET Core framework)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

# Copy global build configuration files first for better layer caching
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["stylecop.json", "stylecop.json"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["global.json", "global.json"]

# COPY .csproj and restore as distinct layers
# This is CRITICAL: If code changes but .csproj doesn't, Docker reuses this cached layer
COPY ["src/demo/microservice/Contracts/Contracts.csproj", "src/Contracts/"]
COPY ["src/demo/microservice/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/demo/microservice/Masstransit.Worker/Host.csproj", "src/Masstransit.Worker/"]

# Restore dependencies in a separate layer for better caching
RUN dotnet restore "src/Masstransit.Worker/Host.csproj"

# Copy the rest of the source code
COPY ["src/demo/microservice/Contracts/", "src/Contracts/"]
COPY ["src/demo/microservice/Domain/", "src/Domain/"]
COPY ["src/demo/microservice/Masstransit.Worker/", "src/Masstransit.Worker/"]

WORKDIR "/src/src/Masstransit.Worker"
RUN dotnet build "Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Library.Demo.Masstransit.Worker.dll"]