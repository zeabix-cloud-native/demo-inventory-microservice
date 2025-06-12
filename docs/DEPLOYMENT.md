# Deployment Guide

## Overview

The Demo Inventory Microservice is designed for flexible deployment using Docker containers. This guide covers local development, containerized deployment, and production deployment strategies.

## Quick Start with Docker

### Prerequisites

- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** 
- **[Docker Compose](https://docs.docker.com/compose/install/)** (included with Docker Desktop)

### Option 1: Complete Stack (Recommended)

```bash
# Clone the repository
git clone https://github.com/zeabix-cloud-native/demo-inventory-microservice.git
cd demo-inventory-microservice

# Start all services
docker-compose up -d

# Verify services are running
docker-compose ps
```

**Services Available:**
- **API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **React Frontend**: http://localhost:3000
- **PostgreSQL**: localhost:5432

### Option 2: Full Stack with Standalone Swagger UI

```bash
# Start complete stack with additional Swagger UI frontend
docker-compose -f docker-compose.full.yml up -d
```

**Additional Service:**
- **Standalone Swagger UI**: http://localhost:8080

## Container Architecture

### Docker Images

#### Backend API (`Dockerfile`)

```dockerfile
# Multi-stage build for optimized production image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy and restore dependencies
COPY backend/src/DemoInventory.API/*.csproj ./DemoInventory.API/
COPY backend/src/DemoInventory.Application/*.csproj ./DemoInventory.Application/
COPY backend/src/DemoInventory.Domain/*.csproj ./DemoInventory.Domain/
COPY backend/src/DemoInventory.Infrastructure/*.csproj ./DemoInventory.Infrastructure/

RUN dotnet restore DemoInventory.API/DemoInventory.API.csproj

# Copy source and build
COPY backend/src/ ./
RUN dotnet publish DemoInventory.API/DemoInventory.API.csproj -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "DemoInventory.API.dll"]
```

#### Frontend (`frontend/Dockerfile`)

```dockerfile
# Multi-stage build for React application
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci --only=production

# Copy source and build
COPY . .
ARG VITE_API_BASE_URL=http://localhost:5000/api
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# Production image with Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Service Configuration

#### docker-compose.yml

```yaml
version: '3.8'

services:
  db:
    image: postgres:15
    environment:
      POSTGRES_DB: demo_inventory
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Host=db;Database=demo_inventory;Username=postgres;Password=password
    ports:
      - "5000:8080"
    depends_on:
      db:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
      args:
        VITE_API_BASE_URL: http://localhost:5000/api
    ports:
      - "3000:80"
    depends_on:
      - api

volumes:
  postgres_data:
```

## Environment Configuration

### Environment Variables

#### Backend API

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` | `Development` |
| `ASPNETCORE_URLS` | URL bindings | `http://+:8080` | `http://+:5000` |
| `ConnectionStrings__DefaultConnection` | Database connection | - | `Host=localhost;Database=demo_inventory...` |
| `USE_IN_MEMORY_DB` | Use in-memory database | `false` | `true` |

#### Frontend

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `VITE_API_BASE_URL` | Backend API URL | `http://localhost:5126/api` | `http://api.example.com/api` |

### Configuration Files

#### appsettings.json (Backend)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=demo_inventory;Username=postgres;Password=password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

#### .env (Frontend)

```bash
VITE_API_BASE_URL=http://localhost:5126/api
```

## Production Deployment

### Cloud Deployment Options

#### Option 1: Container Registry + Orchestration

```bash
# Build and tag images
docker build -t demo-inventory-api:latest .
docker build -t demo-inventory-frontend:latest ./frontend

# Tag for registry
docker tag demo-inventory-api:latest your-registry/demo-inventory-api:latest
docker tag demo-inventory-frontend:latest your-registry/demo-inventory-frontend:latest

# Push to registry
docker push your-registry/demo-inventory-api:latest
docker push your-registry/demo-inventory-frontend:latest
```

#### Option 2: Docker Swarm

```yaml
# docker-stack.yml
version: '3.8'

services:
  db:
    image: postgres:15
    environment:
      POSTGRES_DB: demo_inventory
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD_FILE: /run/secrets/db_password
    secrets:
      - db_password
    volumes:
      - postgres_data:/var/lib/postgresql/data
    deploy:
      replicas: 1
      placement:
        constraints: [node.role == manager]

  api:
    image: your-registry/demo-inventory-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Database=demo_inventory;Username=postgres;Password_FILE=/run/secrets/db_password
    secrets:
      - db_password
    ports:
      - "5000:8080"
    deploy:
      replicas: 2
      update_config:
        parallelism: 1
        delay: 10s
      restart_policy:
        condition: on-failure

  frontend:
    image: your-registry/demo-inventory-frontend:latest
    ports:
      - "3000:80"
    deploy:
      replicas: 2
      update_config:
        parallelism: 1
        delay: 10s

secrets:
  db_password:
    external: true

volumes:
  postgres_data:
```

Deploy with:
```bash
docker stack deploy -c docker-stack.yml demo-inventory
```

#### Option 3: Kubernetes

```yaml
# k8s-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: demo-inventory-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: demo-inventory-api
  template:
    metadata:
      labels:
        app: demo-inventory-api
    spec:
      containers:
      - name: api
        image: your-registry/demo-inventory-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-connection
              key: connection-string
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: demo-inventory-api-service
spec:
  selector:
    app: demo-inventory-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: LoadBalancer
```

### Managed Service Deployments

#### Azure Container Apps

```bash
# Create resource group
az group create --name demo-inventory-rg --location eastus

# Create container app environment
az containerapp env create \
  --name demo-inventory-env \
  --resource-group demo-inventory-rg \
  --location eastus

# Deploy API
az containerapp create \
  --name demo-inventory-api \
  --resource-group demo-inventory-rg \
  --environment demo-inventory-env \
  --image your-registry/demo-inventory-api:latest \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 5

# Deploy Frontend
az containerapp create \
  --name demo-inventory-frontend \
  --resource-group demo-inventory-rg \
  --environment demo-inventory-env \
  --image your-registry/demo-inventory-frontend:latest \
  --target-port 80 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 3
```

#### AWS ECS Fargate

```json
{
  "family": "demo-inventory-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "256",
  "memory": "512",
  "executionRoleArn": "arn:aws:iam::ACCOUNT:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "demo-inventory-api",
      "image": "your-registry/demo-inventory-api:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__DefaultConnection",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:demo-inventory-db"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/demo-inventory-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

## Database Deployment

### PostgreSQL Options

#### Option 1: Managed Database Service

**AWS RDS PostgreSQL:**
```bash
aws rds create-db-instance \
  --db-instance-identifier demo-inventory-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --engine-version 13.7 \
  --allocated-storage 20 \
  --db-name demo_inventory \
  --master-username postgres \
  --master-user-password YourSecurePassword \
  --vpc-security-group-ids sg-xxxxxxxxx \
  --availability-zone us-east-1a \
  --backup-retention-period 7 \
  --storage-encrypted
```

**Azure Database for PostgreSQL:**
```bash
az postgres server create \
  --resource-group demo-inventory-rg \
  --name demo-inventory-db-server \
  --location eastus \
  --admin-user postgres \
  --admin-password YourSecurePassword \
  --sku-name GP_Gen5_2 \
  --version 13
```

#### Option 2: Self-Managed PostgreSQL

```yaml
# postgresql-deployment.yml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgresql
spec:
  serviceName: postgresql
  replicas: 1
  selector:
    matchLabels:
      app: postgresql
  template:
    metadata:
      labels:
        app: postgresql
    spec:
      containers:
      - name: postgresql
        image: postgres:15
        env:
        - name: POSTGRES_DB
          value: demo_inventory
        - name: POSTGRES_USER
          value: postgres
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgresql-secret
              key: password
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: postgresql-storage
          mountPath: /var/lib/postgresql/data
  volumeClaimTemplates:
  - metadata:
      name: postgresql-storage
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 20Gi
```

### Database Migrations

#### Automatic Migrations (Development)

```csharp
public static async Task Main(string[] args)
{
    var host = CreateHostBuilder(args).Build();
    
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await context.Database.MigrateAsync();
    }
    
    await host.RunAsync();
}
```

#### Manual Migrations (Production)

```bash
# Generate migration script
dotnet ef migrations script --output migration.sql

