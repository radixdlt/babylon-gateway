# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f apps/DataAggregator/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 1235

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "apps/DataAggregator/DataAggregator.csproj"
RUN dotnet build "apps/DataAggregator/DataAggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apps/DataAggregator/DataAggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataAggregator.dll"]
COPY Directory.Build.props .

# TODO use local paths and adjust context for CI/CD
