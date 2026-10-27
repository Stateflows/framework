using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows.Actions
{
    /// <summary>A client handle for sending events to an action and executing it with token input and output.</summary>
    /// <remarks>Obtaining a handle does not execute the action. See <see href="https://github.com/Stateflows/framework/wiki/behaviors">behaviors</see>.</remarks>
    public interface IActionBehavior : IBehavior, IInputOutput
    {
        /// <summary>Executes the action with an empty token-input request.</summary>
        /// <returns>The request delivery result and output tokens produced by the action, when available.</returns>
        public Task<RequestResult<TokensOutput>> ExecuteAsync()
            => RequestAsync(new TokensInput());
    }
}
