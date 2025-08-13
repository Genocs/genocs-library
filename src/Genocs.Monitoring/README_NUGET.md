# .NET Core Telemetry and Tracing library

This package contains a set of functionalities for telemetry and tracing, designed by Genocs.
The libraries are built using .NET7.


## Description

Core NuGet package contains Open Telemetry and logging useful for DDD service.

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

## Support

Please check the [GitHub repository](https://github.com/Genocs/genocs-library) to get more info.


## Release notes

The change log and breaking changes are listed here.

- [releases](https://github.com/Genocs/genocs-library/releases)



