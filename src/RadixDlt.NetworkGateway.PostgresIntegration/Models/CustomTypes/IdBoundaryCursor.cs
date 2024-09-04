namespace RadixDlt.NetworkGateway.PostgresIntegration.Models.CustomTypes;

public readonly record struct IdBoundaryCursor(long StateVersion, long Id);
