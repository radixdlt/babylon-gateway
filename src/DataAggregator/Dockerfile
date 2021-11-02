FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DataAggregator/DataAggregator.csproj", "DataAggregator/"]
RUN dotnet restore "DataAggregator/DataAggregator.csproj"
COPY . .
WORKDIR "/src/DataAggregator"
RUN dotnet build "DataAggregator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DataAggregator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DataAggregator.dll"]
