# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f apps/DataAggregator/Dockerfile .

# We use 7.0-bookworm-slim for Debian 12 with glibc 2.36+, to get at least glibc 2.33 for the engine toolkit
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bookworm-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 1235

FROM mcr.microsoft.com/dotnet/sdk:7.0-bookworm-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore "apps/DataAggregator/DataAggregator.csproj"
RUN dotnet build "apps/DataAggregator/DataAggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apps/DataAggregator/DataAggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN groupadd --gid 1000 data-aggregator && \
    useradd --uid 1000 --gid 1000 -m data-aggregator

USER 1000

ENTRYPOINT ["dotnet", "DataAggregator.dll"]

# TODO use local paths and adjust context for CI/CD
