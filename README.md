# RadixDLT Network Gateway

> ⚠️ &nbsp; This service is yet to be officially released as a v1, and we do not yet recommend running the current build for production workloads.

This system is designed to be the Radix-run publicly exposed gateway into the Olympia Radix network, and replaces the archive module which previously ran on-node.

The system is in two main parts:
* **Data Aggregator** - Reads from the Core API of one or more full nodes, ingesting from their Transaction API and Mempool Contents API, and committing transactions to a PostGres database. This project has ownership of the schema migrations.
* **Gateway API** - Provides the public API for Wallets and Explorers.

## Using the Gateway API

For documentation on the Gateway API, see the [Gateway API docs on ReDocly](https://raw.githubusercontent.com/radixdlt/radixdlt-network-gateway/develop/generation/gateway-api-spec.yaml).

## Technical Docs

For technical docs, including development set-up to develop on the Network Gateway, see the [docs folder](./docs).

For docs on the recommended deployment set-up, including to run a toy-deployment locally to test against, see the [deployment folder](./deployment).
