# CI / Release Workflows

Reference for the GitHub Actions workflows in [`.github/workflows/`](workflows/), grouped by whether they run automatically or need to be dispatched manually. Branching/release strategy is documented in [`docs/branching-strategy.md`](../docs/branching-strategy.md) and [`docs/releasing-new-version.md`](../docs/releasing-new-version.md) — briefly: `main` (last official release) ← unreleased `release/*` branches ← `develop` (primary integration branch), with merges propagated downstream (`main` → `release/*` → `develop`) by whoever lands them. CI/workflow changes should branch from the most upstream supported branch (typically the active `release/*`) so the fix propagates down.

## Automatic workflows

These trigger on push/PR/release — nothing to run by hand.

| Workflow | Trigger | Purpose | Action |
|---|---|---|---|
| `main.yml` | Every PR/push to `main`, `develop`, `release/*` | .NET build+unit tests, SonarCloud, builds/pushes private multiarch Docker images (gateway-api, data-aggregator, database-migrations), triggers ephemeral PR-namespace deploy + (on PRs into `develop`) a Jenkins smoke test | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/main.yml) |
| `build-typescript-sdk.yml` | Push to any branch, only when `sdk/typescript/**` changes | Builds + tests the TypeScript SDK (no publish) | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/build-typescript-sdk.yml) |
| `postman.yml` | Push to `develop` (also `workflow_dispatch`, see below) | Syncs Gateway API OpenAPI spec into Postman collections for `gilganet` | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/postman.yml) |
| `pr-namespace-deletion.yml` | PR closed (and merged) | Tears down the ephemeral k8s namespace that `main.yml`'s `deploy-pr` job created for that PR | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/pr-namespace-deletion.yml) |
| `release-drafter.yml` | Push to `develop` | Keeps a draft GitHub Release with auto-generated notes up to date (calls `release-drafter-reusable.yml`) | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/release-drafter.yml) |
| `releases.yml` | GitHub Release published | Publishes `.zip` build artifacts to the Release, builds+pushes public multiarch Docker images (gateway-api, data-aggregator, database-migrations) to Docker Hub | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/releases.yml) |

## Manual workflows

