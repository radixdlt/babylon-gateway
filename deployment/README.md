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
  * [Run a full node against a development build](https://github.com/radixdlt/radixdlt/blob/develop/docs/development/run-configurations/connecting-to-a-live-network-in-docker.md).
  * Run against a release candidate build of the node at build version 1.1.0+. At time of writing, the latest is [release 1.1.0-rc.1](https://github.com/radixdlt/radixdlt/releases/tag/1.1.0-rc.1), available as docker tag [radixdlt/radixdlt-core:1.1.0-rc.1](https://hub.docker.com/layers/radixdlt/radixdlt-core/1.1.0-rc.1/images/sha256-912939c55aa8abf6ecd0b7ae329daf8448a5b0d6137078000dc5a8797a86f045?context=explore). You should run this with similar configuration as per running a full node against a development build, in particular, with `api.transactions.enable=true`.

* For production purposes, you should run a radixdlt full node exposing the Core API. We don't yet have a full node build exposing the Core API which is
  released for production use.

# Example toy Network Gateway set-up with docker compose

An example docker-compose file to run a single docker image of each of the Gateway API, Database and Aggregator is given in this folder, and demonstrates how the projects can be configured.

The docker file is based on locally built docker images for the code, and a PostGres image from the docker registry.

Standard caveat: it is recommended not to run stateful services such as databases in containers. As such, we would recommend the toy set-up not to be used for production, but can be amended for your requirements.

## Preparing the toy set-up

Install docker compose if you don't already have it. Then, ensure your temimal has this `/deployment` folder as its working directory.

First, we need to set up the environment variables:

```
cp .template.env .env
```

Now, make changes to any of the values you wish in `.env` (to eg point it at your locally running node's core API). Comments in the `.env` file should help with configuring this correctly.

## Running the toy set-up

Finally, you can bring up the whole stack with:

```sh
./build-and-start-network-gateway.sh
```

### Links to try

* GET http://localhost:5308/swagger/ - Swagger on Gateway API (if enabled)

Or some diagnosis endpoints:

* GET http://localhost:5207 - Root overview for Data Aggregator
* GET http://localhost:5207/health - Health check on Data Aggregator
* GET http://localhost:1234/metrics - Metrics for Data Aggregator
* GET http://localhost:5308 - Root overview check for Gateway API
* GET http://localhost:5308/health - Health check on Gateway API
* GET http://localhost:1235/metrics - Metrics for Gateway API
