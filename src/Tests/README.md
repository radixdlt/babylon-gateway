# Tests

We have two types of tests:
* [Mini Unit Tests](./MiniUnitTests) - a few class based tests for functional helpers.
* [Integrated Unit Tests](./IntegratedUnitTests) - testing units of behaviour through the actual application, using the full DI kernel

## Running the tests

Run `dotnet test` from the solution root.

## Testing framework and conventions

We use xUnit: https://xunit.net/docs/comparisons

For some conventions and best practice, consult [this dotnet guide](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices).
