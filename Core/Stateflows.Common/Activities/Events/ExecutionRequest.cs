using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Stateflows.Activities.Events
{
    public interface IInputContainer
    {
        void AddInputToken<TToken>(TToken token);

        void AddInputTokens<TToken>(IEnumerable<TToken> tokens);
    }

    public sealed class ExecutionRequest : Request<ExecutionResponse>, IInputContainer
    {
        public Event InitializationEvent { get; set; } = new Initialize();

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public List<TokenHolder> InputTokens { get; set; } = new List<TokenHolder>();

        public void AddInputToken<TToken>(TToken token)
            => InputTokens.Add(new TokenHolder<TToken>() { Payload = token });

        public void AddInputTokens<TToken>(IEnumerable<TToken> tokens)
            => InputTokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));
    }
}
