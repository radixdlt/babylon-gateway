# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f apps/DatabaseMigrations/Dockerfile .

# We use 7.0-bookworm-slim for Debian 12 with glibc 2.36+, to get at least glibc 2.33 for the engine toolkit
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bookworm-slim AS base
WORKDIR /app

# Fixes CVE-2023-4911 can be removed when we update the base OS image to include this fix
RUN apt-get update -y \
  && apt-get -y --no-install-recommends install \    
    libc6=2.36-9+deb12u3 \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore "apps/DatabaseMigrations/DatabaseMigrations.csproj"
RUN dotnet build "apps/DatabaseMigrations/DatabaseMigrations.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apps/DatabaseMigrations/DatabaseMigrations.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN groupadd --gid 1000 app && \
    useradd --uid 1000 --gid 1000 -m app

USER 1000

ENTRYPOINT ["dotnet", "DatabaseMigrations.dll"]

# TODO use local paths and adjust context for CI/CD
