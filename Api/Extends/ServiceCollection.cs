using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LightestNight.System.Api.Extends
{
    public static class ServiceCollection
    {
        public static IServiceCollection AddApiClientFactory(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(IApiClientFactory), typeof(ApiClientFactory));
            return services;
        } 
    }
}