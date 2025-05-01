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

## Support

Please check the [GitHub repository](https://github.com/Genocs/genocs-library) to get more info.


## Release notes

The change log and breaking changes are listed here.

- [releases](https://github.com/Genocs/genocs-library/releases)



