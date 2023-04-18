using System.Collections.Generic;

namespace Stateflows.Common
{
    public class InitializationRequest<TResponse> : Request<TResponse>
        where TResponse : InitializationResponse
    {
        public override string Name => nameof(InitializationRequest);

        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
    }

    public class InitializationRequest : InitializationRequest<InitializationResponse>
    { }
}
