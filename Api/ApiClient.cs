using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using RestSharp;

namespace LightestNight.System.Api
{
    public interface IApiClient
    {
        /// <summary>
        /// The function to use to get a machine token
        /// </summary>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <returns>A populated <see cref="TokenData" /> object containing the machine token metadata</returns>
        Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <returns></returns>
        Task<ApiResponse> MakeApiRequest(ApiRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <typeparam name="T">The type of the object to return</typeparam>
        /// <returns></returns>
        Task<ApiResponse<T>> MakeApiRequest<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class;
    }

    public abstract class ApiClient : IApiClient
    {
        private TokenData _machineToken;

        private readonly IRestClient _restClient;

        private TokenData MachineToken
        {
            get
            {
                if (_machineToken != null)
                    return _machineToken;

                var continuation = GetMachineToken().ContinueWith(machineToken => _machineToken = machineToken.Result);
                continuation.Wait();

                return _machineToken;
            }
        }

        /// <inheritdoc cref="IApiClient.GetMachineToken" />
        public abstract Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default);

        protected ApiClient(string baseUrl)
        {
            _restClient = new RestClient();
            _restClient.UseSerializer(new Serializer());
            ((RestClient) _restClient).UseJson();

            if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
                throw new UriFormatException(baseUrl);

            _restClient.BaseUrl = uri;
        }

        /// <inheritdoc cref="IApiClient.MakeApiRequest" />
        public async Task<ApiResponse> MakeApiRequest(ApiRequest request, CancellationToken cancellationToken = default)
        {
            // Prepare the request
            var restRequest = PrepareRequest(request);

            // Send the request
            var restResponse = await _restClient.ExecuteTaskAsync(restRequest, cancellationToken);

            // If the response is unsuccessful with an Unauthorized response, and we are expected to be using a Machine Token, refresh the token and try again
            if (!restResponse.IsSuccessful && request.Authorization == null && request.UseMachineToken && restResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                _machineToken = null;
                restRequest.AddOrUpdateParameter(HeaderNames.Authorization, $"Bearer {MachineToken.AccessToken}", ParameterType.HttpHeader);
                restResponse = await _restClient.ExecuteTaskAsync(restRequest, cancellationToken);
            }

            // Deal with the response, either by failing, or returning a success object
            if (restResponse.IsSuccessful)
                return ApiResponse.FromRestResponse(restResponse);

            if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedException();

            throw new RestException(request.Resource, restResponse.Content);
        }

        /// <inheritdoc cref="IApiClient.MakeApiRequest{T}" />
        public async Task<ApiResponse<T>> MakeApiRequest<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class
        {
            // Prepare the request
            var restRequest = PrepareRequest(request);

            // Send the request
            var restResponse = await _restClient.ExecuteTaskAsync<T>(restRequest, cancellationToken);

            // If the response is unsuccessful with an Unauthorized response, and we are expected to be using a Machine Token, refresh the token and try again
            if (!restResponse.IsSuccessful && request.Authorization == null && request.UseMachineToken && restResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                _machineToken = null;
                restRequest.AddOrUpdateParameter(HeaderNames.Authorization, $"Bearer {MachineToken.AccessToken}", ParameterType.HttpHeader);
                restResponse = await _restClient.ExecuteTaskAsync<T>(restRequest, cancellationToken);
            }

            // Deal with the response, either by failing, or returning a success object
            if (restResponse.IsSuccessful)
                return ApiResponse<T>.FromRestResponse(restResponse);

            if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedException();

            throw new RestException(request.Resource, restResponse.Content);
        }

        private IRestRequest PrepareRequest(ApiRequest request)
        {
            if (string.IsNullOrEmpty(request.Resource))
                throw new ArgumentNullException(nameof(request.Resource));

            var dataFormat = GetDataFormat(request);

            // Define base Request
            var restRequest = new RestRequest(request.FormattedRequestUri, GetMethod(request.HttpMethod), dataFormat);

            // Add Authorization if necessary
            if (request.Authorization != null)
            {
                if (request.Authorization.IsHeader)
                    restRequest.AddHeader(HeaderNames.Authorization, $"{request.Authorization.AuthorizationType.ToString()} {request.Authorization.AccessToken}");
                else
                    restRequest.AddQueryParameter(request.Authorization.UrlProperty, request.Authorization.AccessToken);
            }
            else if (request.UseMachineToken)
            {
                restRequest.AddHeader(HeaderNames.Authorization, $"Bearer {MachineToken.AccessToken}");
            }

            // Add Headers if any
            if (request.Headers.Any())
            {
                foreach (var header in request.Headers)
                    restRequest.AddHeader(header.Key, header.Value);
            }

            // Add Query Parameters if any
            if (request.QueryParams.Any())
            {
                foreach (var queryParam in request.QueryParams)
                    restRequest.AddQueryParameter(queryParam.Key, queryParam.Value);
            }

            // Add Request Body if necessary
            if (!MethodHasBody(request.HttpMethod))
                return restRequest;

            switch (dataFormat)
            {
                case DataFormat.Json:
                    restRequest.AddJsonBody(request.Body);
                    break;
                case DataFormat.Xml:
                    restRequest.AddXmlBody(request.Body);
                    break;
            }

            return restRequest;
        }

        private static Method GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method))
                return Method.DELETE;

            if (HttpMethods.IsGet(method))
                return Method.GET;

            if (HttpMethods.IsHead(method))
                return Method.HEAD;

            if (HttpMethods.IsOptions(method))
                return Method.OPTIONS;

            if (HttpMethods.IsPatch(method))
                return Method.PATCH;

            if (HttpMethods.IsPost(method))
                return Method.POST;

            if (HttpMethods.IsPut(method))
                return Method.PUT;

            throw new NotSupportedException($"Http Method {method} is not supported in {typeof(Method)}.");
        }

        private static bool MethodHasBody(string method)
            => HttpMethods.IsPost(method) ||
               HttpMethods.IsPut(method) ||
               HttpMethods.IsPatch(method);

        private static DataFormat GetDataFormat(ApiRequest request)
            => request.UseJson
                ? DataFormat.Json
                : request.UseXml
                    ? DataFormat.Xml
                    : DataFormat.None;
    }
}