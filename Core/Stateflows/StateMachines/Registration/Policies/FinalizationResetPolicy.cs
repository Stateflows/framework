using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    internal class ResetObserver : IStateMachineObserver
    {
        private readonly ResetMode resetMode;

        public ResetObserver(ResetMode resetMode)
        {
            this.resetMode = resetMode;
        }

        public Task AfterStateMachineFinalizeAsync(IStateMachineActionContext context)
        {
            var stateflowsContext = (context as IRootContext).Context.Context;
            if (stateflowsContext.Stored)
            {
                context.StateMachine.Send(new ResetRequest() { Mode = resetMode });
            }
            else
            {
                stateflowsContext.Deleted = true;
            }
            //var c = (context as IRootContext);
            //if (c.Context.Context.Stored)
            //{
            //    context.StateMachine.Send(new ResetRequest() { Mode = resetMode });
            //}

            return Task.CompletedTask;
        }
    }

    public static class FinalizationResetPolicy
    {
        public static IStateMachineBuilder AddFinalizationResetPolicy(this IStateMachineBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
