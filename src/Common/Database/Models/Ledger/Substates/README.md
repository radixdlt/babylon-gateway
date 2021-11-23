## Substates

DB substates represent Radix Engine substates, there are two key kinds:

* Balance substates - conveyed by an "Amount" part on the operation object
* Data substates - conveyed by a "Data" part on the operation object

Each Table represents 1 or more Radix Engine substates.

### Adding a substate

As per 2021/11/23, you'll need to perform the following:

* Create a new substate class in this folder, inheriting from `SubstateBase`
* Add it as a DbSet in `CommonDbContext`
* Ensure `HookUpSubstate` is called in `CommonDbContext`
* Add the handling logic in `TransactionContentProcessor`
* Ensure that it's added to `DbActionsPlanner.LoadDependencies`
* If you have a new enum, also create the enum converter, and register that in CommonDbContext (see eg `ValidatorDataSubstate` for reference)

Then, as per any db change:

* Add a migration
* Test
