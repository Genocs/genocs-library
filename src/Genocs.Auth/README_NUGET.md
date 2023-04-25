# .NET Core Auth client library

This package contains a set of functionalities to handling authorization logic as JWT.
First of all I have to say thanks to devmentors.

The libraries are built using .NET7.


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
    "issuer": "identity",
    "validIssuer": "identity",
    "validateAudience": false,
    "validateIssuer": true,
    "validateLifetime": true,
    "expiry": "01:00:00"
  }
```


## Release notes

### [2023-03-24] 5.0.0
- First release