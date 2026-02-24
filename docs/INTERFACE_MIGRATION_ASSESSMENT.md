# Interface Migration Assessment: Genocs.Core → Genocs.Common

## Executive Summary

This document provides an assessment of interfaces in **Genocs.Core** that can be moved to **Genocs.Common** based on the requirement that Genocs.Common must have **zero NuGet package dependencies**.

---

## Current State Analysis

### Genocs.Common
- **NuGet Dependencies**: ✅ NONE
- **Purpose**: Common components with no external dependencies
- **Existing Interfaces**: 
  - Domain: `IEntity<T>`, `IAggregateRoot`, `ISoftDelete`, `IConnectionStringValidator`, `IConnectionStringSecurer`
  - Cqrs: `IEvent`, `ICommand`, `IQuery`, `IMessage`, `IDispatcher`, `ICommandHandler`, `IQueryHandler`, etc.
  - Types: `IIdentifiable<T>`, `IInitializer`, `IStartupInitializer`
  - Services: `ITransientService`, `IScopedService`, `ISerializerService`, etc.

### Genocs.Core
- **NuGet Dependencies**: 
  - `Ardalis.Specification` v9.3.1
  - `MediatR.Contracts` v2.0.1
  - `Scrutor` v7.0.0
  - `Spectre.Console` v0.54.0
  - `Microsoft.AspNetCore.App` (Framework Reference)
- **Purpose**: Core components with richer functionality
- **Contains**: 56 interface files across Collections, Cqrs, Domain, and Repositories

---

## Migration Candidates

### ✅ Category 1: Collections (RECOMMENDED - HIGH PRIORITY)

| Interface | Current Location | Dependencies | Migration Complexity |
|-----------|-----------------|--------------|---------------------|
| `ITypeList` | Genocs.Core/Collections | `IList<Type>` (System) | LOW |
| `ITypeList<TBaseType>` | Genocs.Core/Collections | `IList<Type>` (System) | LOW |

**Rationale**: These are pure collection abstractions with no external dependencies. The implementation class `TypeList<T>` should also be moved.

**Benefits**:
- Useful for type registration and discovery patterns
- No dependency on external packages
- Self-contained functionality

---

### ✅ Category 2: Cqrs Event Handlers (RECOMMENDED - HIGH PRIORITY)

| Interface | Current Location | Dependencies | Migration Complexity |
|-----------|-----------------|--------------|---------------------|
| `IEventHandler<TEvent>` | Genocs.Core/Cqrs/Events | `IEvent` (Genocs.Common) | LOW |
| `IEventDispatcher` | Genocs.Core/Cqrs/Events | `IEvent` (Genocs.Common) | LOW |
| `IRejectedEvent` | Genocs.Core/Cqrs/Events | `IEvent` (Genocs.Common) | LOW |

**Rationale**: These interfaces only depend on `IEvent` which is already in Genocs.Common. They represent core Cqrs patterns.

**Benefits**:
- Complete the Cqrs pattern in Genocs.Common
- Enable event-driven architecture without Genocs.Core dependency
- Align with existing `ICommandHandler` and `IQueryHandler` in Genocs.Common

**Note**: The `RejectedEvent` class implementation could also be moved.

---

### ✅ Category 3: Query Paging (RECOMMENDED - MEDIUM PRIORITY)

| Interface/Class | Current Location | Dependencies | Migration Complexity |
|-----------------|-----------------|--------------|---------------------|
| `IPagedFilter<TResult, TQuery>` | Genocs.Core/Cqrs/Queries | `IQuery` (Genocs.Common), `PagedResult<T>` | MEDIUM |
| `PagedResult<T>` | Genocs.Core/Cqrs/Queries | `PagedResultBase` (Genocs.Common) | LOW |

**Rationale**: Paging is a common requirement. `PagedResultBase` is already in Genocs.Common, so `PagedResult<T>` and `IPagedFilter` can follow.

**Benefits**:
- Standard paging functionality without dependencies
- Already partially in Genocs.Common (PagedResultBase)
- Commonly used across all types of applications

---

### ✅ Category 4: Auditing Interfaces (RECOMMENDED - HIGH PRIORITY)

#### Time-Based Auditing (No User References)

| Interface | Current Location | Dependencies | Migration Complexity |
|-----------|-----------------|--------------|---------------------|
| `IHasCreationTime` | Genocs.Core/Domain/Entities/Auditing | `DateTime` (System) | LOW |
| `IHasModificationTime` | Genocs.Core/Domain/Entities/Auditing | `DateTime` (System) | LOW |
| `IHasDeletionTime` | Genocs.Core/Domain/Entities/Auditing | `ISoftDelete` (Genocs.Common), `DateTime` | LOW |

**Rationale**: These are fundamental timestamp interfaces with no external dependencies.

**Benefits**:
- Essential for entity tracking without user context
- Pure timestamp markers
- No dependencies on external packages

#### User-Based Auditing

