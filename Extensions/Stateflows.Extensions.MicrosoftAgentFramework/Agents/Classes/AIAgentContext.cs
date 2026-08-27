using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.MAF.AIAgents;

namespace Stateflows.Extensions.MicrosoftAgentFramework.AIAgents.Classes;

internal class AIAgentContext(IBehaviorContext functionContext, IInput input, IOutput output) : IAIAgentContext
{
    public Task SubscribeAsync<TNotification>(BehaviorId behaviorId)
        => functionContext.SubscribeAsync<TNotification>(behaviorId);
    public Task UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
        => functionContext.UnsubscribeAsync<TNotification>(behaviorId);
    public IServiceProvider ServiceProvider => functionContext.ServiceProvider;

    BehaviorId IBehaviorContext.Id => ((IBehaviorContext)functionContext).Id;

    AIAgentId IAIAgentContext.Id => new(functionContext.Id);

    public IContextValues Values => functionContext.Values;

    public void Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
    {
        functionContext.Send(@event, headers);
    }

    public void PublishRange<TNotification>(IEnumerable<TNotification> notifications, IDictionary<string, EventHeader>? headers = null)
    {
        functionContext.PublishRange(notifications, headers);
    }

    public bool IsEmbedded => functionContext.IsEmbedded;

    // todo
    // public IEnumerable<object> GetTokens()
    //     => input.GetTokens();
    //
    // public bool HasTokensOfType<TToken>()
    //     => input.HasTokensOfType<TToken>();

    public IEnumerable<TToken> GetTokensOfType<TToken>()
        => input.GetTokensOfType<TToken>();

    public bool HasTokensOfType<TToken>()
        => input.HasTokensOfType<TToken>();

    public void Output<TToken>(TToken token)
    {
        output.Output(token);
    }

    public void OutputRange<TToken>(IEnumerable<TToken> tokens)
    {
        output.OutputRange(tokens);
    }

    public void PassTokensOfTypeOn<TToken>()
    {
        output.PassTokensOfTypeOn<TToken>();
    }

    public void PassAllTokensOn()
    {
        output.PassAllTokensOn();
    }

    public Task<bool> TryMutateAsync<TMutationEvent>(TMutationEvent mutationEvent, IDictionary<string, EventHeader> headers = null)
    {
        throw new NotImplementedException();
    }

    public Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(IDictionary<string, EventHeader> headers = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> TrySetAsync<T>(string fieldName, T fieldValue, IDictionary<string, EventHeader> headers = null)
    {
        throw new NotImplementedException();
    }

    public Task<(bool Success, T Field)> TryGetAsync<T>(string fieldName, IDictionary<string, EventHeader> headers = null)
    {
        throw new NotImplementedException();
    }
}