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

        public override Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            var stateflowsContext = (context as IRootContext).Context.Context;
            if (stateflowsContext.Stored)
            {
                context.StateMachine.Send(new Reset() { Mode = resetMode });
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
        public static IStateMachineBuilder AddFinalizationResetPolicy(this IStateMachineBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
