# Project Setup & Environment Configuration
## AI-Assisted Development Environment and Project Initialization

**Duration: 45 minutes**  
**Difficulty: Intermediate**

## 🎯 Learning Objectives

Master AI-assisted project setup and environment configuration:
- **Create GitHub repository** with optimal structure using AI
- **Generate project templates** and boilerplate code
- **Configure development environment** for team collaboration
- **Set up CI/CD pipelines** and automation workflows
- **Establish development standards** and quality gates

## 📋 Prerequisites

- Completed [Architecture Design](02-architecture-design.md)
- GitHub account with Copilot access
- Local development tools installed (.NET 8+, Node.js 20+, Docker)
- Basic understanding of DevOps and CI/CD concepts

## 🚀 Step 1: GitHub Repository Setup (10 minutes)

### 1.1 Repository Creation and Structure

**Copilot Chat Prompt:**
```
Help me create an optimal GitHub repository structure for the Line Dealer BYD system:

Requirements:
- .NET 8 backend with Clean Architecture
- Angular latest version TypeScript frontend
- SQL Server database with EF Core migrations
- Docker containerization
- Comprehensive testing setup
- CI/CD with GitHub Actions
- Documentation and API specs

Create:
1. Complete folder structure with descriptions
2. Essential configuration files (.gitignore, .editorconfig, etc.)
3. README.md template with project overview
4. Contributing guidelines and development setup
5. Issue and PR templates for team collaboration

Provide shell commands to create the entire structure.
```

### 1.2 GitHub Repository Configuration

**Copilot Chat Prompt:**
```
Configure the GitHub repository for optimal team collaboration and AI development:

1. Repository Settings
   - Branch protection rules for main/develop
   - Required status checks and reviews
   - Auto-merge and delete branch policies
   - Issue and PR templates configuration

2. GitHub Features Setup
   - Projects and milestones for project management
   - Wiki for comprehensive documentation
   - Discussions for team communication
   - Security advisories and dependabot

3. Team Collaboration
   - CODEOWNERS file for code review assignments
   - Team access and permission configuration
   - GitHub Copilot configuration for team
   - Development workflow documentation

Provide detailed configuration scripts and setup instructions.
```

### Expected Repository Structure

```bash
# Create the repository structure
mkdir line-dealer-byd && cd line-dealer-byd

# Backend structure (Clean Architecture)
mkdir -p backend/src/LineDealerByd.Domain
mkdir -p backend/src/LineDealerByd.Application  
mkdir -p backend/src/LineDealerByd.Infrastructure
mkdir -p backend/src/LineDealerByd.API
mkdir -p backend/tests/LineDealerByd.Domain.Tests
mkdir -p backend/tests/LineDealerByd.Application.Tests
mkdir -p backend/tests/LineDealerByd.Infrastructure.Tests
mkdir -p backend/tests/LineDealerByd.API.Tests

# Frontend structure
mkdir -p frontend/src/components
mkdir -p frontend/src/pages
mkdir -p frontend/src/services
mkdir -p frontend/src/hooks
mkdir -p frontend/src/utils
mkdir -p frontend/src/types
mkdir -p frontend/public

# Database and scripts
mkdir -p database/migrations
mkdir -p database/seed-data
mkdir -p scripts/development
mkdir -p scripts/deployment

# Documentation
mkdir -p docs/api
mkdir -p docs/architecture
mkdir -p docs/deployment
mkdir -p docs/development

# CI/CD and configuration
mkdir -p .github/workflows
mkdir -p .github/ISSUE_TEMPLATE
mkdir -p .github/PULL_REQUEST_TEMPLATE
```

## 🏗️ Step 2: Project Template Generation (15 minutes)

### 2.1 Backend Project Template Creation

