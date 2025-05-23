using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal class ResetObserver : StateMachineObserver
    {
        private readonly ResetMode resetMode;

        public ResetObserver(ResetMode resetMode)
        {
            this.resetMode = resetMode;
        }

        public override void AfterStateMachineFinalize(IStateMachineActionContext context)
        {
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
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
