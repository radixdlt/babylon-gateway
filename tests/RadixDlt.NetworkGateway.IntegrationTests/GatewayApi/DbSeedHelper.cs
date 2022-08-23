using NodaTime.Text;
using RadixDlt.NetworkGateway.Common.Database;
using RadixDlt.NetworkGateway.Common.Database.Models.Ledger;
using RadixDlt.NetworkGateway.Common.Database.Models.SingleEntries;
using RadixDlt.NetworkGateway.Common.Extensions;
using RadixDlt.NetworkGateway.Common.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public static class DbSeedHelper
    {
        public static readonly string NetworkName = "integrationtestsnet";

        public static readonly byte[] DefaultHash = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };

        public static void InitializeDbForTests(ReadOnlyDbContext db)
        {
            db.NetworkConfiguration.AddRange(GetSeedingNetworkConfiguration());

            var rawTransactions = GetSeedingRawTransactions();
            db.RawTransactions.AddRange(rawTransactions);

            var transactions = GetSeedingLedgerTransactions(rawTransactions.First());
            db.LedgerTransactions.AddRange(transactions);

            db.LedgerStatus.AddRange(GetSeedingLedgerStatus(transactions.First()));
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(ReadOnlyDbContext db)
        {
            db.NetworkConfiguration.RemoveRange(db.NetworkConfiguration);
            db.LedgerStatus.RemoveRange(db.LedgerStatus);
            db.RawTransactions.RemoveRange(db.RawTransactions);
            db.LedgerTransactions.RemoveRange(db.LedgerTransactions);

            InitializeDbForTests(db);
        }

        public static List<LedgerTransaction> GetSeedingLedgerTransactions(RawTransaction rawTransaction)
        {
            return new List<LedgerTransaction>()
            {
                new LedgerTransaction(
                    resultantStateVersion: 1,
                    payloadHash: rawTransaction.TransactionPayloadHash,
                    intentHash: DefaultHash,
                    signedTransactionHash: DefaultHash,
                    transactionAccumulator: DefaultHash,
                    message: DefaultHash,
                    feePaid: TokenAmount.Zero,
                    epoch: 1,
                    indexInEpoch: 0,
                    roundInEpoch: 0,
                    isStartOfEpoch: true,
                    isStartOfRound: true,
                    roundTimestamp: InstantPattern.General.Parse("1970-01-20T05:07:28Z").Value,
                    createdTimestamp: InstantPattern.General.Parse("2022-08-09T17:03:39Z").Value,
                    normalizedRoundTimestamp: InstantPattern.General.Parse("1970-01-20T05:07:28Z").Value)
                {
                     RawTransaction = rawTransaction,
                },
            };
        }

        public static List<LedgerStatus> GetSeedingLedgerStatus(LedgerTransaction topTransaction)
        {
            return new List<LedgerStatus>()
            {
                new LedgerStatus()
                {
                    Id = 1,
                    TopOfLedgerStateVersion = 1,
                    SyncTarget = new SyncTarget() { TargetStateVersion = 1 },
                    LastUpdated = InstantPattern.General.Parse("2022-08-09T17:03:39Z").Value,
                    TopOfLedgerTransaction = topTransaction,
                },
            };
        }

        public static List<NetworkConfiguration> GetSeedingNetworkConfiguration()
        {
            return new List<NetworkConfiguration>()
            {
                new NetworkConfiguration()
                {
                    Id = 1,
                    NetworkDefinition = new NetworkDefinition() { NetworkName = NetworkName },
                    NetworkAddressHrps = new NetworkAddressHrps() { AccountHrp = "ddx", ResourceHrpSuffix = "_dr", ValidatorHrp = "dv", NodeHrp = "dn" },
                    WellKnownAddresses = new WellKnownAddresses() { XrdAddress = "xrd_dr1qyrs8qwl" },
                },
            };
        }

        public static List<RawTransaction> GetSeedingRawTransactions()
        {
            return new List<RawTransaction>()
            {
                new RawTransaction()
                {
                    Payload = DefaultHash,
                    TransactionPayloadHash = DefaultHash,
                },
            };
        }
    }
}
