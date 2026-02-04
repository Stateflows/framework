using Microsoft.ClearScript;
using Stateflows.Common;

namespace Stateflows.StateMachines;

public class ClearScriptTransition(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext,
    ITransitionContext transitionContext,
    IExecutionContext executionContext
) : ScriptedTransition(serviceProvider, behaviorContext, commonExecutionContext, transitionContext, executionContext),
    ITransitionEffect,
    ITransitionGuard
{
    protected virtual string EffectScript => "";
    
    public async Task EffectAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(EffectScript);
    }

    protected virtual string GuardScript => "";
    
    public async Task<bool> GuardAsync()
    {
        var functionScript = $"(function() {{ {(GuardScript.Contains("return") ? "" : "return")} {GuardScript} }})";

        using var engine = await GetEngineAsync();
        dynamic guardFunction = engine.Evaluate(functionScript);
        var result = guardFunction();
        
        return result is true;
    }
}

internal sealed class ClearScriptTransition<TEvent>(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext,
    ITransitionContext transitionContext,
    IExecutionContext executionContext
) : ClearScriptTransition(serviceProvider, behaviorContext, commonExecutionContext, transitionContext, executionContext),
    ITransitionEffect<TEvent>,
    ITransitionGuard<TEvent>
{
    public async Task EffectAsync(TEvent @event)
    {
        using var engine = await GetEngineAsync();
        engine.AddRestrictedHostObject("event", HostItemFlags.None, @event);

        await EffectAsync();
    }

    public async Task<bool> GuardAsync(TEvent @event)
    {
        using var engine = await GetEngineAsync();
        engine.AddRestrictedHostObject("event", HostItemFlags.None, @event);

        return await GuardAsync();
    }
}