namespace RadixDlt.NetworkGateway.Abstractions;

public record struct CommittedStateIdentifiers(long StateVersion, string StateTreeHash, string TransactionTreeHash, string ReceiptTreeHash);
