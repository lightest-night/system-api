namespace LightestNight.System.Api
{
    public interface IApiClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="IApiClient" /> of the type specified
        /// </summary>
        /// <param name="baseUrl">Any base URL to set in the client</param>
        /// <typeparam name="TClient">The type of the Client to create</typeparam>
        /// <returns>A new instance of <see cref="IApiClient" /></returns>
        IApiClient Create<TClient>(string baseUrl = default)
            where TClient : ApiClient, IApiClient, new();
    }
}