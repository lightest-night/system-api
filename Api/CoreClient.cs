using System;
using System.Threading;
using System.Threading.Tasks;

namespace LightestNight.System.Api
{
    public class CoreClient : ApiClient
    {
        public CoreClient(string baseUrl) : base(baseUrl) { }

        public override Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}