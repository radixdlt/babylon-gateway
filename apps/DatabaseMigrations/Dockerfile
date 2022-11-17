# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f apps/DatabaseMigrations/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "apps/DatabaseMigrations/DatabaseMigrations.csproj"
RUN dotnet build "apps/DatabaseMigrations/DatabaseMigrations.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "apps/DatabaseMigrations/DatabaseMigrations.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseMigrations.dll"]

# TODO use local paths and adjust context for CI/CD
