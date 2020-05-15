# .NET Core libraries by Genocs [![Build Status](https://travis-ci.org/Genocs/genocs-library.svg?branch=master)](https://travis-ci.org/Genocs/genocs-library) [![NuGet](https://img.shields.io/badge/nuget-v1.0.1-blue)](https://www.nuget.org/packages/Genocs.Core)


This repo contains a set of useful libraries designed by Genocs. The libraries are built using .NET standard 2.1 to be used both on project based on .NET Framework or the Core. The package version is hosted on [nuget](https://www.nuget.org/packages).


To build the project type following command
```ps
dotnet build .\src
```

To pack the project type following command
```ps
dotnet pack ./src

cd src/Genocs.Core
dotnet pack -p:NuspecFile=./Genocs.Core.nuspec --no-restore -o .
```


To push the project type following command
```ps
dotnet nuget push
dotnet nuget push *.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
```

