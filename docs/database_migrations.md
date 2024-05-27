# Database migrations
For most of the releases database schema differs between versions. To easily migrate/deploy database schema we are using EntityFramework migrations.

For further reading, visit the official Microsoft documentation on Entity Framework migration https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli

## Executing migrations
There are two supported methods of applying migrations. Executing Idempotent SQL script or using programmatic Entity Framework migrations (for simplicity they can be applied using docker image). You can choose whichever you prefer.

### Docker image
For each release, we publish a docker image which is responsible for migrating the database. Similarly to the idempotent SQL script, it takes care of applying each migration only once and applying only missing migrations.

Image can be found here:
https://hub.docker.com/r/radixdlt/babylon-ng-database-migrations

To migrate the database you just need to run that image providing a connection string to the database. To do that you need to set an environment variable for the docker container `ConnectionStrings__NetworkGatewayMigrations`.

### Idempotent SQL script
It is a Raw SQL script that has to be executed on the database. It takes care of applying each migration only once and applying only missing migrations.

It can be found in the [migrations directory](../src/RadixDlt.NetworkGateway.PostgresIntegration/Migrations/IdempotentApplyMigrations.sql)

For more information check:
https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli#idempotent-sql-scripts

Because of a bug in CLI responsible for generating these SQL scripts https://github.com/dotnet/efcore/issues/24512, it is possible that it will generate invalid SQL scripts (missing semicolon at the end of each command). We are trying to make sure it does not happen but since we are internally using docker images to migrate the database there is a chance that it might slip through. To fix that, simply add a semicolon at the end of the invalid command.

## Schema upgrade scenarios
There are two scenarios that might happen between gateway versions.

### There are migrations that will update the database schema and migrate the data
If the situation allows us to migrate existing schemas without losing data, there will be database migrations that take care of that. To migrate to the new version you will only need to execute migrations on top of the existing database.

### Version requires full database sync
There are situations in which it is impossible to migrate the database schema and migrate existing data to the new schema.

In such cases, we are making it clear in release notes that it has to be deployed on a clean database and requires syncing the gateway database from the first transaction.

To do that:

1. Make sure your database is empty.
    1. there is no `__EFMigrationsHistory` table in the database.
    2. there are no tables in that database.
2. Execute migrations using the preferred method (idempotent SQL script or docker image).
3. Run DataAggregator and process the entire transaction stream.

Keep in mind that processing the entire transaction stream might take a significant amount of time. It is dependent on multiple factors (i.e. machine resources, the latency on connecting to node/database, number of transactions on the network), As of now (1st June 2024) foundation gateway is able to sync within around 30 hours. Gateway API will not return up-to-date information before the entire transaction stream is processed.


  
