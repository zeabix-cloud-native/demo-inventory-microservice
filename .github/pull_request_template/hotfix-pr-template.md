---
name: 🔥 Hotfix Pull Request
about: Emergency fix for critical production issues
title: "hotfix: [Brief description of critical fix]"
labels: ["hotfix", "critical", "production", "emergency"]
assignees: []
---

## 🚨 EMERGENCY HOTFIX

**Severity:** `CRITICAL`
**Hotfix Branch:** `hotfix/[hotfix-name]`
**Target Branch:** `main`
**Incident ID:** `[INC-XXXX]` (if applicable)

### 🔥 Critical Issue Description
<!-- Describe the critical issue that requires immediate fixing -->

### 💥 Impact Assessment
- [ ] **Production Down** - Complete service outage
- [ ] **Critical Functionality Broken** - Core features not working
- [ ] **Security Vulnerability** - Security breach or vulnerability
- [ ] **Data Integrity Issue** - Risk of data corruption/loss
- [ ] **Performance Degradation** - Severe performance impact

### 🎯 Affected Systems
- [ ] API Backend
- [ ] Frontend Application  
- [ ] Database
- [ ] Authentication System
- [ ] External Integrations
- [ ] Payment Processing
- [ ] User Management

## 🛠️ Root Cause Analysis

### What Caused the Issue?
<!-- Detailed explanation of the root cause -->

### When Was It Introduced?
- **Commit**: `[commit-hash]`
- **Release**: `v[X.Y.Z]`
- **Date**: `[YYYY-MM-DD HH:MM]`

### Why Wasn't It Caught Earlier?
- [ ] Missing test coverage
- [ ] Edge case not considered
- [ ] Environment-specific issue
- [ ] Third-party service issue
- [ ] Configuration problem

## 🔧 Fix Implementation

### Technical Solution
<!-- Describe the technical fix being implemented -->

### Files Changed
<!-- List only the files changed for the hotfix -->
- `[file1.cs]` - [Brief description of change]
- `[file2.tsx]` - [Brief description of change]

### Code Changes Summary
```diff
// Show minimal diff of critical changes
+ Fixed critical null reference check
- Removed problematic code path
```

## 🧪 Emergency Testing

### Fast-Track Testing Completed
- [ ] **Critical Path Testing** - Core functionality verified
- [ ] **Regression Testing** - No new issues introduced
- [ ] **Security Testing** - No security regressions
- [ ] **Performance Testing** - Performance impact assessed

### Test Results
- [ ] **Unit Tests**: ✅ Critical tests passing
- [ ] **Integration Tests**: ✅ Key integrations working
- [ ] **Smoke Tests**: ✅ Basic functionality verified
- [ ] **Security Scan**: ✅ No new vulnerabilities

### Manual Verification
- [ ] **Local Testing**: Hotfix verified locally
- [ ] **Staging Testing**: Deployed and tested in staging
- [ ] **Production Simulation**: Production-like environment tested

## 🚀 Deployment Plan

### Deployment Strategy
- [ ] **Immediate Deployment** - Deploy as soon as approved
- [ ] **Scheduled Window** - Deploy during maintenance window
- [ ] **Gradual Rollout** - Canary deployment first

### Deployment Steps
1. [ ] Merge to main branch
2. [ ] Create hotfix tag `v[X.Y.Z]`
3. [ ] Deploy to production
4. [ ] Verify fix in production
5. [ ] Monitor for 30 minutes
6. [ ] Merge back to develop
7. [ ] Update incident status

### Rollback Plan
- [ ] **Rollback Strategy**: Previous version deployment ready
- [ ] **Database Rollback**: No database changes / Rollback script ready
- [ ] **Configuration Rollback**: Previous configuration backed up
- [ ] **Estimated Rollback Time**: [X] minutes

## 🔒 Security & Risk Assessment

### Security Impact
- [ ] No security implications
- [ ] Security vulnerability being fixed
- [ ] New security considerations introduced
- [ ] Security team review completed

### Risk Level
- [ ] **Low Risk**: Minimal change, well-tested
- [ ] **Medium Risk**: Moderate change, good test coverage
- [ ] **High Risk**: Significant change, requires careful monitoring

