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

        public override void AfterActionFinalize(IActionDelegateContext context)
        {
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

    public static class FinalizationResetPolicy
    {
        public static IActionBuilder AddFinalizationResetPolicy(this IActionBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
