using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions
{
    public interface IActionBehavior : IBehavior, IInputOutput
    {
        public Task<RequestResult<TokensOutput>> ExecuteAsync()
            => RequestAsync(new TokensInput());
    }
}