### Risk Mitigation
- [ ] **Monitoring**: Enhanced monitoring during deployment
- [ ] **Team Standby**: Critical team members available
- [ ] **Communication**: Stakeholders notified
- [ ] **Incident Response**: Response team activated

## 📊 Monitoring & Verification

### Post-Deployment Monitoring
- [ ] **Application Health**: Health endpoints responding
- [ ] **Error Rates**: Error rates back to normal
- [ ] **Performance Metrics**: Response times acceptable
- [ ] **User Experience**: User flows working correctly

### Success Criteria
- [ ] **Issue Resolved**: Root cause issue fixed
- [ ] **No Regressions**: No new issues introduced
- [ ] **Performance Stable**: Performance metrics normal
- [ ] **Error Rates Low**: Error rates below threshold

### Monitoring Duration
- [ ] **30 minutes**: Initial verification period
- [ ] **2 hours**: Extended monitoring period
- [ ] **24 hours**: Full monitoring cycle

## 📢 Communication Plan

### Immediate Notifications
- [ ] **Incident Commander**: Notified of fix deployment
- [ ] **Operations Team**: Standing by for deployment
- [ ] **Support Team**: Prepared for user inquiries
- [ ] **Management**: Executive summary prepared

### Status Updates
- [ ] **Status Page**: Incident status updated
- [ ] **Customer Communication**: Customer notification prepared
- [ ] **Internal Teams**: All teams notified of fix
- [ ] **Post-Incident**: Post-mortem scheduled

## 🔍 Approval Requirements

### Emergency Approval Process
- [ ] **Tech Lead**: Approved ✅ @[tech-lead]
- [ ] **DevOps Lead**: Approved ✅ @[devops-lead]
- [ ] **Security Team**: Approved ✅ @[security-lead] (if security-related)
- [ ] **Incident Commander**: Approved ✅ @[incident-commander]

### Expedited Review Criteria
- [ ] **Business Impact**: High business impact requiring immediate fix
- [ ] **Security Severity**: Critical security vulnerability
- [ ] **Data Risk**: Risk of data loss or corruption
- [ ] **Compliance Risk**: Regulatory compliance requirement

## 📝 Documentation Updates

### Immediate Documentation
- [ ] **Incident Log**: Updated with fix details
- [ ] **Runbook**: Emergency procedures followed
- [ ] **Deployment Log**: Deployment steps documented

### Follow-up Documentation
- [ ] **Post-Mortem**: Root cause analysis documented
- [ ] **Process Improvement**: Lessons learned documented
- [ ] **Test Updates**: Test coverage gaps addressed
- [ ] **Monitoring Updates**: Alerting rules updated

## 🔄 Follow-up Actions

### Immediate Actions (Next 24 hours)
- [ ] **Monitor Production**: Continuous monitoring
- [ ] **Merge to Develop**: Sync hotfix with develop branch
- [ ] **Update Tests**: Add tests to prevent regression
- [ ] **Close Incident**: Update incident management system

### Short-term Actions (Next Week)
- [ ] **Post-Mortem**: Conduct blame-free post-mortem
- [ ] **Process Review**: Review emergency response process
- [ ] **Test Coverage**: Improve test coverage for affected area
- [ ] **Monitoring**: Enhance monitoring and alerting

### Long-term Actions (Next Month)
- [ ] **Architecture Review**: Assess architectural improvements
- [ ] **Process Automation**: Automate manual processes
- [ ] **Training**: Team training on lessons learned
- [ ] **Documentation**: Update emergency procedures

---

## 🚨 EMERGENCY CONTACT INFORMATION

**Incident Commander**: @[incident-commander] ([phone])
**Tech Lead**: @[tech-lead] ([phone])
**DevOps Lead**: @[devops-lead] ([phone])
**On-Call Engineer**: @[on-call] ([phone])

## 📞 Escalation Path

1. **Technical Issues**: Tech Lead → Engineering Manager → CTO
2. **Business Impact**: Product Owner → VP Product → CEO
3. **Security Issues**: Security Lead → CISO → CTO
4. **Customer Impact**: Customer Success → VP Customer → CEO

---

**⚠️ CRITICAL: This hotfix addresses a production emergency. Please review and approve immediately.**

**Deployment Window**: ASAP after approval
**Expected Downtime**: [X] minutes
**Rollback Time**: [X] minutes if needed
