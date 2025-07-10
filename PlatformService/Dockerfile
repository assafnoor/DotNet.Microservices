# Base image (runtime only)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

RUN adduser --disabled-password --gecos "" appuser && \
    mkdir -p /app/logs && chown -R appuser:appuser /app && chmod 750 /app/logs && \
    apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["PlatformService/PlatformService.csproj", "PlatformService/"]
RUN dotnet restore "PlatformService/PlatformService.csproj"

COPY . .
WORKDIR "/src/PlatformService"
RUN dotnet build "PlatformService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish image
FROM build AS publish
RUN dotnet publish "PlatformService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PlatformService.dll"]
