FROM babylon_gateway:build as build

FROM babylon_gateway:runtime
ENTRYPOINT ["dotnet", "DatabaseMigrations.dll"]
COPY --from=build /apps/DatabaseMigrations .
