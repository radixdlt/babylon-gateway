# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f apps/GatewayAPI/Dockerfile .

# We use 7.0-bookworm-slim for Debian 12 with glibc 2.36+, to get at least glibc 2.33 for the engine toolkit
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bookworm-slim AS base
WORKDIR /app

# Fixes CVE-2023-4911 can be removed when we update the base OS image to include this fix
# docker run -it mcr.microsoft.com/dotnet/aspnet:7.0-bookworm-slim ldd --version
# This fix can be removed as long as the version printed in the above command is 2.36-9+deb12u3 or above
RUN apt-get update -y \
  && apt-get -y --no-install-recommends install \    
    libc6=2.36-9+deb12u3 \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim AS build
WORKDIR /src
COPY sdk /src/sdk
COPY Directory.Build.props /src/Directory.Build.props
COPY Directory.Packages.props /src/Directory.Packages.props
COPY babylon-gateway.sln /src/
COPY babylon-gateway.sln.DotSettings /src/
COPY tests /src/tests
COPY CHANGELOG.md /src/
COPY .editorconfig /src/
COPY stylecop.json /src/
COPY src /src/src
COPY apps /src/apps
RUN dotnet restore "apps/GatewayApi/GatewayApi.csproj"
RUN dotnet build "apps/GatewayApi/GatewayApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apps/GatewayApi/GatewayApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN groupadd --gid 1000 app && \
    useradd --uid 1000 --gid 1000 -m app

USER 1000

ENTRYPOINT ["dotnet", "GatewayApi.dll"]

# TODO use local paths and adjust context for CI/CD
