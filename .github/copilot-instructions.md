---
applyTo: "**/*.cs"
---

# ASP.NET Core 10 Project Rules

- Use C# 14 language features where appropriate
- Follow SOLID principles in class and interface design
- Implement dependency injection for loose coupling
- Use primary constructors for dependency injection in services, use cases, etc.
- Use async/await for I/O-bound operations
- Prefer record types for immutable data structures
- Prefer minimal APIs  endpoints over controller
  - Utilize minimal APIs for simple endpoints (when explicitly stated or when it makes sense)
- Implement proper exception handling and logging
- Use strongly-typed configuration with IOptions pattern
- Implement proper authentication and authorization
- Use Entity Framework Core for database operations
- Implement unit tests for business logic
- Use integration tests for API endpoints
- Implement proper versioning for APIs
- Implement proper caching strategies
- Use middleware for cross-cutting concerns
- Implement health checks for the application
- Use environment-specific configuration files
- Implement proper CORS policies
- Use secure communication with HTTPS
- Implement proper model validation
- Use OpenAPI for API documentation
- Implement proper logging with structured logging
- Use background services for long-running tasks
- Favor explicit typing (this is very important). Only use var when evident.
- Make types internal and sealed by default unless otherwise specified
- Prefer Guid for identifiers unless otherwise specified
- Use `is null` checks instead of `== null`. The same goes for `is not null` and `!= null`.

---
# Test Project Rules

- **Testing Framework**: Use **xUnit**
- **Assertions**: Use **Shouldly** for expressive and readable assertions
- **Mocking**: Use **NSubstitute** for dependency mocking
- **Naming Convention**:  
  - Follow the pattern:  
    ```
    MethodName_ShouldExpectedBehavior_WhenCondition
    ```
    Examples:  
    - `GetMovieByIdAsync_ShouldThrowException_WhenIdIsNotValid`  
    - `CreateUserAsync_ShouldReturnUser_WhenDataIsValid`  
    - `LoginAsync_ShouldFail_WhenPasswordIsIncorrect`  
  - Prefer clarity over brevity  
- **Structure**: Follow **Arrange–Act–Assert (AAA)** pattern for readability  
- **Async Tests**: Always mark with `async Task`  
- **Determinism**: Avoid external dependencies in **unit tests**—mock them instead  
- **Data-driven Tests**: Prefer `[Theory]` with `[InlineData]` or `[MemberData]` over multiple similar `[Fact]` tests for better coverage and maintainability  