# Branching Strategy

Once you have read the [contributing guide](CONTRIBUTING.md), if you want to start development, you will need to know which branches to use.

> [!NOTE]
>
> As of 2024-03-22, the strictly ordered list of supported base branches, starting from earliest/furthest upstream, is:
>
> * `main` (contains latest release/v1.4.3)
> * `develop`
>
> When clicking merge on a PR to one of these branches, it is your duty to ensure that PRs are raised to merge that branch into all later/downstream supported base branches.


## Summary of approach

### The base branches

We follow `git-flow` strategy, where there are three main types of base branches: the `main`, `develop`, and `release/*` branches.

* The `main` branch is the public-facing base branch and **represents the last official release**. It's also used for docs.
* The `develop` branch is the primary integration branch, for work targeting the next releases.
* The `release/*` branches are following naming strategy `release/v{major}.{minor}.{patch}` (i.e. `release/v1.4.3` will contain all changes released with `v1.4.3` release). 

At any given time, there is a strict ordering on the supported base branches, where the process aims to guarantee all work on previous branches is in the later/downstream branches. This order (from earliest/most upstream to latest/most downstream) is as follows:

* `main`
* Unreleased `release/*` branches (if any)
* `develop`

The latest ordering is at the top of this document.

### Development process

When working on changes:
* You will first need to select the correct base branch to create your feature branch from. For some epics, it is acceptable to choose a long running `feature/*` or `epic/*` branch as your base, to break up the work into separate reviews.
* Your branch should start `feature/*`, or variants on naming such as `hotfix/*`, `tweak/*`, `docs/*` are permitted. The specific name should be prefixed by a JIRA ticket or Github issue reference where appropriate, e.g. `feature/NODE-123-my-feature` for JIRA tickets or `feature/gh-1235-my-feature` for github issues.
* When you raise a PR, you will need to ensure you select the appropriate base branch before creating the PR. **The default, `main` is typically not correct!**

> [!IMPORTANT]
>
> Finally, when a PR is merged, it is **the PR merger's responsibility** to ensure that the _base branch_ that was merged into is then merged into _all downstream base branches_ (ideally one by one, like a waterfall).
>
> If there is a merge conflict, this should be handled by creating a special `conflict/X-into-Y-DATE` branch (for branches `X`, `Y` and `DATE`) from `X`, and putting in a PR with a merge target of `Y`.
>
> But if this process is properly followed, such merge conflicts will be rare.

## Which base branch should I use for X?

### Code changes

Features branches usually branch from `develop`, unless they need to target the currently released version (hotfixes or some urgent features), in which case, they will need to target the `main` branch.

### Stand-alone doc changes

Public facing docs change unrelated to another ticket should use a base branch of `main` - as this is the branch which is first visible when someone looks at the repository.

### Workflow / CI changes

Workflow changes should branch from the _most upstream_ (earliest) supported branch. Typically this is a `release/*` branch.

Once the post-merge process is followed, this change will find itself on all later/downstream base branches too.

This ensures that these changes are on all supported branches, so builds can be built on `develop` or on all supported branches.

## Merge or Rebase/Cherry-pick?

This strategy relies on the fact that we always merge.

We avoid rebases after publicly pushing a branch / seeking a review because:

* Rebases cause potential conflicts with other people's work on the same branches, overwrite the history of the project and overwrite any GPG signed commits from other developers
* Rebases result in more merge conflicts
* Various other benefits discussed in the below section.

We acknowledge the weakness of merging that this can make the git history messier to display.

At merge time, it is acceptable but not recommended to squash-merge. We encourage developers to instead squash commits before asking for a review. This results in a better record of the review / iteration process.
