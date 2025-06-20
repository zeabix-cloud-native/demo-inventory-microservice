---
name: 🌟 Feature Pull Request
about: Submit a feature branch for review and merge into develop
title: "feat: [Brief description of the feature]"
labels: ["feature", "needs-review"]
assignees: []
---

## 🌟 Feature Description

**Feature Name:** <!-- Name of the feature -->
**Jira/Issue:** <!-- Link to related issue/ticket -->

### What does this feature do?
<!-- Provide a clear and concise description of the feature -->

### Business Value
<!-- Explain the business value and user benefit -->

## 🔄 Git Flow Information

- **Source Branch:** `feature/[feature-name]`
- **Target Branch:** `develop`
- **Feature Type:** <!-- Core Feature / Enhancement / Integration -->

### Related Branches
- [ ] This feature depends on other feature branches (list them)
- [ ] This feature will be included in the next release
- [ ] This feature requires database changes

## 🛠️ Implementation Details

### Architecture Layer Changes
- [ ] **Domain Layer** - Business logic changes
- [ ] **Application Layer** - Service/use case changes  
- [ ] **Infrastructure Layer** - Data access changes
- [ ] **API Layer** - Controller/endpoint changes
- [ ] **Frontend** - UI component changes

### Files Changed
<!-- List the main files that were modified -->
- `backend/src/DemoInventory.Domain/Entities/[Entity].cs`
- `backend/src/DemoInventory.Application/Services/[Service].cs`
- `frontend/src/components/[Component].tsx`

### Database Changes
- [ ] No database changes
- [ ] Migration included: `[migration-name]`
- [ ] Seed data updated
- [ ] Database schema changes documented

## 🧪 Testing

### Test Coverage
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] API tests updated (Postman collection)
- [ ] E2E tests added/updated
- [ ] All tests pass locally

### Test Types
- [ ] **Happy Path Testing** - Normal use cases tested
- [ ] **Edge Case Testing** - Boundary conditions tested
- [ ] **Error Handling** - Error scenarios tested
- [ ] **Performance Testing** - Performance impact assessed

### Manual Testing Checklist
- [ ] Feature works as expected in development environment
- [ ] Feature integrates properly with existing functionality
- [ ] UI/UX meets design requirements
- [ ] Responsive design tested (if applicable)
- [ ] Browser compatibility verified (if applicable)

## 📚 Documentation

- [ ] Code documentation updated (XML docs for C#, JSDoc for TypeScript)
- [ ] API documentation updated (if API changes)
- [ ] User documentation updated (if user-facing changes)
- [ ] README updated (if needed)
- [ ] Architecture documentation updated (if architectural changes)

## 🔒 Security & Performance

### Security Considerations
- [ ] Input validation implemented
- [ ] Authorization checks in place
- [ ] No sensitive data exposed in logs
- [ ] SQL injection prevention measures
- [ ] XSS protection implemented (frontend)

### Performance Impact
- [ ] No performance degradation expected
- [ ] Database query optimization reviewed
- [ ] Frontend bundle size impact assessed
- [ ] Memory usage impact considered

## ✅ Definition of Done

- [ ] Code follows project coding standards
- [ ] All automated tests pass
- [ ] Code has been manually tested
- [ ] Documentation is updated
- [ ] Security considerations addressed
- [ ] Performance impact assessed
- [ ] Code review checklist completed

## 🔍 Code Review Checklist

### For Reviewers
- [ ] **Architecture**: Code follows Clean Architecture principles
- [ ] **SOLID Principles**: Code follows SOLID design principles
- [ ] **Error Handling**: Proper error handling implemented
- [ ] **Testing**: Adequate test coverage provided
- [ ] **Security**: Security best practices followed
- [ ] **Performance**: No obvious performance issues
- [ ] **Documentation**: Code is well-documented
- [ ] **Consistency**: Code follows existing patterns

### Review Areas
- [ ] **Business Logic** - Domain logic is correct and testable
- [ ] **Data Access** - Repository patterns properly implemented
- [ ] **API Design** - RESTful principles followed
- [ ] **Frontend** - Component design and state management
- [ ] **Integration** - Proper integration between layers

## 🚀 Deployment

### Environment Testing
- [ ] Tested in development environment
- [ ] Ready for integration testing in develop branch
- [ ] Feature flags implemented (if needed)
- [ ] Rollback plan considered

### Dependencies
- [ ] No new external dependencies
- [ ] New dependencies documented and approved
- [ ] Package versions locked
- [ ] Security scan of dependencies completed

## 📝 Additional Notes

### Breaking Changes
- [ ] No breaking changes
- [ ] Breaking changes documented with migration guide

### Future Considerations
<!-- Any technical debt, future improvements, or follow-up tasks -->

### Screenshots/Videos
<!-- If UI changes, include screenshots or videos -->

---

## 📋 Reviewer Instructions

1. **Pull and Test Locally**
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/[feature-name] origin/feature/[feature-name]
   dotnet restore && dotnet build
   cd frontend && npm install && npm run dev
   ```

2. **Run Tests**
   ```bash
   dotnet test
   newman run tests/postman/collection.json
   cd tests/e2e && npm run test:e2e
   ```

3. **Review Checklist**
   - Code quality and architecture
   - Test coverage and quality
   - Documentation completeness
   - Security considerations
   - Performance impact

**Thank you for reviewing this feature! 🙏**
