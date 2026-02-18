---
applyTo: '**'
---

# Copilot Instructions

## 1. Language & Terminology
- **Technical Terms:** English.
- **Domain/Business Logic:** Italian (specifically in comments and documentation).
- **Tone:** Professional, clear, and concise.

## 2. Coding Standards
- Follow standard C# conventions.
- **Comments:** Strictly "Why" not "What." Do not explain syntax; explain business context and architectural decisions.
- **Self-Documenting Code:** Prioritize meaningful variable/method names over comments.

## 3. Tech Stack & Architecture
### Backend (C# .NET)
- **DI Container:** Castle Windsor (Do not use Microsoft.Extensions.DependencyInjection).
- **Data Access:** - **EF Core:** Default for command/write operations and simple fetches.
  - **Dapper:** Use specifically for complex, high-performance read queries (Raw SQL).
- **Messaging:** NServiceBus.
- **Mediator:** MediatR (Ensure handlers are isolated).
- **Validation:** FluentValidation.
- **Testing:** Reqnroll.

### Frontend (Angular)
- **Core:** Angular + TypeScript.
- **State/Async:** RxJS (Prioritize observables over promises).

### Infrastructure
- **DB:** SQL Server.
- **Caching:** Redis.
