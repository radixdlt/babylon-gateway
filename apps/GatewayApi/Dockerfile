# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f apps/GatewayAPI/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src
COPY babylon-gateway.sln \
     babylon-gateway.sln.DotSettings \
     CHANGELOG.md \
     Directory.Packages.props \
     .editorconfig \
     stylecop.json /src/
COPY src /src/src
COPY apps /src/apps
COPY Directory.Build.props /src/Directory.Build.props
RUN dotnet restore "apps/GatewayApi/GatewayApi.csproj"
RUN dotnet build "apps/GatewayApi/GatewayApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apps/GatewayApi/GatewayApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER app

ENTRYPOINT ["dotnet", "GatewayApi.dll"]
