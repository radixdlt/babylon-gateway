using NodaTime.Text;
using RadixDlt.NetworkGateway.Core.Database;
using RadixDlt.NetworkGateway.Core.Database.Models.Ledger;
using RadixDlt.NetworkGateway.Core.Database.Models.SingleEntries;
using RadixDlt.NetworkGateway.Core.Extensions;
using RadixDlt.NetworkGateway.Core.Numerics;

namespace RadixDlt.NetworkGateway.IntegrationTests.GatewayApi
{
    public static class InMemoryDb
    {
        public static void InitializeDbForTests(ReadOnlyDbContext db)
        {
            db.NetworkConfiguration.AddRange(GetSeedingNetworkConfiguration());
            db.LedgerTransactions.AddRange(GetSeedingLedgerTransactions());
            db.LedgerStatus.AddRange(GetSeedingLedgerStatus());
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(ReadOnlyDbContext db)
        {
            db.NetworkConfiguration.RemoveRange(db.NetworkConfiguration);
            db.LedgerStatus.RemoveRange(db.LedgerStatus);
            db.LedgerTransactions.RemoveRange(db.LedgerTransactions);
            InitializeDbForTests(db);
        }

        public static List<LedgerTransaction> GetSeedingLedgerTransactions()
        {
            return new List<LedgerTransaction>()
            {
                new LedgerTransaction(resultantStateVersion: 1, payloadHash: new byte[] {1,2,3,4,5,6,7,8 }, intentHash: new byte[] {1,2,3,4,5,6,7,8 }, signedTransactionHash: new byte[] {1,2,3,4,5,6,7,8 }, transactionAccumulator: new byte[] {1,2,3,4,5,6,7,8 }, message: new byte[] {1,2,3,4,5,6,7,8 }, feePaid: new TokenAmount(), epoch: 1, indexInEpoch: 0, roundInEpoch: 0,  isStartOfEpoch: true, isStartOfRound: true, roundTimestamp: InstantPattern.General.Parse("1970-01-20T05:07:28Z").Value, createdTimestamp: InstantPattern.General.Parse("2022-08-09T17:03:39Z").Value, normalizedRoundTimestamp: InstantPattern.General.Parse("1970-01-20T05:07:28Z").Value)
            };
        }

        public static List<LedgerStatus> GetSeedingLedgerStatus()
        {
            return new List<LedgerStatus>()
            {                                                                                                                                                             
                new LedgerStatus() { Id = 1, TopOfLedgerStateVersion = 1, SyncTarget = new SyncTarget() { TargetStateVersion = 1 }, LastUpdated = InstantPattern.General.Parse("2022-08-09T17:03:39Z").Value }
            };
        }

        public static List<NetworkConfiguration> GetSeedingNetworkConfiguration()
        {
            return new List<NetworkConfiguration>()
            {
                new NetworkConfiguration() { Id = 1, NetworkDefinition= new NetworkDefinition() { NetworkName = "localnet" }, NetworkAddressHrps = new NetworkAddressHrps() { AccountHrp = "ddx", ResourceHrpSuffix = "_dr", ValidatorHrp = "dv", NodeHrp = "dn" }, WellKnownAddresses = new WellKnownAddresses() { XrdAddress = "xrd_dr1qyrs8qwl"} }
            };
        }
    }
}
