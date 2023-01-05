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