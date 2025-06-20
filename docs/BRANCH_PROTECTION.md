# Branch Protection Rules Configuration

This document outlines the recommended branch protection rules for implementing Git Flow best practices in the Demo Inventory Microservice repository.

## Overview

Branch protection rules enforce Git Flow workflow by preventing direct pushes to protected branches and ensuring code quality through required reviews and status checks.

## Branch Protection Configuration

### 🔒 `main` Branch Protection

**Purpose**: Production-ready code only
**Protection Level**: Maximum

#### Required Settings
```yaml
Protection Rules:
  - Require pull request reviews before merging: ✅
    - Required reviewers: 2
    - Dismiss stale reviews when new commits are pushed: ✅
    - Require review from code owners: ✅
    - Restrict reviews to users with write access: ✅
  
  - Require status checks to pass before merging: ✅
    - Require branches to be up to date before merging: ✅
    - Status checks that are required:
      - build-and-test
      - postman-tests  
      - cypress-tests
      - security-checks
      - CodeQL
  
  - Require conversation resolution before merging: ✅
  - Require signed commits: ✅ (recommended)
  - Require linear history: ✅
  - Include administrators: ✅
  - Restrict pushes that create files larger than 100MB: ✅
  - Allow force pushes: ❌
  - Allow deletions: ❌
```

#### Allowed Merge Sources
- `release/*` branches (via PR)
- `hotfix/*` branches (via PR)

#### Restricted Actions
- ❌ Direct pushes
- ❌ Force pushes
- ❌ Branch deletion
- ❌ Bypassing reviews
- ❌ Bypassing status checks

### 🚧 `develop` Branch Protection

**Purpose**: Integration branch for features
**Protection Level**: High

#### Required Settings
```yaml
Protection Rules:
  - Require pull request reviews before merging: ✅
    - Required reviewers: 1
    - Dismiss stale reviews when new commits are pushed: ✅
    - Require review from code owners: ✅ (for critical areas)
  
  - Require status checks to pass before merging: ✅
    - Require branches to be up to date before merging: ✅
    - Status checks that are required:
      - build-and-test
      - postman-tests
      - cypress-tests (optional for small features)
  
  - Require conversation resolution before merging: ✅
  - Require linear history: ✅
  - Include administrators: ❌ (allows emergency fixes)
  - Restrict pushes that create files larger than 100MB: ✅
  - Allow force pushes: ❌
  - Allow deletions: ❌
```

#### Allowed Merge Sources
- `feature/*` branches (via PR)
- `bugfix/*` branches (via PR)
- `release/*` branches (via PR - backmerge)
- `hotfix/*` branches (via PR - backmerge)

#### Restricted Actions
- ❌ Direct pushes
- ❌ Force pushes
- ❌ Branch deletion

### 📝 `release/*` Branch Protection

**Purpose**: Release preparation and stabilization
**Protection Level**: Medium

#### Required Settings
```yaml
Protection Rules:
  - Require pull request reviews before merging: ✅
    - Required reviewers: 1 (for release preparation commits)
    - Allow specific actors to bypass: Release managers
  
  - Require status checks to pass before merging: ✅
    - Status checks that are required:
      - build-and-test
      - postman-tests
      - cypress-tests
      - security-checks
  
  - Include administrators: ❌
  - Restrict pushes that create files larger than 100MB: ✅
  - Allow force pushes: ❌ (except for release managers)
  - Allow deletions: ❌
```

#### Allowed Actions
- Direct pushes for release preparation (by release managers)
- Bug fix commits during release stabilization
- Version bump commits

### 🔥 `hotfix/*` Branch Protection

**Purpose**: Critical production fixes
**Protection Level**: Emergency

#### Required Settings
```yaml
Protection Rules:
  - Require pull request reviews before merging: ❌
    - Emergency exception: Critical fixes can be pushed directly
    - Post-deployment review required
  
  - Require status checks to pass before merging: ✅
    - Status checks that are required:
      - critical-tests (fast subset)
      - security-scan
  
  - Include administrators: ❌
  - Allow force pushes: ❌
  - Allow deletions: ❌
```

#### Emergency Bypass
- Hotfix branches allow direct pushes for emergency situations
- Mandatory post-deployment code review
- All changes must be merged back to `develop` and `main`

## Required Status Checks

### Core Status Checks (All Protected Branches)

#### `build-and-test`
- **Purpose**: Ensure code compiles and unit tests pass
- **Timeout**: 10 minutes
- **Required for**: `main`, `develop`, `release/*`, `hotfix/*`

#### `postman-tests`
- **Purpose**: Validate API functionality
- **Timeout**: 15 minutes  
- **Required for**: `main`, `develop`, `release/*`

#### `cypress-tests`
- **Purpose**: End-to-end functionality validation
- **Timeout**: 20 minutes
- **Required for**: `main`, `release/*`
- **Optional for**: `develop` (feature-dependent)

#### `security-checks`
- **Purpose**: Security vulnerability scanning
- **Timeout**: 30 minutes
- **Required for**: `main`, `release/*`, `hotfix/*` (if security-related)

#### `CodeQL`
- **Purpose**: Code quality and security analysis
- **Timeout**: 45 minutes
- **Required for**: `main`, `release/*`

### Additional Checks

#### `dependency-check`
- **Purpose**: Dependency vulnerability scanning
- **Required for**: `main`, `release/*`

