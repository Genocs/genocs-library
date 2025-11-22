# .NET Core Telemetry and Tracing library

This package contains a set of functionalities for telemetry and tracing, designed by Genocs.
The libraries are built using .NET10, .NET9, .NET8.


## Description

Monitoring NuGet package provides application monitoring, health checks, and observability features including integration with Application Insights and Jaeger for DDD services.

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

## Documentation

The documentation is available at [Genocs - Open-Source Framework for Enterprise Applications](https://genocs-blog.netlify.app/).

## Release notes

The change log and breaking changes are listed here.

- [CHANGELOG](https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md)
- [releases](https://github.com/Genocs/genocs-library/releases)



