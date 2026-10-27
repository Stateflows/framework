using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Common.Exceptions;

namespace Stateflows.Extensions.ClearScript;

/// <summary>Base class for ClearScript elements with restricted access to the active behavior and execution contexts.</summary>
/// <param name="serviceProvider">The application's service provider used to resolve the configured engine factory.</param>
/// <param name="behaviorContext">The context object exposed to script code as <c>behaviorContext</c>.</param>
/// <param name="executionContext">The execution context exposed to script code as <c>executionContext</c>.</param>
public abstract class ClearScriptElement(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
)
{
    /// <summary>Configures the script engine's host objects before script execution.</summary>
    /// <param name="engine">The engine created by the registered engine factory.</param>
    /// <remarks>Override this method to add host objects, and call the base implementation to retain the standard context objects.</remarks>
    protected virtual void ConfigureEngine(IScriptEngine engine)
    {
        engine.AddHostType(nameof(Console), typeof(Console));
        
        engine.AddRestrictedHostObject(nameof(behaviorContext), HostItemFlags.None, behaviorContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
    }

    /// <summary>Creates and configures an engine for one script execution.</summary>
    /// <returns>The configured engine returned by the registered factory.</returns>
    /// <exception cref="StateflowsException">No engine factory has been registered with <c>AddClearScript</c>.</exception>
    protected async Task<IScriptEngine> GetEngineAsync()
    {
        if (DependencyInjection.EngineFactory == null)
        {
            throw new StateflowsException(
                "There is no ClearScript engine factory registered. Add AddClearScript() call to your Program.cs.");
        }

        var engine = await DependencyInjection.EngineFactory(serviceProvider);
        
        ConfigureEngine(engine);

        return engine;
    }
}