# Apply to production database
psql -h your-db-host -U postgres -d demo_inventory -f migration.sql
```

## Monitoring and Health Checks

### Health Check Endpoints

```csharp
// Configure health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddCheck("api", () => HealthCheckResult.Healthy());

// Configure endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
```

### Docker Health Checks

```dockerfile
# Add health check to Dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
```

### Monitoring with Prometheus

```yaml
# prometheus-config.yml
global:
  scrape_interval: 15s

scrape_configs:
  - job_name: 'demo-inventory-api'
    static_configs:
      - targets: ['api:8080']
    metrics_path: '/metrics'
    scrape_interval: 5s
```

## Security Considerations

### Production Security Checklist

- [ ] **Secrets Management**: Use secure secret storage (AWS Secrets Manager, Azure Key Vault)
- [ ] **HTTPS**: Enable TLS/SSL certificates
- [ ] **CORS**: Configure appropriate CORS policies
- [ ] **Authentication**: Implement JWT or OAuth 2.0
- [ ] **Rate Limiting**: Implement API rate limiting
- [ ] **Input Validation**: Validate all user inputs
- [ ] **Database Security**: Use connection pooling and prepared statements
- [ ] **Container Security**: Run containers as non-root users
- [ ] **Network Security**: Use private networks and security groups

### Example Security Configuration

```csharp
// HTTPS and security headers
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    await next();
});

