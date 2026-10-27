using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Context.Classes;
using Stateflows.Actions.Context.Interfaces;
using Stateflows.Actions.Registration.Interfaces;

namespace Stateflows.Actions
{
    internal class ResetObserver : ActionObserver
    {
        private readonly ResetMode resetMode;

        public ResetObserver(ResetMode resetMode)
        {
            this.resetMode = resetMode;
        }

        public override async Task OnActionFinalizeAsync(IActionDelegateContext context, Func<Task> next)
        {
            await next();
            var stateflowsContext = ((BaseContext)context).Context;
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

    /// <summary>Provides the action finalization-reset registration policy.</summary>
    public static class FinalizationResetPolicy
    {
        /// <summary>Resets a stored action instance after finalization, or deletes its transient context.</summary>
        /// <param name="builder">The delegate-based action builder.</param>
        /// <param name="resetMode">The reset mode used for a stored instance.</param>
        /// <returns>The action builder for further configuration.</returns>
        /// <remarks>Reset is requested only after the finalization observer continuation completes successfully.</remarks>
        public static IActionBuilder AddFinalizationResetPolicy(this IActionBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    
        /// <summary>Resets a stored typed action instance after finalization, or deletes its transient context.</summary>
        /// <typeparam name="TAction">The typed action implementation.</typeparam>
        /// <param name="builder">The typed action builder.</param>
        /// <param name="resetMode">The reset mode used for a stored instance.</param>
        /// <returns>The action builder for further configuration.</returns>
        public static IActionBuilder<TAction> AddFinalizationResetPolicy<TAction>(this IActionBuilder<TAction> builder, ResetMode resetMode = ResetMode.Full)
            where TAction : class, IAction
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
