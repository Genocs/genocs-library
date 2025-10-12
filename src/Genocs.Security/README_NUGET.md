# The Genocs Library - Security components

Genocs Enterprise Library - Genocs.Security. This package contains functionalities to handle standard security concerns.
The library is built to be used with NET6, NET7, NET8 and NET9.

## Description

Security NuGet package contains encryption, hashing, and JWT authentication functionalities for DDD services.

## Dependencies

- **Genocs.Core**: 7.5.\*
- **Microsoft.AspNetCore.Authentication.JwtBearer**: Version varies by target framework
  - NET9.0: 9.0.9
  - NET8.0: 8.0.\*
  - NET7.0: 7.0.\*
  - NET6.0: 6.0.\*

### Framework references

- **NONE**

## Example RSA Key

Following an example of how the key should be structured.

**WARNING: DO NOT USE IT IN PROD!!!**

```xml
<RSAKeyValue>
    <Modulus>svbEQ96xMdgUpnkDiSaULDbM/HVFLHLc46BdyqwEzIhK+Ml2dqWq/RZIh8kLWmYwpB5gqfOya8Wid3GKIpq7Ke8ciV53qW/1ImOZZPxOtwX1mNzvIEagq80QJoMLphtU1ytPWRXvOjBdGUeTzmdV2kpHNax41n4Uv0QpOPIhzME=</Modulus>
    <Exponent>AQAB</Exponent>
    <P></P>
    <Q></Q>
    <DP></DP>
    <DQ></DQ>
    <InverseQ></InverseQ>
    <D></D>
</RSAKeyValue>
```

## Support

Please check the [GitHub repository](https://github.com/Genocs/genocs-library) to get more info.

## Documentation

The documentation is available at [Genocs - Open-Source Framework for Enterprise Applications](https://genocs-blog.netlify.app/).

## Release notes

The change log and breaking changes are listed here.

- [CHANGELOG](https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md)

- [releases](https://github.com/Genocs/genocs-library/releases)
