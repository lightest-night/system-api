using System;
using System.Threading;
using System.Threading.Tasks;

namespace LightestNight.System.Api
{
    public interface IApiClient
    {
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
}