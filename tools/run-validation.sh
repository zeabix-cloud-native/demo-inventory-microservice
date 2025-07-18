#!/bin/bash

# Legacy AI Code Validator Runner Script
# 
# 🚨 NOTICE: This script has been DEPRECATED
# 
# The validation tools have been refactored into a standalone, flexible toolkit.
# Please use the new code-validator directly for better experience:
#
#   ./tools/code-validator/run-validation.sh [options]
#
# This legacy script delegates to the new location for backward compatibility.

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Check if the new code-validator exists
NEW_SCRIPT="$SCRIPT_DIR/code-validator/run-validation.sh"

if [[ ! -f "$NEW_SCRIPT" ]]; then
    echo "❌ Error: Code validator not found at expected location: $NEW_SCRIPT"
    echo ""
    echo "The validation tools have been refactored. Please ensure the code-validator"
    echo "directory exists at: tools/code-validator/"
    echo ""
    echo "For more information, see: tools/README.md"
    exit 1
fi

# Show deprecation notice
echo "⚠️  DEPRECATION NOTICE"
echo "===================="
echo "This script is deprecated. Please use the new standalone code-validator:"
echo ""
echo "  ./tools/code-validator/run-validation.sh [options]"
echo ""
echo "The new version offers:"
echo "  • Standalone operation (works in any .NET project)"
echo "  • No hardcoded dependencies"
echo "  • Enhanced CI/CD integration"
echo "  • Better documentation and examples"
echo ""
echo "For now, delegating to the new script..."
echo ""

# Delegate to the new script with all arguments
exec "$NEW_SCRIPT" "$@"