// CORS configuration
app.UseCors(builder =>
{
    builder.WithOrigins("https://yourdomain.com")
           .AllowCredentials()
           .AllowAnyMethod()
           .AllowAnyHeader();
});
```

## Performance Optimization

### Production Optimizations

#### Backend Optimizations

```csharp
// Connection pooling
builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(3);
        npgsqlOptions.CommandTimeout(30);
    });
}, ServiceLifetime.Scoped);

// Response caching
builder.Services.AddResponseCaching();
app.UseResponseCaching();

// Response compression
builder.Services.AddResponseCompression();
app.UseResponseCompression();
```

#### Frontend Optimizations

```dockerfile
# Multi-stage build with optimization
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build

# Optimized Nginx configuration
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
RUN gzip -9 -r /usr/share/nginx/html/
EXPOSE 80
```

### Scaling Strategies

1. **Horizontal Scaling**: Multiple API instances behind load balancer
2. **Database Scaling**: Read replicas for read-heavy workloads
3. **Caching**: Redis for frequently accessed data
4. **CDN**: Content delivery network for static assets
5. **Auto-scaling**: Automatic scaling based on CPU/memory usage

## Troubleshooting

### Common Deployment Issues

#### Container Startup Issues

```bash
# Check container logs
docker logs demo-inventory-api

# Check container status
docker ps -a

# Inspect container configuration
docker inspect demo-inventory-api

# Execute shell in container
docker exec -it demo-inventory-api /bin/bash
```

#### Database Connection Issues

```bash
# Test database connectivity
docker exec -it demo-inventory-db psql -U postgres -d demo_inventory

# Check database logs
docker logs demo-inventory-db

# Verify network connectivity
docker network ls
docker network inspect demo-inventory_default
```

#### Performance Issues

```bash
# Monitor resource usage
docker stats

# Check application metrics
curl http://localhost:5000/metrics

# Database performance
docker exec -it demo-inventory-db psql -U postgres -c "SELECT * FROM pg_stat_activity;"
```

### Rollback Strategies

#### Docker Compose Rollback

```bash
# Tag current version
docker tag demo-inventory-api:latest demo-inventory-api:backup

# Deploy previous version
docker-compose down
docker-compose up -d --build
```

#### Kubernetes Rollback

```bash
# Check rollout history
kubectl rollout history deployment/demo-inventory-api

# Rollback to previous version
kubectl rollout undo deployment/demo-inventory-api

# Rollback to specific revision
kubectl rollout undo deployment/demo-inventory-api --to-revision=2
```