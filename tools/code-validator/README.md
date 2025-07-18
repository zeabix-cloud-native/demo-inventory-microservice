# Code Validator - Standalone .NET Clean Architecture Validation Toolkit

This is a **standalone, flexible code validation toolkit** designed to work with any .NET Clean Architecture project. It provides comprehensive code quality validation with **severity-specific recommendations**.

## 🚀 Quick Start

The easiest way to run all validations is using the helper script:

```bash
# Run all validations on current directory
./tools/code-validator/run-validation.sh

# Run with verbose output (recommended for detailed severity analysis)
./tools/code-validator/run-validation.sh --verbose

# Validate specific path
./tools/code-validator/run-validation.sh --path backend/src

# Generate comprehensive JSON report with severity metrics
./tools/code-validator/run-validation.sh --report validation-report.json

# Generate CTRF format report for CI/CD integration
./tools/code-validator/run-validation.sh --ctrf security-results.json

# Skip specific validations
./tools/code-validator/run-validation.sh --skip security,architecture

# Combine multiple output formats for comprehensive analysis
./tools/code-validator/run-validation.sh --verbose --report full-report.json --ctrf ci-report.json
```

## ✨ Features

### 🔧 **Standalone & Flexible**
- **No hardcoded dependencies** on specific project structures
- **Works with any .NET Clean Architecture project**
- **Easy to integrate** into existing projects
- **Self-contained** - all dependencies managed internally

### 🎯 **Comprehensive Validation**
- **Static Analysis** - Code quality, complexity, and maintainability
- **Security Scanning** - OWASP compliance and vulnerability detection
- **Architecture Validation** - Clean Architecture principle enforcement
- **Multiple Output Formats** - JSON reports and CTRF for CI/CD

### 📊 **Severity-Based Recommendations**
All validation tools provide **targeted recommendations for each severity level**:

- 🔴 **CRITICAL**: Immediate action required, deployment blocked
- 🟠 **HIGH**: Address within 24-48 hours, prioritize over features  
- 🟡 **MEDIUM**: Plan for current sprint, safe to deploy
- 🔵 **LOW**: Technical debt, address during maintenance

## 📁 Structure

```
tools/code-validator/
├── AICodeValidator/          # Main orchestrator tool
├── StaticAnalysis/          # Code quality and complexity analysis
├── SecurityScan/            # Security vulnerability detection
├── ArchitectureValidation/  # Clean Architecture compliance
├── Tests/                   # Integration tests
├── run-validation.sh        # Standalone runner script
└── README.md               # This file
```

## 🛠️ Installation & Setup

### Option 1: Copy to Your Project
1. Copy the entire `tools/code-validator/` directory to your .NET project
2. Run `./tools/code-validator/run-validation.sh` from your project root

### Option 2: Clone and Use Standalone
1. Clone this repository
2. Navigate to `tools/code-validator/`
3. Run `./run-validation.sh --path /path/to/your/project`

### Prerequisites
- .NET 8.0 SDK or later
- Bash (for the runner script)

## 🔍 Available Tools

### 1. Static Analysis (`StaticAnalysis`)
**Enhanced with severity categorization:**
- Analyzes code quality and complexity with **4-tier severity levels**
- Checks naming conventions with **priority-based recommendations**
- Validates documentation requirements with **public API focus**
- Identifies SOLID principle violations with **urgency indicators**

**Severity Examples:**
- 🔴 **Critical**: 20+ methods in class (SRP violation)
- 🟠 **High**: Cyclomatic complexity >15, 100+ line methods
- 🟡 **Medium**: Missing XML documentation on public APIs
- 🔵 **Low**: Naming convention inconsistencies

### 2. Security Scan (`SecurityScan`)
**Enhanced with comprehensive security recommendations:**
- Detects hardcoded secrets with **immediate action plans**
- Identifies SQL injection vulnerabilities with **fix strategies**
- Validates authentication/authorization with **implementation guidance**
- Checks input validation patterns with **OWASP compliance**

