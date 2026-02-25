# Genocs.Common Library

## Overview

**Genocs.Common** is a foundational library that provides essential building blocks for developing enterprise-grade applications using .NET. This library contains core abstractions, interfaces, and base types that support Domain-Driven Design (DDD), Command Query Responsibility Segregation (CQRS), and other architectural patterns commonly used in modern microservices and distributed systems.

[![NuGet](https://img.shields.io/nuget/v/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Common.svg)](https://www.nuget.org/packages/Genocs.Common/)

## Target Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0

## Key Design Principles

The library is designed with the following principles in mind:

- **Zero External Dependencies**: Pure abstractions with no third-party package dependencies
- **Framework Agnostic**: Core interfaces that can be implemented in any persistence or messaging framework
- **SOLID Principles**: Strong adherence to object-oriented design principles
- **Clean Architecture**: Separation of concerns and dependency inversion
- **Enterprise-Ready**: Production-tested patterns for scalable applications

## Core Components

### 1. Domain-Driven Design Building Blocks

#### Entities and Aggregates

The library provides essential DDD interfaces for building domain models:

- **`IEntity`**: Base interface for all entities with identities
- **`IEntity<TKey>`**: Generic entity with typed primary key
- **`IAggregateRoot`**: Marker interface for aggregate roots
- **`IAggregateRoot<TKey>`**: Generic aggregate root with domain event support
- **`IGeneratesDomainEvents`**: Interface for entities that generate domain events
- **`ISoftDelete`**: Soft deletion support for entities

**Key Features:**
- Entity identities management
- Aggregate boundary enforcement
- Domain event tracking and publishing
- Soft delete capabilities

**Example Use Cases:**
```csharp
public interface IEntity<TKey>
{
    TKey Id { get; }
    bool IsTransient();
}

public interface IAggregateRoot<TKey>
{
    List<IEvent>? DomainEvents { get; }
}
```

#### Value Objects

Support for value object patterns through base interfaces that enforce:
- Immutability
- Value-based equality
- Domain-specific validation

#### Domain Repositories

Repository pattern interfaces for data access:

- **`IRepository<TEntity>`**: Generic repository interface
- **`IUnitOfWork`**: Transaction management and consistency
- **`ISupportsExplicitLoading`**: Lazy loading support

**Capabilities:**
- CRUD operations abstraction
- Specification pattern support
- Transaction management
- Query composition
- Explicit relationship loading

### 2. CQRS Implementation

Complete CQRS pattern support with separation of read and write operations.

#### Commands

- **`ICommand`**: Command marker interface
- **`ICommandHandler<TCommand>`**: Command handler abstraction
- **`ICommandDispatcher`**: Command routing and execution

**Purpose:** 
Commands represent write operations that change system state. They follow the command pattern and enable:
- Clear separation of write concerns
- Validation before execution
- Audit trail and logging
- Command pipeline behaviors

#### Queries

- **`IQuery<TResult>`**: Query marker interface with result type
- **`IQueryHandler<TQuery, TResult>`**: Query handler abstraction
- **`IQueryDispatcher`**: Query routing and execution
- **`IPagedQuery<TResult>`**: Paginated query support
- **`PagedResult<T>`**: Paginated result wrapper
- **`IPagedFilter`**: Filtering interface for queries
- **`ISearchRequest`**: Generic search request abstraction

**Purpose:**
Queries represent read operations that don't modify state. They support:
- Efficient data retrieval
- Pagination and filtering
- Sorting and searching
- Result transformation
- Caching strategies

**Pagination Support:**
- `PagedQueryBase`: Base class for paginated queries
- `PagedQueryWithFilter<TFilter>`: Query with filter support
- `PagedResultBase`: Base paginated result
- `PagedResult<T>`: Strongly-typed paginated result

#### Events

- **`IEvent`**: Event marker interface
- **`IEventHandler<TEvent>`**: Event handler abstraction
- **`IEventDispatcher`**: Event publishing and routing
- **`IRejectedEvent`**: Failed event marker
- **`RejectedEvent`**: Implementation of rejected events

**Purpose:**
Events represent things that have happened in the system:
- Domain event publishing
- Integration event handling
- Event sourcing support
- Asynchronous processing
- Event-driven architectures

### 3. Dependency Injection Markers

Convention-based dependency injection registration through marker interfaces:

- **`ISingletonDependency`**: Registers implementing classes as singletons
- **`ITransientDependency`**: Registers implementing classes as transient
- **`IScopedService`**: Scoped service lifetime marker
- **`ITransientService`**: Alternative transient service marker

**Benefits:**
- Auto-registration by scanning assemblies
- Consistent service lifetime management
- Convention-over-configuration approach
- Reduced boilerplate code

**Usage Pattern:**
```csharp
// Any class implementing this interface is automatically 
// registered as singleton
public class MyService : ISingletonDependency
{
    // Implementation
}
```

### 4. Notification System

Comprehensive notification infrastructure:

- **`INotificationMessage`**: Base notification interface
- **`INotificationSender`**: Notification dispatch abstraction
- **`BasicNotification`**: Simple notification implementation
- **`JobNotification`**: Job status notifications
- **`StatsChangedNotification`**: Statistics change notifications
- **`NotificationConstants`**: Common notification constants

**Capabilities:**
- Real-time notifications
- Job progress tracking
- Statistics updates
- Multi-channel notification support
- SignalR integration ready

### 5. Service Interfaces

Common service abstractions for cross-cutting concerns:

- **`ICurrentUser`**: Current user context and claims
- **`IDto`**: Data Transfer Object marker
- **`IJobService`**: Background job management
- **`ISerializerService`**: Serialization abstraction

**Purpose:**
These interfaces provide standard abstractions for:
- User identities and authorization
- Data serialization (JSON, XML, etc.)
- Background job scheduling
- DTO validation and mapping

### 6. Persistence Infrastructure

Database initialization and seeding:

- **`ICustomSeeder`**: Custom data seeding interface
- **`IDatabaseInitializer`**: Database initialization abstraction

**Features:**
- Database schema initialization
- Master data seeding
- Test data population
- Multi-tenant setup
- Migration support

### 7. Type System and Utilities

#### Type Collections

- **`ITypeList`**: Dynamic type collection
- **`TypeList`**: Implementation of type collection

**Use Cases:**
- Plugin architectures
- Module discovery
- Assembly scanning
- Type registration

#### Service Identification

- **`IServiceId`**: Unique service identifier
- **`ServiceId`**: Implementation with GUID-based IDs

**Use Cases:**
- Service discovery
- Distributed tracing
- Service mesh integration
- Unique instance identification

#### Common Attributes

- **`DecoratorAttribute`**: Marks decorator classes
- **`HiddenAttribute`**: Hides from automatic discovery
- **`MessageAttribute`**: Marks message types
- **`PublicContractAttribute`**: Marks public API contracts

**Benefits:**
- Metadata-driven behaviors
- Automatic registration control
- API versioning support
- Contract documentation

#### Configuration

- **`AppOptions`**: Application-wide configuration options

### 8. Extension Methods

- **`Extensions`**: Common extension methods for core types

## Architecture Integration

### Bounded Contexts

The library supports bounded context implementation through:
- Clear aggregate boundaries
- Repository abstraction per aggregate
- Domain event isolation
- Context-specific interfaces

### Microservices

Designed for microservices architectures:
- Service independence through abstractions
- Event-driven communication
- CQRS separation
- Distributed transaction patterns

### Clean Architecture

Supports clean architecture principles:
- Core domain isolation
- Infrastructure abstraction
- Dependency inversion
- Framework independence

## Design Patterns Supported

1. **Repository Pattern**: Data access abstraction
2. **Unit of Work**: Transaction management
3. **Specification Pattern**: Query composition
4. **Command Pattern**: CQRS commands
5. **Mediator Pattern**: Command/Query dispatching
6. **Observer Pattern**: Event handling
7. **Factory Pattern**: Entity creation
8. **Decorator Pattern**: Cross-cutting concerns

## Best Practices

### Entity Design

- Keep aggregate boundaries small
- Use value objects for immutable concepts
- Generate domain events for state changes
- Implement ISoftDelete for audit trails

### Command and Query Separation

- Commands should not return data
- Queries should not modify state
- Use different models for read and write
- Validate commands before execution

### Event-Driven Design

- Publish events after successful persistence
- Use events for loose coupling
- Handle events asynchronously
- Implement idempotent event handlers

### Repository Usage

- One repository per aggregate root
- Keep repository interfaces in domain layer
- Implement in infrastructure layer
- Use specifications for complex queries

## Usage Scenarios

### Enterprise Applications

- Multi-layer architectures
- Complex domain models
- Audit and compliance requirements
- Scalability needs

### Microservices

- Service boundaries
- Inter-service communication
- Event-driven architectures
- Distributed systems

### Domain-Driven Design Projects

- Ubiquitous language implementation
- Aggregate modeling
- Domain event sourcing
- Bounded context separation

### CQRS Applications

- Read/write separation
- Event sourcing
- Command validation
- Query optimization

## Dependencies

**Zero External Dependencies** - The library has no NuGet package dependencies, only framework references.

## Installation

```bash
dotnet add package Genocs.Common
```

## Related Libraries

- **Genocs.Core**: Concrete implementations and builders
- **Genocs.Persistence.MongoDB**: MongoDB repository implementations
- **Genocs.Persistence.EFCore**: Entity Framework Core implementations
- **Genocs.Messaging**: Message broker integrations

## Support and Documentation

- **Documentation**: [https://learn.fiscanner.net/](https://learn.fiscanner.net/)
- **Source Code**: [https://github.com/Genocs/genocs-library](https://github.com/Genocs/genocs-library)
- **Issues**: [https://github.com/Genocs/genocs-library/issues](https://github.com/Genocs/genocs-library/issues)
- **Changelog**: [https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md](https://github.com/Genocs/genocs-library/blob/main/CHANGELOG.md)

## License

This library is released under the MIT License. See [LICENSE](https://github.com/Genocs/genocs-library/blob/main/LICENSE) file for details.

## Contributing

Contributions are welcome! Please read the [Code of Conduct](https://github.com/Genocs/genocs-library/blob/main/CODE_OF_CONDUCT.md) before submitting pull requests.

## Author

**Giovanni Emanuele Nocco**

Enterprise Architect and Software Engineer specializing in .NET, microservices, and distributed systems.
