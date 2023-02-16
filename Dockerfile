FROM babylon_gateway:restore
COPY ./apps ./apps
COPY ./src ./src
COPY ./tests ./tests
RUN dotnet build ./babylon-gateway.sln -c Release --no-restore && \
    dotnet publish ./apps/DatabaseMigrations/DatabaseMigrations.csproj -c Release --no-restore --no-build -o /apps/DatabaseMigrations && \
    dotnet publish ./apps/DataAggregator/DataAggregator.csproj -c Release --no-restore --no-build -o /apps/DataAggregator && \
    dotnet publish ./apps/GatewayApi/GatewayApi.csproj -c Release --no-restore --no-build -o /apps/GatewayApi
