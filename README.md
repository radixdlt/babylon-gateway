# Babylon Network Gateway

This system is designed to be the Radix-run publicly exposed gateway into the Babylon Radix network. It is the successor to the [Olympia Gateway](https://github.com/radixdlt/radixdlt-network-gateway).

## Using the Gateway API

For documentation on the Gateway API, see the [Gateway API docs](https://docs-babylon.radixdlt.com/main/apis/api-specification.html).

## Version Support & Network Compatibility

We only support the latest version of Network Gateway and we don't plan to fix bugs in older versions. If you're facing some issues, please upgrade to the latest version of Network Gateway first [Releases](https://github.com/radixdlt/babylon-gateway/releases)

Be aware that protocol updates on the network might result in a mandatory Network Gateway update (e.g. `anemone` protocol update required gateway upgrade to `v1.3.0`) 

## Community Involvement
Please see  [CONTRIBUTING.md](./CONTRIBUTING.md)

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

Read more about [database maintenance](./docs/database-maintenance.md) and [database migrations](./docs/database-migrations.md). 

## Dependencies

Mandatory dependencies:

* Connection to at least one RadixDLT Node - source of network transactions,
* PostgreSQL version 15.2 or newer - primary storage.

Optional dependencies:

* Prometheus - for metrics collection.

## License

The executable components of the Babylon Gateway Code are licensed under the [Radix Software EULA](http://www.radixdlt.com/terms/genericEULA).

The Babylon Gateway Code is released under the [Radix License 1.0 (modified Apache 2.0)](LICENSE):

```
Copyright 2023 Radix Publishing Ltd incorporated in Jersey, Channel Islands.

Licensed under the Radix License, Version 1.0 (the "License"); you may not use
this file except in compliance with the License.

You may obtain a copy of the License at:
https://www.radixfoundation.org/licenses/license-v1

The Licensor hereby grants permission for the Canonical version of the Work to
be published, distributed and used under or by reference to the Licensor’s
trademark Radix® and use of any unregistered trade names, logos or get-up.

The Licensor provides the Work (and each Contributor provides its Contributions)
on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
express or implied, including, without limitation, any warranties or conditions
of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A PARTICULAR
PURPOSE.

Whilst the Work is capable of being deployed, used and adopted (instantiated) to
create a distributed ledger it is your responsibility to test and validate the
code, together with all logic and performance of that code under all foreseeable
scenarios.

The Licensor does not make or purport to make and hereby excludes liability for
all and any representation, warranty or undertaking in any form whatsoever,
whether express or implied, to any entity or person, including any
representation, warranty or undertaking, as to the functionality security use,
value or other characteristics of any distributed ledger nor in respect the
functioning or value of any tokens which may be created stored or transferred
using the Work.

The Licensor does not warrant that the Work or any use of the Work complies with
any law or regulation in any territory where it may be implemented or used or
that it will be appropriate for any specific purpose.

Neither the licensor nor any current or former employees, officers, directors,
partners, trustees, representatives, agents, advisors, contractors, or
volunteers of the Licensor shall be liable for any direct or indirect, special,
incidental, consequential or other losses of any kind, in tort, contract or
otherwise (including but not limited to loss of revenue, income or profits, or
loss of use or data, or loss of reputation, or loss of any economic or other
opportunity of whatsoever nature or howsoever arising), arising out of or in
connection with (without limitation of any use, misuse, of any ledger system or
use made or its functionality or any performance or operation of any code or
protocol caused by bugs or programming or logic errors or otherwise);

A. any offer, purchase, holding, use, sale, exchange or transmission of any
cryptographic keys, tokens or assets created, exchanged, stored or arising from
any interaction with the Work;

B. any failure in a transmission or loss of any token or assets keys or other
digital artifacts due to errors in transmission;

C. bugs, hacks, logic errors or faults in the Work or any communication;

D. system software or apparatus including but not limited to losses caused by
errors in holding or transmitting tokens by any third-party;

E. breaches or failure of security including hacker attacks, loss or disclosure
of password, loss of private key, unauthorised use or misuse of such passwords
or keys;

F. any losses including loss of anticipated savings or other benefits resulting
from use of the Work or any changes to the Work (however implemented).

You are solely responsible for; testing, validating and evaluation of all
operation logic, functionality, security and appropriateness of using the Work
for any commercial or non-commercial purpose and for any reproduction or
redistribution by You of the Work. You assume all risks associated with Your use
of the Work and the exercise of permissions under this Licence.
```
