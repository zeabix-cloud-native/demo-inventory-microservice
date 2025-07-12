# Automated Code Verification

This document describes the automated validation tools created for the Demo Inventory Microservice project, implementing the verification concepts outlined in [docs/copilot/workshop/code-verification.md](../copilot/workshop/code-verification.md).

## 🎯 Overview

The automated validation tools provide comprehensive code quality checks for AI-generated and human-written code, covering:

- **Static Analysis**: Code quality, complexity, and best practices
- **Security Scanning**: Vulnerability detection and security patterns
- **Architecture Validation**: Clean Architecture compliance and boundary enforcement
- **Comprehensive Orchestration**: Combined validation with reporting

## 🛠️ Available Tools

### 1. Static Analysis Tool
**Location**: `tools/DemoInventory.Tools.StaticAnalysis`

Validates code quality and adherence to best practices:

#### Features
- **Cyclomatic Complexity Analysis**: Identifies overly complex methods (threshold: ≤ 10)
- **Method Length Validation**: Ensures methods stay focused (threshold: ≤ 50 lines)
- **Parameter Count Checking**: Prevents too many parameters (threshold: ≤ 7)
- **Nesting Depth Analysis**: Maintains readability (threshold: ≤ 3 levels)
- **Naming Convention Validation**: Enforces C# naming standards
- **Documentation Requirements**: Checks for XML documentation on public APIs
- **SOLID Principle Compliance**: Identifies Single Responsibility violations

#### Usage
```bash
# Analyze current directory
dotnet run --project tools/DemoInventory.Tools.StaticAnalysis

# Analyze specific path
dotnet run --project tools/DemoInventory.Tools.StaticAnalysis -- --path "backend/src"

# Verbose output
dotnet run --project tools/DemoInventory.Tools.StaticAnalysis -- --path "backend/src" --verbose
```

#### Output Example
```
🔍 Starting static analysis on: backend/src
Found 45 C# files to analyze

📊 Analysis Summary
==================
Files analyzed: 45
Files with issues: 12
Total issues found: 28

📋 Issues by category:
  Complexity: 8
  Naming: 5
  Documentation: 10
  Length: 3
  Parameters: 2
```

### 2. Security Scan Tool
**Location**: `tools/DemoInventory.Tools.SecurityScan`

Detects security vulnerabilities and anti-patterns:

#### Features
- **Hardcoded Secrets Detection**: Finds passwords, API keys, tokens, connection strings
- **SQL Injection Prevention**: Identifies string concatenation in SQL queries
- **Authentication/Authorization Checks**: Validates controller and action security
- **Input Validation Analysis**: Ensures proper parameter validation
- **Error Handling Security**: Prevents information disclosure
- **Logging Security**: Detects sensitive data in log statements
- **Resource Management**: Identifies improper HttpClient usage

#### Usage
```bash
# Scan current directory
dotnet run --project tools/DemoInventory.Tools.SecurityScan

# Scan specific path
dotnet run --project tools/DemoInventory.Tools.SecurityScan -- --path "backend/src"

# Verbose output with code snippets
dotnet run --project tools/DemoInventory.Tools.SecurityScan -- --path "backend/src" --verbose
```

#### Output Example
```
🔒 Security Scan Summary
========================
Critical: 2
High:     5
Medium:   8
Low:      3
Total:    18

📋 Issues by category:
  SQL Injection: 2
  Authorization: 5
  Input Validation: 8
  Logging Security: 3

🚨 Security Findings:
🔴 Critical - Controllers/ProductController.cs:45
   Potential SQL injection via string concatenation
```

### 3. Architecture Validation Tool
**Location**: `tools/DemoInventory.Tools.ArchitectureValidation`

Enforces Clean Architecture principles and boundaries:

#### Features
- **Layer Dependency Validation**: Ensures proper dependency flow
- **Domain Purity Checks**: Validates Domain layer has no external dependencies
- **Entity Pattern Validation**: Checks proper encapsulation in entities
- **Value Object Immutability**: Ensures value objects are immutable
- **Application Service Patterns**: Validates async patterns and orchestration
- **Repository Pattern Compliance**: Checks interface implementations
- **Controller Thickness**: Ensures controllers remain thin
- **SOLID Principle Enforcement**: Validates design principles

