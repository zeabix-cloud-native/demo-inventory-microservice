# Git Flow Workflow Guide

## Overview

This project follows the **Git Flow** branching strategy to maintain a clean, predictable development workflow. Git Flow provides a robust framework for managing releases, features, and hotfixes in a collaborative environment.

## Branch Structure

### Main Branches

#### 🔒 `main` branch
- **Purpose**: Production-ready code
- **Protection**: Highly protected, no direct pushes
- **Merges from**: `release/*` and `hotfix/*` branches only
- **Triggers**: Production deployments, major releases
- **Auto-deploy**: Production environment

#### 🚧 `develop` branch  
- **Purpose**: Integration branch for features
- **Protection**: Protected, requires PR approval
- **Merges from**: `feature/*`, `release/*`, and `hotfix/*` branches
- **Triggers**: Development environment deployments
- **Auto-deploy**: Development environment

### Supporting Branches

#### 🌟 `feature/*` branches
- **Purpose**: New feature development
- **Naming**: `feature/description-of-feature`
- **Created from**: `develop`
- **Merged to**: `develop`
- **Lifetime**: Until feature is complete

**Examples:**
```bash
feature/add-product-categories
feature/implement-user-authentication
feature/enhance-search-functionality
```

#### 🚀 `release/*` branches
- **Purpose**: Release preparation and bug fixes
- **Naming**: `release/v1.2.0`
- **Created from**: `develop`
- **Merged to**: `main` and `develop`
- **Lifetime**: Until release is deployed

#### 🔥 `hotfix/*` branches
- **Purpose**: Critical production fixes
- **Naming**: `hotfix/fix-critical-issue`
- **Created from**: `main`
- **Merged to**: `main` and `develop`
- **Lifetime**: Until fix is deployed

## Workflow Processes

### 🌟 Feature Development Workflow

1. **Create Feature Branch**
```bash
# Ensure you're on develop and up to date
git checkout develop
git pull origin develop

# Create and switch to feature branch
git checkout -b feature/add-product-categories

# Or using git flow (if installed)
git flow feature start add-product-categories
```

2. **Develop Feature**
```bash
# Make your changes
# Commit regularly with meaningful messages
git add .
git commit -m "feat(products): add product category entity and repository"

# Push to remote regularly
git push origin feature/add-product-categories
```

3. **Submit Pull Request**
- Create PR from `feature/add-product-categories` to `develop`
- Use PR template and fill in all required information
- Ensure all CI checks pass
- Request code review from team members

4. **Merge and Cleanup**
```bash
# After PR approval and merge
git checkout develop
git pull origin develop
git branch -d feature/add-product-categories
git push origin --delete feature/add-product-categories

# Or using git flow
git flow feature finish add-product-categories
```

### 🚀 Release Workflow

1. **Create Release Branch**
```bash
# Create release branch from develop
git checkout develop
git pull origin develop
git checkout -b release/v1.2.0

# Or using git flow
git flow release start v1.2.0
```

2. **Prepare Release**
```bash
# Update version numbers
# Update CHANGELOG.md
# Fix any release-specific bugs
# Update documentation

git add .
git commit -m "chore(release): prepare v1.2.0 release"
git push origin release/v1.2.0
```

3. **Release Testing**
- Deploy to staging environment
- Run comprehensive tests
- Fix any release-blocking bugs
- Update documentation

4. **Complete Release**
```bash
# Create PR to main
# After approval, merge to main
git checkout main
git pull origin main
git merge --no-ff release/v1.2.0
git tag -a v1.2.0 -m "Release version 1.2.0"
git push origin main
git push origin v1.2.0

# Merge back to develop
git checkout develop
git merge --no-ff release/v1.2.0
git push origin develop

# Cleanup
git branch -d release/v1.2.0
git push origin --delete release/v1.2.0

# Or using git flow
git flow release finish v1.2.0
```

### 🔥 Hotfix Workflow

1. **Create Hotfix Branch**
```bash
# Create hotfix from main
git checkout main
git pull origin main
git checkout -b hotfix/fix-critical-security-issue

# Or using git flow
git flow hotfix start fix-critical-security-issue
```

2. **Fix Issue**
```bash
# Make the critical fix
git add .
git commit -m "fix(security): resolve critical authentication bypass"
git push origin hotfix/fix-critical-security-issue
```

3. **Complete Hotfix**
```bash
# Create PR to main (expedited review)
# After approval, merge to main
git checkout main
git merge --no-ff hotfix/fix-critical-security-issue
git tag -a v1.2.1 -m "Hotfix version 1.2.1"
git push origin main
git push origin v1.2.1

# Merge back to develop
git checkout develop
git merge --no-ff hotfix/fix-critical-security-issue
git push origin develop

# Cleanup
git branch -d hotfix/fix-critical-security-issue
git push origin --delete hotfix/fix-critical-security-issue

# Or using git flow
git flow hotfix finish fix-critical-security-issue
```

## Branching Rules and Conventions

### Branch Naming Conventions

| Branch Type | Format | Example |
|-------------|--------|---------|
| Feature | `feature/description-with-dashes` | `feature/add-product-categories` |
| Release | `release/vX.Y.Z` | `release/v1.2.0` |
| Hotfix | `hotfix/description-with-dashes` | `hotfix/fix-memory-leak` |
| Bugfix | `bugfix/description-with-dashes` | `bugfix/fix-validation-error` |

