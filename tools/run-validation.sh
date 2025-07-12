#!/bin/bash

# AI Code Validator Runner Script
# Usage: ./run-validation.sh [options]

set -e

# Default values
VERBOSE=""
PATH_TO_VALIDATE="."
REPORT_FILE=""
SKIP_VALIDATIONS=""

# Help function
show_help() {
    echo "AI Code Validator Runner"
    echo "========================"
    echo ""
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  -h, --help              Show this help message"
    echo "  -v, --verbose           Enable verbose output"
    echo "  -p, --path PATH         Path to validate (default: current directory)"
    echo "  -r, --report FILE       Generate JSON report to specified file"
    echo "  -s, --skip TYPES        Skip specific validation types (comma-separated)"
    echo "                          Options: static,security,architecture"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Run all validations on current directory"
    echo "  $0 --verbose                         # Run with detailed output"
    echo "  $0 --path backend/src                # Validate specific path"
    echo "  $0 --report validation-report.json   # Generate JSON report"
    echo "  $0 --skip security,architecture      # Skip security and architecture checks"
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

# Find the project root (look for DemoInventory.sln)
PROJECT_ROOT="$(pwd)"
while [[ "$PROJECT_ROOT" != "/" && ! -f "$PROJECT_ROOT/DemoInventory.sln" ]]; do
    PROJECT_ROOT="$(dirname "$PROJECT_ROOT")"
done

if [[ ! -f "$PROJECT_ROOT/DemoInventory.sln" ]]; then
    echo "Error: Could not find DemoInventory.sln. Please run this script from within the project directory."
    exit 1
fi

# Change to project root
cd "$PROJECT_ROOT"

# Build the validator tool first
echo "🔨 Building AI Code Validator..."
dotnet build tools/DemoInventory.Tools.AICodeValidator --nologo --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Failed to build AI Code Validator"
    exit 1
fi

echo "✅ Build successful"
echo ""

# Run the validator
echo "🤖 Running AI Code Validator..."
echo "================================"

# Construct the command
CMD="dotnet run --project tools/DemoInventory.Tools.AICodeValidator --no-build -- --path \"$PATH_TO_VALIDATE\" $VERBOSE $REPORT_FILE $SKIP_VALIDATIONS"

# Execute the command
eval $CMD

# Capture exit code
EXIT_CODE=$?

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo "✅ Validation completed successfully!"
else
    echo "❌ Validation completed with issues (exit code: $EXIT_CODE)"
fi

# Show report location if generated
if [[ -n "$REPORT_FILE" ]]; then
    REPORT_PATH=$(echo "$REPORT_FILE" | sed 's/--report //')
    if [[ -f "$REPORT_PATH" ]]; then
        echo "📄 JSON report saved to: $REPORT_PATH"
    fi
fi

exit $EXIT_CODE