**Copilot Chat Prompt:**
```
Generate complete .NET 8 project templates for the Line Dealer BYD backend using Clean Architecture:

1. Domain Project (LineDealerByd.Domain)
   - Base entity classes and interfaces
   - Domain exceptions and validation
   - Folder structure for entities, value objects, events
   - Project file with minimal dependencies

2. Application Project (LineDealerByd.Application)
   - CQRS pattern setup with MediatR
   - Common interfaces and DTOs
   - Validation and mapping profiles
   - Dependency injection configuration

3. Infrastructure Project (LineDealerByd.Infrastructure)
   - Entity Framework configuration
   - Repository implementations
   - External service integrations
   - Database context and migrations

4. API Project (LineDealerByd.API)
   - ASP.NET Core Web API setup
   - Swagger/OpenAPI configuration
   - Authentication and authorization
   - Middleware and error handling

Provide complete project files (.csproj), Program.cs, and basic class templates.
```

### 2.2 Frontend Project Template Creation

**Copilot Chat Prompt:**
```
Generate Angular latest version TypeScript project template for Line Dealer BYD frontend:

1. Angular CLI Configuration
   - TypeScript configuration with strict settings
   - ESLint and Prettier setup
   - Path aliases and build optimization
   - Environment variable handling

2. Project Structure Setup
   - Angular module structure
   - Routing configuration with Angular Router
   - State management setup (NgRx or Angular Services)
   - API client configuration with Angular HttpClient

3. Development Tools
   - Testing setup with Jasmine and Angular Testing Library
   - Storybook for component development
   - Angular Material or Tailwind CSS for styling
   - Development scripts and automation

4. Quality Tools
   - TypeScript strict configuration
   - ESLint rules for Angular and TypeScript
   - Prettier code formatting
   - Husky git hooks for quality gates

Provide complete package.json, configuration files, and folder structure.
```

### 2.3 Database Setup and Configuration

**Copilot Chat Prompt:**
```
Create database setup and configuration for the Line Dealer BYD system:

1. Entity Framework Core Setup
   - DbContext configuration for SQL Server
   - Entity configurations and relationships
   - Migration generation and management
   - Connection string management

2. Database Schema Design
   - Initial migration for core entities
   - Seed data for reference tables
   - Indexing strategy for performance
   - Audit and soft delete implementation

3. Development Database Setup
   - Docker Compose for local SQL Server
   - Database initialization scripts
   - Test data generation
   - Backup and restore procedures

4. Production Considerations
   - Connection pooling configuration
   - Performance monitoring setup
   - Backup and disaster recovery
   - Security and encryption

Provide Entity Framework configurations, migration scripts, and Docker setup.
```

## ⚙️ Step 3: Development Environment Configuration (10 minutes)

### 3.1 Docker Development Environment

**Copilot Chat Prompt:**
```
Create comprehensive Docker development environment for Line Dealer BYD:

1. Multi-Container Setup
   - SQL Server database with initialization
   - Redis for caching and sessions (if required)
   - Azure Event Hub for message queuing (if required)
   - SQL Server Management Studio for database management

2. Backend Container
   - Multi-stage Dockerfile for .NET API
   - Development and production configurations
   - Health checks and monitoring
   - Volume mounting for development

3. Frontend Container
   - Node.js container for Angular development
   - Hot reload configuration
   - Build optimization for production
   - Nginx for production serving

4. Development Workflow
   - Docker Compose for local development
   - Environment variable management
   - Log aggregation and monitoring
   - Database seeding and migrations

Provide complete Dockerfile and docker-compose.yml configurations.
```

### 3.2 VS Code Workspace Configuration

**Copilot Chat Prompt:**
```
Create optimal VS Code workspace configuration for Line Dealer BYD development:

1. Workspace Settings
   - Multi-root workspace for frontend and backend
   - TypeScript and C# language settings
   - Debugging configurations for all projects
   - Task definitions for build and test

2. Recommended Extensions
   - GitHub Copilot and Copilot Chat
   - Language-specific extensions (C#, TypeScript, Angular)
   - Database tools and Docker extensions
   - Code quality tools (ESLint, Prettier, SonarLint)

3. Code Snippets and Templates
   - Custom snippets for Clean Architecture patterns
   - Angular component templates
   - API endpoint templates
   - Test case templates

4. Development Productivity
   - Launch configurations for debugging
   - Build tasks and test runners
   - Git integration and workflow
   - Live Share configuration for collaboration

Provide complete .vscode folder configuration with all settings and extensions.
```

