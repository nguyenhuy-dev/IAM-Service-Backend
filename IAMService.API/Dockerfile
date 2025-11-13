# Optimized for Google Kubernetes Engine (GKE)
# Using Ubuntu Chiseled images for better GCP compatibility and security

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy only project files first for better layer caching
COPY ["IAMService.Domain/IAMService.Domain.csproj", "IAMService.Domain/"]
COPY ["IAMService.Application/IAMService.Application.csproj", "IAMService.Application/"]
COPY ["IAMService.Infrastructure/IAMService.Infrastructure.csproj", "IAMService.Infrastructure/"]
COPY ["IAMService.API/IAMService.API.csproj", "IAMService.API/"]

# Restore dependencies - this layer is cached unless project files change
# Ignore test projects to avoid missing assets errors
RUN dotnet restore "IAMService.API/IAMService.API.csproj" --ignore-failed-sources

# Copy the rest of the source code
COPY . .

# Build and publish
WORKDIR "/src/IAMService.API"
RUN dotnet publish "IAMService.API.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode
FROM base AS final
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "IAMService.API.dll"]