#!/usr/bin/env node

const { spawn } = require('child_process');
const { existsSync } = require('fs');
const path = require('path');
const http = require('http');

// Configuration
const COLLECTION_FILE = path.join(__dirname, 'collection.json');
const LOCAL_ENV_FILE = path.join(__dirname, 'environment.json');
const DOCKER_ENV_FILE = path.join(__dirname, 'docker-environment.json');

// Function to check if a URL is accessible
function checkUrl(url) {
    return new Promise((resolve) => {
        const timeout = setTimeout(() => {
            resolve(false);
        }, 3000);

        const req = http.get(url, (res) => {
            clearTimeout(timeout);
            resolve(res.statusCode >= 200 && res.statusCode < 400);
        });

        req.on('error', () => {
            clearTimeout(timeout);
            resolve(false);
        });

        req.setTimeout(3000, () => {
            clearTimeout(timeout);
            req.destroy();
            resolve(false);
        });
    });
}

// Function to detect environment
async function detectEnvironment() {
    console.log('Auto-detecting environment...');
    
    // Check for environment variable override
    if (process.env.NEWMAN_BASE_URL) {
        console.log(`Environment variable NEWMAN_BASE_URL is set to: ${process.env.NEWMAN_BASE_URL}`);
        // Determine environment based on URL
        if (process.env.NEWMAN_BASE_URL.includes(':5126')) {
            return 'local';
        } else if (process.env.NEWMAN_BASE_URL.includes(':5000')) {
            return 'docker';
        } else {
            return 'local';  // Default to local for custom URLs
        }
    }
    
    // Check if local API is running (port 5126)
    if (await checkUrl('http://localhost:5126/api/products')) {
        return 'local';
    }
    
    // Check if Docker API is running (port 5000)  
    if (await checkUrl('http://localhost:5000/api/products')) {
        return 'docker';
    }
    
    // Default to local if neither is detected
    return 'local';
}

// Main function
async function main() {
    const args = process.argv.slice(2);
    
    // If first argument is not a known environment, default to 'auto'
    let environment = 'auto';
    let newmanArgs = args;
    
    if (args.length > 0 && ['local', 'docker', 'auto'].includes(args[0])) {
        environment = args[0];
        newmanArgs = args.slice(1);
    }

    let environmentFile;
    let environmentName;

    // Determine which environment file to use
    switch (environment) {
        case 'local':
            environmentFile = LOCAL_ENV_FILE;
            environmentName = 'local environment (port 5126)';
            break;
        case 'docker':
            environmentFile = DOCKER_ENV_FILE;
            environmentName = 'Docker environment (port 5000)';
            break;
        case 'auto':
            // Check for environment variable override
            if (process.env.NEWMAN_BASE_URL) {
                console.log(`Environment variable NEWMAN_BASE_URL is set to: ${process.env.NEWMAN_BASE_URL}`);
                // Create a temporary environment file
                const tempEnvFile = path.join(__dirname, 'temp-environment.json');
                const tempEnv = {
                    id: 'temp-environment',
                    name: 'Custom Environment',
                    values: [
                        {
                            key: 'baseUrl',
                            value: process.env.NEWMAN_BASE_URL,
                            type: 'default',
                            enabled: true
                        }
                    ],
                    _postman_variable_scope: 'environment'
                };
                
                require('fs').writeFileSync(tempEnvFile, JSON.stringify(tempEnv, null, 2));
                environmentFile = tempEnvFile;
                environmentName = `custom environment (${process.env.NEWMAN_BASE_URL})`;
                console.log(`Using custom environment: ${process.env.NEWMAN_BASE_URL}`);
            } else {
                // Original auto-detection logic
                const detectedEnv = await detectEnvironment();
                if (detectedEnv === 'local') {
                    environmentFile = LOCAL_ENV_FILE;
                    environmentName = 'local environment (port 5126)';
                    console.log('Auto-detected: local environment (port 5126)');
                } else {
                    environmentFile = DOCKER_ENV_FILE;
                    environmentName = 'Docker environment (port 5000)';
                    console.log('Auto-detected: Docker environment (port 5000)');
                }
            }
            break;
        default:
            console.error(`Error: Invalid environment '${environment}'. Use: local, docker, or auto`);
            process.exit(1);
    }

    // Check if files exist
    if (!existsSync(COLLECTION_FILE)) {
        console.error(`Error: Collection file not found: ${COLLECTION_FILE}`);
        process.exit(1);
    }

    if (!existsSync(environmentFile)) {
        console.error(`Error: Environment file not found: ${environmentFile}`);
        process.exit(1);
    }

    // Build Newman command
    const newmanCommand = [
        'run',
        COLLECTION_FILE,
        '--environment',
        environmentFile,
        ...newmanArgs
    ];

    console.log(`Using ${environmentName}`);
    console.log(`Running Newman collection with environment file: ${path.basename(environmentFile)}`);

    // Run Newman
    const newman = spawn('newman', newmanCommand, {
        stdio: 'inherit'
    });

    newman.on('close', (code) => {
        // Cleanup temporary files
        const tempEnvFile = path.join(__dirname, 'temp-environment.json');
        if (existsSync(tempEnvFile)) {
            require('fs').unlinkSync(tempEnvFile);
        }
        process.exit(code);
    });

    newman.on('error', (err) => {
        // Cleanup temporary files
        const tempEnvFile = path.join(__dirname, 'temp-environment.json');
        if (existsSync(tempEnvFile)) {
            require('fs').unlinkSync(tempEnvFile);
        }
        
        if (err.code === 'ENOENT') {
            console.error('Error: newman command not found. Please install Newman: npm install -g newman');
        } else {
            console.error('Error running Newman:', err.message);
        }
        process.exit(1);
    });
}

// Handle unhandled promise rejections
process.on('unhandledRejection', (err) => {
    console.error('Error:', err.message);
    process.exit(1);
});

main();