#### `performance-tests`
- **Purpose**: Performance regression detection
- **Required for**: `main` (major releases)

#### `sonarcloud`
- **Purpose**: Code quality metrics
- **Required for**: `main`, `develop`

## Implementation Guide

### Step 1: GitHub Repository Settings

Navigate to **Settings** → **Branches** in your GitHub repository.

### Step 2: Configure `main` Branch Protection

1. Click **Add rule**
2. Branch name pattern: `main`
3. Configure settings as specified above
4. Save rule

### Step 3: Configure `develop` Branch Protection

1. Click **Add rule** 
2. Branch name pattern: `develop`
3. Configure settings as specified above
4. Save rule

### Step 4: Configure Pattern-Based Rules

#### Release Branches
1. Branch name pattern: `release/*`
2. Configure protection settings
3. Save rule

#### Hotfix Branches  
1. Branch name pattern: `hotfix/*`
2. Configure emergency settings
3. Save rule

### Step 5: Configure Status Checks

1. Go to **Settings** → **Branches**
2. Edit each branch protection rule
3. Add required status checks
4. Save changes

## Code Owners Configuration

Create `.github/CODEOWNERS` file to define automatic reviewers:

```bash
# Global ownership
* @team-leads @senior-developers

# Backend code ownership
backend/ @backend-team @tech-lead
backend/src/DemoInventory.Domain/ @domain-experts @tech-lead
backend/src/DemoInventory.Infrastructure/ @infrastructure-team

# Frontend code ownership  
frontend/ @frontend-team @ui-ux-lead

# Database and migrations
*.sql @database-team @tech-lead
**/Migrations/ @database-team @tech-lead

# Security-sensitive files
**/*Security* @security-team @tech-lead
**/*Auth* @security-team @tech-lead

# CI/CD and deployment
.github/ @devops-team @tech-lead
docker*.yml @devops-team
Dockerfile* @devops-team

# Documentation
docs/ @tech-writers @product-team
*.md @tech-writers

# Configuration files
appsettings*.json @devops-team @tech-lead
*.config @devops-team

# Release and hotfix critical paths
release/ @release-managers @tech-lead
hotfix/ @incident-commanders @tech-lead @devops-team
```

## Bypass Procedures

### Emergency Hotfix Bypass

For critical production issues:

1. **Create hotfix branch** from `main`
2. **Push emergency fix** (bypass reviews temporarily)
3. **Deploy immediately** to resolve issue
4. **Create post-deployment PR** for code review
5. **Merge back** to `develop` with full review

### Release Manager Bypass

For release preparation activities:

1. **Release managers** can push directly to `release/*` branches
2. **Version bumps** and **changelog updates** allowed
3. **Final review** required before merging to `main`

## Monitoring and Enforcement

### Audit Trail
- All protection rule changes logged
- Review activity tracked
- Bypass usage monitored
- Compliance reporting available

### Alerts and Notifications
- Failed status checks → Slack notifications
- Protection rule violations → Email alerts
- Emergency bypass usage → Incident tracking
- Security scan failures → Security team alerts

### Metrics and Reporting
- **Pull Request Metrics**: Review time, approval rate
- **Status Check Metrics**: Pass rate, failure analysis
- **Compliance Metrics**: Rule adherence, bypass frequency
- **Security Metrics**: Vulnerability detection, resolution time

## Troubleshooting

### Common Issues

#### Status Check Not Running
```bash
# Check GitHub Actions workflow
# Verify branch name matches trigger patterns
# Ensure required secrets are configured
```

#### Review Requirements Not Met
```bash
# Check if reviewers have write access
# Verify CODEOWNERS file syntax
# Ensure required reviewers are available
```

#### Merge Blocked by Linear History
```bash
# Use squash merge or rebase before merging
git rebase develop
git push --force-with-lease origin feature/branch-name
```

### Emergency Procedures

#### Critical Production Issue
1. Follow hotfix process with emergency bypass
2. Notify incident commander immediately
3. Document all actions in incident log
4. Schedule post-incident review

#### Repository Access Issues
1. Contact repository administrators
2. Escalate to organization owners if needed
3. Use emergency access procedures
4. Document access changes

## Best Practices

### For Developers
- ✅ Always create feature branches from `develop`
- ✅ Keep branches small and focused
- ✅ Run tests locally before pushing
- ✅ Write meaningful commit messages
- ✅ Respond to review feedback promptly

### For Reviewers
- ✅ Review within 24 hours
- ✅ Focus on code quality and security
- ✅ Provide constructive feedback
- ✅ Test changes locally when needed
- ✅ Approve only when confident

### For Release Managers
- ✅ Follow release checklist
- ✅ Coordinate with all teams
- ✅ Ensure comprehensive testing
- ✅ Communicate release status
- ✅ Monitor post-deployment metrics

## Compliance and Governance

### Regulatory Requirements
- **SOX Compliance**: Separation of duties enforced
- **PCI DSS**: Security reviews for payment code
- **GDPR**: Data privacy reviews for user data handling
- **ISO 27001**: Security controls documented

### Audit Requirements
- All code changes tracked and reviewable
- Protection rule changes logged
- Emergency bypass usage documented
- Security scan results archived

---

This branch protection configuration ensures code quality, security, and compliance while supporting efficient Git Flow workflows for the Demo Inventory Microservice project.
