# .NET Core Telemetry and Tracing library

This package contains a set of functionalities for telemetry and tracing, designed by Genocs.
The libraries are built using .NET7.


## Description

Core NuGet package contains Open Telemetry and logging useful for DDD service.

## Support

Please check the GitHub repository getting more info.


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

### [2023-02-06] 1.0.0-rc4.0
- Updated Masstransit

### [2023-01-01] 1.0.0-rc3.1
- Updated Masstransit

### [2023-01-01] 1.0.0-rc3.0
- Moved to netstandart

### [2023-01-01] 1.0.0-rc2.0
- Refactory and standardization



