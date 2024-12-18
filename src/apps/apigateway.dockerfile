#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Build.targets", "."]
COPY ["NuGet.config", "."]
COPY ["dotnet.ruleset", "."]
COPY ["stylecop.json", "."]

COPY ["api-gateway/Genocs.APIGateway", "Genocs.APIGateway/"]

WORKDIR "/src/Genocs.APIGateway"

RUN dotnet build "Genocs.APIGateway.csproj" -c Release -o /app/build
FROM build-env AS publish
RUN dotnet publish "Genocs.APIGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.APIGateway.dll"]