# .NET Core Auth client library

This package contains a set of functionalities to handling authorization logic as JWT.

The libraries are built using .NET9, .NET8, .NET7, .NET6.

## Description

This package contains a set of functionalities to handling authorization logic as JWT.


## Support

Please check the GitHub repository getting more info.


### DataProvider Settings
Following are the project settings needed to enable monitoring

``` json
  "jwt": {
    "certificate": {
      "location": "certs/localhost.pfx",
      "password": "test",
      "rawData": ""
    },
    "issuer": "genocs-identities-service",
    "validIssuer": "genocs-identities-service",
    "validateAudience": false,
    "validateIssuer": true,
    "validateLifetime": true,
    "expiry": "01:00:00"  
  }
```

## Support

Please check the [GitHub repository](https://github.com/Genocs/genocs-library) to get more info.


## Release notes

The change log and breaking changes are listed here.

- [releases](https://github.com/Genocs/genocs-library/releases)