#### Layer Dependency Rules
```
API Layer ──────────┐
│                   ▼
├─── Application ───┤
│                   ▼
├─── Infrastructure │
│                   ▼
└─── Domain ────────┘
```

#### Usage
```bash
# Validate architecture (auto-detects backend/src)
dotnet run --project tools/DemoInventory.Tools.ArchitectureValidation

# Validate specific backend source path
dotnet run --project tools/DemoInventory.Tools.ArchitectureValidation -- --path "backend/src"

# Detailed violation reporting
dotnet run --project tools/DemoInventory.Tools.ArchitectureValidation -- --path "backend/src" --verbose
```

#### Output Example
```
🏛️  Architecture Validation Summary
===================================
Dependency violations:     3
Domain pattern violations: 2
Application violations:    5
Infrastructure violations: 1
API violations:           2
Total violations:         13

🚨 Architecture Violations:
🔗 DemoInventory.Application/Services/ProductService.cs:15
   Application layer should not depend on Infrastructure layer
```

### 4. AI Code Validator (Orchestrator)
**Location**: `tools/DemoInventory.Tools.AICodeValidator`

Comprehensive validation orchestrator that runs all tools and provides unified reporting:

#### Features
- **Multi-Tool Orchestration**: Runs all validation tools in sequence
- **Comprehensive Reporting**: Combines results from all tools
- **JSON Report Generation**: Creates structured reports for CI/CD integration
- **Performance Metrics**: Tracks execution time for each validation
- **Intelligent Recommendations**: Provides specific improvement suggestions
- **Selective Validation**: Allows skipping specific validation types

#### Usage
```bash
# Run all validations
dotnet run --project tools/DemoInventory.Tools.AICodeValidator

# Skip specific validations
dotnet run --project tools/DemoInventory.Tools.AICodeValidator -- --skip security architecture

# Generate JSON report
dotnet run --project tools/DemoInventory.Tools.AICodeValidator -- --report "validation-report.json"

# Comprehensive validation with verbose output
dotnet run --project tools/DemoInventory.Tools.AICodeValidator -- --path "." --verbose --report "report.json"
```

#### Output Example
```
🤖 AI Code Validator
====================
Validating: /home/user/project
Timestamp: 2024-07-12 14:30:15 UTC

🔍 Running Static Analysis...
🔒 Running Security Scan...
🏛️  Running Architecture Validation...

📊 Comprehensive Validation Summary
===================================
✅ Static Analysis - 1245ms
❌ Security Scan - 892ms
✅ Architecture Validation - 2103ms

Overall Status: ❌ FAILED
Tools Run: 3
Total Duration: 4240ms

💡 Recommendations
==================
🔧 Areas for improvement:

📌 Security Scan:
  • Move secrets to configuration files or environment variables
  • Add proper authorization attributes to controllers and actions
  • Use parameterized queries and input validation to prevent injection attacks
```

## 🚀 CI/CD Integration

### GitHub Actions Integration

Add the following step to your GitHub Actions workflow:

```yaml
- name: Run Code Validation
  run: |
    dotnet run --project tools/DemoInventory.Tools.AICodeValidator -- \
      --report "validation-report.json"
  
- name: Upload Validation Report
  uses: actions/upload-artifact@v3
  if: always()
  with:
    name: validation-report
    path: validation-report.json
```

### Pre-commit Hook

Create a git pre-commit hook to run validation before commits:

```bash
#!/bin/sh
echo "Running code validation..."
dotnet run --project tools/DemoInventory.Tools.AICodeValidator --path "."
exit $?
```

### Build Integration

Add to your build script or Makefile:

```makefile
validate:
	dotnet run --project tools/DemoInventory.Tools.AICodeValidator

validate-report:
	dotnet run --project tools/DemoInventory.Tools.AICodeValidator -- --report "validation-$(shell date +%Y%m%d-%H%M%S).json"
```

## 📊 Report Formats

### Console Output
Human-readable format with colored output, progress indicators, and detailed recommendations.

### JSON Report
Structured format for CI/CD integration and automated processing:

