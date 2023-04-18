using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Common;
using Stateflows.Common.Interfaces;

namespace Stateflows
{
    public static class BehaviorExtensions
    {
        public static async Task<bool> InitializeAsync(this IBehavior behavior, Dictionary<string, object> values = null)
            => (await behavior.RequestAsync(new InitializationRequest() { Values = values }))?.InitializationSuccessful ?? false;

        public static async Task<bool> GetInitializedAsync(this IBehavior behavior)
            => (await behavior.RequestAsync(new InitializedRequest()))?.Initialized ?? false;
    }
}