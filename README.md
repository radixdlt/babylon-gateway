# RadixDLT Network Gateway

> ⚠️ &nbsp; This codebase is under development, and any part of it - including any Gateway API interfaces - **are subject to change before v1**.

This system is designed to be the Radix-run public gateway into the Olympia Radix network, and is planned to replace the archive module which currently lives on-node.

The system is in two main parts:
* **Data Aggregator** - Reads from the Core API of one or more full nodes, ingesting from their Transaction API and Mempool Contents API, and committing transactions to a PostGres database. This project has ownership of the schema migrations.
* **Gateway API** - Provides the public API for Wallets and Explorers.


## Technical Docs

For technical docs, including developer set-up, see the [docs folder](./docs).

For docs on the recommended deployment set-up, see the [deployment folder](./deployment).
