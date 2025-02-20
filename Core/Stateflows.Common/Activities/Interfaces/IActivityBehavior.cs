using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityBehavior : IBehavior, IInputOutput
    {
        public Task<RequestResult<TokensOutput>> GetOutputAsync()
            => RequestAsync(new TokensOutputRequest());
        
        public Task<RequestResult<TokensOutput<T>>> GetOutputAsync<T>()
            => RequestAsync(new TokensOutputRequest<T>());
    }
}