```json
{
  "timestamp": "2024-07-12T14:30:15.123Z",
  "summary": {
    "totalTools": 3,
    "passedTools": 2,
    "failedTools": 1,
    "totalDuration": 4240.5
  },
  "results": [
    {
      "validationName": "Static Analysis",
      "success": true,
      "duration": 1245.2,
      "metrics": [
        "Files analyzed: 45",
        "Total issues found: 12"
      ]
    }
  ]
}
```

## 🔧 Configuration

### Tool Configuration

Each tool can be configured through command-line arguments:

| Tool | Options | Description |
|------|---------|-------------|
| All Tools | `--path` | Directory or file to analyze |
| All Tools | `--verbose` | Enable detailed output |
| AI Validator | `--skip` | Skip specific validation types |
| AI Validator | `--report` | Generate JSON report file |

### Customization

To customize validation rules, modify the respective tool source code:

- **Static Analysis**: Update thresholds in `StaticAnalyzer.cs`
- **Security Scan**: Add/modify patterns in `SecurityScanner.cs`
- **Architecture**: Update layer rules in `ArchitectureValidator.cs`

## 📈 Metrics and Thresholds

### Static Analysis Thresholds
| Metric | Threshold | Recommendation |
|--------|-----------|----------------|
| Cyclomatic Complexity | ≤ 10 | Break down complex methods |
| Method Length | ≤ 50 lines | Keep methods focused |
| Parameter Count | ≤ 7 parameters | Use parameter objects |
| Nesting Depth | ≤ 3 levels | Extract nested logic |

### Security Severity Levels
| Level | Description | Action Required |
|-------|-------------|-----------------|
| Critical | Immediate security risk | Fix before deployment |
| High | Significant vulnerability | Fix within 24 hours |
| Medium | Potential security issue | Fix within 1 week |
| Low | Best practice violation | Fix when convenient |

### Architecture Compliance
| Layer | Allowed Dependencies | Forbidden Dependencies |
|-------|---------------------|------------------------|
| Domain | None | All external frameworks |
| Application | Domain | Infrastructure, Presentation |
| Infrastructure | Domain, Application | Presentation |
| API/Presentation | All layers | None |

## 🎯 Best Practices

### For AI-Generated Code
1. **Always run validation** after accepting AI-generated code
2. **Review the complete output**, not just pass/fail status
3. **Address critical and high-severity issues** immediately
4. **Use verbose mode** for detailed analysis of new code
5. **Generate reports** for audit trails and team reviews

### For Existing Codebases
1. **Run validation regularly** as part of CI/CD pipeline
2. **Set up pre-commit hooks** to catch issues early
3. **Monitor trends** in validation reports over time
4. **Gradually improve** code quality by addressing issues
5. **Document exceptions** for legitimate violations

### For Code Reviews
1. **Include validation reports** in pull request reviews
2. **Verify AI-generated code** meets all validation criteria
3. **Check for regression** in previously clean code
4. **Use metrics** to guide refactoring decisions
5. **Share knowledge** about common violations with the team

## 🔍 Troubleshooting

### Common Issues

**Tool fails to build:**
```bash
# Restore dependencies
dotnet restore tools/DemoInventory.Tools.StaticAnalysis
# Clean and rebuild
dotnet clean && dotnet build
```

**Permission denied:**
```bash
# On Linux/Mac, ensure executable permissions
chmod +x tools/scripts/run-validation.sh
```

**Memory issues with large codebases:**
```bash
# Run tools individually instead of orchestrator
dotnet run --project tools/DemoInventory.Tools.StaticAnalysis -- --path "specific-folder"
```

**False positives:**
Document legitimate exceptions and consider customizing thresholds.

## 📚 References

- [Code Verification Guide](../copilot/workshop/code-verification.md) - Comprehensive manual verification checklist
- [Clean Architecture Documentation](../ARCHITECTURE.md) - Project architecture overview
- [Microsoft Code Analysis](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis) - Roslyn API documentation
- [OWASP Security Guidelines](https://owasp.org/www-project-cheat-sheets/) - Security best practices
- [.NET Coding Standards](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/) - Microsoft coding conventions

---

**Next Steps:**
1. Integrate tools into your development workflow
2. Set up CI/CD pipeline integration
3. Train team members on using the validation tools
4. Establish code quality gates based on validation results
5. Continuously improve validation rules based on project needs