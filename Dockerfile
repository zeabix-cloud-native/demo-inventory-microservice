# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all source files
COPY . .

# Restore dependencies and build (only for the API project to avoid missing test projects)
RUN dotnet restore backend/src/DemoInventory.API/DemoInventory.API.csproj
RUN dotnet publish backend/src/DemoInventory.API/DemoInventory.API.csproj -c Release -o /app/publish

# Use the official .NET 8 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "DemoInventory.API.dll"]