**Severity Examples:**
- 🔴 **Critical**: SQL injection vulnerabilities, hardcoded secrets
- 🟠 **High**: Sensitive data in logs, missing authorization
- 🟡 **Medium**: Missing input validation, unprotected endpoints
- 🔵 **Low**: Resource management improvements, DateTime usage

### 3. Architecture Validation (`ArchitectureValidation`)
- Enforces Clean Architecture boundaries with **violation severity**
- Validates layer dependencies with **architectural impact assessment**
- Checks domain purity with **immediate escalation for violations**
- Ensures proper design patterns with **refactoring guidance**

### 4. AI Code Validator (`AICodeValidator`)
**Significantly enhanced orchestrator:**
- Orchestrates all validation tools with **severity-aware reporting**
- Provides comprehensive reporting with **priority-based timelines**
- Generates enhanced JSON reports with **detailed metrics**
- Offers **intelligent, actionable recommendations** per severity level

## 📝 Usage Examples

### Basic Usage
```bash
# Run from project root (where the tools/code-validator/ directory exists)
./tools/code-validator/run-validation.sh

# Run from within the code-validator directory
cd tools/code-validator
./run-validation.sh --path ../../backend/src
```

### Advanced Usage
```bash
# Comprehensive analysis with multiple outputs
./tools/code-validator/run-validation.sh \
  --path backend/src \
  --verbose \
  --report detailed-analysis.json \
  --ctrf ci-integration.json

# Security-focused scan only
./tools/code-validator/run-validation.sh \
  --skip static,architecture \
  --verbose \
  --ctrf security-only.json

# Architecture validation only
./tools/code-validator/run-validation.sh \
  --skip static,security \
  --path backend/src \
  --verbose
```

### Individual Tool Usage
```bash
# Run specific tools individually
dotnet run --project tools/code-validator/StaticAnalysis -- --path backend/src --verbose
dotnet run --project tools/code-validator/SecurityScan -- --path backend/src --ctrf security.json
dotnet run --project tools/code-validator/ArchitectureValidation -- --path backend/src --verbose
```

## 🔧 CI/CD Integration

### GitHub Actions Example
```yaml
name: Code Quality Validation

on: [push, pull_request]

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Run Code Validation
      run: ./tools/code-validator/run-validation.sh --report validation-report.json --ctrf security-ctrf.json --verbose
    
    - name: Check Critical Issues
      run: |
        if grep -q '"critical":' validation-report.json && ! grep -q '"critical": 0' validation-report.json; then
          echo "CRITICAL issues found - blocking deployment"
          exit 1
        fi
    
    - name: Upload Reports
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: validation-reports
        path: |
          validation-report.json
          security-ctrf.json
```

### Jenkins Pipeline Example
```groovy
pipeline {
    agent any
    stages {
        stage('Code Validation') {
            steps {
                sh './tools/code-validator/run-validation.sh --report validation-report.json --verbose'
                
                script {
                    def report = readJSON file: 'validation-report.json'
                    if (report.summary.criticalIssues > 0) {
                        error("Critical issues found - deployment blocked")
                    }
                }
            }
            post {
                always {
                    archiveArtifacts artifacts: 'validation-report.json', fingerprint: true
                }
            }
        }
    }
}
```

## 📊 Output Formats

### 1. Console Output
Real-time progress with severity-coded emojis and actionable recommendations.

### 2. Comprehensive JSON Report (`--report`)
Detailed analysis with metrics and recommendations:

```json
{
  "timestamp": "2025-07-18T10:42:23Z",
  "summary": {
    "totalTools": 3,
    "passedTools": 3,
    "failedTools": 0,
    "totalDuration": 7740.5,
    "criticalIssues": 0,
    "highIssues": 0,
    "mediumIssues": 0,
    "lowIssues": 10
  },
  "results": [
    {
      "validationName": "Static Analysis",
      "success": true,
      "duration": 3543.0,
      "metrics": [
        "Critical: 0",
        "High: 0", 
        "Medium: 0",
        "Low: 10"
      ]
    }
  ]
}
```

