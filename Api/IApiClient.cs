using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace LightestNight.System.Api
{
    public interface IApiClient
    {
        /// <summary>
        /// Updates the Serializer used within the client
        /// </summary>
        /// <param name="settings">Any <see cref="JsonSerializerSettings" /> to set</param>
        /// <param name="dataFormat">The <see cref="DataFormat" /> to use</param>
        /// <param name="supportedContentTypes">The Supported Content Types</param>
        /// <returns>The instance of the <see cref="IApiClient" /> but with the serializer settings set</returns>
        IApiClient SetSerializerSettings(Action<JsonSerializerSettings> settings, DataFormat dataFormat = DataFormat.Json, params string[] supportedContentTypes);

        /// <summary>
        /// Resets the Serializer used with the client to the default
        /// </summary>
        /// <returns>The instance of the <see cref="IApiClient" /> but with the serializer settings reset to the defaults</returns>
        IApiClient ResetSerializer();
        
        /// <summary>
        /// Sets the base Uri for the Rest Client
        /// </summary>
        /// <param name="baseUri">The URI to set</param>
        void SetBaseUri(Uri baseUri);
        
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
        Task<ApiResponse> MakeApiRequest(ApiRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <typeparam name="T">The type of the object to return</typeparam>
        Task<ApiResponse<T>> MakeApiRequest<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Makes an API request using the POST Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of POST</remarks>
        Task<ApiResponse> Post(ApiRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request using the POST Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of POST</remarks>
        /// <typeparam name="T">The type of the object to return</typeparam>
        Task<ApiResponse<T>> Post<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class;
        
        /// <summary>
        /// Makes an API request using the PUT Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of PUT</remarks>
        Task<ApiResponse> Put(ApiRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request using the PUT Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of PUT</remarks>
        /// <typeparam name="T">The type of the object to return</typeparam>
        Task<ApiResponse<T>> Put<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class;
        
        /// <summary>
        /// Makes an API request using the PATCH Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of PATCH</remarks>
        Task<ApiResponse> Patch(ApiRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request using the PATCH Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of PATCH</remarks>
        /// <typeparam name="T">The type of the object to return</typeparam>
        Task<ApiResponse<T>> Patch<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class;
        
        /// <summary>
        /// Makes an API request using the DELETE Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of DELETE</remarks>
        Task<ApiResponse> Delete(ApiRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request using the GET Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of GET</remarks>
        Task<ApiResponse> Get(ApiRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Makes an API request using the GET Http Method
        /// </summary>
        /// <param name="request">The <see cref="ApiRequest" /> containing the metadata of this request</param>
        /// <param name="cancellationToken">Any <see cref="CancellationToken" /> to use during the request</param>
        /// <remarks>Any Http Method set in the <see cref="ApiRequest" /> will be ignored in favour of GET</remarks>
        /// <typeparam name="T">The type of the object to return</typeparam>
        Task<ApiResponse<T>> Get<T>(ApiRequest request, CancellationToken cancellationToken = default)
            where T : class;
    }
}