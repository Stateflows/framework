using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines;

internal class ResetObserver : StateMachineObserver
{
    private readonly ResetMode resetMode;

    public ResetObserver(ResetMode resetMode)
    {
        this.resetMode = resetMode;
    }

    public override async Task OnStateMachineFinalizeAsync(IStateMachineActionContext context, Func<Task> next)
    {
        await next();
        var stateflowsContext = (context as IRootContext).Context.Context;
        if (stateflowsContext.Stored)
        {
            context.Behavior.Send(new Reset() { Mode = resetMode });
        }
        else
        {
            stateflowsContext.Deleted = true;
        }
    }
}

public static class FinalizationResetPolicy
{
    public static IStateMachineBuilder AddFinalizationResetPolicy(this IStateMachineBuilder builder, ResetMode resetMode = ResetMode.Full)
        => builder.AddObserver((_, _) => new ResetObserver(resetMode));
    
    public static IStateMachineUtilsBuilder AddFinalizationResetPolicy(this IStateMachineUtilsBuilder builder, ResetMode resetMode = ResetMode.Full)
        => builder.AddObserver((_, _) => new ResetObserver(resetMode));
}