# .NET Core Persistence MongoDB library

This package contains a repository pattern implementation using MongoDB. The library is designed by Genocs.
The libraries are built using .NET standard 2.1.


## Description

Persistence MongoDB Core NuGet package contains general purpose functionalities to be used on DDD service.


## Support

Please check the GitHub repository getting more info.

## MongoDb Convention

- CamelCaseElementNameConvention(),
- IgnoreExtraElementsConvention(true),
- EnumRepresentationConvention(BsonType.String)

### DataProvider Settings
Following is about how to setup **MongoDb**

``` json
  "mongoDb": {
    "ConnectionString": "mongodb://localhost",
    "Database": "demo_database",
    "EnableTracing": false
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

### [2023-03-11] 3.4.0
- Updated to Genocs.Core 3.0.0

### [2023-03-11] 3.3.1
- Fix register conventions

### [2023-03-11] 3.3.0
- No issue in case of registration without MongoDbSettings section
- MongoDatabaseProvider register as Singleton
- Added standard RegisterConventions

### [2023-03-04] 3.2.1
- Updated Scrutor

### [2023-01-10] 3.2.0
- Updated MongoDbSettings section name

### [2023-01-10] 3.1.1
- Added MongoDbSettings validation

### [2023-01-10] 3.1.0
- Added Service collection extension

### [2023-01-07] 3.0.0
- Changed DBSettings to MongoDbSettings