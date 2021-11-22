# Deployment guidelines

## Expected deployment

The Network Gateway, once complete, is designed to be deployed as per the below image.

![Expected Network Gateway Deployment](./network-gateway-deployment.png)

Whilst the system can be run with one of each component, a resilient deployment would include:

* Multiple Gateway APIs.
* A managed database cluster, with read replicas.
* [Support coming after v1] A Data Aggregator primary and inactive secondary (which will take over should the primary fail to write to the database for a pre-determined time)
* [Support coming after v1] The Data Aggregator reading from one or more full nodes.

## Example toy set-up with docker compose

An example docker-compose file to run a single docker image of each of the Gateway API, Database and Aggregator is given in this folder, and demonstrates how the projects can be configured.

The docker file is based on locally built docker images for the code, and a PostGres image from the docker registry.

Standard caveat: it is recommended not to run stateful services such as databases in containers. As such, we would recommend use of It should not be used for production, but can be amended for your requirements.

### Preparing the toy set-up

Install docker compose if you don't already have it. Then, ensure your temimal has this `/deployment` folder as its working directory.

First, we need to set up the environment variables:

```
cp .template.env .env
```

Now, make changes to any of the values you wish in `.env` (to eg point it at your locally running node's cope API)

### Running the toy set-up

Finally, you can bring up the whole stack with:

```sh
./build-and-start-network-gateway.sh
```

