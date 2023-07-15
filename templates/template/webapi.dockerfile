#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/Genocs.Template.WebApi/Genocs.Template.WebApi.csproj", "Genocs.Template.WebApi/"]
COPY ["src/Genocs.Template.Application/Genocs.Template.Application.csproj", "Genocs.Template.Application/"]
RUN dotnet restore "src/Genocs.Template.WebApi/Genocs.Template.WebApi.csproj"

COPY . .
WORKDIR "/src/Genocs.Template.WebApi"
RUN dotnet build "Genocs.Template.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Genocs.Template.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Genocs.Template.WebApi.dll"]