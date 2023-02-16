FROM babylon_gateway:build as build

FROM babylon_gateway:runtime
EXPOSE 80
EXPOSE 443
EXPOSE 1235
ENTRYPOINT ["dotnet", "DataAggregator.dll"]
COPY --from=build /apps/DataAggregator .
