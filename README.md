# FO.NET CORE

This is the PixelFarm-based Pdf Generator evolution. The project has been ported to .NET Core 3.1.
A simple test with Docker container is also provided. The nuget package is hosted on myget. 

The project is build by Travis CI 

[![Build Status](https://travis-ci.org/Genocs/fonet.svg?branch=master)](https://travis-ci.org/Genocs/fonet)

(see original GDI+ version at  http://fonet.codeplex.com/).

---

The project reference the following license
```
//Apache2, 2017, WinterDev
//Apache2, 2009, griffm, FO.NET
//MIT, 2014-2017, WinterDev
```

To build the project type following command
```ps
dotnet build .\src
```

To pack the project type following command
```ps
dotnet pack .\src
```


To pack the project type following command
```ps
dotnet nuget push
```

