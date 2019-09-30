using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using RestSharp;

namespace LightestNight.System.Api
{
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

        protected ApiClient()
        {
            _restClient = new RestClient();
            _restClient.UseSerializer(new Serializer());
        }
        
        protected ApiClient(string baseUrl) : this()
        {
            if (string.IsNullOrEmpty(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
                throw new UriFormatException(baseUrl);

            SetBaseUri(uri);
        }

        /// <inheritdoc cref="IApiClient.SetSerializerSettings" />
        public IApiClient SetSerializerSettings(Action<JsonSerializerSettings> settings, DataFormat dataFormat = DataFormat.Json, params string[] supportedContentTypes)
        {
            _restClient.UseSerializer(new Serializer(settings, dataFormat, supportedContentTypes));
            return this;
        }

        /// <inheritdoc cref="IApiClient.ResetSerializer" />
        public IApiClient ResetSerializer()
        {
            _restClient.UseSerializer(new Serializer());
            return this;
        }

        /// <inheritdoc cref="IApiClient.SetBaseUri" />
        public void SetBaseUri(Uri baseUri)
        {
            _restClient.BaseUrl = baseUri;
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

            switch (restResponse.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedException();
                
                default:
                    throw new RestException(_restClient.BuildUri(restRequest).ToString(), restResponse.StatusCode, restResponse.Content, request.Body);
            }
        }

        public Task<ApiResponse> Get(ApiRequest request, CancellationToken cancellationToken = default)
        {
            request.HttpMethod = HttpMethods.Get;
            return MakeApiRequest(request, cancellationToken);
        }
        
        public Task<ApiResponse> Post(ApiRequest request, CancellationToken cancellationToken = default)
        {
            request.HttpMethod = HttpMethods.Post;
            return MakeApiRequest(request, cancellationToken);
        }
        
        public Task<ApiResponse> Patch(ApiRequest request, CancellationToken cancellationToken = default)
        {
            request.HttpMethod = HttpMethods.Patch;
            return MakeApiRequest(request, cancellationToken);
        }
        
        public Task<ApiResponse> Put(ApiRequest request, CancellationToken cancellationToken = default)
        {
            request.HttpMethod = HttpMethods.Put;
            return MakeApiRequest(request, cancellationToken);
        }
        
        public Task<ApiResponse> Delete(ApiRequest request, CancellationToken cancellationToken = default)
        {
            request.HttpMethod = HttpMethods.Delete;
            return MakeApiRequest(request, cancellationToken);
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

            switch (restResponse.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedException();
                
                default:
                    throw new RestException(_restClient.BuildUri(restRequest).ToString(), restResponse.StatusCode, restResponse.Content, request.Body);
            }
        }
        
        public Task<ApiResponse<T>> Get<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class
        {
            request.HttpMethod = HttpMethods.Get;
            return MakeApiRequest<T>(request, cancellationToken);
        }
        
        public Task<ApiResponse<T>> Post<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class
        {
            request.HttpMethod = HttpMethods.Post;
            return MakeApiRequest<T>(request, cancellationToken);
        }
        
        public Task<ApiResponse<T>> Patch<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class
        {
            request.HttpMethod = HttpMethods.Patch;
            return MakeApiRequest<T>(request, cancellationToken);
        }
        
        public Task<ApiResponse<T>> Put<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class
        {
            request.HttpMethod = HttpMethods.Put;
            return MakeApiRequest<T>(request, cancellationToken);
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