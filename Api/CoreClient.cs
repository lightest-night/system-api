using System.Threading;
using System.Threading.Tasks;
using NotImplementedException = System.NotImplementedException;

namespace LightestNight.System.Api
{
    public class CoreClient : ApiClient
    {
        public CoreClient(string baseUrl) : base(baseUrl) {}

        protected override string ApiRoute { get; }
        
        protected override Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}