| Interface | Current Location | Dependencies | Migration Complexity |
|-----------|-----------------|--------------|---------------------|
| `ICreationAudited` | Genocs.Core/Domain/Entities/Auditing | `IHasCreationTime`, `DefaultIdType` | MEDIUM |
| `ICreationAudited<TUser>` | Genocs.Core/Domain/Entities/Auditing | Above + `IEntity<DefaultIdType>` | MEDIUM |
| `IModificationAudited` | Genocs.Core/Domain/Entities/Auditing | `IHasModificationTime`, `DefaultIdType` | MEDIUM |
| `IModificationAudited<TUser>` | Genocs.Core/Domain/Entities/Auditing | Above + `IEntity<DefaultIdType>` | MEDIUM |
| `IDeletionAudited` | Genocs.Core/Domain/Entities/Auditing | `IHasDeletionTime`, `DefaultIdType` | MEDIUM |
| `IDeletionAudited<TUser>` | Genocs.Core/Domain/Entities/Auditing | Above + `IEntity<DefaultIdType>` | MEDIUM |
| `IAudited` | Genocs.Core/Domain/Entities/Auditing | `ICreationAudited`, `IModificationAudited` | MEDIUM |
| `IAudited<TUser>` | Genocs.Core/Domain/Entities/Auditing | Above + User variants | MEDIUM |
| `IFullAudited` | Genocs.Core/Domain/Entities/Auditing | `IAudited`, `IDeletionAudited` | MEDIUM |
| `IFullAudited<TUser>` | Genocs.Core/Domain/Entities/Auditing | Above + User variants | MEDIUM |

**Rationale**: These interfaces only use `DefaultIdType` (which is `Guid` via global using) and interfaces already in or moveable to Genocs.Common.

**Benefits**:
- Standard audit pattern across all entities
- No external package dependencies
- Essential for GDPR, compliance, and debugging

**Note**: `DefaultIdType` is defined as a global using alias for `System.Guid` in Directory.Build.props (line 63).

---

### ✅ Category 5: Repository Patterns (CONDITIONAL - MEDIUM PRIORITY)

#### Can Be Moved

| Interface | Current Location | Dependencies | Migration Complexity |
|-----------|-----------------|--------------|---------------------|
| `IUnitOfWork` | Genocs.Core/Domain/Repositories | `Task<int>` (System) | LOW |
| `ISupportsExplicitLoading<TEntity, TPrimaryKey>` | Genocs.Core/Domain/Repositories | `IEntity<T>` (Genocs.Common), `Expression` | LOW |
| `IRepositoryOfEntity<TEntity, TKey>` | Genocs.Core/Domain/Repositories | `IEntity<T>` (Genocs.Common), LINQ | MEDIUM |

**Rationale**: 
- `IUnitOfWork` is a simple abstraction with no dependencies
- `ISupportsExplicitLoading` only uses standard System types and IEntity
- `IRepositoryOfEntity` is comprehensive but only uses System types and IEntity

**Benefits**:
- Complete repository pattern in Genocs.Common
- Enables generic data access without Genocs.Core
- Framework-agnostic persistence abstractions

**Consideration**: `IRepositoryOfEntity<TEntity, TKey>` is quite comprehensive (300+ lines). Consider if this complexity belongs in "Common" or should stay in "Core".

#### Cannot Be Moved (External Dependencies)

| Interface | Current Location | Blocking Dependency | Reason |
|-----------|-----------------|---------------------|--------|
| `IRepository<T>` | Genocs.Core/Domain/Repositories | `Ardalis.Specification.IRepositoryBase<T>` | External package |
| `IReadRepository<T>` | Genocs.Core/Domain/Repositories | `Ardalis.Specification.IReadRepositoryBase<T>` | External package |
| `IRepositoryWithEvents<T>` | Genocs.Core/Domain/Repositories | `Ardalis.Specification.IRepositoryBase<T>` | External package |
| `IDapperRepository` | Genocs.Core/Domain/Repositories | `IDbTransaction`, `IEntity` | System.Data dependency (acceptable), but specialized |

---

### ⚠️ Category 6: Builder/Infrastructure (NOT RECOMMENDED)

| Interface | Current Location | Blocking Dependency | Reason |
|-----------|-----------------|---------------------|--------|
| `IGenocsBuilder` | Genocs.Core/Builders | `Microsoft.AspNetCore.Builder` | Framework dependency |

**Rationale**: Cannot move due to ASP.NET Core framework reference. Should remain in Genocs.Core.

---

### ⚠️ Category 7: Service-Specific (NOT RECOMMENDED)

| Interface | Current Location | Reason Not to Move |
|-----------|-----------------|-------------------|
| `IAuditService` | Genocs.Core/Domain/Entities/Auditing | Service implementation, depends on specific DTOs |

**Rationale**: While technically moveable, service implementations are better suited for Core layer.

---

## Migration Strategy

### Phase 1: Foundation (Immediate)
1. **ITypeList** and **ITypeList<TBaseType>** + implementation
2. **IHasCreationTime**, **IHasModificationTime**, **IHasDeletionTime**

### Phase 2: Cqrs Completion (High Priority)
1. **IEventHandler<TEvent>**
2. **IEventDispatcher**
3. **IRejectedEvent** + RejectedEvent class

