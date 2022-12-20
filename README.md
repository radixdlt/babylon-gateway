# RadixDLT Network Gateway

This system is designed to be the Radix-run publicly exposed gateway into the Babylon Radix network, and replaces the archive module which previously ran on-node.

The system is in three main parts:
* **Database Migrations** - This project has ownership of the PostgreSQL database schema migrations.
* **Data Aggregator** - Reads from the Core API of one or more full nodes, ingesting from their Transaction Stream API and Mempool Contents API, and committing transactions to a PostgreSQL database. It also handles the pruning (and resubmission where relevant) of submitted transactions.
* **Gateway API** - Provides the public API for Wallets and Explorers, and maps construction and submission requests to the Core API of one or more full nodes.

## Using the Gateway API

For documentation on the Gateway API, see the [Gateway API docs on ReDocly](https://redocly.github.io/redoc/?url=https://raw.githubusercontent.com/radixdlt/babylon-gateway/main/src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-schema.yaml).

## Technical Docs

For docs giving an overview of the Network Gateway and its place in the Radix Ecosystem - including information on the Radix-run Network Gateway, and how to run one of your own - check out [the Radix docs site](https://docs.radixdlt.com/).

For docs related to development on the Network Gateway, see the [docs folder](./docs).

For docs related to running a Network Gateway locally, see the instructions about running a local toy deployment in the [deployment folder](./deployment).
