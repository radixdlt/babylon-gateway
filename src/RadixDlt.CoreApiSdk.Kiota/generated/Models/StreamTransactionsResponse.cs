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

// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace RadixDlt.CoreApiSdk.Kiota.Models {
    public class StreamTransactionsResponse : IBackedModel, IParsable 
    {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>An integer between `0` and `10000`, giving the total count of transactions in the returned response</summary>
        public int? Count {
            get { return BackingStore?.Get<int?>("count"); }
            set { BackingStore?.Set("count", value); }
        }
        /// <summary>The from_state_version property</summary>
        public long? FromStateVersion {
            get { return BackingStore?.Get<long?>("from_state_version"); }
            set { BackingStore?.Set("from_state_version", value); }
        }
        /// <summary>The max_ledger_state_version property</summary>
        public long? MaxLedgerStateVersion {
            get { return BackingStore?.Get<long?>("max_ledger_state_version"); }
            set { BackingStore?.Set("max_ledger_state_version", value); }
        }
        /// <summary>The previous_state_identifiers property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public LedgerHashes? PreviousStateIdentifiers {
            get { return BackingStore?.Get<LedgerHashes?>("previous_state_identifiers"); }
            set { BackingStore?.Set("previous_state_identifiers", value); }
        }
#nullable restore
#else
        public LedgerHashes PreviousStateIdentifiers {
            get { return BackingStore?.Get<LedgerHashes>("previous_state_identifiers"); }
            set { BackingStore?.Set("previous_state_identifiers", value); }
        }
#endif
        /// <summary>A ledger proof list starting from `from_state_version` (inclusive) stored by this node.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public List<LedgerProof>? Proofs {
            get { return BackingStore?.Get<List<LedgerProof>?>("proofs"); }
            set { BackingStore?.Set("proofs", value); }
        }
#nullable restore
#else
        public List<LedgerProof> Proofs {
            get { return BackingStore?.Get<List<LedgerProof>>("proofs"); }
            set { BackingStore?.Set("proofs", value); }
        }
#endif
        /// <summary>A committed transactions list starting from the `from_state_version` (inclusive).</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public List<CommittedTransaction>? Transactions {
            get { return BackingStore?.Get<List<CommittedTransaction>?>("transactions"); }
            set { BackingStore?.Set("transactions", value); }
        }
#nullable restore
#else
        public List<CommittedTransaction> Transactions {
            get { return BackingStore?.Get<List<CommittedTransaction>>("transactions"); }
            set { BackingStore?.Set("transactions", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="StreamTransactionsResponse"/> and sets the default values.
        /// </summary>
        public StreamTransactionsResponse()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="StreamTransactionsResponse"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static StreamTransactionsResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new StreamTransactionsResponse();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"count", n => { Count = n.GetIntValue(); } },
                {"from_state_version", n => { FromStateVersion = n.GetLongValue(); } },
                {"max_ledger_state_version", n => { MaxLedgerStateVersion = n.GetLongValue(); } },
                {"previous_state_identifiers", n => { PreviousStateIdentifiers = n.GetObjectValue<LedgerHashes>(LedgerHashes.CreateFromDiscriminatorValue); } },
                {"proofs", n => { Proofs = n.GetCollectionOfObjectValues<LedgerProof>(LedgerProof.CreateFromDiscriminatorValue)?.ToList(); } },
                {"transactions", n => { Transactions = n.GetCollectionOfObjectValues<CommittedTransaction>(CommittedTransaction.CreateFromDiscriminatorValue)?.ToList(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteIntValue("count", Count);
            writer.WriteLongValue("from_state_version", FromStateVersion);
            writer.WriteLongValue("max_ledger_state_version", MaxLedgerStateVersion);
            writer.WriteObjectValue<LedgerHashes>("previous_state_identifiers", PreviousStateIdentifiers);
            writer.WriteCollectionOfObjectValues<LedgerProof>("proofs", Proofs);
            writer.WriteCollectionOfObjectValues<CommittedTransaction>("transactions", Transactions);
        }
    }
}
