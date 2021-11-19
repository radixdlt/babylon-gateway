# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f src/DataAggregator/Dockerfile .

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/DataAggregator/DataAggregator.csproj"
RUN dotnet build "src/DataAggregator/DataAggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/DataAggregator/DataAggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataAggregator.dll"]
