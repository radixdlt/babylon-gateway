General approach to releasing Gateway API:

1. Crate Git release branch `release/v{major.minor.path}` i.e `release/v1.10.6`.
2. Test Gateway and verify that it works correctly.
3. Make sure to update software version numbers.
   1. update `VersionPrefix` property in `directory.build.props`
   2. update `Version` in [gateway-api-schema.yaml](../src/RadixDlt.NetworkGateway.GatewayApi/gateway-api-schema.yaml) file.
4. Regenerate gateway models by executing [script](../generation/regenerate-gateway-api.py).
```
cd generation/
python regenerate-gateway-api.py
```
5. Regenerate typescript SDK by executing [script](../sdk/typescript/regeneration/generate-typescript-client.py).
```
cd sdk/typescript/regeneration/
python generate-typescript-client.py
```
6. Update [docker-compose.yml](../deployment/docker-compose.yml) image versions pre-emptively (images are still not yet published but to prevent multiple roundtrips to git we can already provide them).
```
cd generation/
python update-docker-compose.py {old_version} {new_version} #i.e python update-docker-compose.py v1.10.6 v1.10.7
```
7. Make sure to commit all changes made in steps 3, 4, 5, 6.
8. Merge the release branch created in step 1. (`release/v{major.minor.path}`) to `main` branch.
9. Create GitHub release `v{major.minor.path}` draft. 
   1. Create a new tag with `v{major.minor.path}` version on `main` branch.
   2. Mark it as pre-release.
10. Publish docker images to repository:
    1. https://hub.docker.com/r/radixdlt/babylon-ng-gateway-api
    2. https://hub.docker.com/r/radixdlt/babylon-ng-data-aggregator
    3. https://hub.docker.com/r/radixdlt/babylon-ng-database-migrations
11. Publish new images to Gateway running on **stokenet** network.
12. Publish API specs docs for **stokenet** network
TODO1: link docs here and give url once we establish that.
13. Verify if it works correctly on **stokenet** network.
14. Publish new images to Gateway running on **mainnet** network.
15. Publish API specs docs for **mainnet** network
TODO2: link docs here and give url once we establish that.
16. Publish typescript SDK created in step 5. to npmjs repository (https://www.npmjs.com/package/@radixdlt/babylon-gateway-api-sdk). 
17. Update GitHub release created in step 9 and mark it `Set as the latest release`.
18. Announce release publicly.
19. Merge `main` branch back to `develop`.


TODO1: ?

TODO2:
   https://radix-api-docs.radixdlt.workers.dev/gateway-api-specs ?
