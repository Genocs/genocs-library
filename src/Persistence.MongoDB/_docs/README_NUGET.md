# .NET Core Persistence MongoDB library

This package contains a repository pattern implementation using MongoDB. The library is designed by Genocs.
The library is built to be used with .NET10, .NET9, .NET8.


## Description

Persistence MongoDB Core NuGet package contains general purpose functionalities to be used on DDD service.


## Support

Please check the GitHub repository getting more info.

## MongoDB Convention

- CamelCaseElementNameConvention(),
- IgnoreExtraElementsConvention(true),
- EnumRepresentationConvention(BsonType.String)

### DataProvider Settings
Following is about how to setup **MongoDB**

``` json
  "mongoDb": {
    "ConnectionString": "mongodb://localhost",
    "Database": "demo_database",
    "EnableTracing": false,
    "GuidRepresentationMode": "Standard"
  }
```

### GUID representation compatibility

- Default mode: `Standard`
- Legacy compatibility mode: `CSharpLegacy`

For existing legacy datasets, use `GuidRepresentationMode: "CSharpLegacy"` during migration and switch back to `Standard` after data normalization.

### Runtime behavior notes

- Mongo provider, session factory, and repository access paths share one DI-managed `IMongoClient` pipeline.
- Startup seed execution is tracked per database key (`connectionString|database`) rather than process-wide only.

### Encryption guidance

Built-in client-side field encryption wiring is not provided by this package runtime surface.
If encryption is required, compose and validate it at application level.

## Support

Please check the [GitHub repository](https://github.com/Genocs/genocs-library) to get more info.


## Release notes

The change log and breaking changes are listed here.

- [releases](https://github.com/Genocs/genocs-library/releases)