﻿using Stateflows.Common;
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

        public override void AfterActivityFinalize(IActivityFinalizationContext context)
        {
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

    public static class FinalizationResetPolicy
    {
        public static IActivityBuilder AddFinalizationResetPolicy(this IActivityBuilder builder, ResetMode resetMode = ResetMode.Full)
            => builder.AddObserver(_ => new ResetObserver(resetMode));
    }
}
