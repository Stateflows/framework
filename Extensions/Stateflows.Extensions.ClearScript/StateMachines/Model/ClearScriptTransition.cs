using Microsoft.ClearScript;
using Stateflows.Common;

namespace Stateflows.StateMachines;

/// <summary>Transition base that evaluates overridable ClearScript effect and guard source.</summary>
/// <param name="serviceProvider">The provider used to resolve the registered script engine factory.</param>
/// <param name="behaviorContext">The active behavior context exposed to scripts.</param>
/// <param name="commonExecutionContext">The common execution context used to create the script engine.</param>
/// <param name="transitionContext">The active transition context exposed to scripts.</param>
/// <param name="executionContext">The transition execution context exposed to scripts.</param>
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
    /// <summary>Gets the source executed when the transition effect runs.</summary>
    protected virtual string EffectScript => "";
    
    /// <summary>Executes <see cref="EffectScript"/> when the transition effect runs.</summary>
    /// <returns>A task that completes when the effect script finishes.</returns>
    public async Task EffectAsync()
    {
        using var engine = await GetEngineAsync();
        engine.Execute(EffectScript);
    }

    /// <summary>Gets the expression body evaluated when the transition guard runs.</summary>
    protected virtual string GuardScript => "";
    
    /// <summary>Evaluates <see cref="GuardScript"/> and accepts the transition only when it returns Boolean <see langword="true"/>.</summary>
    /// <returns>A task containing the guard result.</returns>
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