using Microsoft.ClearScript;
using Stateflows.Common;
using Stateflows.Common.Exceptions;

namespace Stateflows.Extensions.ClearScript;

public abstract class ClearScriptElement(
    IServiceProvider serviceProvider,
    IBehaviorContext behaviorContext,
    IExecutionContext executionContext
)
{
    protected virtual void ConfigureEngine(IScriptEngine engine)
    {
        engine.AddHostType(nameof(Console), typeof(Console));
        
        engine.AddRestrictedHostObject(nameof(behaviorContext), HostItemFlags.None, behaviorContext);
        engine.AddRestrictedHostObject(nameof(executionContext), HostItemFlags.None, executionContext);
    }

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