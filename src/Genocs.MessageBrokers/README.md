# .NET Logging library

This package contains a set of functionalities to use http client for microservice, the library is designed by Genocs even thought a lot of insight came from community.
First of all I have to say thanks to devmentors

The libraries are built using .NET7.


## Description

Core NuGet package contains Open Telemetry and logging useful for DDD service.


## Support

Please check the GitHub repository getting more info.


### DataProvider Settings
Following are the project settings needed to enable monitoring

``` json
  "AppSettings": {
    "ServiceName": "Demo WebApi",
  },
  "ConnectionStrings": {
    "ApplicationInsights": ""
  },
  "Monitoring": {
    "Jaeger": "localhost",
  }
```

## Release notes

### [2024-01-23] 5.0.6
- Refactory Settings
- Updated nuget packages

### [2023-11-25] 5.0.5
- Moved to NET8

### [yyyy-mm-dd] 5.0.4
- 

### [yyyy-mm-dd] 5.0.3
- 

### [yyyy-mm-dd] 5.0.2
- 

### [yyyy-mm-dd] 5.0.1
- 

### [2023-11-25] 5.0.0
- Moved to NET8

### [2023-10-13] 5.0.0-preview.5.0
- Added [editorconfig](https://editorconfig.org/)
- Added StyleCop
- Updated logo
- Updated readme

### [2023-03-12] 5.0.0-preview.4.0
- Implemented MongoDB repository interfaces

### [2023-03-12] 5.0.0
- New Architecture

### [2023-03-12] 3.1.0
- Added Builders

### [2023-03-12] 3.0.0
- Refactory to implement CQRS pattern

### [2023-03-04] 2.4.1
- Updated System.Text.Json

### [2023-03-04] 2.0.1
- Updated nuget packages

### [2023-03-04] 2.0.0
- Revisited

### [2023-02-06] 1.0.0-rc4.0
- Updated Masstransit

### [2023-01-01] 1.0.0-rc3.1
- Updated Masstransit

### [2023-01-01] 1.0.0-rc3.0
- Moved to netstandart

### [2023-01-01] 1.0.0-rc2.0
- Refactory and standardization



