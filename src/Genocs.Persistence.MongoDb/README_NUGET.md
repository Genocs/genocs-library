# The Genocs Library - Persistence MongoDB components

Genocs Enterprise Library - Genocs.Persistence.MongoDb. This package contains a repository pattern implementation using MongoDB.
The library is built to be used with NET6, NET7, NET8 and NET9.

## Description

Persistence MongoDB NuGet package contains general purpose functionalities to be used on DDD services with MongoDB as the persistence data layer.

## Dependencies

- **Genocs.Core**: 7.5.\*
- **MongoDB.Driver**: 3.5.0
- **MongoDB.Driver.Core.Extensions.DiagnosticSources**: 2.1.0

### Framework references

- **NONE**

## MongoDb Convention

- CamelCaseElementNameConvention(),
- IgnoreExtraElementsConvention(true),
- EnumRepresentationConvention(BsonType.String)

### DataProvider Settings

Following is about how to setup **MongoDb**

```json
  "mongoDb": {
    "ConnectionString": "mongodb://localhost",
    "Database": "demo_database",
    "EnableTracing": false
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