### 3.3 GitHub Copilot Configuration

**Copilot Chat Prompt:**
```
Create comprehensive GitHub Copilot configuration for the Line Dealer BYD project:

1. Copilot Instructions File
   - Project-specific context and patterns
   - Architecture principles and constraints
   - Code style and naming conventions
   - Business domain knowledge

2. Workspace Optimization
   - File and folder patterns for context
   - Code example repositories
   - Documentation integration
   - Team collaboration patterns

3. Development Workflows
   - AI-assisted code review processes
   - Automated code generation templates
   - Testing strategy with AI assistance
   - Documentation generation workflows

4. Quality Assurance
   - Code validation prompts and checklists
   - Security review guidelines
   - Performance optimization patterns
   - Best practice enforcement

Create the .github/copilot-instructions.md file with comprehensive project guidance.
```

## 🔄 Step 4: CI/CD Pipeline Setup (10 minutes)

### 4.1 GitHub Actions Workflow Design

**Copilot Chat Prompt:**
```
Design comprehensive GitHub Actions CI/CD workflows for Line Dealer BYD:

1. Continuous Integration Pipeline
   - Multi-stage build for backend and frontend
   - Automated testing with coverage reporting
   - Code quality checks (SonarCloud, CodeQL)
   - Security scanning and vulnerability assessment
   - Docker image building and scanning

2. Continuous Deployment Pipeline
   - Environment-specific deployments (dev, staging, prod)
   - Database migration automation
   - Infrastructure as Code with Terraform/ARM
   - Blue-green or canary deployment strategies
   - Rollback procedures and health checks

3. Quality Gates
   - Branch protection with required checks
   - Pull request validation workflows
   - Automated dependency updates
   - Performance regression testing
   - Security and compliance validation

4. Monitoring and Notifications
   - Build status notifications
   - Deployment success/failure alerts
   - Performance monitoring integration
   - Error tracking and reporting
   - Slack/Teams integration for team updates

Provide complete GitHub Actions workflow files for all scenarios.
```

### 4.2 Environment Configuration Management

**Copilot Chat Prompt:**
```
Create environment configuration management for Line Dealer BYD:

1. Environment Strategy
   - Development environment configuration
   - Staging environment for testing
   - Production environment setup
   - Feature branch environments

2. Configuration Management
   - Environment-specific appsettings.json
   - Docker environment variables
   - Kubernetes ConfigMaps and Secrets
   - Azure Key Vault for secrets management

3. Database Management
   - Environment-specific database configurations
   - Migration strategy across environments
   - Seed data management
   - Backup and restore procedures

4. Security and Compliance
   - Secrets management and rotation
   - Environment access controls
   - Audit logging and monitoring
   - Compliance reporting and validation

Provide configuration templates and deployment scripts for all environments.
```

## 📚 Step 5: Documentation and Standards (10 minutes)

### 5.1 Project Documentation Setup

**Copilot Chat Prompt:**
```
Create comprehensive documentation structure for Line Dealer BYD:

1. README and Getting Started
   - Project overview and objectives
   - Quick start guide for developers
   - Development environment setup
   - Architecture overview and decisions

2. API Documentation
   - OpenAPI/Swagger specifications
   - API usage examples and tutorials
   - Authentication and authorization guide
   - Integration documentation for external systems

3. Development Documentation
   - Coding standards and conventions
   - Architecture decision records (ADRs)
   - Database schema documentation
   - Deployment and operational guides

4. User Documentation
   - Feature documentation and user guides
   - Installation and configuration instructions
   - Troubleshooting and FAQ
   - Release notes and changelog

Create markdown templates and documentation structure with AI-generated content.
```

### 5.2 Development Standards and Guidelines

