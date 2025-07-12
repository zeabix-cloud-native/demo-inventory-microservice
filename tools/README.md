# Validation Tools

This directory contains automated code validation tools for the Demo Inventory Microservice project.

## Quick Start

The easiest way to run all validations is using the helper script:

```bash
# Run all validations on current directory
./tools/run-validation.sh

# Run with verbose output
./tools/run-validation.sh --verbose

# Validate specific path
./tools/run-validation.sh --path backend/src

# Generate JSON report
./tools/run-validation.sh --report validation-report.json

# Skip specific validations
./tools/run-validation.sh --skip security,architecture
```

## Available Tools

### 1. Static Analysis (`DemoInventory.Tools.StaticAnalysis`)
- Analyzes code quality and complexity
- Checks naming conventions
- Validates documentation requirements
- Identifies SOLID principle violations

### 2. Security Scan (`DemoInventory.Tools.SecurityScan`)
- Detects hardcoded secrets
- Identifies SQL injection vulnerabilities
- Validates authentication/authorization
- Checks input validation patterns

### 3. Architecture Validation (`DemoInventory.Tools.ArchitectureValidation`)
- Enforces Clean Architecture boundaries
- Validates layer dependencies
- Checks domain purity
- Ensures proper design patterns

### 4. AI Code Validator (`DemoInventory.Tools.AICodeValidator`)
- Orchestrates all validation tools
- Provides comprehensive reporting
- Generates JSON reports for CI/CD
- Offers intelligent recommendations

## Manual Usage

Each tool can be run individually:

```bash
# Static Analysis
dotnet run --project tools/DemoInventory.Tools.StaticAnalysis -- --path "backend/src" --verbose

# Security Scan
dotnet run --project tools/DemoInventory.Tools.SecurityScan -- --path "backend/src" --verbose

# Architecture Validation
dotnet run --project tools/DemoInventory.Tools.ArchitectureValidation -- --path "backend/src" --verbose

# AI Code Validator (all tools)
dotnet run --project tools/DemoInventory.Tools.AICodeValidator -- --path "." --verbose --report "report.json"
```

## Integration

### CI/CD Pipeline
Add to your GitHub Actions workflow:

```yaml
- name: Run Code Validation
  run: ./tools/run-validation.sh --report validation-report.json
```

### Pre-commit Hook
Create `.git/hooks/pre-commit`:

```bash
#!/bin/sh
./tools/run-validation.sh --path "."
```

## Testing

Run the validation tool tests:

```bash
dotnet test tools/Tests/
```

## Documentation

For detailed information, see:
- [Automated Code Verification Guide](../docs/automated-code-verification.md)
- [Code Verification Manual](../docs/copilot/workshop/code-verification.md)

## Development

To modify or extend the validation tools:

1. Each tool is a separate .NET console application
2. Follow the existing patterns for consistency
3. Add tests in the `Tests` project
4. Update documentation as needed

## Support

For issues or questions about the validation tools, please refer to the comprehensive documentation or create an issue in the repository.