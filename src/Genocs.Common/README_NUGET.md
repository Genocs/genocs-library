# .NET Core Common library

[![NuGet](https://img.shields.io/nuget/v/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)
[![NuGet](https://img.shields.io/nuget/dt/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)

## Overview

Genocs Enterprise Library - Genocs.Common.

This package contains a set of core common functionalities and types used across the framework.

## Description

Core Common NuGet package contains foundational types, interfaces and classes for building enterprise applications following Domain-Driven Design (DDD) and CQRS patterns. 

### Key Features:

**DDD Building Blocks:**
- Entities, Aggregate Roots, Value Objects
- Domain Repositories (IRepositoryOfEntity, IUnitOfWork)
- Soft delete and auditing support

**CQRS Implementation:**
- Commands, Queries, and Events with their respective handlers and dispatchers
- Paged query support with filtering capabilities
- Domain event generation and handling

**Infrastructure Components:**
- Dependency injection lifecycle markers (Singleton, Transient, Scoped)
- Notification system with various notification types
- Service interfaces (Serialization, Current User, Job Services)
- Persistence initialization (Database seeding, Custom seeders)

**Utilities:**
- Type collections and service identification
- Common attributes (Decorator, Hidden, Message, PublicContract)
- Configuration options (AppOptions)
- Extension methods and helper classes

The library is built to be used with .NET10, .NET9, .NET8.

## Support

Please check the [GitHub repository](https://github.com/Genocs/genocs-library) to get more info.

## Dependencies

- **NONE**

### Framework references

- **NONE**

## Documentation

The documentation is available at [Genocs - Open-Source Framework for Enterprise Applications](https://learn.fiscanner.net/).

## Release notes

The change log and breaking changes are listed here.

- [CHANGELOG](https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md)
- [releases](https://github.com/Genocs/genocs-library/releases)
