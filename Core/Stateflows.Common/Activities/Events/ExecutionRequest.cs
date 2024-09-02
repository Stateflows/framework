using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Stateflows.Common;

namespace Stateflows.Activities.Events
{
    //public interface IInputContainer
    //{
    //    void AddInputToken<TToken>(TToken token);

    //    void AddInputTokens<TToken>(IEnumerable<TToken> tokens);
    //}

    //public sealed class ExecutionRequest : Request<ExecutionResponse>, IInputContainer
    //{
    //    public EventHolder InitializationEvent { get; set; } = new EventHolder<Initialize>();

    //    [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
    //    public List<TokenHolder> InputTokens { get; set; } = new List<TokenHolder>();

    //    public void SetInitializationEvent<TEvent>(TEvent initializationEvent)
    //        => InitializationEvent = new EventHolder<TEvent>() { Payload = initializationEvent };

    //    public void AddInputToken<TToken>(TToken token)
    //        => InputTokens.Add(new TokenHolder<TToken>() { Payload = token });

    //    public void AddInputTokens<TToken>(IEnumerable<TToken> tokens)
    //        => InputTokens.AddRange(tokens.Select(token => new TokenHolder<TToken>() { Payload = token }));
    //}
}
