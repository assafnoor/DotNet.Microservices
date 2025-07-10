
# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 6000
EXPOSE 6001

ENV DOTNET_RUNNING_IN_CONTAINER=true

RUN adduser --disabled-password --gecos "" appuser && \
    mkdir -p /app/logs && chown -R appuser:appuser /app && chmod 750 /app/logs && \
    apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=20s --retries=3 \
    CMD curl --fail http://localhost:6000/health || exit 1

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CommandsService/CommandsService.csproj", "CommandsService/"]
RUN dotnet restore "./CommandsService/CommandsService.csproj"
COPY . .
WORKDIR "/src/CommandsService"
RUN dotnet build "./CommandsService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CommandsService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CommandsService.dll"]