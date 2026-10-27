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

/// <summary>Resets a machine after successful finalization so its instance can be initialized again.</summary>
/// <remarks>Persisted instances receive a reset event; an instance not yet stored is marked for deletion.</remarks>
public static class FinalizationResetPolicy
{
    /// <summary>Registers the post-finalization reset observer on a fluent definition.</summary>
    /// <param name="builder">The machine builder receiving the observer.</param>
    /// <param name="resetMode">The reset mode sent to a stored instance; defaults to a full reset.</param>
    /// <returns>The original machine builder.</returns>
    public static IStateMachineBuilder AddFinalizationResetPolicy(this IStateMachineBuilder builder, ResetMode resetMode = ResetMode.Full)
        => builder.AddObserver((_, _) => new ResetObserver(resetMode));
    
    /// <summary>Registers the post-finalization reset observer on an already defined machine.</summary>
    /// <param name="builder">The utilities builder receiving the observer.</param>
    /// <param name="resetMode">The reset mode sent to a stored instance; defaults to a full reset.</param>
    /// <returns>The original utilities builder.</returns>
    public static IStateMachineUtilsBuilder AddFinalizationResetPolicy(this IStateMachineUtilsBuilder builder, ResetMode resetMode = ResetMode.Full)
        => builder.AddObserver((_, _) => new ResetObserver(resetMode));
}