### Commit Message Conventions

Follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding missing tests
- `chore`: Maintenance tasks

**Examples:**
```bash
feat(products): add product category management
fix(api): resolve duplicate product search results
docs(readme): update installation instructions
chore(deps): update dependencies to latest versions
```

## Branch Protection Rules

### `main` Branch Protection
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- ✅ Require conversation resolution before merging
- ✅ Restrict pushes that create files larger than 100MB
- ✅ Do not allow bypassing the above settings
- ❌ Allow force pushes
- ❌ Allow deletions

### `develop` Branch Protection  
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- ✅ Restrict pushes that create files larger than 100MB
- ❌ Allow force pushes
- ❌ Allow deletions

### Required Status Checks
- ✅ `build-and-test` - Unit tests must pass
- ✅ `postman-tests` - API tests must pass  
- ✅ `cypress-tests` - E2E tests must pass
- ✅ `CodeQL` - Security analysis must pass

## Release Management

### Version Numbering

Follow [Semantic Versioning (SemVer)](https://semver.org/):
- **MAJOR** version: Incompatible API changes
- **MINOR** version: Backwards compatible functionality additions
- **PATCH** version: Backwards compatible bug fixes

### Release Schedule

- **Major releases**: Quarterly (every 3 months)
- **Minor releases**: Monthly or feature-driven
- **Patch releases**: As needed for bug fixes
- **Hotfixes**: Immediate for critical issues

### Pre-release Testing

1. **Feature Testing**: Each feature tested in feature branch
2. **Integration Testing**: All features tested together in develop
3. **Release Testing**: Comprehensive testing in release branch
4. **Staging Deployment**: Deploy release branch to staging
5. **User Acceptance Testing**: Stakeholder approval
6. **Production Deployment**: Deploy to production from main

## Git Flow Tools

### Installing Git Flow Extensions

```bash
# macOS (using Homebrew)
brew install git-flow

# Ubuntu/Debian
sudo apt-get install git-flow

# Initialize git flow in your repository
git flow init
```

### Git Flow Commands Quick Reference

```bash
# Feature workflow
git flow feature start <name>
git flow feature finish <name>
git flow feature publish <name>

# Release workflow  
git flow release start <version>
git flow release finish <version>

# Hotfix workflow
git flow hotfix start <name>
git flow hotfix finish <name>
```

## Integration with CI/CD

### Automated Deployments

| Branch | Environment | Trigger |
|--------|-------------|---------|
| `develop` | Development | Every push/merge |
| `release/*` | Staging | Every push |
| `main` | Production | Every merge + tag |

### Automated Testing

- **Feature branches**: Run unit tests only
- **develop**: Run unit + integration tests
- **release/***: Run full test suite + performance tests
- **main**: Run full test suite + security scans

## Best Practices

### Do's ✅

- **Always** create feature branches from `develop`
- **Always** create pull requests for code review
- **Always** write meaningful commit messages
- **Always** update documentation with features
- **Always** run tests before pushing
- **Always** keep branches focused and small
- **Always** clean up merged branches

### Don'ts ❌

- **Never** commit directly to `main` or `develop`
- **Never** merge without code review
- **Never** push untested code
- **Never** create long-lived feature branches
- **Never** mix multiple features in one branch
- **Never** ignore CI/CD failures
- **Never** skip documentation updates

## Troubleshooting

### Common Issues

#### Merge Conflicts
```bash
# Pull latest changes from target branch
git checkout develop
git pull origin develop
git checkout feature/your-feature
git merge develop

# Resolve conflicts and commit
git add .
git commit -m "resolve merge conflicts with develop"
```

#### Accidentally Committed to Wrong Branch
```bash
# If you committed to develop instead of a feature branch
git checkout develop
git reset --soft HEAD~1  # Undo the commit but keep changes
git stash                # Stash the changes
git checkout -b feature/correct-branch
git stash pop            # Apply the changes
git add .
git commit -m "your commit message"
```

#### Need to Update Feature Branch with Latest Develop
```bash
# Option 1: Merge (preserves history)
git checkout feature/your-feature
git merge develop

# Option 2: Rebase (cleaner history)
git checkout feature/your-feature
git rebase develop
```

## GitHub Integration

### Setting Up Branch Protection

1. Go to **Settings** → **Branches**
2. Add protection rules for `main` and `develop`
3. Configure required status checks
4. Enable required reviews

### Pull Request Templates

Use the provided PR templates in `.github/pull_request_template.md`:
- Feature PR template
- Release PR template  
- Hotfix PR template

### Automated Workflows

The project includes automated workflows for:
- **Continuous Integration**: Run tests on all PRs
- **Release Automation**: Create releases from tags
- **Dependency Updates**: Automated dependency PRs

## Team Collaboration

### Code Review Guidelines

- **Reviewer Assignment**: At least 2 reviewers for main/develop PRs
- **Review Criteria**: Code quality, test coverage, documentation
- **Response Time**: 24-48 hours for review responses
- **Approval Process**: All conversations must be resolved

### Communication

- **Feature Planning**: Discuss in GitHub Issues before starting
- **Progress Updates**: Regular updates in PR comments
- **Blockers**: Communicate blockers immediately
- **Release Planning**: Weekly release planning meetings

---

This Git Flow implementation ensures a professional, scalable, and maintainable development workflow for the Demo Inventory Microservice project. Follow these guidelines to maintain code quality and release stability.
