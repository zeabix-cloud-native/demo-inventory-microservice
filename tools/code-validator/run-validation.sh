#!/bin/bash

# Code Validator Runner Script (Standalone Version)
# Usage: ./run-validation.sh [options]

set -e

# Default values
VERBOSE=""
PATH_TO_VALIDATE="."
REPORT_FILE=""
SKIP_VALIDATIONS=""
CTRF_REPORT=""

# Help function
show_help() {
    echo "Code Validator Runner (Standalone)"
    echo "=================================="
    echo ""
    echo "This is a standalone, flexible code validation toolkit that can be used"
    echo "in any .NET Clean Architecture project."
    echo ""
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  -h, --help              Show this help message"
    echo "  -v, --verbose           Enable verbose output with detailed issue information"
    echo "  -p, --path PATH         Path to validate (default: current directory)"
    echo "  -r, --report FILE       Generate comprehensive JSON report to specified file"
    echo "  -c, --ctrf FILE         Generate CTRF (Common Test Report Format) JSON report"
    echo "  -s, --skip TYPES        Skip specific validation types (comma-separated)"
    echo "                          Available types: static,security,architecture"
    echo ""
    echo "Examples:"
    echo "  $0                                        # Run all validations on current directory"
    echo "  $0 --verbose                             # Run with detailed output and issue explanations"
    echo "  $0 --path backend/src                    # Validate specific path"
    echo "  $0 --report validation-report.json       # Generate comprehensive JSON report"
    echo "  $0 --ctrf security-results.json          # Generate CTRF format report for CI/CD"
    echo "  $0 --skip security,architecture          # Skip security and architecture checks"
    echo "  $0 -v -r report.json -c ctrf.json        # Combine multiple output formats"
    echo ""
    echo "Features:"
    echo "  • Works with any .NET Clean Architecture project"
    echo "  • No hardcoded dependencies on specific project structure"
    echo "  • Comprehensive security scanning with OWASP guidelines"
    echo "  • Architecture validation for Clean Architecture principles"
    echo "  • Static analysis for code quality and complexity"
    echo "  • Multiple output formats for CI/CD integration"
    echo ""
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -v|--verbose)
            VERBOSE="--verbose"
            shift
            ;;
        -p|--path)
            PATH_TO_VALIDATE="$2"
            shift 2
            ;;
        -r|--report)
            REPORT_FILE="--report $2"
            shift 2
            ;;
        -c|--ctrf)
            CTRF_REPORT="--ctrf $2"
            shift 2
            ;;
        -s|--skip)
            # Convert comma-separated values to space-separated for dotnet command
            IFS=',' read -ra SKIP_ARRAY <<< "$2"
            SKIP_VALIDATIONS="--skip ${SKIP_ARRAY[*]}"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Change to the script directory (code-validator directory)
cd "$SCRIPT_DIR"

# Build the validator tool first
echo "🔨 Building Code Validator..."
dotnet build AICodeValidator --nologo --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Failed to build Code Validator"
    exit 1
fi

echo "✅ Build successful"
echo ""

# Run the validator
echo "🤖 Running Code Validator..."
echo "==============================="

# For security scanning, exclude the tools directory to prevent false positives from security pattern definitions
if [[ "$SKIP_VALIDATIONS" != *"security"* ]]; then
    echo "📋 Note: Excluding 'tools' directory from security scan to prevent false positives from pattern definitions"
fi

# Construct the command - use absolute path for the validation target
if [[ "$PATH_TO_VALIDATE" == "." ]]; then
    VALIDATION_TARGET="$(pwd)"
else
    VALIDATION_TARGET="$(realpath "$PATH_TO_VALIDATE" 2>/dev/null || echo "$PATH_TO_VALIDATE")"
fi

CMD="dotnet run --project AICodeValidator --no-build -- --path \"$VALIDATION_TARGET\" $VERBOSE $REPORT_FILE $CTRF_REPORT $SKIP_VALIDATIONS"

# Execute the command
eval $CMD

# Capture exit code
EXIT_CODE=$?

echo ""
echo "📋 Validation Summary"
echo "==================="
echo "🎯 Target: $PATH_TO_VALIDATE"
echo "⏱️  Completed: $(date '+%Y-%m-%d %H:%M:%S')"
if [[ -n "$VERBOSE" ]]; then
    echo "📝 Output: Detailed (verbose mode enabled)"
else
    echo "📝 Output: Standard (use --verbose for more details)"
fi

# Show what validations were run
VALIDATIONS_RUN=""
if [[ ! "$SKIP_VALIDATIONS" =~ "static" ]]; then
    VALIDATIONS_RUN="$VALIDATIONS_RUN Static Analysis,"
fi
if [[ ! "$SKIP_VALIDATIONS" =~ "security" ]]; then
    VALIDATIONS_RUN="$VALIDATIONS_RUN Security Scan,"
fi
if [[ ! "$SKIP_VALIDATIONS" =~ "architecture" ]]; then
    VALIDATIONS_RUN="$VALIDATIONS_RUN Architecture Validation,"
fi
VALIDATIONS_RUN=${VALIDATIONS_RUN%,}  # Remove trailing comma
echo "🔍 Validations: ${VALIDATIONS_RUN:-"None (all skipped)"}"

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "✅ Validation completed successfully!"
    echo "   └─ All checks passed - your code meets quality standards"
else
    echo "❌ Validation completed with issues (exit code: $EXIT_CODE)"
    echo "   └─ Review the detailed output above for specific recommendations"
    echo "   └─ Use --verbose flag for additional context and guidance"
fi

# Show report location if generated
if [[ -n "$REPORT_FILE" ]]; then
    REPORT_PATH=$(echo "$REPORT_FILE" | sed 's/--report //')
    if [[ -f "$REPORT_PATH" ]]; then
        echo "📄 Comprehensive JSON report saved to: $REPORT_PATH"
        echo "   └─ Contains detailed metrics, recommendations, and implementation timelines"
    fi
fi

# Show CTRF report location if generated
if [[ -n "$CTRF_REPORT" ]]; then
    CTRF_PATH=$(echo "$CTRF_REPORT" | sed 's/--ctrf //')
    if [[ -f "$CTRF_PATH" ]]; then
        echo "🔍 CTRF security report saved to: $CTRF_PATH"
        echo "   └─ Industry-standard format for CI/CD pipeline integration"
    fi
fi

exit $EXIT_CODE