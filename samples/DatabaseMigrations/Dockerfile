# This dockerfile should be build with the context at the root of the repo, eg, from the repo root:
# docker build -f samples/DatabaseMigrations/Dockerfile .

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "samples/DatabaseMigrations/DatabaseMigrations.csproj"
RUN dotnet build "samples/DatabaseMigrations/DatabaseMigrations.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "samples/DatabaseMigrations/DatabaseMigrations.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseMigrations.dll"]

# TODO use local paths and adjust context for CI/CD
