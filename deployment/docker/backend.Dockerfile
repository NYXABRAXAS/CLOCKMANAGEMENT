# STLMS API (ASP.NET Core / .NET 10)
# Build context MUST be the repo root, e.g.:
#   docker build -f deployment/docker/backend.Dockerfile -t stlms-api .

# ---------- Stage 1: build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/src/STLMS.Domain/STLMS.Domain.csproj backend/src/STLMS.Domain/
COPY backend/src/STLMS.Application/STLMS.Application.csproj backend/src/STLMS.Application/
COPY backend/src/STLMS.Infrastructure/STLMS.Infrastructure.csproj backend/src/STLMS.Infrastructure/
COPY backend/src/STLMS.API/STLMS.API.csproj backend/src/STLMS.API/
RUN dotnet restore backend/src/STLMS.API/STLMS.API.csproj

COPY backend/src backend/src
RUN dotnet publish backend/src/STLMS.API/STLMS.API.csproj -c Release -o /app/publish --no-restore

# ---------- Stage 2: runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN adduser --disabled-password --gecos "" stlms

COPY --from=build /app/publish .

RUN mkdir -p /app/App_Data /app/logs && chown -R stlms:stlms /app
USER stlms

# Render (and most PaaS hosts) inject PORT at runtime and expect the container to bind to it -
# ASP.NET Core doesn't read that env var itself, so expand it into --urls via a shell entrypoint.
# Falls back to 8080 for plain `docker run`/docker-compose where PORT isn't set.
ENV PORT=8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s CMD wget -qO- http://localhost:${PORT}/api/v1/health || exit 1

ENTRYPOINT ["/bin/sh", "-c", "exec dotnet STLMS.API.dll --urls http://+:${PORT}"]
