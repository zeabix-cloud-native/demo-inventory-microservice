# Troubleshooting Guide
## Common Issues and Solutions for GitHub Copilot

This guide provides solutions to the most common problems developers encounter when using GitHub Copilot for professional software development. Each issue includes symptoms, root causes, and step-by-step solutions.

## 🚨 Quick Problem Finder

**Having issues with:**
- [Copilot Not Working](#copilot-not-working) - Installation, authentication, activation
- [Poor Code Quality](#poor-code-quality) - Bad suggestions, wrong patterns
- [Context Problems](#context-problems) - Copilot doesn't understand your project
- [Performance Issues](#performance-issues) - Slow responses, timeouts
- [Security Concerns](#security-concerns) - Sensitive data, vulnerabilities
- [Team Integration](#team-integration) - Inconsistent usage, collaboration
- [Architecture Violations](#architecture-violations) - Clean Architecture boundaries
- [Testing Problems](#testing-problems) - Test generation, coverage issues

## 🔧 Copilot Not Working

### Issue: Copilot Extension Not Activating

**Symptoms:**
- No Copilot icon in VS Code status bar
- No inline suggestions appearing
- Chat not responding

**Solutions:**

**1. Verify Installation:**
```bash
# Check installed extensions
code --list-extensions | grep copilot

# Expected output:
# github.copilot
# github.copilot-chat
```

**2. Check Authentication:**
```
1. Open VS Code Command Palette (Ctrl+Shift+P)
2. Run "GitHub Copilot: Sign In"
3. Complete authentication flow
4. Verify status bar shows Copilot icon
```

**3. Verify Subscription:**
- Check your GitHub account has active Copilot subscription
- Ensure organization policies allow Copilot usage
- Verify repository is not in organization's blocked list

**4. Reset Extension:**
```
1. Disable GitHub Copilot extensions
2. Reload VS Code
3. Re-enable extensions
4. Restart VS Code
```

### Issue: Copilot Chat Not Responding

**Symptoms:**
- Chat window opens but doesn't respond
- Error messages in chat
- Connection timeouts

**Solutions:**

**1. Check Network Connection:**
```bash
# Test GitHub API connectivity
curl -H "Authorization: token YOUR_TOKEN" https://api.github.com/user

# Test Copilot API connectivity (if accessible)
# Usually handled automatically by extension
```

**2. Clear Extension Cache:**
```
1. Close VS Code
2. Navigate to extensions folder:
   - Windows: %USERPROFILE%\.vscode\extensions
   - macOS: ~/.vscode/extensions
   - Linux: ~/.vscode/extensions
3. Delete github.copilot* folders
4. Reinstall extensions
```

**3. Update Extensions:**
```
1. Go to Extensions view (Ctrl+Shift+X)
2. Check for updates to Copilot extensions
3. Update to latest versions
4. Reload VS Code
```

## 📉 Poor Code Quality

### Issue: Copilot Generates Wrong Patterns

**Symptoms:**
- Code doesn't follow project conventions
- Incorrect architectural patterns
- Outdated or deprecated approaches

**Root Cause:** Insufficient context or conflicting patterns in codebase

**Solutions:**

**1. Improve Context Provision:**
```
❌ Bad prompt:
"Create a service"

✅ Good prompt:
"@workspace Create CategoryService in Application layer following Clean Architecture. 
Use the same pattern as ProductService.cs with:
- Dependency injection for repository and logger
- Async/await throughout
- DTO mapping
- Comprehensive error handling
- Following our established naming conventions"
```

**2. Clean Up Conflicting Patterns:**
```
1. Identify inconsistent code patterns in your project
2. Standardize on preferred approaches
3. Remove or refactor legacy code that confuses Copilot
4. Add clear examples of preferred patterns
```

**3. Use Explicit Pattern References:**
```
@workspace Follow exactly the same pattern as ProductService.cs but for Categories:
- Same constructor pattern
- Same method signatures
- Same error handling approach
- Same logging format
- Same async/await usage
```

### Issue: Generated Code Lacks Error Handling

**Symptoms:**
- Missing try-catch blocks
- No validation logic
- Unhandled edge cases

**Solutions:**

**1. Always Request Error Handling:**
```
@workspace Create CategoryService.CreateAsync with comprehensive error handling:
- Input validation with custom exceptions
- Database constraint violation handling
- Logging for all error scenarios
- Proper exception propagation
- Graceful degradation where appropriate
```

**2. Provide Error Handling Examples:**
```
@workspace Follow this error handling pattern from ProductService:

try
{
    ValidateInput(dto);
    var entity = CreateEntity(dto);
    await _repository.SaveAsync(entity);
    return MapToDto(entity);
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed for {Operation}", nameof(CreateAsync));
    throw;
}
catch (DuplicateException ex)
{
    _logger.LogWarning(ex, "Duplicate entity in {Operation}", nameof(CreateAsync));
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in {Operation}", nameof(CreateAsync));
    throw new ServiceException("Operation failed", ex);
}
```

### Issue: Code Doesn't Follow Clean Architecture

**Symptoms:**
- Domain layer has infrastructure dependencies
- Controllers contain business logic
- Repositories have business rules

**Solutions:**

**1. Specify Layer Context Explicitly:**
```
@workspace I'm working in the DOMAIN layer of Clean Architecture. 
Create Category entity that:
- Has NO dependencies on other layers
- Contains business rules and invariants
- Uses private setters for encapsulation
- Has factory methods for creation
- Raises domain events when appropriate
```

**2. Review and Enforce Boundaries:**
```
@workspace Review this code for Clean Architecture violations:

[paste code here]

Check for:
- Domain depending on Infrastructure
- Application containing business logic
- Infrastructure containing business rules
- Presentation handling domain concerns

Suggest refactoring to fix any violations.
```

## 🧠 Context Problems

### Issue: Copilot Doesn't Understand Project Structure

**Symptoms:**
- Generated code doesn't match project patterns
- Wrong namespaces or imports
- Incorrect file placement suggestions

**Solutions:**

**1. Optimize File Organization:**
```
Keep these files open when working:
✅ Target file you're editing
✅ Related interface/base class
✅ Similar implementation for reference
✅ Test file for the component
✅ Configuration files if relevant

Close unrelated files to reduce noise.
```

**2. Provide Explicit Structure Context:**
```
@workspace Project structure context:
- Domain layer: /backend/src/DemoInventory.Domain
- Application layer: /backend/src/DemoInventory.Application  
- Infrastructure: /backend/src/DemoInventory.Infrastructure
- API layer: /backend/src/DemoInventory.API
- Tests: /backend/tests/

I'm creating CategoryService in Application layer, following the pattern of ProductService.
```

**3. Use Workspace References:**
```
@workspace Analyze the existing project structure and patterns:
1. How are services organized in the Application layer?
2. What's the naming convention for interfaces?
3. How is dependency injection configured?
4. What's the standard error handling approach?

Then create CategoryService following these discovered patterns.
```

### Issue: Generated Code Uses Wrong Dependencies

**Symptoms:**
- Incorrect NuGet packages referenced
- Wrong framework versions
- Deprecated libraries used

**Solutions:**

**1. Specify Technology Stack:**
```
@workspace Technology stack context:
- .NET 9 with C# 12
- Entity Framework Core 9.0
- PostgreSQL with Npgsql
- xUnit for testing
- NSubstitute for mocking
- FluentAssertions for test assertions
- Serilog for logging

Create CategoryRepository using these specific technologies.
```

**2. Reference Project Files:**
```
@workspace Check the existing .csproj files to understand our dependencies:
- Look at DemoInventory.Application.csproj
- Check DemoInventory.Infrastructure.csproj  
- Review test project dependencies

Use only the packages already referenced in the project.
```

## ⚡ Performance Issues

### Issue: Copilot Responses Are Slow

**Symptoms:**
- Long delays for suggestions
- Timeouts in chat responses
- Incomplete code generation

**Solutions:**

**1. Optimize Context Size:**
```
# Reduce context noise
1. Close unnecessary files and tabs
2. Clear output/terminal windows
3. Minimize open extensions
4. Use specific rather than broad prompts
```

**2. Break Down Complex Requests:**
```
❌ Complex request:
"Create complete Category feature with entity, service, repository, controller, tests, and documentation"

✅ Broken down:
Step 1: "Create Category domain entity with business rules"
Step 2: "Create ICategoryService interface"  
Step 3: "Implement CategoryService"
Step 4: "Create CategoryRepository"
... and so on
```

**3. Use More Specific Prompts:**
```
❌ Vague: "Fix the performance issue"
✅ Specific: "Optimize this LINQ query to avoid N+1 problem by using Include() for related data"
```

### Issue: Generated Code Has Performance Problems

**Symptoms:**
- N+1 query problems
- Inefficient algorithms
- Memory leaks

**Solutions:**

**1. Request Performance-Optimized Code:**
```
@workspace Create CategoryService.GetCategoriesWithProductCountAsync optimized for performance:
- Avoid N+1 queries using proper joins
- Use projection to select only needed data
- Implement pagination for large datasets
- Add appropriate database indexes
- Include query execution time logging
```

**2. Review for Common Performance Issues:**
```
@workspace Review this code for performance issues:

[paste code]

Check for:
- N+1 query problems
- Inefficient LINQ operations
- Unnecessary data loading
- Missing async/await
- Resource disposal issues
- Memory allocation patterns

Suggest optimizations with explanations.
```

## 🔒 Security Concerns

### Issue: Generated Code Has Security Vulnerabilities

**Symptoms:**
- SQL injection possibilities
- Missing input validation
- Exposed sensitive data

**Solutions:**

**1. Always Request Security Considerations:**
```
@workspace Create CategoryController.Search with security focus:
- Prevent SQL injection using parameterized queries
- Validate and sanitize all inputs
- Implement rate limiting
- Add authentication/authorization checks
- Ensure no sensitive data in error messages
- Add security headers
- Implement audit logging
```

**2. Security Code Review Checklist:**
```
@workspace Review this code for security vulnerabilities:

[paste code]

Check for:
- SQL injection risks
- Cross-site scripting (XSS) vulnerabilities  
- Missing authentication/authorization
- Sensitive data exposure
- Input validation gaps
- Error message information disclosure
- Missing rate limiting
- Insecure dependencies

Provide secure alternatives for any issues found.
```

### Issue: Sensitive Data in Generated Code

**Symptoms:**
- Hardcoded passwords or API keys
- Personal information in examples
- Production data in code

**Solutions:**

**1. Use Placeholder Data:**
```
@workspace Generate example code using placeholder data:
- Use "example.com" for domains
- Use "user@example.com" for emails
- Use "YOUR_API_KEY" for API keys
- Use "localhost" for database connections
- Use generic names like "John Doe" for people
```

**2. Configuration Management:**
```
@workspace Show how to properly handle sensitive configuration:
- Use appsettings.json with environment variables
- Implement proper secrets management
- Use Azure Key Vault or similar services
- Show development vs production configurations
- Never hardcode sensitive values
```

## 👥 Team Integration

### Issue: Inconsistent Code Across Team

**Symptoms:**
- Different coding styles from AI
- Conflicting architectural approaches
- Inconsistent error handling

**Solutions:**

**1. Create Team Prompt Standards:**
```markdown
# Team Copilot Standards

## Standard Context Template
@workspace In our Clean Architecture .NET microservice project:
- Follow patterns from [ReferenceService].cs
- Use our standard error handling approach
- Include comprehensive logging
- Add unit tests with xUnit/NSubstitute/FluentAssertions
- Follow our naming conventions

## Standard Service Prompt
@workspace Create [Entity]Service in Application layer following our established patterns...

## Standard Repository Prompt  
@workspace Implement [Entity]Repository in Infrastructure layer following our repository patterns...
```

**2. Shared Prompt Library:**
```javascript
// Store in team documentation or shared tooling
const TEAM_PROMPTS = {
  createService: (entityName) => `
    @workspace Create ${entityName}Service in Application layer following our patterns:
    - Use ProductService.cs as reference
    - Include all CRUD operations
    - Add comprehensive error handling
    - Follow our async/await conventions
    - Include unit tests
  `,
  
  createEntity: (entityName) => `
    @workspace Create ${entityName} domain entity following DDD:
    - Private setters for encapsulation
    - Constructor validation
    - Business rule methods
    - No infrastructure dependencies
  `
};
```

### Issue: Code Review Challenges

**Symptoms:**
- Difficulty reviewing AI-generated code
- Unclear if code follows standards
- Missing context for review

**Solutions:**

**1. AI Code Review Process:**
```markdown
## AI-Generated Code Review Checklist

### Context Verification
- [ ] Prompt used was appropriate for the task
- [ ] Generated code follows team patterns
- [ ] Architecture principles are maintained

### Code Quality  
- [ ] Error handling is comprehensive
- [ ] Security considerations are addressed
- [ ] Performance is acceptable
- [ ] Tests are included and meaningful

### Integration
- [ ] Code integrates well with existing system
- [ ] Dependencies are appropriate
- [ ] Interfaces are correctly implemented
```

**2. Documentation Requirements:**
```markdown
When submitting AI-generated code:

1. Include the prompt(s) used
2. Explain any manual modifications made
3. Document any assumptions or limitations
4. Highlight areas needing special review attention
5. Include rationale for architectural decisions
```

## 🏗️ Architecture Violations

### Issue: Layer Boundary Violations

**Symptoms:**
- Domain layer importing Infrastructure
- Controllers with business logic
- Repositories with validation rules

**Solutions:**

**1. Architecture Validation Prompts:**
```
@workspace Validate this code for Clean Architecture compliance:

[paste code]

Check that:
- Domain has no external dependencies
- Application only depends on Domain
- Infrastructure implements Application interfaces
- Presentation only orchestrates, no business logic
- Dependencies flow inward toward Domain

Flag any violations and suggest fixes.
```

**2. Automated Architecture Testing:**
```csharp
// Add to your test suite
[Test]
public void Domain_Should_Not_Reference_Infrastructure()
{
    var domainAssembly = Assembly.GetAssembly(typeof(Category));
    var infrastructureAssembly = Assembly.GetAssembly(typeof(CategoryRepository));
    
    var domainTypes = domainAssembly.GetTypes();
    var infrastructureTypes = infrastructureAssembly.GetTypes();
    
    foreach (var domainType in domainTypes)
    {
        var dependencies = domainType.GetReferencedAssemblies();
        dependencies.Should().NotContain(infrastructureAssembly.GetName());
    }
}
```

### Issue: SOLID Principle Violations

**Symptoms:**
- Classes with multiple responsibilities
- Tight coupling between components
- Difficulty testing code

**Solutions:**

**1. SOLID Validation Requests:**
```
@workspace Review this class for SOLID principle violations:

[paste code]

Check for:
- Single Responsibility: Does class have one reason to change?
- Open/Closed: Can behavior be extended without modification?
- Liskov Substitution: Can subclasses replace base classes?
- Interface Segregation: Are interfaces focused and minimal?
- Dependency Inversion: Does class depend on abstractions?

Suggest refactoring to fix violations.
```

## 🧪 Testing Problems

### Issue: Generated Tests Are Insufficient

**Symptoms:**
- Only happy path tests
- Missing edge cases
- Poor test coverage

**Solutions:**

**1. Comprehensive Test Requests:**
```
@workspace Generate comprehensive tests for CategoryService.CreateAsync including:

Happy Path:
- Valid input creates category successfully
- Returns correct DTO mapping

Edge Cases:
- Empty/null name validation
- Name length boundary testing (99, 100, 101 characters)
- Special characters in names
- Unicode character handling

Error Scenarios:
- Duplicate name handling
- Database constraint violations
- Repository exceptions
- Validation failures

Concurrency:
- Multiple simultaneous creation attempts
- Race condition handling

Use xUnit, NSubstitute for mocking, FluentAssertions for assertions.
```

**2. Test Quality Review:**
```
@workspace Review these tests for quality and completeness:

[paste test code]

Check for:
- Test method naming clarity
- Arrange-Act-Assert pattern
- Appropriate mocking strategies  
- Edge case coverage
- Error scenario testing
- Test independence and isolation
- Performance test considerations

Suggest improvements and missing test cases.
```

### Issue: Tests Are Flaky or Unreliable

**Symptoms:**
- Tests pass/fail intermittently
- Time-dependent test failures
- Order-dependent test results

**Solutions:**

**1. Test Reliability Fixes:**
```
@workspace Fix these flaky tests to be more reliable:

[paste flaky test code]

Address:
- Remove time dependencies with fixed test dates
- Ensure proper test isolation and cleanup
- Fix race conditions in async tests
- Remove order dependencies between tests
- Use deterministic test data
- Properly mock external dependencies
```

## 🔍 Diagnostic Tools and Commands

### VS Code Diagnostic Commands

```
# Check Copilot status
Ctrl+Shift+P -> "GitHub Copilot: Check Status"

# View Copilot logs
Ctrl+Shift+P -> "Developer: Show Logs" -> Select "GitHub Copilot"

# Restart Copilot
Ctrl+Shift+P -> "GitHub Copilot: Restart"

# Toggle Copilot
Ctrl+Shift+P -> "GitHub Copilot: Toggle"
```

### Network Diagnostics

```bash
# Test GitHub connectivity
curl -I https://github.com

# Test API access (requires token)
curl -H "Authorization: token YOUR_TOKEN" https://api.github.com/user

# Check DNS resolution
nslookup github.com
```

### Extension Diagnostics

```bash
# List installed extensions
code --list-extensions

# Check extension logs
# Windows: %APPDATA%\Code\logs
# macOS: ~/Library/Application Support/Code/logs
# Linux: ~/.config/Code/logs
```

## 📞 Getting Further Help

### When to Escalate

1. **Technical Issues**: Extension not working after trying all solutions
2. **Policy Issues**: Organization or repository access problems
3. **Billing Issues**: Subscription or payment problems
4. **Security Concerns**: Potential security vulnerabilities or data exposure

### Support Channels

1. **GitHub Copilot Support**: https://support.github.com
2. **VS Code Issues**: https://github.com/microsoft/vscode/issues
3. **Community Forums**: GitHub Community Discussions
4. **Team Lead/Admin**: For organization policy issues

### Documentation Resources

- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [VS Code Copilot Extension](https://code.visualstudio.com/docs/copilot/overview)
- [Project-Specific Guidelines](../README.md)

## 🎯 Prevention Strategies

### Proactive Measures

1. **Regular Updates**: Keep extensions updated
2. **Team Training**: Regular team sessions on effective Copilot usage  
3. **Documentation**: Maintain team standards and patterns
4. **Code Reviews**: Implement AI-aware review processes
5. **Monitoring**: Track code quality metrics over time

### Quality Assurance

1. **Automated Testing**: Comprehensive test suites
2. **Static Analysis**: Code quality tools and linters
3. **Security Scanning**: Regular vulnerability assessments
4. **Architecture Testing**: Automated boundary validation
5. **Performance Monitoring**: Track system performance

---

## 📋 Quick Reference Checklist

When encountering issues:

- [ ] Check Copilot extension status and authentication
- [ ] Verify network connectivity to GitHub
- [ ] Review prompt quality and context provision
- [ ] Check for conflicting code patterns in project
- [ ] Validate generated code for security and quality
- [ ] Ensure architectural boundaries are maintained
- [ ] Test thoroughly before committing
- [ ] Document issues and solutions for team

Remember: Most Copilot issues stem from insufficient context or unclear requirements. When in doubt, provide more specific context and examples of desired patterns.

**Need immediate help?** Check the [Quick Problem Finder](#quick-problem-finder) at the top of this guide for fast solutions to common issues.