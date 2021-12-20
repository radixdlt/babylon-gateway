# Deployment guidelines

## Expected deployment

The Network Gateway, once complete, is designed to be deployed as per the below image.

![Expected Network Gateway Deployment](./network-gateway-deployment.png)

Whilst the system can be run with one of each component, a resilient deployment would include:

* Multiple Gateway APIs
* A managed database cluster, with read replicas.
* [Support coming soon] The Data Aggregator reading from one or more full nodes.
* [Support coming soon] A Data Aggregator primary and inactive secondary (which will take over should the primary fail to write to the database for a pre-determined time)

## Running a radixdlt full node for the Core API

If you wish to run a Network Gateway, you will need to have a radixdlt full node to connect to.

As the Core API is designed to not be exposed publicly, you will need to run your own full node/s, and expose this API for your own needs.

* For development purposes, you can either:
  * Connect to a pre-existing full node. A syncing full node and the data aggregator are both quite resource intensive, so it can help to run at least the full node off of your local machine. If at RDX Works, we have some Core APIs you can connect to off your local machine - talk to your team lead about getting access to these.
  * Run a full node locally, using a docker image at build version 1.1.0+. At time of writing, the latest is [release 1.1.0-rc.1](https://github.com/radixdlt/radixdlt/releases/tag/1.1.0-rc.1), available as docker tag [radixdlt/radixdlt-core:1.1.0-rc.1](https://hub.docker.com/r/radixdlt/radixdlt-core/tags). The toy deployment in this folder uses this approach.
  * Run a development build of a full node: [eg following this guide](https://github.com/radixdlt/radixdlt/blob/develop/docs/development/run-configurations/connecting-to-a-live-network-in-docker.md)

* For production purposes, you should run a radixdlt full node exposing the Core API. We do not yet have a full node build exposing the Core API which is
  released for production use.

# Configuration

For information on how to configure the Network Gateway components, see [/docs/configuration.md](../docs/configuration.md).

# Example toy Network Gateway set-up with docker compose

An example docker-compose file is given in this folder, and demonstrates how the projects can be configured. It runs a single docker image of each of:

* A RadixDLT Core - Full Node (OPTIONAL)
* A PostgreSQL Database
* A Network Gateway - Data Aggregator
* A Network Gateway - Gateway API is given in this folder, and .

This toy set-up should **NOT** be used for production - the memory limits, passwords etc are all incorrect for production use. It is also recommended not to run stateful services such as databases in containers.

The aim of the toy deployment is to:
* Demonstrate how the services can be connected, and how to configure the Network Gateway components
* Allow the full stack to be run locally, to develop integrations against a non-released build of the Network Gateway.

## Preparing the toy set-up

Install docker compose if you don't already have it. Then, ensure your terminal has this `/deployment` folder as its working directory.

First, we need to set up the environment variables:

```
cp .template.env .env
```

By default, the `.env` should be set up to connect to `stokenet`. If you have a different full node to connect to, you can configure that instead.

Now, make changes to any of the values you wish in `.env`, eg in order to:
* Change which network it runs against - there are a few parameters which will need changing (`FULLNODE_NETWORK_ID`, `FULLNODE_NETWORK_BOOTSTRAP_NODE` and `NETWORK_NAME`)
* Configure to point to a different full node / Core API

## Running the toy set-up

Finally, you can bring up the toy Network Gateway deployment in two modes - including a full node, or without a full node.

* To bring up the network gateway and a full node, run `./build-and-start-all.sh`
* If you've configured `.env` to point at an existing Core API, you can use `./build-and-start-network-gateway.sh` to just spin up a Network Gateway

On first load, you might get a few transient errors as things boot-up, and connection or precondition checks fail - but after 30 seconds or so,
errors should stabilise and logs should appear in a working state, with the data aggregator ingesting transactions.

At this point, it's time to try out some of the links below. 

### Links to try

* GET http://localhost:5308/swagger/ - Swagger on Gateway API (if enabled)

Or some diagnosis endpoints:

* GET http://localhost:5207 - Root overview for Data Aggregator
* GET http://localhost:5207/health - Health check on Data Aggregator
* GET http://localhost:1234/metrics - Metrics for Data Aggregator
* GET http://localhost:5308 - Root overview check for Gateway API
* GET http://localhost:5308/health - Health check on Gateway API
* GET http://localhost:1235/metrics - Metrics for Gateway API

If you chose to run a full node through docker, you can also try out the Core API, changing out "stokenet" for the current network:

* `curl --request POST 'localhost:3333/network/configuration' --data-raw '{}'`
* `curl --request POST 'localhost:3333/network/status' --data-raw '{"network_identifier":{"network":"stokenet"}}'`

# Running just a full node to develop against

If you're developing on the Network Gateway, you will likely with to run the Network Gateway locally, but you'll want to have a full node
to connect to. You can run that full node from the `deployment` folder as follows:

* Bring up a new terminal in this `deployment` folder.
* Run `cp .template.env .env`
  * This creates a local set of configuration which you can amend - see the "Preparing the toy set-up" section
* Run `./only-start-fullnode.sh`
