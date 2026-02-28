# See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the official .NET runtime as base image (Worker services don't need ASP.NET)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER app
WORKDIR /app

# Use the official .NET SDK for building
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy global build configuration files first for better layer caching
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["stylecop.json", "stylecop.json"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["global.json", "global.json"]
# Copy additional files needed for build
COPY ["LICENSE", "LICENSE"]
COPY ["icon.png", "icon.png"]

# Copy project files and restore dependencies
COPY ["src/demo/Contracts/Contracts.csproj", "src/Contracts/"]
COPY ["src/demo/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/demo/Worker/Host.csproj", "src/Worker/"]

# Restore dependencies in a separate layer for better caching
RUN dotnet restore "src/Worker/Host.csproj"

# Copy source code
COPY ["src/demo/Contracts/", "src/Contracts/"]
COPY ["src/demo/Domain/", "src/Domain/"]
COPY ["src/demo/Worker/", "src/Worker/"]

# Build the application
WORKDIR "/src/src/Worker"
RUN dotnet build "Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Create final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Library.Demo.Worker.dll"]