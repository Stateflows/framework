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

        public override Task AfterActivityFinalizeAsync(IActivityFinalizationContext context)
        {
            var stateflowsContext = (context as IRootContext).Context.Context;
            if (stateflowsContext.Stored)
            {
                context.Activity.Send(new Reset() { Mode = resetMode });
            }
            else
            {
                stateflowsContext.Deleted = true;
            }

            return Task.CompletedTask;
        }
    }

    public static class FinalizationResetPolicy
    {
        public static IActivityBuilder AddFinalizationResetPolicy(this IActivityBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
