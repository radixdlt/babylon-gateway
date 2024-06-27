# Database Maintenance

Network Gateway relies on a PostgreSQL database and some basic tuning is highly recommended for most operations and use cases.

> [!NOTE]
> Be advised that the exact configuration setup is heavily dependent on your infrastructure and intended use case.

### Automatic Vacuuming

If your instance of the Network Gateway is expected to operate predominantly on the recently added or modified data it is crucial to ensure PostgreSQL vacuums and analyzes all the tables frequently.

Here's a suggested auto-vacuum configuration:

```ini
autovacuum = true
autovacuum_vacuum_insert_threshold = 1000     # or lower
autovacuum_vacuum_insert_scale_factor = 0.001 # or lower
autovacuum_analyze_threshold = 1000           # or lower
autovacuum_analyze_scale_factor = 0.001       # or lower
```

Read more on automatic vacuuming in PostgreSQL documentation: [20.10. Automatic Vacuuming](https://www.postgresql.org/docs/current/runtime-config-autovacuum.html).

### Connection String Parameters

Data Aggregator component pushes significant volume of data over the wire, especially while catching up with the network. 
Thus, it may be beneficial to configure connection string parameter `Write Buffer Size` to at least 64 KiB.

Read more on Npgsql connection performance-related settings: [Npgsql: Connection String Parameters](https://www.npgsql.org/doc/connection-string-parameters.html#performance).

Gateway API on the other hand is used is almost exclusively relying on database reads, many of them. Ensure database connection pooling is enabled.

Read more on Npgsql connection pool settings: [Npgsql: Connection String Parameters](https://www.npgsql.org/doc/connection-string-parameters.html#pooling).

### Permissions

- Database Migrations connection requires essentially unrestricted access to the database including DDL-related operations (think: `CREATE TABLE`, `ALTER VIEW`).
- Data Aggregator requires read-write permissions to all the tables.
- Gateway API requires read-only permissions to all the tables with the exception of `pending_transactions_*` tables where write permissions are needed.