### 3. CTRF Security Report (`--ctrf`)
Industry-standard format for CI/CD integration:

```json
{
  "results": {
    "tool": {
      "name": "CodeValidator.SecurityScan",
      "version": "1.0.0"
    },
    "summary": {
      "tests": 25,
      "passed": 25,
      "failed": 0,
      "pending": 0,
      "skipped": 0
    },
    "tests": [
      {
        "name": "SQL Injection Check",
        "status": "passed",
        "duration": 0
      }
    ]
  }
}
```

## 🎯 Severity Guidelines

### 🔴 CRITICAL (Immediate Action)
- **Timeline**: Fix within hours, before any deployment
- **Resources**: All hands on deck, escalate to tech lead/architect  
- **Deployment**: BLOCKED until all critical issues resolved
- **Examples**: SQL injection, architectural violations, hardcoded secrets

### 🟠 HIGH (24-48 Hours)
- **Timeline**: Resolve this sprint, prioritize over new features
- **Resources**: Senior developer review, pair programming recommended
- **Deployment**: Can deploy with monitoring, fix before next release
- **Examples**: High complexity, authorization missing, sensitive data exposure

### 🟡 MEDIUM (Current Sprint)
- **Timeline**: Address within current or next sprint
- **Resources**: Regular development workflow
- **Deployment**: Safe to deploy with planned fixes
- **Examples**: Missing documentation, input validation, moderate complexity

### 🔵 LOW (Technical Debt)
- **Timeline**: Address during maintenance windows
- **Resources**: Junior developers, code review process
- **Deployment**: No deployment blockers
- **Examples**: Naming conventions, minor improvements, code hygiene

## 🧪 Testing

Run the validation tool tests:

```bash
cd tools/code-validator
dotnet test Tests/
```

## 🔧 Development & Customization

### Adding Custom Rules
Each tool can be extended with custom rules by modifying the respective Program.cs files:

- **StaticAnalysis/Program.cs** - Add code quality rules
- **SecurityScan/Program.cs** - Add security patterns
- **ArchitectureValidation/Program.cs** - Add architecture rules

### Building from Source
```bash
cd tools/code-validator
dotnet build
```

### Project Structure
Each tool is a separate .NET console application with:
- **No external project dependencies** (fully standalone)
- **Microsoft.CodeAnalysis** for C# code parsing
- **System.CommandLine** for CLI interface
- **Severity-aware analysis** with actionable recommendations

## 🤝 Contributing

1. Each tool follows the same pattern for consistency
2. Use the **IssueSeverity/SecurityLevel** patterns for severity classification
3. Add **actionable, severity-specific recommendations** for each issue type
4. Update tests to validate new functionality
5. Follow the existing code style and conventions

## 🔗 Integration Examples

### Pre-commit Hook
Create `.git/hooks/pre-commit`:

```bash
#!/bin/sh
./tools/code-validator/run-validation.sh --path "."
EXIT_CODE=$?

if [ $EXIT_CODE -ne 0 ]; then
    echo ""
    echo "🔴 Code quality issues found - commit blocked"
    echo "   └─ Run with --verbose for detailed recommendations"
    exit 1
fi

echo "✅ Code quality validation passed - commit allowed"
```

### Docker Integration
```dockerfile
# Add to your Dockerfile
COPY tools/code-validator/ /app/tools/code-validator/
RUN cd /app && ./tools/code-validator/run-validation.sh --path src/
```

## 📄 License

This code validation toolkit is designed to be used freely in any .NET Clean Architecture project. See the main project license for details.

## 🆘 Support

For issues or questions about the code validation tools:
1. Check the verbose output for detailed guidance: `./run-validation.sh --verbose`
2. Review the comprehensive JSON report for implementation timelines
3. Refer to OWASP guidelines for security-related questions
4. Create an issue in the repository for bugs or feature requests

---

**🎉 Ready to validate your code? Run `./tools/code-validator/run-validation.sh --help` to get started!**