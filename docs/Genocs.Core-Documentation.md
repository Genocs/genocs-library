# Genocs.Core Library

## Overview

**Genocs.Core** is the core implementation library that extends **Genocs.Common** with concrete implementations, utility classes, and the Genocs application builder infrastructure. It provides ready-to-use base classes, dispatchers, extensions, and tools that accelerate development of enterprise applications following DDD, Cqrs, and clean architecture principles.

[![NuGet](https://img.shields.io/nuget/v/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Genocs.Core.svg)](https://www.nuget.org/packages/Genocs.Core/)

## Target Frameworks

- .NET 10.0
- .NET 9.0
- .NET 8.0

## Key Features

**Genocs.Core** transforms the abstractions from Genocs.Common into working implementations:

- **Genocs Builder**: Fluent API for application configuration
- **Base Entity Classes**: Concrete entity and aggregate implementations
- **Cqrs Dispatchers**: Command, query, and event dispatcher implementations
- **Domain Events**: Base domain event classes
- **Repository Base**: Base repository implementation with Dapper support
- **Extension Methods**: Rich set of utility extensions
- **Exception Handling**: Custom exception types and handling
- **Encryption Utilities**: RSA encryption helpers

## Dependencies

### NuGet Packages

- **Genocs.Common**: Core abstractions and interfaces
- **Spectre.Console**: ASCII art generation for application branding
- **Ardalis.Specification**: Specification pattern implementation
- **MediatR.Contracts**: Mediator pattern contracts
- **Scrutor**: Assembly scanning and decoration

### Framework References

- **Microsoft.AspNetCore.App**: ASP.NET Core runtime

## Core Components

### 1. Genocs Builder Infrastructure

The heart of the library - a fluent API for bootstrapping applications.

#### IGenocsBuilder

The main builder interface providing:

```csharp
public interface IGenocsBuilder
{
    IServiceCollection Services { get; }
    IConfiguration? Configuration { get; }
    WebApplicationBuilder? WebApplicationBuilder { get; }
    
    bool TryRegister(string name);
    void AddBuildAction(Action<IServiceProvider> execute);
    void AddInitializer(IInitializer initializer);
    void AddInitializer<TInitializer>() where TInitializer : IInitializer;
    IServiceProvider Build();
}
```

**Key Capabilities:**

- **Service Registration**: Access to IServiceCollection for DI configuration
- **Configuration Management**: Access to application configuration
- **Registration Tracking**: Prevents duplicate service registration
- **Build Actions**: Execute actions after container is built
- **Initializer Support**: Register startup initializers
- **Web Application Integration**: Direct integration with WebApplicationBuilder

#### GenocsBuilder Implementation

Concrete implementation providing:

- **Singleton Pattern**: Ensures single instance of builder
- **Registration Registry**: Tracks registered services by name
- **Build Actions Queue**: Deferred execution after container build
- **Startup Initialization**: Automatic initializer orchestration

**Usage Example:**

```csharp
var builder = WebApplication.CreateBuilder(args);
var genocsBuilder = builder.Services.AddGenocs();

// Register services
genocsBuilder.AddCommandHandlers();
genocsBuilder.AddQueryHandlers();
genocsBuilder.AddEventHandlers();

// Add custom initialization
genocsBuilder.AddInitializer<DatabaseInitializer>();

// Build the service provider
var serviceProvider = genocsBuilder.Build();
```

#### StartupInitializer

Orchestrates application initialization:

- Executes all registered `IInitializer` implementations
- Ensures proper startup sequence
- Exception handling during initialization
- Logging of initialization steps

**Benefits:**

- **Consistent Bootstrapping**: Standardized application setup
- **Modular Configuration**: Plugin-style feature registration
- **Fluent API**: Readable and maintainable configuration code
- **Convention Over Configuration**: Automatic service discovery
- **Type Safety**: Compile-time checking of registrations

### 2. Domain Layer Implementations

#### Entity Base Classes

**Entity\<TPrimaryKey\>**

Abstract base class for all entities:

```csharp
public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
{
    public virtual TPrimaryKey Id { get; set; }
    public virtual bool IsTransient();
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public static bool operator ==(Entity<TPrimaryKey>? left, Entity<TPrimaryKey>? right);
    public static bool operator !=(Entity<TPrimaryKey>? left, Entity<TPrimaryKey>? right);
}
```

**Features:**
- Unique identity management
- Value-based equality comparison
- Transient entity detection
- Operator overloading for comparisons
- GetHashCode optimization
- Reflection-based property comparison

**Entity (Shortcut)**

Default implementation using `DefaultIdType`:

```csharp
public abstract class Entity : Entity<DefaultIdType>;
```

#### AggregateRoot Base Classes

**AggregateRoot\<TPrimaryKey\>**

Base class for aggregate roots with domain event support:

```csharp
public class AggregateRoot<TPrimaryKey> 
    : Entity<TPrimaryKey>, IAggregateRoot<TPrimaryKey>
{
    [NotMapped]
    public virtual List<IEvent>? DomainEvents { get; }
}
```

**Features:**
- Inherits all Entity capabilities
- Domain event collection
- NotMapped attribute for ORM integration
- Event tracking and publishing

**AggregateRoot (Shortcut)**

Default implementation:

```csharp
public class AggregateRoot : AggregateRoot<DefaultIdType>, IAggregateRoot;
```

#### Domain Events

**DomainEvent**

Base class for all domain events:

```csharp
public class DomainEvent : IEvent
{
    public DateTime OccurredOn { get; init; }
    public Guid Id { get; init; }
}
```

**Pre-built Domain Events:**

1. **EntityCreatedEvent\<TEntity\>**
   - Raised when an entity is created
   - Contains the created entity
   - Timestamp of creation

2. **EntityUpdatedEvent\<TEntity\>**
   - Raised when an entity is modified
   - Contains updated entity
   - Timestamp of update

3. **EntityDeletedEvent\<TEntity\>**
   - Raised when an entity is deleted
   - Contains deleted entity ID
   - Timestamp of deletion

**Usage Example:**

```csharp
public class Order : AggregateRoot
{
    public void PlaceOrder()
    {
        // Business logic
        DomainEvents.Add(new OrderPlacedEvent(this));
    }
}
```

#### Entity Extensions

- **EntityExtensions**: Helper methods for entity operations

#### Entity Exceptions

- **EntityNotFoundException**: Thrown when entity not found
- **GenocsException**: Base exception for framework

#### Auditing Support

**Trail Entity**

Audit trail entity for tracking changes:

```csharp
public class Trail : Entity<DefaultIdType>
{
    public string UserId { get; set; }
    public string Type { get; set; }
    public string TableName { get; set; }
    public DateTime DateTime { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string AffectedColumns { get; set; }
    public string PrimaryKey { get; set; }
}
```

**Audit DTOs and Handlers:**

- **AuditDto**: Data transfer object for audit logs
- **GetAuditLogsRequest**: Query for retrieving audit logs
- **GetAuditLogsRequestHandler**: Handler for audit log queries

**Auditing Interfaces:**

Built on top of Genocs.Common auditing:
- `ICreationAuditedObject`
- `IModificationAuditedObject`
- `IDeletionAuditedObject`

### 3. Repository Infrastructure

#### IRepository\<TEntity\>

Enhanced repository interface extending the common repository with additional methods.

#### IDapperRepository

Dapper-specific repository interface:

```csharp
public interface IDapperRepository
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object param);
    Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param);
    Task<int> ExecuteAsync(string sql, object param);
}
```

**Features:**
- Raw SQL query support
- Stored procedure execution
- Bulk operations
- Performance optimization

#### RepositoryBase\<TEntity\>

Abstract base class for repository implementations:

```csharp
public abstract class RepositoryBase<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
    // Common repository operations
}
```

**Provided Operations:**
- CRUD operations
- Bulk insert/update/delete
- Specification-based queries
- Transaction support
- Caching hooks

#### Repository Attributes

**TableMappingAttribute**

Maps entities to database tables:

```csharp
[TableMapping("Orders")]
public class Order : Entity
{
    // Properties
}
```

**AutoRepositoryTypesAttribute**

Configures automatic repository registration:

```csharp
[assembly: AutoRepositoryTypes(
    typeof(IRepository<>),
    typeof(Repository<>)
)]
```

### 4. Cqrs Dispatchers

Complete implementation of command, query, and event dispatching.

#### Command Dispatching

**InMemoryCommandDispatcher**

Default in-memory command dispatcher:

```csharp
public class InMemoryCommandDispatcher : ICommandDispatcher
{
    public Task SendAsync<TCommand>(TCommand command) 
        where TCommand : ICommand;
}
```

**Features:**
- Synchronous and asynchronous dispatch
- Handler resolution via DI
- Validation pipeline
- Exception handling
- Logging and telemetry

#### Query Dispatching

**InMemoryQueryDispatcher**

Default in-memory query dispatcher:

```csharp
public class InMemoryQueryDispatcher : IQueryDispatcher
{
    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query);
}
```

**Features:**
- Strongly-typed results
- Query validation
- Caching support
- Performance monitoring

#### Event Dispatching

**InMemoryEventDispatcher**

Default in-memory event dispatcher:

```csharp
public class InMemoryEventDispatcher : IEventDispatcher
{
    public Task PublishAsync<TEvent>(TEvent @event) 
        where TEvent : IEvent;
}
```

**Features:**
- Multi-subscriber support
- Asynchronous publishing
- Error isolation
- Event ordering

#### Registration Extensions

Fluent extension methods for registering dispatchers:

```csharp
services.AddCommandHandlers();
services.AddQueryHandlers();
services.AddEventHandlers();
services.AddDispatchers();
```

### 5. Exception Handling

#### GenocsException

Base exception class for all framework exceptions:

```csharp
public class GenocsException : Exception
{
    public GenocsException(string message) : base(message) { }
    public GenocsException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

**Derived Exceptions:**

- **EntityNotFoundException**: Entity not found in repository
- **InvalidConfigurationException**: Configuration errors
- **ValidationException**: Business rule violations

#### Exception Extensions

- **ExceptionExtensions**: Helper methods for exception handling

### 6. Extension Methods

Rich set of utility extension methods across multiple categories.

#### String Extensions

```csharp
public static class StringExtensions
{
    public static bool IsNullOrWhiteSpace(this string str);
    public static string ToCamelCase(this string str);
    public static string ToSnakeCase(this string str);
    public static string ToKebabCase(this string str);
    // ... more string utilities
}
```

**Capabilities:**
- Case conversions (camelCase, snake_case, kebab-case)
- Null/empty checks
- Trimming and formatting
- String manipulation

#### Object Extensions

```csharp
public static class ObjectExtensions
{
    public static T Clone<T>(this T obj);
    public static Dictionary<string, object> ToDictionary(this object obj);
    public static bool HasProperty(this object obj, string propertyName);
    // ... more object utilities
}
```

**Capabilities:**
- Deep cloning
- Property reflection
- Dynamic object creation
- Serialization helpers

#### Encryption Extensions

```csharp
public static class Encryption
{
    public static void FromXmlFile(this RSA rsa, string xmlFilePath);
    public static void ToXmlFile(this RSA rsa, string xmlFilePath);
    public static string Encrypt(this RSA rsa, string plainText);
    public static string Decrypt(this RSA rsa, string cipherText);
}
```

**Features:**
- RSA encryption/decryption
- XML key import/export
- Secure key management
- Certificate handling

### 7. Collections

#### Type List Implementation

**TypeList**

Concrete implementation of `ITypeList`:

```csharp
public class TypeList : ITypeList
{
    public void Add<T>();
    public void Add(Type type);
    public bool Contains<T>();
    public bool Contains(Type type);
    public IEnumerable<Type> GetTypes();
}
```

**Use Cases:**
- Plugin discovery
- Module registration
- Assembly scanning results
- Type filtering and selection

### 8. Builder Extensions

Fluent extension methods for GenocsBuilder:

```csharp
public static class Extensions
{
    public static IGenocsBuilder AddGenocs(this IServiceCollection services);
    public static IGenocsBuilder AddGenocs(this WebApplicationBuilder builder);
    
    public static IGenocsBuilder AddErrorHandler<T>(this IGenocsBuilder builder)
        where T : IExceptionToResponseMapper;
    
    public static IGenocsBuilder AddServiceId(this IGenocsBuilder builder);
}
```

**Builder Methods:**
- Service registration helpers
- Module configuration
- Middleware setup
- Feature toggles

## Architecture Patterns

### Application Bootstrap

Typical application setup using Genocs.Core:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Create Genocs builder
var genocsBuilder = builder.Services.AddGenocs();

// Register Cqrs components
genocsBuilder.AddCommandHandlers();
genocsBuilder.AddQueryHandlers();
genocsBuilder.AddEventHandlers();

// Add persistence
genocsBuilder.AddMongoDb();
genocsBuilder.AddPostgres();

// Add messaging
genocsBuilder.AddRabbitMq();

// Add monitoring
genocsBuilder.AddOpenTelemetry();

// Build and run
var app = builder.Build();
await app.UseGenocs().RunAsync();
```

### Domain Event Flow

1. Aggregate root generates domain event
2. Event stored in DomainEvents collection
3. After persistence, dispatcher publishes events
4. Event handlers process asynchronously
5. Integration events sent to message broker

### Repository Pattern Usage

```csharp
public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(DbContext context) : base(context) { }
    
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }
}
```

### Cqrs Implementation

**Command:**
```csharp
public record CreateOrderCommand(string CustomerId, List<OrderItem> Items) 
    : ICommand;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    public async Task HandleAsync(CreateOrderCommand command)
    {
        // Implementation
    }
}
```

**Query:**
```csharp
public record GetOrderQuery(string OrderId) : IQuery<OrderDto>;

public class GetOrderQueryHandler : IQueryHandler<GetOrderQuery, OrderDto>
{
    public async Task<OrderDto> HandleAsync(GetOrderQuery query)
    {
        // Implementation
    }
}
```

## Best Practices

### Entity Design

1. Inherit from `Entity` or `Entity<TKey>` for entities
2. Inherit from `AggregateRoot` or `AggregateRoot<TKey>` for aggregates
3. Generate domain events for state changes
4. Keep aggregates small and focused

### Repository Implementation

1. One repository per aggregate root
2. Extend `RepositoryBase<TEntity>` for common operations
3. Use specifications for complex queries
4. Implement `IDapperRepository` for performance-critical queries

### Cqrs Best Practices

1. Separate command and query models
2. Validate commands before execution
3. Return void from commands
4. Use query objects for complex reads
5. Implement caching in query handlers

### Event Handling

1. Keep event handlers idempotent
2. Handle events asynchronously
3. Use integration events for cross-service communication
4. Implement retry logic for event processing

### Builder Usage

1. Register services in logical groups
2. Use `TryRegister` to prevent duplicates
3. Add initializers for startup tasks
4. Configure middleware in correct order

## Integration with Other Genocs Libraries

- **Genocs.Persistence.MongoDB**: MongoDB repository implementations
- **Genocs.Persistence.EFCore**: Entity Framework Core support
- **Genocs.Messaging.RabbitMQ**: RabbitMQ integration for events
- **Genocs.WebApi**: Web API utilities and middleware
- **Genocs.Logging**: Structured logging integration
- **Genocs.Telemetry**: Observability and tracing

## Performance Considerations

1. **Dapper Integration**: Use `IDapperRepository` for high-performance scenarios
2. **Event Dispatching**: Async event handling prevents blocking
3. **Caching**: Query handler caching support
4. **Bulk Operations**: Repository base includes bulk methods
5. **Specification Pattern**: Efficient query composition

## Testing Support

The library is designed to be testable:

- Interfaces for all major components
- In-memory dispatchers for unit testing
- Mock-friendly repository abstractions
- Event collection inspection for testing
- Builder testability

## Migration from Other Frameworks

### From MediatR

Genocs.Core provides similar patterns to MediatR:
- Commands → ICommand
- Queries → IQuery
- Handlers → ICommandHandler, IQueryHandler
- Notifications → IEvent, IEventHandler

### From Generic Repositories

Genocs.Core enhances basic repositories with:
- Specification pattern
- Domain event support
- Aggregate root awareness
- Transaction management

## Installation

```bash
dotnet add package Genocs.Core
```

## Documentation and Support

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
