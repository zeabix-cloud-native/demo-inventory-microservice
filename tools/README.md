# Validation Tools

This directory contains automated code validation tools that have been **refactored into a standalone, flexible toolkit** located in the `code-validator/` subdirectory.

## 🚀 New Standalone Structure

The validation tools have been **refactored to be more flexible and reusable** across any .NET Clean Architecture project:

```
tools/
├── code-validator/          # ✨ NEW: Standalone validation toolkit
│   ├── AICodeValidator/     # Main orchestrator
│   ├── StaticAnalysis/      # Code quality analysis  
│   ├── SecurityScan/        # Security vulnerability detection
│   ├── ArchitectureValidation/ # Clean Architecture compliance
│   ├── Tests/              # Integration tests
│   ├── run-validation.sh   # Standalone runner script
│   └── README.md          # Comprehensive documentation
├── README.md              # This file
└── run-validation.sh      # Legacy wrapper (points to new location)
```

## ⚡ Quick Start

Use the **new standalone code-validator**:

```bash
# Run all validations (recommended)
./tools/code-validator/run-validation.sh

# Run with verbose output for detailed analysis
./tools/code-validator/run-validation.sh --verbose

# Validate specific path with comprehensive reporting
./tools/code-validator/run-validation.sh --path backend/src --report analysis.json

# Generate CI/CD compatible reports
./tools/code-validator/run-validation.sh --ctrf security-results.json --verbose
```

## 🆕 What's New?

### ✅ **Standalone & Portable**
- **No hardcoded dependencies** on Demo Inventory project structure
- **Works with any .NET Clean Architecture project**
- **Self-contained** - can be copied to any project
- **Dynamic project discovery** instead of hardcoded paths

### ✅ **Enhanced Flexibility**
- **Configurable validation paths** - validate any directory structure
- **Modular design** - skip specific validations as needed
- **Multiple output formats** - console, JSON, CTRF for CI/CD
- **Independent execution** - each tool can run standalone

### ✅ **Improved Developer Experience**
- **Comprehensive help system** with usage examples
- **Better error messages** and actionable recommendations
- **CI/CD integration examples** for GitHub Actions, Jenkins, etc.
- **Pre-commit hook templates** for automated quality gates

## 📖 Migration Guide

### From Old Structure
If you were using the old tools directly:

```bash
# OLD (deprecated)
dotnet run --project tools/DemoInventory.Tools.AICodeValidator

# NEW (recommended)
./tools/code-validator/run-validation.sh
```

### For CI/CD Pipelines
Update your CI/CD scripts:

```yaml
# OLD
- run: ./tools/run-validation.sh

# NEW  
- run: ./tools/code-validator/run-validation.sh --report validation.json --ctrf security.json
```

## 🎯 Key Benefits

### 🔧 **Reusability**
The code-validator can now be:
- **Copied to any .NET Clean Architecture project**
- **Used as a Git submodule** for shared validation rules
- **Customized** for specific project needs
- **Distributed** as a standalone toolkit

### 📊 **Enhanced Reporting**
- **Severity-based recommendations** (Critical, High, Medium, Low)
- **Multiple output formats** for different use cases
- **Actionable guidance** with implementation timelines
- **CI/CD integration** with industry-standard formats

### 🚀 **Better Integration**
- **GitHub Actions examples** with quality gates
- **Jenkins pipeline templates** for enterprise environments
- **Pre-commit hooks** for early feedback
- **Docker integration** for containerized validation

## 🔧 Legacy Compatibility

For backward compatibility, the old `run-validation.sh` script still works but now delegates to the new code-validator:

```bash
# This still works (but use the new path directly for best experience)
./tools/run-validation.sh --verbose
```

## 📚 Documentation

For comprehensive documentation, usage examples, and integration guides, see:

**📖 [Code Validator Documentation](code-validator/README.md)**

Key sections include:
- **Installation & Setup** - How to integrate into your project
- **Usage Examples** - Common validation scenarios  
- **CI/CD Integration** - GitHub Actions, Jenkins examples
- **Severity Guidelines** - Understanding issue priorities
- **Development Guide** - Customizing rules and patterns

## 🔗 Quick Links

- **[📋 Code Validator README](code-validator/README.md)** - Complete documentation
- **[🚀 Quick Start Guide](code-validator/README.md#-quick-start)** - Get running in 2 minutes
- **[🔧 CI/CD Integration](code-validator/README.md#-cicd-integration)** - Automate quality gates
- **[📊 Output Formats](code-validator/README.md#-output-formats)** - Reports and metrics

## 🆘 Support

For questions about the validation tools:
1. **Check the help**: `./tools/code-validator/run-validation.sh --help`
2. **Read the docs**: [code-validator/README.md](code-validator/README.md)
3. **Run with verbose**: `--verbose` flag for detailed guidance
4. **Review examples**: See the comprehensive documentation for CI/CD integration

---

**🎉 Ready to validate? Start with: `./tools/code-validator/run-validation.sh --verbose`**