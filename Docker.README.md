# Docker Setup

This directory contains Docker configuration files for the Demo Inventory Microservice.

## Files

- `Dockerfile` - Multi-stage Docker build file for the .NET 9 API
- `frontend/Dockerfile` - Multi-stage Docker build file for the React frontend
- `docker-compose.yml` - Basic composition with backend API and React frontend
- `docker-compose.full.yml` - Full composition including backend, React frontend, and standalone Swagger UI
- `.dockerignore` - Files to exclude from Docker build context
- `frontend/.dockerignore` - Files to exclude from frontend Docker build context

## Quick Start

### Building the Docker Image

```bash
docker build -t demo-inventory-api .
```

### Running with Docker Compose (Backend + Frontend)

```bash
docker-compose up -d
```

This setup provides:
- http://localhost:5000 - Backend API endpoints
- http://localhost:5000/swagger - Built-in Swagger UI (backend)
- http://localhost:3000 - React Frontend Application

### Running with Docker Compose (Full Setup with Standalone Swagger UI)

```bash
docker-compose -f docker-compose.full.yml up -d
```

This setup provides:
- http://localhost:5000 - Backend API endpoints
- http://localhost:5000/swagger - Built-in Swagger UI (backend)
- http://localhost:3000 - React Frontend Application  
- http://localhost:8080 - Standalone Swagger UI frontend

### Stopping Services

```bash
docker-compose down
# or for full setup
docker-compose -f docker-compose.full.yml down
```

## Configuration

### Environment Variables

The backend service supports these environment variables:

- `ASPNETCORE_ENVIRONMENT` - Set to `Development` or `Production`
- `ASPNETCORE_URLS` - URL bindings (default: `http://+:8080`)

### Ports

- Backend API: 8080 (container) → 5000 (host)
- React Frontend: 80 (container) → 3000 (host)
- Standalone Swagger UI: 8080 (container) → 8080 (host) - only in full setup

## Architecture

The Docker setup uses:

1. **Multi-stage build** - Builds the application in SDK container, runs in runtime container
2. **Non-root user** - Runs as `appuser` for security
3. **Production optimization** - Minimal runtime image
4. **Network isolation** - Services communicate through a custom bridge network