| Workflow | Trigger | Purpose | Action |
|---|---|---|---|
| `publish-typescript-sdk.yml` | `workflow_dispatch` (input `package_version_number`, required) | Publishes the Gateway TypeScript SDK to npmjs.org — run manually, separately, after cutting a release | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/publish-typescript-sdk.yml) |
| `postman.yml` | `workflow_dispatch` (input `network_name`: `gilganet`\|`enkinet`\|`hammunet`\|`adapanet`, default `gilganet`) | Re-sync Postman collections for a specific testnet on demand, gated behind the **`Postman`** GitHub environment | [Runs](https://github.com/radixdlt/babylon-gateway/actions/workflows/postman.yml) |

### Release sequence

Per [`docs/releasing-new-version.md`](../docs/releasing-new-version.md):

1. Create `release/v{major.minor.patch}` off `develop`, test the Gateway.
2. Bump `VersionPrefix` in `directory.build.props` and `Version` in `gateway-api-schema.yaml`; regenerate the Gateway models and TypeScript SDK; pre-emptively update `deployment/docker-compose.yml` image versions; commit.
3. Merge the release branch into `main`.
4. Create a GitHub Release **draft** tagged `v{major.minor.patch}` on `main`, marked pre-release — this is exactly what `release-drafter.yml`/`release-drafter-reusable.yml` have been continuously drafting on every push to `develop`.
5. **Publishing** that draft auto-fires **`releases.yml`**: uploads the three `.zip` build artifacts (DataAggregator, DatabaseMigrations, GatewayApi) to the Release, then builds+pushes all three services as public multiarch Docker images to Docker Hub (`radixdlt/babylon-ng-gateway-api`, `-data-aggregator`, `-database-migrations`) — the 3 ARM build jobs are gated behind the **`release`** GitHub environment (manual approval).
6. Manual rollout to stokenet then mainnet; publish API spec docs.
7. Separately run **`publish-typescript-sdk.yml`** manually with the new `package_version_number` to publish the SDK to npmjs.
8. Mark the release "latest," announce, then merge `main` back into `develop` per the branching waterfall.

## External dependencies per job

Shared building blocks used across nearly every workflow (not repeated per-row below):
- **`RDXWorks-actions/*`** — the org's own mirror of common third-party actions (`checkout`, `setup-dotnet`, `setup-node`, `configure-aws-credentials`, `aws-secretsmanager-get-secrets`, `sonarscan-dotnet`, `action-gh-release`, `jenkins-job-trigger-action`, `release-drafter`, `cancel-workflow-action`, `variable-mapper`, `action-set-json-field`, `get-release`, etc.).
- **`radixdlt/public-iac-resuable-artifacts`** — external repo providing reusable workflows (`docker-build.yml`, `join-docker-images-all-tags.yml`) used by every Docker build/join job.
- **`./.github/actions/fetch-secrets`** — local composite action: assumes an AWS IAM role via OIDC (`RDXWorks-actions/configure-aws-credentials`), then reads a named secret from Secrets Manager (`RDXWorks-actions/aws-secretsmanager-get-secrets`). Requires `id-token: write` on the calling job. This is the sole mechanism every workflow here uses to obtain runtime secrets.
- **`./.github/actions/set-variables`** — pure git/bash computation of version suffix and per-service Docker tags (also patches `Directory.Build.props`); no secrets.

| Workflow | Secrets | AWS / Secrets Manager | Self-hosted runner(s) | GH environment | Other external deps |
|---|---|---|---|---|---|
| `main.yml` | `GH_COMMON_SECRETS_READ_ROLE`; `GH_BABYLON_GATEWAY_SECRETS_READ_ACCESS_ROLE`; `SONAR_GITHUB_TOKEN` | `GH_COMMON_SECRETS_READ_ROLE` → `github-actions/common/dockerhub-credentials` (join jobs) and `github-actions/common/sonar-token` (sonarcloud job); `GH_BABYLON_GATEWAY_SECRETS_READ_ACCESS_ROLE` → `github-actions/radixdlt/babylon-gateway/cloudflare` (`deploy-pr`) and `.../jenkins-api-token` (`ephemeral-deploy-and-test`) | `selfhosted-ec2-ubuntu-22-arm-4core` (all 3 ARM private-image builds) | none | Docker Hub (private `private-babylon-ng-*` images), SonarCloud, Cloudflare-fronted webhook `github-worker.radixdlt.com` (ephemeral PR-namespace deploy dispatch), Jenkins (`ephemeral-gateway-env-deploy-and-test`, only on PRs targeting `develop`) |
| `build-typescript-sdk.yml` | none | none | none (`ubuntu-22.04`) | none | npm registry (build/test only, no publish) |
| `postman.yml` | none (**hardcoded** AWS role ARN, not a GH secret — see note below) | `arn:aws:iam::308190735829:role/gh-babylon-gateway-secrets-read-access` → `github-actions/radixdlt/babylon-gateway/postman-token` | none | **`Postman`** | Postman API (`api.getpostman.com`), npm registry (pulls `@apideck/portman` via `npx`) |
| `pr-namespace-deletion.yml` | none (same hardcoded ARN as `postman.yml`) | Same literal role → `github-actions/radixdlt/babylon-gateway/cloudflare` | none | none | Cloudflare-fronted webhook `github-worker.radixdlt.com` (namespace teardown dispatch) |
| `release-drafter.yml` / `release-drafter-reusable.yml` | `GITHUB_TOKEN` (built-in) | none | none (`ubuntu-22.04`) | none | GitHub Releases API |
| `releases.yml` | `DOCKERHUB_RELEASER_ROLE` | `DOCKERHUB_RELEASER_ROLE` used as `role_to_assume` for all 6 docker-build jobs, and again for the 3 join jobs alongside Secrets Manager path `github-actions/rdxworks/dockerhub-images/release-credentials` | `babylon-gateway-arm` (all 3 ARM public-image builds) | **`release`** (all 3 ARM docker-build jobs) | Docker Hub (public `radixdlt/babylon-ng-*` images), GitHub Releases API (`.zip` asset upload) |
| `publish-typescript-sdk.yml` | none shown explicitly | none | none (`ubuntu-latest`) | none (relies on `workflow_dispatch` alone) | npmjs.org registry |

Notes:
- **Inconsistent secret sourcing**: `postman.yml` and `pr-namespace-deletion.yml` both use a **hardcoded literal AWS role ARN** (`arn:aws:iam::308190735829:role/gh-babylon-gateway-secrets-read-access`) instead of the `secrets.GH_BABYLON_GATEWAY_SECRETS_READ_ACCESS_ROLE` reference that `main.yml` uses for the same underlying role — worth reconciling to one pattern.
- Any workflow gated behind an `environment:` (`Postman`, `release`) requires whatever manual-approval/reviewer rule is configured for that environment in repo settings before the job proceeds.
- `main.yml`'s `deploy-pr` and `ephemeral-deploy-and-test` jobs are the ones actually spinning up/testing ephemeral infrastructure per PR — `ephemeral-deploy-and-test` only runs when the PR targets `develop`, and its Jenkins trigger only *starts* the job; a green GitHub Actions check means the Jenkins job was triggered, not that it passed (check Jenkins for that).
- `publish-typescript-sdk.yml`'s `npm publish` step explicitly unsets `NODE_AUTH_TOKEN`/`NPM_CONFIG_USERCONFIG` right before publishing — actual publish auth isn't visible in this workflow file itself (likely a runner-level `.npmrc`), worth confirming if this workflow is ever moved to a different runner.
- Both `main.yml` and `releases.yml` build/push all three Gateway services (GatewayApi, DataAggregator, DatabaseMigrations) as separate images for both architectures, then join each pair into a multiarch manifest — 6 build jobs + 3 join jobs per workflow.
