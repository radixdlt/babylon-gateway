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

// <auto-generated>
/*
 * Radix Core API - Babylon
 *
 * This API is exposed by the Babylon Radix node to give clients access to the Radix Engine, Mempool and State in the node.  The default configuration is intended for use by node-runners on a private network, and is not intended to be exposed publicly. Very heavy load may impact the node's function. The node exposes a configuration flag which allows disabling certain endpoints which may be problematic, but monitoring is advised. This configuration parameter is `api.core.flags.enable_unbounded_endpoints` / `RADIXDLT_CORE_API_FLAGS_ENABLE_UNBOUNDED_ENDPOINTS`.  This API exposes queries against the node's current state (see `/lts/state/` or `/state/`), and streams of transaction history (under `/lts/stream/` or `/stream`).  If you require queries against snapshots of historical ledger state, you may also wish to consider using the [Gateway API](https://docs-babylon.radixdlt.com/).  ## Integration and forward compatibility guarantees  Integrators (such as exchanges) are recommended to use the `/lts/` endpoints - they have been designed to be clear and simple for integrators wishing to create and monitor transactions involving fungible transfers to/from accounts.  All endpoints under `/lts/` have high guarantees of forward compatibility in future node versions. We may add new fields, but existing fields will not be changed. Assuming the integrating code uses a permissive JSON parser which ignores unknown fields, any additions will not affect existing code.  Other endpoints may be changed with new node versions carrying protocol-updates, although any breaking changes will be flagged clearly in the corresponding release notes.  All responses may have additional fields added, so clients are advised to use JSON parsers which ignore unknown fields on JSON objects. 
 *
 * The version of the OpenAPI document: v1.0.4
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using RadixDlt.CoreApiSdk.GenericHost.Client;
using RadixDlt.CoreApiSdk.GenericHost.Model;
using System.Diagnostics.CodeAnalysis;

namespace RadixDlt.CoreApiSdk.GenericHost.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// This class is registered as transient.
    /// </summary>
    public interface IStreamApi : IApi
    {
        /// <summary>
        /// The class containing the events
        /// </summary>
        StreamApiEvents Events { get; }

        /// <summary>
        /// Get Committed Transactions
        /// </summary>
        /// <remarks>
        /// Returns the list of committed transactions. 
        /// </remarks>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="IStreamTransactionsPostApiResponse"/>&gt;</returns>
        Task<IStreamTransactionsPostApiResponse> StreamTransactionsPostAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Committed Transactions
        /// </summary>
        /// <remarks>
        /// Returns the list of committed transactions. 
        /// </remarks>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="IStreamTransactionsPostApiResponse"/>?&gt;</returns>
        Task<IStreamTransactionsPostApiResponse?> StreamTransactionsPostOrDefaultAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// The <see cref="IStreamTransactionsPostApiResponse"/>
    /// </summary>
    public interface IStreamTransactionsPostApiResponse : RadixDlt.CoreApiSdk.GenericHost.Client.IApiResponse, IOk<RadixDlt.CoreApiSdk.GenericHost.Model.StreamTransactionsResponse?>, IBadRequest<RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse?>, IInternalServerError<RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse?>
    {
        /// <summary>
        /// Returns true if the response is 200 Ok
        /// </summary>
        /// <returns></returns>
        bool IsOk { get; }

        /// <summary>
        /// Returns true if the response is 400 BadRequest
        /// </summary>
        /// <returns></returns>
        bool IsBadRequest { get; }

        /// <summary>
        /// Returns true if the response is 500 InternalServerError
        /// </summary>
        /// <returns></returns>
        bool IsInternalServerError { get; }
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class StreamApiEvents
    {
        /// <summary>
        /// The event raised after the server response
        /// </summary>
        public event EventHandler<ApiResponseEventArgs>? OnStreamTransactionsPost;

        /// <summary>
        /// The event raised after an error querying the server
        /// </summary>
        public event EventHandler<ExceptionEventArgs>? OnErrorStreamTransactionsPost;

        internal void ExecuteOnStreamTransactionsPost(StreamApi.StreamTransactionsPostApiResponse apiResponse)
        {
            OnStreamTransactionsPost?.Invoke(this, new ApiResponseEventArgs(apiResponse));
        }

        internal void ExecuteOnErrorStreamTransactionsPost(Exception exception)
        {
            OnErrorStreamTransactionsPost?.Invoke(this, new ExceptionEventArgs(exception));
        }
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public sealed partial class StreamApi : IStreamApi
    {
        private JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// The logger factory
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// The logger
        /// </summary>
        public ILogger<StreamApi> Logger { get; }

        /// <summary>
        /// The HttpClient
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// The class containing the events
        /// </summary>
        public StreamApiEvents Events { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamApi"/> class.
        /// </summary>
        /// <returns></returns>
        public StreamApi(ILogger<StreamApi> logger, ILoggerFactory loggerFactory, HttpClient httpClient, JsonSerializerOptionsProvider jsonSerializerOptionsProvider, StreamApiEvents streamApiEvents)
        {
            _jsonSerializerOptions = jsonSerializerOptionsProvider.Options;
            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<StreamApi>();
            HttpClient = httpClient;
            Events = streamApiEvents;
        }

        partial void FormatStreamTransactionsPost(StreamTransactionsRequest streamTransactionsRequest);

        /// <summary>
        /// Validates the request parameters
        /// </summary>
        /// <param name="streamTransactionsRequest"></param>
        /// <returns></returns>
        private void ValidateStreamTransactionsPost(StreamTransactionsRequest streamTransactionsRequest)
        {
            if (streamTransactionsRequest == null)
                throw new ArgumentNullException(nameof(streamTransactionsRequest));
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="streamTransactionsRequest"></param>
        private void AfterStreamTransactionsPostDefaultImplementation(IStreamTransactionsPostApiResponse apiResponseLocalVar, StreamTransactionsRequest streamTransactionsRequest)
        {
            bool suppressDefaultLog = false;
            AfterStreamTransactionsPost(ref suppressDefaultLog, apiResponseLocalVar, streamTransactionsRequest);
            if (!suppressDefaultLog)
                Logger.LogInformation("{0,-9} | {1} | {3}", (apiResponseLocalVar.DownloadedAt - apiResponseLocalVar.RequestedAt).TotalSeconds, apiResponseLocalVar.StatusCode, apiResponseLocalVar.Path);
        }

        /// <summary>
        /// Processes the server response
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="apiResponseLocalVar"></param>
        /// <param name="streamTransactionsRequest"></param>
        partial void AfterStreamTransactionsPost(ref bool suppressDefaultLog, IStreamTransactionsPostApiResponse apiResponseLocalVar, StreamTransactionsRequest streamTransactionsRequest);

        /// <summary>
        /// Logs exceptions that occur while retrieving the server response
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="streamTransactionsRequest"></param>
        private void OnErrorStreamTransactionsPostDefaultImplementation(Exception exception, string pathFormat, string path, StreamTransactionsRequest streamTransactionsRequest)
        {
            bool suppressDefaultLog = false;
            OnErrorStreamTransactionsPost(ref suppressDefaultLog, exception, pathFormat, path, streamTransactionsRequest);
            if (!suppressDefaultLog)
                Logger.LogError(exception, "An error occurred while sending the request to the server.");
        }

        /// <summary>
        /// A partial method that gives developers a way to provide customized exception handling
        /// </summary>
        /// <param name="suppressDefaultLog"></param>
        /// <param name="exception"></param>
        /// <param name="pathFormat"></param>
        /// <param name="path"></param>
        /// <param name="streamTransactionsRequest"></param>
        partial void OnErrorStreamTransactionsPost(ref bool suppressDefaultLog, Exception exception, string pathFormat, string path, StreamTransactionsRequest streamTransactionsRequest);

        /// <summary>
        /// Get Committed Transactions Returns the list of committed transactions. 
        /// </summary>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="IStreamTransactionsPostApiResponse"/>&gt;</returns>
        public async Task<IStreamTransactionsPostApiResponse?> StreamTransactionsPostOrDefaultAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                return await StreamTransactionsPostAsync(streamTransactionsRequest, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get Committed Transactions Returns the list of committed transactions. 
        /// </summary>
        /// <exception cref="ApiException">Thrown when fails to make API call</exception>
        /// <param name="streamTransactionsRequest"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns><see cref="Task"/>&lt;<see cref="IStreamTransactionsPostApiResponse"/>&gt;</returns>
        public async Task<IStreamTransactionsPostApiResponse> StreamTransactionsPostAsync(StreamTransactionsRequest streamTransactionsRequest, System.Threading.CancellationToken cancellationToken = default)
        {
            UriBuilder uriBuilderLocalVar = new UriBuilder();

            try
            {
                ValidateStreamTransactionsPost(streamTransactionsRequest);

                FormatStreamTransactionsPost(streamTransactionsRequest);

                using (HttpRequestMessage httpRequestMessageLocalVar = new HttpRequestMessage())
                {
                    uriBuilderLocalVar.Host = HttpClient.BaseAddress!.Host;
                    uriBuilderLocalVar.Port = HttpClient.BaseAddress.Port;
                    uriBuilderLocalVar.Scheme = HttpClient.BaseAddress.Scheme;
                    uriBuilderLocalVar.Path = ClientUtils.CONTEXT_PATH + "/stream/transactions";

                    httpRequestMessageLocalVar.Content = (streamTransactionsRequest as object) is System.IO.Stream stream
                        ? httpRequestMessageLocalVar.Content = new StreamContent(stream)
                        : httpRequestMessageLocalVar.Content = new StringContent(JsonSerializer.Serialize(streamTransactionsRequest, _jsonSerializerOptions));

                    httpRequestMessageLocalVar.RequestUri = uriBuilderLocalVar.Uri;

                    string[] contentTypes = new string[] {
                        "application/json"
                    };

                    string? contentTypeLocalVar = ClientUtils.SelectHeaderContentType(contentTypes);

                    if (contentTypeLocalVar != null && httpRequestMessageLocalVar.Content != null)
                        httpRequestMessageLocalVar.Content.Headers.ContentType = new MediaTypeHeaderValue(contentTypeLocalVar);

                    string[] acceptLocalVars = new string[] {
                        "application/json"
                    };

                    string? acceptLocalVar = ClientUtils.SelectHeaderAccept(acceptLocalVars);

                    if (acceptLocalVar != null)
                        httpRequestMessageLocalVar.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptLocalVar));

                    httpRequestMessageLocalVar.Method = HttpMethod.Post;

                    DateTime requestedAtLocalVar = DateTime.UtcNow;

                    using (HttpResponseMessage httpResponseMessageLocalVar = await HttpClient.SendAsync(httpRequestMessageLocalVar, cancellationToken).ConfigureAwait(false))
                    {
                        string responseContentLocalVar = await httpResponseMessageLocalVar.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                        ILogger<StreamTransactionsPostApiResponse> apiResponseLoggerLocalVar = LoggerFactory.CreateLogger<StreamTransactionsPostApiResponse>();

                        StreamTransactionsPostApiResponse apiResponseLocalVar = new(apiResponseLoggerLocalVar, httpRequestMessageLocalVar, httpResponseMessageLocalVar, responseContentLocalVar, "/stream/transactions", requestedAtLocalVar, _jsonSerializerOptions);

                        AfterStreamTransactionsPostDefaultImplementation(apiResponseLocalVar, streamTransactionsRequest);

                        Events.ExecuteOnStreamTransactionsPost(apiResponseLocalVar);

                        return apiResponseLocalVar;
                    }
                }
            }
            catch(Exception e)
            {
                OnErrorStreamTransactionsPostDefaultImplementation(e, "/stream/transactions", uriBuilderLocalVar.Path, streamTransactionsRequest);
                Events.ExecuteOnErrorStreamTransactionsPost(e);
                throw;
            }
        }

        /// <summary>
        /// The <see cref="StreamTransactionsPostApiResponse"/>
        /// </summary>
        public partial class StreamTransactionsPostApiResponse : RadixDlt.CoreApiSdk.GenericHost.Client.ApiResponse, IStreamTransactionsPostApiResponse
        {
            /// <summary>
            /// The logger
            /// </summary>
            public ILogger<StreamTransactionsPostApiResponse> Logger { get; }

            /// <summary>
            /// The <see cref="StreamTransactionsPostApiResponse"/>
            /// </summary>
            /// <param name="logger"></param>
            /// <param name="httpRequestMessage"></param>
            /// <param name="httpResponseMessage"></param>
            /// <param name="rawContent"></param>
            /// <param name="path"></param>
            /// <param name="requestedAt"></param>
            /// <param name="jsonSerializerOptions"></param>
            public StreamTransactionsPostApiResponse(ILogger<StreamTransactionsPostApiResponse> logger, System.Net.Http.HttpRequestMessage httpRequestMessage, System.Net.Http.HttpResponseMessage httpResponseMessage, string rawContent, string path, DateTime requestedAt, System.Text.Json.JsonSerializerOptions jsonSerializerOptions) : base(httpRequestMessage, httpResponseMessage, rawContent, path, requestedAt, jsonSerializerOptions)
            {
                Logger = logger;
                OnCreated(httpRequestMessage, httpResponseMessage);
            }

            partial void OnCreated(System.Net.Http.HttpRequestMessage httpRequestMessage, System.Net.Http.HttpResponseMessage httpResponseMessage);

            /// <summary>
            /// Returns true if the response is 200 Ok
            /// </summary>
            /// <returns></returns>
            public bool IsOk => 200 == (int)StatusCode;

            /// <summary>
            /// Deserializes the response if the response is 200 Ok
            /// </summary>
            /// <returns></returns>
            public RadixDlt.CoreApiSdk.GenericHost.Model.StreamTransactionsResponse? Ok()
            {
                // This logic may be modified with the AsModel.mustache template
                return IsOk
                    ? System.Text.Json.JsonSerializer.Deserialize<RadixDlt.CoreApiSdk.GenericHost.Model.StreamTransactionsResponse>(RawContent, _jsonSerializerOptions)
                    : null;
            }

            /// <summary>
            /// Returns true if the response is 200 Ok and the deserialized response is not null
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            public bool TryOk([NotNullWhen(true)]out RadixDlt.CoreApiSdk.GenericHost.Model.StreamTransactionsResponse? result)
            {
                result = null;

                try
                {
                    result = Ok();
                } catch (Exception e)
                {
                    OnDeserializationErrorDefaultImplementation(e, (HttpStatusCode)200);
                }

                return result != null;
            }

            /// <summary>
            /// Returns true if the response is 400 BadRequest
            /// </summary>
            /// <returns></returns>
            public bool IsBadRequest => 400 == (int)StatusCode;

            /// <summary>
            /// Deserializes the response if the response is 400 BadRequest
            /// </summary>
            /// <returns></returns>
            public RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse? BadRequest()
            {
                // This logic may be modified with the AsModel.mustache template
                return IsBadRequest
                    ? System.Text.Json.JsonSerializer.Deserialize<RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse>(RawContent, _jsonSerializerOptions)
                    : null;
            }

            /// <summary>
            /// Returns true if the response is 400 BadRequest and the deserialized response is not null
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            public bool TryBadRequest([NotNullWhen(true)]out RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse? result)
            {
                result = null;

                try
                {
                    result = BadRequest();
                } catch (Exception e)
                {
                    OnDeserializationErrorDefaultImplementation(e, (HttpStatusCode)400);
                }

                return result != null;
            }

            /// <summary>
            /// Returns true if the response is 500 InternalServerError
            /// </summary>
            /// <returns></returns>
            public bool IsInternalServerError => 500 == (int)StatusCode;

            /// <summary>
            /// Deserializes the response if the response is 500 InternalServerError
            /// </summary>
            /// <returns></returns>
            public RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse? InternalServerError()
            {
                // This logic may be modified with the AsModel.mustache template
                return IsInternalServerError
                    ? System.Text.Json.JsonSerializer.Deserialize<RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse>(RawContent, _jsonSerializerOptions)
                    : null;
            }

            /// <summary>
            /// Returns true if the response is 500 InternalServerError and the deserialized response is not null
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            public bool TryInternalServerError([NotNullWhen(true)]out RadixDlt.CoreApiSdk.GenericHost.Model.BasicErrorResponse? result)
            {
                result = null;

                try
                {
                    result = InternalServerError();
                } catch (Exception e)
                {
                    OnDeserializationErrorDefaultImplementation(e, (HttpStatusCode)500);
                }

                return result != null;
            }

            private void OnDeserializationErrorDefaultImplementation(Exception exception, HttpStatusCode httpStatusCode)
            {
                bool suppressDefaultLog = false;
                OnDeserializationError(ref suppressDefaultLog, exception, httpStatusCode);
                if (!suppressDefaultLog)
                    Logger.LogError(exception, "An error occurred while deserializing the {code} response.", httpStatusCode);
            }

            partial void OnDeserializationError(ref bool suppressDefaultLog, Exception exception, HttpStatusCode httpStatusCode);
        }
    }
}
