#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src

COPY ["api-gateway/Genocs.APIGateway", "Genocs.APIGateway/"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["NuGet.config", "NuGet.config"]
COPY ["dotnet.ruleset", "dotnet.ruleset"]
COPY ["stylecop.json", "stylecop.json"]

WORKDIR "/src/Genocs.APIGateway"

RUN dotnet build "Genocs.APIGateway.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "Genocs.APIGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.APIGateway.dll"]