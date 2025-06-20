#!/bin/bash

# Release Management Script for Git Flow
# Usage: ./release-management.sh <command> [options]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
MAIN_BRANCH="main"
DEVELOP_BRANCH="develop"
REPO_URL=$(git config --get remote.origin.url)
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

# Helper functions
log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
    exit 1
}

check_git_status() {
    if [[ -n $(git status --porcelain) ]]; then
        log_error "Working directory is not clean. Please commit or stash your changes."
    fi
}

check_branch_exists() {
    local branch=$1
    if ! git show-ref --verify --quiet refs/heads/$branch; then
        log_error "Branch '$branch' does not exist."
    fi
}

fetch_latest() {
    log_info "Fetching latest changes from remote..."
    git fetch origin
}

# Start a new feature
start_feature() {
    local feature_name=$1
    
    if [[ -z "$feature_name" ]]; then
        log_error "Feature name is required. Usage: $0 start-feature <feature-name>"
    fi
    
    local feature_branch="feature/$feature_name"
    
    log_info "Starting new feature: $feature_name"
    
    # Check prerequisites
    check_git_status
    fetch_latest
    
    # Switch to develop and pull latest
    log_info "Switching to $DEVELOP_BRANCH branch..."
    git checkout $DEVELOP_BRANCH
    git pull origin $DEVELOP_BRANCH
    
    # Create feature branch
    log_info "Creating feature branch: $feature_branch"
    git checkout -b $feature_branch
    
    # Push feature branch
    git push -u origin $feature_branch
    
    log_success "Feature branch '$feature_branch' created and pushed to remote."
    log_info "You can now start working on your feature. When ready, create a PR to merge into $DEVELOP_BRANCH."
}

