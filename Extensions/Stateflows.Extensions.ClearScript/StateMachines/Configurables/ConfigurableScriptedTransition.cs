using Microsoft.ClearScript;
using Stateflows.Common;

namespace Stateflows.StateMachines;

internal class ConfigurableScriptedTransition(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext,
    ITransitionContext transitionContext,
    IExecutionContext executionContext
) : ScriptedTransition(serviceProvider, behaviorContext, commonExecutionContext, transitionContext, executionContext),
    IConfigurable<string>,
    IActionElement,
    IGuardElement
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        using var engine = await GetEngineAsync();
        engine.Execute(script);
    }

    private string? script;

    string IConfigurable<string>.Configuration
    {
        set => script = value ?? throw new ArgumentNullException(nameof(value));
    }

    public async Task<bool> GuardAsync()
    {
        var functionScript = $"(function() {{ {(script.Contains("return") ? "" : "return")} {script} }})";
        
        using var engine = await GetEngineAsync();
        dynamic guardFunction = engine.Evaluate(functionScript);
        var result = guardFunction();
        
        return result is true;
    }
}

internal class ConfigurableScriptedTransition<TEvent>(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext commonExecutionContext,
    ITransitionContext transitionContext,
    IExecutionContext executionContext
) : ConfigurableScriptedTransition(serviceProvider, behaviorContext, commonExecutionContext, transitionContext, executionContext),
    IConfigurable<string>,
    ITransitionEffect<TEvent>,
    IGuardElement<TEvent>
{
    private string script = string.Empty;

    string IConfigurable<string>.Configuration
    {
        set => script = value ?? throw new ArgumentNullException(nameof(value));
    }

    public async Task EffectAsync(TEvent @event)
    {
        using var engine = await GetEngineAsync();
        engine.AddRestrictedHostObject("event", HostItemFlags.None, @event);
        engine.Execute(script);
    }

    public async Task<bool> GuardAsync(TEvent @event)
    {
        using var engine = await GetEngineAsync();
        engine.AddRestrictedHostObject("event", HostItemFlags.None, @event);

        var functionScript = $"(function() {{ {(script.Contains("return") ? "" : "return")} {script} }})";

        dynamic guardFunction = engine.Evaluate(functionScript);
        var result = guardFunction();
        
        return result is true;
    }
}