**Copilot Chat Prompt:**
```
Establish development standards and guidelines for Line Dealer BYD:

1. Code Quality Standards
   - C# coding conventions and style guide
   - TypeScript/Angular best practices
   - Database design and naming conventions
   - API design standards and patterns

2. Testing Standards
   - Unit testing guidelines and coverage requirements
   - Integration testing strategies
   - End-to-end testing approaches
   - Performance testing standards

3. Git Workflow and Branching Strategy
   - Branching model (Git Flow, GitHub Flow)
   - Commit message conventions
   - Pull request templates and review process
   - Release management and versioning

4. Quality Gates and Reviews
   - Code review checklists and guidelines
   - Security review requirements
   - Performance review criteria
   - Documentation review standards

Provide detailed guidelines documents and enforcement mechanisms.
```

## ✅ Setup Validation and Testing

### Environment Validation Checklist

**Development Environment**
- [ ] Repository cloned and structure verified
- [ ] Backend solution builds without errors
- [ ] Frontend application starts and runs
- [ ] Database connection established
- [ ] Docker containers start successfully
- [ ] VS Code workspace configured correctly

**CI/CD Pipeline**
- [ ] GitHub Actions workflows trigger correctly
- [ ] Build pipeline completes successfully
- [ ] Test execution and reporting working
- [ ] Code quality checks passing
- [ ] Security scans completed
- [ ] Deployment to staging environment successful

**Team Collaboration**
- [ ] Branch protection rules active
- [ ] PR templates and issue templates working
- [ ] Code review assignments configured
- [ ] GitHub Copilot accessible to team
- [ ] Documentation accessible and complete

### Quick Validation Script

Create a validation script to verify setup:

```bash
#!/bin/bash
# validate-setup.sh

echo "🔍 Validating Line Dealer BYD project setup..."

# Check backend build
echo "📦 Building backend..."
cd backend && dotnet build

# Check frontend build  
echo "⚛️ Building frontend..."
cd ../frontend && npm install && npm run build

# Check database connection
echo "🗄️ Testing database connection..."
cd ../backend && dotnet ef database update --dry-run

# Check Docker setup
echo "🐳 Validating Docker setup..."
docker-compose config

echo "✅ Setup validation completed!"
```

## 🎯 Success Metrics

### Technical Setup Success
- [ ] All projects build and run without errors
- [ ] Database connectivity and migrations working
- [ ] CI/CD pipeline executing successfully
- [ ] Code quality tools integrated and working
- [ ] Documentation complete and accessible

### Team Collaboration Success
- [ ] Repository access configured for all team members
- [ ] Development workflows documented and understood
- [ ] Code review processes established
- [ ] AI tools accessible and configured for team
- [ ] Communication channels integrated

### Development Productivity Success
- [ ] Local development environment quick to set up
- [ ] Hot reload and debugging working efficiently
- [ ] Automated testing running smoothly
- [ ] Code generation and AI assistance functional
- [ ] Deployment process automated and reliable

## 🚀 Next Steps

After completing project setup:

1. **Team Onboarding**: Share setup documentation with team members
2. **Initial Development**: Begin implementing core domain models
3. **Continuous Improvement**: Iterate on development processes
4. **Foundation Development**: Start building core backend services

### Development Workflow Summary

```markdown
# Daily Development Workflow

## Starting Development
1. Pull latest changes from main branch
2. Create feature branch from develop
3. Start Docker development environment
4. Run database migrations if needed

## During Development
1. Use GitHub Copilot for code generation
2. Write tests for new functionality
3. Run local tests and quality checks
4. Commit regularly with descriptive messages

## Completing Features
1. Ensure all tests pass locally
2. Push changes and create pull request
3. Request code review from team members
4. Address feedback and merge when approved
```

---

**🎯 Success Indicator**: You've successfully completed project setup when your entire team can clone the repository, run the development environment, and begin productive development with all quality gates and collaboration tools working.

**Next**: [Backend Foundation Development](04-backend-foundation.md) - Begin implementing core domain models and business logic.