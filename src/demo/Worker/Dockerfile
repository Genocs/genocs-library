#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src
COPY ["src/Demo/Worker", "src/Worker/"]
COPY ["src/Demo/Contracts", "src/Contracts/"]
COPY ["src/Demo/Domain", "src/Domain/"]
COPY ["src/Genocs.Persistence.MongoDb", "src/Genocs.Persistence.MongoDb/"]
COPY ["src/Genocs.Monitoring", "src/Genocs.Monitoring/"]
COPY ["src/Genocs.Logging", "src/Genocs.Logging/"]
COPY ["src/Genocs.ServiceBusAzure", "src/Genocs.ServiceBusAzure/"]
COPY ["src/Genocs.Core", "src/Genocs.Core/"]
COPY ["src/Genocs.Common", "src/Genocs.Common/"]

COPY ["LICENSE", "LICENSE"]
COPY ["icon.png", "icon.png"]

WORKDIR "/src/src/Worker"

RUN dotnet restore "Host.csproj"

RUN dotnet build "Host.csproj" -c Release -o /app/build
FROM build-env AS publish
RUN dotnet publish "Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Core.Demo.Worker.dll"]