# Finish a feature
finish_feature() {
    local feature_name=$1
    
    if [[ -z "$feature_name" ]]; then
        # Try to infer from current branch
        if [[ $CURRENT_BRANCH == feature/* ]]; then
            feature_name=${CURRENT_BRANCH#feature/}
        else
            log_error "Feature name is required or run from feature branch. Usage: $0 finish-feature <feature-name>"
        fi
    fi
    
    local feature_branch="feature/$feature_name"
    
    log_info "Finishing feature: $feature_name"
    
    # Check prerequisites
    check_git_status
    check_branch_exists $feature_branch
    fetch_latest
    
    # Switch to feature branch
    git checkout $feature_branch
    
    # Ensure branch is up to date
    git pull origin $feature_branch
    
    # Switch to develop and pull latest
    git checkout $DEVELOP_BRANCH
    git pull origin $DEVELOP_BRANCH
    
    # Merge feature branch
    log_info "Merging feature branch into $DEVELOP_BRANCH..."
    git merge --no-ff $feature_branch -m "feat: merge feature/$feature_name into develop"
    
    # Push changes
    git push origin $DEVELOP_BRANCH
    
    # Delete local feature branch
    git branch -d $feature_branch
    
    # Delete remote feature branch
    git push origin --delete $feature_branch
    
    log_success "Feature '$feature_name' has been successfully merged into $DEVELOP_BRANCH and branches have been cleaned up."
}

# Start a release
start_release() {
    local version=$1
    
    if [[ -z "$version" ]]; then
        log_error "Version is required. Usage: $0 start-release <version>"
    fi
    
    # Validate version format (semantic versioning)
    if [[ ! $version =~ ^v?[0-9]+\.[0-9]+\.[0-9]+(-[a-zA-Z0-9.-]+)?$ ]]; then
        log_error "Invalid version format. Use semantic versioning (e.g., v1.2.3 or 1.2.3)"
    fi
    
    # Ensure version starts with 'v'
    if [[ ! $version =~ ^v ]]; then
        version="v$version"
    fi
    
    local release_branch="release/$version"
    
    log_info "Starting release: $version"
    
    # Check prerequisites
    check_git_status
    fetch_latest
    
    # Check if release branch already exists
    if git show-ref --verify --quiet refs/heads/$release_branch; then
        log_error "Release branch '$release_branch' already exists."
    fi
    
    # Switch to develop and pull latest
    log_info "Switching to $DEVELOP_BRANCH branch..."
    git checkout $DEVELOP_BRANCH
    git pull origin $DEVELOP_BRANCH
    
    # Create release branch
    log_info "Creating release branch: $release_branch"
    git checkout -b $release_branch
    
    # Update version in project files
    update_version $version
    
    # Commit version updates
    git add .
    git commit -m "chore: bump version to $version"
    
    # Push release branch
    git push -u origin $release_branch
    
    log_success "Release branch '$release_branch' created and pushed to remote."
    log_info "Next steps:"
    log_info "1. Test the release thoroughly"
    log_info "2. Fix any release-blocking issues"
    log_info "3. Update CHANGELOG.md"
    log_info "4. Run: $0 finish-release $version"
}

# Finish a release
finish_release() {
    local version=$1
    
    if [[ -z "$version" ]]; then
        # Try to infer from current branch
        if [[ $CURRENT_BRANCH == release/* ]]; then
            version=${CURRENT_BRANCH#release/}
        else
            log_error "Version is required or run from release branch. Usage: $0 finish-release <version>"
        fi
    fi
    
    # Ensure version starts with 'v'
    if [[ ! $version =~ ^v ]]; then
        version="v$version"
    fi
    
    local release_branch="release/$version"
    
    log_info "Finishing release: $version"
    
    # Check prerequisites
    check_git_status
    check_branch_exists $release_branch
    fetch_latest
    
    # Switch to release branch
    git checkout $release_branch
    git pull origin $release_branch
    
    # Merge into main
    log_info "Merging release into $MAIN_BRANCH..."
    git checkout $MAIN_BRANCH
    git pull origin $MAIN_BRANCH
    git merge --no-ff $release_branch -m "release: $version"
    
    # Create tag
    log_info "Creating release tag: $version"
    git tag -a $version -m "Release $version"
    
    # Push main and tag
    git push origin $MAIN_BRANCH
    git push origin $version
    
    # Merge back into develop
    log_info "Merging release back into $DEVELOP_BRANCH..."
    git checkout $DEVELOP_BRANCH
    git pull origin $DEVELOP_BRANCH
    git merge --no-ff $release_branch -m "chore: merge release $version back into develop"
    git push origin $DEVELOP_BRANCH
    
    # Delete release branch
    git branch -d $release_branch
    git push origin --delete $release_branch
    
    log_success "Release $version has been successfully completed!"
    log_info "Tag $version has been created and pushed."
    log_info "Release branch has been cleaned up."
}

# Start a hotfix
start_hotfix() {
    local hotfix_name=$1
    
    if [[ -z "$hotfix_name" ]]; then
        log_error "Hotfix name is required. Usage: $0 start-hotfix <hotfix-name>"
    fi
    
    local hotfix_branch="hotfix/$hotfix_name"
    
    log_warning "Starting EMERGENCY hotfix: $hotfix_name"
    
    # Check prerequisites
    check_git_status
    fetch_latest
    
    # Check if hotfix branch already exists
    if git show-ref --verify --quiet refs/heads/$hotfix_branch; then
        log_error "Hotfix branch '$hotfix_branch' already exists."
    fi
    
    # Switch to main and pull latest
    log_info "Switching to $MAIN_BRANCH branch..."
    git checkout $MAIN_BRANCH
    git pull origin $MAIN_BRANCH
    
    # Create hotfix branch
    log_info "Creating hotfix branch: $hotfix_branch"
    git checkout -b $hotfix_branch
    
    # Push hotfix branch
    git push -u origin $hotfix_branch
    
    log_success "Hotfix branch '$hotfix_branch' created and pushed to remote."
    log_warning "This is an emergency hotfix. Please:"
    log_warning "1. Fix the critical issue immediately"
    log_warning "2. Test the fix thoroughly"
    log_warning "3. Create emergency PR to $MAIN_BRANCH"
    log_warning "4. Deploy to production ASAP"
    log_warning "5. Run: $0 finish-hotfix $hotfix_name"
}

# Finish a hotfix
finish_hotfix() {
    local hotfix_name=$1
    
    if [[ -z "$hotfix_name" ]]; then
        # Try to infer from current branch
        if [[ $CURRENT_BRANCH == hotfix/* ]]; then
            hotfix_name=${CURRENT_BRANCH#hotfix/}
        else
            log_error "Hotfix name is required or run from hotfix branch. Usage: $0 finish-hotfix <hotfix-name>"
        fi
    fi
    
    local hotfix_branch="hotfix/$hotfix_name"
    local hotfix_version="v$(get_next_patch_version)"
    
    log_warning "Finishing EMERGENCY hotfix: $hotfix_name"
    
    # Check prerequisites
    check_git_status
    check_branch_exists $hotfix_branch
    fetch_latest
    
    # Switch to hotfix branch
    git checkout $hotfix_branch
    git pull origin $hotfix_branch
    
    # Update version for hotfix
    update_version $hotfix_version
    git add .
    git commit -m "chore: bump version to $hotfix_version for hotfix"
    git push origin $hotfix_branch
    
    # Merge into main
    log_info "Merging hotfix into $MAIN_BRANCH..."
    git checkout $MAIN_BRANCH
    git pull origin $MAIN_BRANCH
    git merge --no-ff $hotfix_branch -m "hotfix: $hotfix_name ($hotfix_version)"
    
    # Create tag
    log_info "Creating hotfix tag: $hotfix_version"
    git tag -a $hotfix_version -m "Hotfix $hotfix_version: $hotfix_name"
    
    # Push main and tag
    git push origin $MAIN_BRANCH
    git push origin $hotfix_version
    
    # Merge back into develop
    log_info "Merging hotfix back into $DEVELOP_BRANCH..."
    git checkout $DEVELOP_BRANCH
    git pull origin $DEVELOP_BRANCH
    git merge --no-ff $hotfix_branch -m "chore: merge hotfix $hotfix_name into develop"
    git push origin $DEVELOP_BRANCH
    
    # Delete hotfix branch
    git branch -d $hotfix_branch
    git push origin --delete $hotfix_branch
    
    log_success "Emergency hotfix $hotfix_name has been successfully completed!"
    log_success "Hotfix version $hotfix_version has been deployed."
    log_info "Hotfix branch has been cleaned up."
}

# Update version in project files
update_version() {
    local version=$1
    local version_number=${version#v}  # Remove 'v' prefix
    
    log_info "Updating version to $version in project files..."
    
    # Update .NET project files
    if [[ -f "backend/src/DemoInventory.API/DemoInventory.API.csproj" ]]; then
        sed -i.bak "s/<Version>.*<\/Version>/<Version>$version_number<\/Version>/g" backend/src/DemoInventory.API/DemoInventory.API.csproj
        rm -f backend/src/DemoInventory.API/DemoInventory.API.csproj.bak
    fi
    
    # Update package.json files
    if [[ -f "frontend/package.json" ]]; then
        sed -i.bak "s/\"version\": \".*\"/\"version\": \"$version_number\"/g" frontend/package.json
        rm -f frontend/package.json.bak
    fi
    
    # Update docker-compose.yml
    if [[ -f "docker-compose.yml" ]]; then
        sed -i.bak "s/image: .*:v.*/image: demo-inventory:$version/g" docker-compose.yml
        rm -f docker-compose.yml.bak
    fi
    
    log_success "Version updated to $version in project files."
}

# Get next patch version
get_next_patch_version() {
    local latest_tag=$(git describe --tags --abbrev=0 2>/dev/null || echo "v0.0.0")
    local version_number=${latest_tag#v}
    local major=$(echo $version_number | cut -d. -f1)
    local minor=$(echo $version_number | cut -d. -f2)
    local patch=$(echo $version_number | cut -d. -f3)
    local next_patch=$((patch + 1))
    echo "$major.$minor.$next_patch"
}

# Show current Git Flow status
status() {
    log_info "Git Flow Status"
    echo "===================="
    echo "Current branch: $CURRENT_BRANCH"
    echo "Repository: $REPO_URL"
    echo ""
    
    # Show active feature branches
    echo "Feature branches:"
    git branch -r | grep "origin/feature/" | sed 's/origin\///' || echo "  No active feature branches"
    echo ""
    
    # Show active release branches
    echo "Release branches:"
    git branch -r | grep "origin/release/" | sed 's/origin\///' || echo "  No active release branches"
    echo ""
    
    # Show active hotfix branches
    echo "Hotfix branches:"
    git branch -r | grep "origin/hotfix/" | sed 's/origin\///' || echo "  No active hotfix branches"
    echo ""
    
    # Show latest tags
    echo "Latest tags:"
    git tag --sort=-version:refname | head -5 || echo "  No tags found"
    echo ""
    
    # Show commits ahead/behind develop
    if [[ $CURRENT_BRANCH != $DEVELOP_BRANCH ]]; then
        local ahead_behind=$(git rev-list --left-right --count $DEVELOP_BRANCH...$CURRENT_BRANCH 2>/dev/null || echo "0	0")
        local ahead=$(echo $ahead_behind | cut -f2)
        local behind=$(echo $ahead_behind | cut -f1)
        echo "Compared to $DEVELOP_BRANCH:"
        echo "  $ahead commits ahead, $behind commits behind"
    fi
}

# Show help
show_help() {
    echo "Git Flow Release Management Script"
    echo "=================================="
    echo ""
    echo "Usage: $0 <command> [options]"
    echo ""
    echo "Commands:"
    echo "  start-feature <name>    Start a new feature branch"
    echo "  finish-feature [name]   Finish a feature branch"
    echo "  start-release <version> Start a new release branch"
    echo "  finish-release [version] Finish a release branch"
    echo "  start-hotfix <name>     Start a new hotfix branch"
    echo "  finish-hotfix [name]    Finish a hotfix branch"
    echo "  status                  Show Git Flow status"
    echo "  help                    Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 start-feature add-user-management"
    echo "  $0 finish-feature add-user-management"
    echo "  $0 start-release v1.2.0"
    echo "  $0 finish-release v1.2.0"
    echo "  $0 start-hotfix fix-security-vulnerability"
    echo "  $0 finish-hotfix fix-security-vulnerability"
    echo ""
    echo "Notes:"
    echo "  - Feature names should be descriptive (kebab-case)"
    echo "  - Versions should follow semantic versioning (vX.Y.Z)"
    echo "  - Hotfixes are for emergency production fixes only"
}

# Main script logic
case "$1" in
    "start-feature")
        start_feature "$2"
        ;;
    "finish-feature")
        finish_feature "$2"
        ;;
    "start-release")
        start_release "$2"
        ;;
    "finish-release")
        finish_release "$2"
        ;;
    "start-hotfix")
        start_hotfix "$2"
        ;;
    "finish-hotfix")
        finish_hotfix "$2"
        ;;
    "status")
        status
        ;;
    "help"|"--help"|"-h")
        show_help
        ;;
    *)
        log_error "Unknown command: $1"
        echo ""
        show_help
        ;;
esac
