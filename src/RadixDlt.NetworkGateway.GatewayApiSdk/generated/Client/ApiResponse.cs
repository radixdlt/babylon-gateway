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

/*
 * Babylon Gateway API - RCnet V3
 *
 * This API is exposed by the Babylon Radix Gateway to enable clients to efficiently query current and historic state on the RadixDLT ledger, and intelligently handle transaction submission.  It is designed for use by wallets and explorers. For simple use cases, you can typically use the Core API on a Node. A Gateway is only needed for reading historic snapshots of ledger states or a more robust set-up.  The Gateway API is implemented by the [Network Gateway](https://github.com/radixdlt/babylon-gateway), which is configured to read from [full node(s)](https://github.com/radixdlt/babylon-node) to extract and index data from the network.  This document is an API reference documentation, visit [User Guide](https://docs-babylon.radixdlt.com/) to learn more about how to run a Gateway of your own.  ## Migration guide  Please see [the latest release notes](https://github.com/radixdlt/babylon-gateway/releases).  ## Integration and forward compatibility guarantees  We give no guarantees that other endpoints will not change before Babylon mainnet launch, although changes are expected to be minimal. 
 *
 * The version of the OpenAPI document: 0.5.0
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Net;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Client
{
    /// <summary>
    /// Provides a non-generic contract for the ApiResponse wrapper.
    /// </summary>
    public interface IApiResponse
    {
        /// <summary>
        /// The data type of <see cref="Content"/>
        /// </summary>
        Type ResponseType { get; }

        /// <summary>
        /// The content of this response
        /// </summary>
        Object Content { get; }

        /// <summary>
        /// Gets or sets the status code (HTTP status code)
        /// </summary>
        /// <value>The status code.</value>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets or sets the HTTP headers
        /// </summary>
        /// <value>HTTP headers</value>
        Multimap<string, string> Headers { get; }

        /// <summary>
        /// Gets or sets any error text defined by the calling client.
        /// </summary>
        string ErrorText { get; set; }

        /// <summary>
        /// Gets or sets any cookies passed along on the response.
        /// </summary>
        List<Cookie> Cookies { get; set; }

        /// <summary>
        /// The raw content of this response
        /// </summary>
        string RawContent { get; }
    }

    /// <summary>
    /// API Response
    /// </summary>
    public class ApiResponse<T> : IApiResponse
    {
        #region Properties

        /// <summary>
        /// Gets or sets the status code (HTTP status code)
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets or sets the HTTP headers
        /// </summary>
        /// <value>HTTP headers</value>
        public Multimap<string, string> Headers { get; }

        /// <summary>
        /// Gets or sets the data (parsed HTTP body)
        /// </summary>
        /// <value>The data.</value>
        public T Data { get; }

        /// <summary>
        /// Gets or sets any error text defined by the calling client.
        /// </summary>
        public string ErrorText { get; set; }

        /// <summary>
        /// Gets or sets any cookies passed along on the response.
        /// </summary>
        public List<Cookie> Cookies { get; set; }

        /// <summary>
        /// The content of this response
        /// </summary>
        public Type ResponseType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// The data type of <see cref="Content"/>
        /// </summary>
        public object Content
        {
            get { return Data; }
        }

        /// <summary>
        /// The raw content
        /// </summary>
        public string RawContent { get; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse{T}" /> class.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="data">Data (parsed HTTP body)</param>
        /// <param name="rawContent">Raw content.</param>
        public ApiResponse(HttpStatusCode statusCode, Multimap<string, string> headers, T data, string rawContent)
        {
            StatusCode = statusCode;
            Headers = headers;
            Data = data;
            RawContent = rawContent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse{T}" /> class.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="data">Data (parsed HTTP body)</param>
        public ApiResponse(HttpStatusCode statusCode, Multimap<string, string> headers, T data) : this(statusCode, headers, data, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse{T}" /> class.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="data">Data (parsed HTTP body)</param>
        /// <param name="rawContent">Raw content.</param>
        public ApiResponse(HttpStatusCode statusCode, T data, string rawContent) : this(statusCode, null, data, rawContent)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse{T}" /> class.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="data">Data (parsed HTTP body)</param>
        public ApiResponse(HttpStatusCode statusCode, T data) : this(statusCode, data, null)
        {
        }

        #endregion Constructors
    }
}