### Phase 3: Auditing (High Priority)
1. All **ICreationAudited** variants
2. All **IModificationAudited** variants
3. All **IDeletionAudited** variants
4. All **IAudited** and **IFullAudited** variants

### Phase 4: Queries (Medium Priority)
1. **PagedResult<T>** class
2. **IPagedFilter<TResult, TQuery>**

### Phase 5: Repository Abstractions (Optional)
1. **IUnitOfWork**
2. **ISupportsExplicitLoading<TEntity, TPrimaryKey>**
3. **IRepositoryOfEntity<TEntity, TKey>** (if desired)

---

## Impact Analysis

### Benefits
1. ✅ **Reduced Dependencies**: Projects needing basic abstractions won't require Genocs.Core
2. ✅ **Cleaner Architecture**: Clear separation between common patterns and framework-specific implementations
3. ✅ **Better Reusability**: Interfaces can be used in projects that can't use ASP.NET Core
4. ✅ **Consistency**: Complete Cqrs and auditing patterns in one place

### Risks and Mitigation
1. ⚠️ **Breaking Change**: Moving interfaces changes namespaces
   - **Mitigation**: Use type forwarding attributes or provide migration guide
   - **Mitigation**: Consider adding namespace aliases in Genocs.Core for backward compatibility
   
2. ⚠️ **Dependency Reversal**: Genocs.Core would depend on more from Genocs.Common
   - **Assessment**: This is actually desirable and follows proper layering
   
3. ⚠️ **Testing Impact**: Tests referencing old namespaces will break
   - **Mitigation**: Update test projects simultaneously
   - **Mitigation**: Use global usings to minimize changes

---

## Recommendations

### ✅ Highly Recommended (24 interfaces)
- All auditing interfaces (10 interfaces)
- Event handling interfaces (3 interfaces)
- Collection abstractions (2 interfaces)
- Time-based interfaces (3 interfaces)
- Paging interfaces/classes (2 interfaces)
- UnitOfWork (1 interface)

### 🤔 Consider Carefully (2 interfaces)
- `IRepositoryOfEntity<TEntity, TKey>` - Comprehensive but large
- `ISupportsExplicitLoading<TEntity, TPrimaryKey>` - Useful but specific

### ❌ Do Not Move (5+ interfaces/classes)
- Anything depending on `Ardalis.Specification`
- Anything depending on `Microsoft.AspNetCore.*`
- `IGenocsBuilder` and related
- Service implementations like `IAuditService`
- MediatR-dependent classes

---

## Implementation Checklist

- [ ] Create migration plan document
- [ ] Set up feature branch for migration
- [ ] Move interfaces following Phase 1-5 order
- [ ] Update namespaces in moved files
- [ ] Add type forwarding in Genocs.Core (for backward compatibility)
- [ ] Update internal usings in Genocs.Core
- [ ] Update test projects
- [ ] Update documentation
- [ ] Run full test suite
- [ ] Update changelog with breaking changes
- [ ] Consider version bump (major if breaking, minor if backward compatible)

---

## File Structure Recommendation

After migration, Genocs.Common should have:

```
Genocs.Common/
├── Collections/
│   ├── ITypeList.cs
│   └── TypeList.cs (implementation)
├── Cqrs/
│   ├── Events/
│   │   ├── IEvent.cs (existing)
│   │   ├── IEventHandler.cs (NEW)
│   │   ├── IEventDispatcher.cs (NEW)
│   │   ├── IRejectedEvent.cs (NEW)
│   │   └── RejectedEvent.cs (NEW - implementation)
│   ├── Queries/
│   │   ├── PagedResult.cs (NEW)
│   │   └── IPagedFilter.cs (NEW)
│   └── ...
├── Domain/
│   ├── Entities/
│   │   ├── Auditing/
│   │   │   ├── IHasCreationTime.cs (NEW)
│   │   │   ├── IHasModificationTime.cs (NEW)
│   │   │   ├── IHasDeletionTime.cs (NEW)
│   │   │   ├── ICreationAudited.cs (NEW)
│   │   │   ├── IModificationAudited.cs (NEW)
│   │   │   ├── IDeletionAudited.cs (NEW)
│   │   │   ├── IAudited.cs (NEW)
│   │   │   └── IFullAudited.cs (NEW)
│   │   └── ...
│   └── Repositories/
│       ├── IUnitOfWork.cs (NEW - optional)
│       ├── ISupportsExplicitLoading.cs (NEW - optional)
│       └── IRepositoryOfEntity.cs (NEW - optional)
└── ...
```

---

## Conclusion

**Total Moveable Interfaces**: 24-26 interfaces (depending on repository pattern decisions)

**Recommended Action**: Proceed with Phases 1-4 (22 interfaces) as these provide the most value with minimal risk. Evaluate Phase 5 repository abstractions based on project needs and architectural goals.

**Timeline Estimate**: 
- Phase 1-2: 1-2 days
- Phase 3-4: 2-3 days
- Testing & Documentation: 1-2 days
- **Total**: 4-7 days for complete migration

---

**Document Version**: 1.0  
**Date**: February 13, 2026  
**Author**: GitHub Copilot  
**Status**: Assessment Complete - Awaiting Approval
