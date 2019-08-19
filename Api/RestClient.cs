using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using RestSharp;

namespace LightestNight.System.Api
{
    public abstract class ApiClient
    {
        private TokenData _machineToken;

        private readonly IRestClient _restClient;
        
        /// <summary>
        /// If set, the Access Token to use during machine to machine requests
        /// </summary>
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
        
        /// <summary>
        /// The function to use to get a machine token
        /// </summary>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <returns>A populated <see cref="TokenData" /> object containing the machine token metadata</returns>
        protected abstract Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// If set, the route of the API request, all subsequent requests made will have this route prefixed if not already
        /// </summary>
        protected abstract string ApiRoute { get; }

        /// <summary>
        /// Sets or Gets the current BaseUrl value
        /// </summary>
        protected Uri BaseUrl
        {
            get => _restClient.BaseUrl;
            set => _restClient.BaseUrl = value;
        }

        protected ApiClient(IRestClient restClient)
        {
            _restClient = restClient;
            _restClient.UseSerializer(new Serializer());
            
            if (_restClient is RestClient client)
                client.UseJson();
        }

        /// <summary>
        /// Makes an API request
        /// </summary>
        /// <param name="request">The <see cref="IRestRequest" /> containing the metadata of this request</param>
        /// <param name="authorizationProvided">Denotes whether authorization has been provided or whether a machine token will be required</param>
        /// <param name="isApiRequest">Denotes whether this is an API request and if so, whether the <see cref="ApiRoute" /> should be prepended</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <typeparam name="T">The type of the object to return</typeparam>
        /// <returns>If request is successful, a populated instance of <see cref="T" /></returns>
        public async Task<IRestResponse<T>> MakeRequest<T>(IRestRequest request, bool authorizationProvided = false, bool isApiRequest = true, CancellationToken cancellationToken = default)
        {
            if (isApiRequest && string.IsNullOrEmpty(request.Resource))
                throw new ArgumentNullException(nameof(request.Resource));
            
            if (!authorizationProvided)
                request.AddHeader(HeaderNames.Authorization, $"Bearer {MachineToken.AccessToken}");

            if (isApiRequest && !string.IsNullOrEmpty(ApiRoute) && !request.Resource.StartsWith(ApiRoute))
                request.Resource = FormatResource(request.Resource);

            var response = await _restClient.ExecuteTaskAsync<T>(request, cancellationToken);
            if (!response.IsSuccessful && response.StatusCode == HttpStatusCode.Unauthorized && !authorizationProvided)
            {
                _machineToken = null;
                request.AddOrUpdateParameter(HeaderNames.Authorization, $"Bearer {MachineToken.AccessToken}", ParameterType.HttpHeader);
                response = await _restClient.ExecuteTaskAsync<T>(request, cancellationToken);
            }

            if (response.IsSuccessful) 
                return response;
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedException();
                
            throw new RestException(request.Resource, response.Content);
        }
        
        /// <summary>
        /// Makes an API request
        /// </summary>
        /// <param name="request">The <see cref="IRestRequest" /> containing the metadata of this request</param>
        /// <param name="authorizationProvided">Denotes whether authorization has been provided or whether a machine token will be required</param>
        /// <param name="isApiRequest">Denotes whether this is an API request and if so, whether the <see cref="ApiRoute" /> should be prepended</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <returns>A populated <see cref="IRestResponse" /> object</returns>
        public async Task<IRestResponse> MakeRequest(IRestRequest request, bool authorizationProvided = false, bool isApiRequest = true, CancellationToken cancellationToken = default)
        {
            if (isApiRequest && string.IsNullOrEmpty(request.Resource))
                throw new ArgumentNullException(nameof(request.Resource));
            
            if (!authorizationProvided)
                request.AddHeader("Authorization", $"Bearer {MachineToken.AccessToken}");

            if (isApiRequest && !string.IsNullOrEmpty(ApiRoute) && !request.Resource.StartsWith(ApiRoute))
                request.Resource = FormatResource(request.Resource);

            var response = await _restClient.ExecuteTaskAsync(request, cancellationToken);
            if (!response.IsSuccessful && response.StatusCode == HttpStatusCode.Unauthorized && !authorizationProvided)
            {
                _machineToken = null;
                request.AddOrUpdateParameter("Authorization", $"Bearer {MachineToken.AccessToken}", ParameterType.HttpHeader);
                response = await _restClient.ExecuteTaskAsync(request, cancellationToken);
            }
            
            if (response.IsSuccessful) 
                return response;
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new UnauthorizedException();
                
            throw new RestException(request.Resource, response.Content);
        }

        private string FormatResource(string resource)
        {
            var apiRoute = ApiRoute;
            if (!apiRoute.EndsWith("/"))
                apiRoute += "/";

            return resource.StartsWith("/")
                ? $"{apiRoute}{resource.Substring(1)}"
                : $"{apiRoute}{resource}";
        }
    }
}