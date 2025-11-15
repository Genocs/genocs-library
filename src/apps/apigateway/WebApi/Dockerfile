# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the official .NET runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy global build configuration files first for better layer caching
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["stylecop.json", "stylecop.json"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["global.json", "global.json"]

# Copy project files and restore dependencies
COPY ["src/apps/apigateway/WebApi/Host.csproj", "src/WebApi/"]
# Restore dependencies in a separate layer for better caching
RUN dotnet restore "src/WebApi/Host.csproj"

# Copy source code
COPY ["src/apps/apigateway/", "src/WebApi/"]

# Build the application
WORKDIR "/src/src/WebApi"
RUN dotnet build "Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.APIGateway.WebApi.dll"]