#!/bin/bash

# Newman test runner with environment detection
# Usage: ./run-newman.sh [environment] [additional_newman_args...]
# Environment can be: local, docker, auto (default: auto)

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
COLLECTION_FILE="$SCRIPT_DIR/collection.json"

# Function to check if a URL is accessible
check_url() {
    local url="$1"
    timeout 5 curl -s -f "$url" > /dev/null 2>&1
}

# Function to detect environment
detect_environment() {
    echo "Auto-detecting environment..." >&2
    
    # Check for environment variable override
    if [ -n "$NEWMAN_BASE_URL" ]; then
        echo "Environment variable NEWMAN_BASE_URL is set to: $NEWMAN_BASE_URL" >&2
        # Determine environment based on URL
        if [[ "$NEWMAN_BASE_URL" == *":5126"* ]]; then
            echo "local"
        elif [[ "$NEWMAN_BASE_URL" == *":5000"* ]]; then
            echo "docker"
        else
            echo "local"  # Default to local for custom URLs
        fi
        return 0
    fi
    
    # Check if local API is running (port 5126)
    if check_url "http://localhost:5126/api/products"; then
        echo "local"
        return 0
    fi
    
    # Check if Docker API is running (port 5000)  
    if check_url "http://localhost:5000/api/products"; then
        echo "docker"
        return 0
    fi
    
    # Default to local if neither is detected
    echo "local"
}

# Get environment parameter
ENVIRONMENT="${1:-auto}"
shift 2>/dev/null || true  # Remove first argument if it exists

# Determine which environment file to use
case "$ENVIRONMENT" in
    "local")
        ENV_FILE="$SCRIPT_DIR/environment.json"
        echo "Using local environment (port 5126)" >&2
        ;;
    "docker")
        ENV_FILE="$SCRIPT_DIR/docker-environment.json"
        echo "Using Docker environment (port 5000)" >&2
        ;;
    "auto")
        DETECTED_ENV=$(detect_environment)
        case "$DETECTED_ENV" in
            "local")
                ENV_FILE="$SCRIPT_DIR/environment.json"
                echo "Auto-detected: local environment (port 5126)" >&2
                ;;
            "docker")
                ENV_FILE="$SCRIPT_DIR/docker-environment.json"
                echo "Auto-detected: Docker environment (port 5000)" >&2
                ;;
        esac
        ;;
    *)
        echo "Error: Invalid environment '$ENVIRONMENT'. Use: local, docker, or auto" >&2
        exit 1
        ;;
esac

# Check if environment file exists
if [ ! -f "$ENV_FILE" ]; then
    echo "Error: Environment file not found: $ENV_FILE" >&2
    exit 1
fi

# Run Newman with the selected environment
echo "Running Newman collection with environment file: $(basename "$ENV_FILE")" >&2
exec newman run "$COLLECTION_FILE" --environment "$ENV_FILE" "$@"