/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

using RadixDlt.NetworkGateway.Common.Numerics;
using RadixDlt.NetworkGateway.PostgresIntegration;
using RadixDlt.NetworkGateway.PostgresIntegration.Models;
using RadixDlt.NetworkGateway.UnitTests;
using System.Collections.Generic;
using System.Linq;

namespace RadixDlt.NetworkGateway.IntegrationTests.Utilities
{
    public static class DbSeedHelper
    {
        public static readonly string NetworkName = "integrationtestsnet";

        public static readonly byte[] DefaultHash = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32 };

        public static readonly string SubmitTransaction = $"{{\"network_identifier\": {{\"network\": \"{NetworkName}\"}}, \"notarized_transaction\": \"10020000001002000000100200000010070000000701110f000000496e7465726e616c546573746e6574000000000a00000000000000000a64000000000000000a0600000000000000912100000002f9308a019258c31049344f85f89d5229b531c845836f99b08601f113bce036f9010010010000003011010000000d000000436c656172417574685a6f6e65000000003023020000000200000091210000000279be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f817989240000000d6f37bebb4c67ebb0844dd48e447c415a13b47fafdf13495f58b21826dc044a043fb00243cfe573bbb38b8ae9371801c2b91ec92ae764238e4ff40d857e58a3002000000912100000002c6047f9441ed7d6d3045406e95c07cd85c778e4b8cef3ca7abac09b95c709ee5924000000047da0da82cdceed2a227ebd305ece670cf12aaedad6863ebce173d0952eea73b3a1b136bae431d82bae822ceb11eaed406dddc1a4a94756201cb7292139584bf9240000000a767554290bd2cba8e63bc1feeefc1534ebcd33fe345f9a8d0ac76abc1d3bd5968e847ec5ca55d6e9fe18227f13c5c114463751e9bc5a38f563ba8819d7fc882\"}}";

        internal static void InitializeDbForTests(ReadOnlyDbContext db)
        {
            db.NetworkConfiguration.AddRange(GetSeedingNetworkConfiguration());

            var rawTransactions = GetSeedingRawTransactions();
            db.RawTransactions.AddRange(rawTransactions);

            var transactions = GetSeedingLedgerTransactions(rawTransactions.First());
            db.LedgerTransactions.AddRange(transactions);

            db.LedgerStatus.AddRange(GetSeedingLedgerStatus(transactions.First()));
            db.SaveChanges();
        }

        internal static void ReinitializeDbForTests(ReadOnlyDbContext db)
        {
            db.NetworkConfiguration.RemoveRange(db.NetworkConfiguration);
            db.LedgerStatus.RemoveRange(db.LedgerStatus);
            db.RawTransactions.RemoveRange(db.RawTransactions);
            db.LedgerTransactions.RemoveRange(db.LedgerTransactions);

            InitializeDbForTests(db);
        }

        private static List<LedgerTransaction> GetSeedingLedgerTransactions(RawTransaction rawTransaction)
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
                    roundTimestamp: new FakeClock().UtcNow,
                    createdTimestamp: new FakeClock().UtcNow,
                    normalizedRoundTimestamp: new FakeClock().UtcNow)
                {
                     RawTransaction = rawTransaction,
                },
            };
        }

        private static List<LedgerStatus> GetSeedingLedgerStatus(LedgerTransaction topTransaction)
        {
            return new List<LedgerStatus>()
            {
                new LedgerStatus()
                {
                    Id = 1,
                    TopOfLedgerStateVersion = 1,
                    SyncTarget = new SyncTarget() { TargetStateVersion = 1 },
                    LastUpdated = new FakeClock().UtcNow,
                    TopOfLedgerTransaction = topTransaction,
                },
            };
        }

        private static NetworkConfiguration[] GetSeedingNetworkConfiguration()
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
            }.ToArray();
        }

        private static List<RawTransaction> GetSeedingRawTransactions()
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
