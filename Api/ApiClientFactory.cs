using System;

namespace LightestNight.System.Api
{
    public class ApiClientFactory : IApiClientFactory
    {
        public IApiClient Create<TClient>(string baseUrl = default)
            where TClient : ApiClient, IApiClient, new()
        {
            var client = new TClient();

            if (baseUrl == default) 
                return client;
            
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                throw new UriFormatException($"{baseUrl} is not a valid Absolute URI");
                
            client.SetBaseUri(baseUri);

            return client;
        }
    }
}