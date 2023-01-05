# .NET Core Persistence MongoDB library

This package contains a repository pattern implementation using MongoDB. The library is designed by Genocs.
The libraries are built using .NET standard 2.1.


## Description

Persistence MongoDB Core NuGet package contains general purpose functionalities to be used on DDD service.


## Support

Please check the GitHub repository getting more info.


### DataProvider Settings
Following is about how to setup **DBSettings**

``` json
  "DBSettings": {
    "ConnectionString": "mongodb://localhost",
    "Database": "demo_database",
    "EnableTracing": "false"
  }
```