FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY ["WannabeTrello.sln", "./"]
COPY ["global.json", "./"]
COPY ["WannabeTrello.API/WannabeTrello.API.csproj", "WannabeTrello.API/"]
COPY ["WannabeTrello.Application/WannabeTrello.Application.csproj", "WannabeTrello.Application/"]
COPY ["WannabeTrello.Domain/WannabeTrello.Domain.csproj", "WannabeTrello.Domain/"]
COPY ["WannabeTrello.Infrastructure/WannabeTrello.Infrastructure.csproj", "WannabeTrello.Infrastructure/"]

# Restore dependencies (this layer will be cached if project files don't change)
RUN dotnet restore "WannabeTrello.API/WannabeTrello.API.csproj"

# Copy source code (only this layer will be rebuilt when code changes)
COPY . .

# Build the application
WORKDIR "/src/WannabeTrello.API"
RUN dotnet build "WannabeTrello.API.csproj" -c Release -o /app/build --no-restore

# Publish the application
FROM build AS publish
RUN dotnet publish "WannabeTrello.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup --shell /bin/false appuser

RUN apt-get update && apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/*


COPY --from=publish /app/publish .

RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# HEALTH CHECK - Docker proverava da li je kontejner zdrav
# --interval=30s  - proverava svakih 30 sekundi
# --timeout=10s   - čeka max 10 sekundi na odgovor
# --start-period=5s - čeka 5 sekundi pre prve provere (za startup)
# --retries=3     - nakon 3 neuspešna pokušaja, kontejner je "unhealthy"
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "WannabeTrello.API.dll"]