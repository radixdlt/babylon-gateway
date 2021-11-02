# RadixDLT Network Gateway

This system is designed to be the Radix-run public gateway into the Olympia Radix network, and is planned to replace the archive module which currently lives on-node.

The system is in two main parts:
* **DataAggregator** - Reads from one or more full nodes, ingesting from their Transaction API and Mempool API, and committing them to a transactional database
* **WalletExplorerApi** - Provides the public API for Wallets and Explorers

## Technical Docs

For technical docs, including developer set-up, see the [docs folder](./docs).
