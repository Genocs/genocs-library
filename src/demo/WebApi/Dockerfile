#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /src
COPY ["src/demo/Contracts", "src/Contracts/"]
COPY ["src/demo/Domain", "src/Domain/"]
COPY ["src/demo/WebApi", "src/WebApi/"]

COPY ["LICENSE", "LICENSE"]
COPY ["icon.png", "icon.png"]

WORKDIR "/src/src/WebApi"

RUN dotnet restore "Host.csproj"

RUN dotnet build "Host.csproj" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish "Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Library.Demo.WebApi.dll"]