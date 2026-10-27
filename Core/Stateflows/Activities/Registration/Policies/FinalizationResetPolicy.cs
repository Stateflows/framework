using System;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    internal class ResetObserver : ActivityObserver
    {
        private readonly ResetMode resetMode;

        public ResetObserver(ResetMode resetMode)
        {
            this.resetMode = resetMode;
        }

        public override async Task OnActivityFinalizeAsync(IActivityFinalizationContext context, Func<Task> next)
        {
            await next();
            var stateflowsContext = ((IRootContext)context).Context.Context;
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

    /// <summary>Provides the activity finalization-reset registration policy.</summary>
    public static class FinalizationResetPolicy
    {
        /// <summary>Resets a stored activity instance after finalization, or deletes its transient context.</summary>
        /// <param name="builder">The activity graph builder.</param>
        /// <param name="resetMode">The reset mode used for a stored instance.</param>
        /// <returns>The activity builder for further configuration.</returns>
        /// <remarks>Reset is requested after the activity-finalization observer continuation completes successfully.</remarks>
        public static IActivityBuilder AddFinalizationResetPolicy(this IActivityBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
        
        /// <summary>Enables activity finalization reset behavior through activity-wide utilities.</summary>
        /// <param name="builder">The activity utility builder.</param>
        /// <param name="resetMode">The reset mode used for a stored instance.</param>
        /// <returns>The activity utility builder for further configuration.</returns>
        public static IActivityUtilsBuilder AddFinalizationResetPolicy(this IActivityUtilsBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
