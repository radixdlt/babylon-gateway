# Monitoring

## Logging

Logging configuration follows the [ASP.NET paradigms](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/#dnrvs).

In particular, both the log levels and the logger can be configured in the configuration.

By default, a simple one-line console logger is used in development, and a JSON logger is used in production. These can be configured further in the [app configuration](../configuration), as per [the ASP.NET guidance](https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter) - eg this is done in the [deployment folder](../deployment) for optimising log readability in Docker.

An example of configuring for the systemd console logger is given below.

```
{
    "Logging": {
        "Console": {
            "FormatterName": "systemd",
            "FormatterOptions": {
                "IncludeScopes": true,
                "UseUtcTimestamp": true,
                "TimestampFormat": "yyyy-MM-ddTHH\\:mm\\:ss.fff\\Z "
            }
        }
    }
}
```

## Metrics

The Network Gateway services export metrics in Prometheus format, via metric endpoints; to be picked up by a Prometheus Agent
(eg in Kubernetes).

The default endpoints are:
* Data Aggregator - http://localhost:1234
* Gateway API - http://localhost:1235

But these can be changed with the configuration variable `PrometheusMetricsPort`.

### Metric Types

Metrics fall into a number of groupings, separated out by distinct prefixes.

These are metrics provided by libraries:

* **dotnet** - prefix: `dotnet_` - Metrics about the runtime (eg threadpool, known allocated memory)
* **process** - prefix: `process_` - Metrics about the process (eg process threads, process memory)
* **http_request** - prefix: `http_request_` or `http_requests_` - Metrics about controller actions
* **httpclient** - prefix `httpclient_` - Metrics about requests that the service makes to upstream services (is the full nodes)
* **aspnetcore** - prefix `aspnetcore_` - Metrics related to ASP.NET core (eg healthcheck status)

There are custom metrics, all prefixed by `ng_` (for network gateway):

* **aggregator** - prefix: `ng_aggregator_` - metrics about aggregator status
* **node_fetch** - prefix: `ng_node_fetch_` - metrics about fetching data from a node
* **ledger_sync** - prefix: `ng_ledger_sync_` - metrics about syncing the ledger from full nodes
* **ledger_commit** - prefix: `ng_ledger_commit_` - metrics about committing the agreed ledger to the database
* **node_ledger** - prefix: `ng_node_ledger_` - metrics about the ledger / state of the full node/s (with `node` label)
* **node_mempool** - prefix: `ng_node_mempool_` - metrics about full node mempool/s (or the combination of them)
* **db_mempool** - prefix: `ng_db_mempool_` - metrics about the MempoolTransactions in the database
* **construction_transaction** - prefix: `ng_construction_transaction_` - metrics relating to construction, submission or resubmission
