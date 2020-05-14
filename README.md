# Genocs CORE By Genocs

This is the base project designad by Genocs. The project has been ported to .NET Core 3.1.
A simple test with Docker container is also provided. The nuget package is hosted on myget. 

The project is build by Travis CI 

[![Build Status](https://travis-ci.org/Genocs/genocs-core.svg?branch=master)](https://travis-ci.org/Genocs/genocs-core)



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


To pack the project type following command
```ps
dotnet nuget push
dotnet nuget push *.nupkg -k $NUGET_API_KEY -s $NUGET_SOURCE
```

