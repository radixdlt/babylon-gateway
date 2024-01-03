# Babylon Network Gateway

This system is designed to be the Radix-run publicly exposed gateway into the Babylon Radix network. It is the successor to the [Olympia Gateway](https://github.com/radixdlt/radixdlt-network-gateway).

## License

The Babylon Gateway code is released under the [Radix License](LICENSE). Binaries/Executable components are licensed under the [Radix Software EULA](http://www.radixdlt.com/terms/genericEULA).

## Using the Gateway API

For documentation on the Gateway API, see the [Gateway API docs](https://docs-babylon.radixdlt.com/main/apis/api-specification.html).

## Community Involvement

If you have questions, suggestions or bug reports, please come to the [Network Gateway channel](https://discord.com/channels/417762285172555786/1149370695206318151) on the [Radix DLT Discord server](http://discord.gg/radixdlt).

## Structure

The system is in three main parts:
* **Database Migrations** - This project has ownership of the PostgreSQL database schema migrations.
* **Data Aggregator** - Reads from the Core API of one or more full nodes, ingesting from their Transaction Stream API and Mempool Contents API, and committing transactions to a PostgreSQL database. It also handles the pruning (and resubmission where relevant) of submitted transactions.
* **Gateway API** - Provides the public API for Wallets and Explorers, and maps construction and submission requests to the Core API of one or more full nodes.

## Technical Docs

For docs giving an overview of the Network Gateway and its place in the Radix Ecosystem - including information on the Radix-run Network Gateway, and how to run one of your own - check out [the Radix Babylon docs site](https://docs-babylon.radixdlt.com/).

For docs related to development on the Network Gateway, see the [docs folder](./docs).

For docs related to running a Network Gateway locally, see the instructions about running a local toy deployment in the [deployment folder](./deployment).

## Database Migrations & Application Deployment

While all three main system parts are technically independent of each other it is assumed that during overall stack deployment the following order is preserved:

1. Deploy Database Migrations. This is a short-lived container that executes new database migrations, if any, and exists successfully with `0` exit code. Should this container fail to apply database migrations (non-successful exit code) deployment procedure must be aborted. 
2. Deploy Data Aggregator. Wait until application healthcheck endpoints report `Healthy` status. 
3. Deploy Gateway API. 

**Hint:** For Kubernetes cluster deployments it is recommended to set up Database Migrations as init container of Data Aggregator.

**Note:** Babylon Network Gateway is **NOT** compatible with previous Olympia version. Brand-new, clean database must be used. 
If you're upgrading from Olympia and deploying for the very first time you may want to run Database Migrations application with `WIPE_DATABASE=true` configuration parameter to drop existing database and recreate it. This is irreversible operation. **Proceed with caution!**  

## Dependencies

Mandatory dependencies:

* Connection to at least one RadixDLT Node - source of network transactions,
* PostgreSQL version 15.2 or newer - primary storage.

Optional dependencies:

* Prometheus - for metrics collection.
