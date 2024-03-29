﻿using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityObserver
    {
        Task BeforeActivityInitializationAsync(IActivityInitializationContext context);
        Task AfterActivityInitializationAsync(IActivityInitializationContext context);

        Task BeforeActivityFinalizationAsync(IActivityFinalizationContext context);
        Task AfterActivityFinalizationAsync(IActivityFinalizationContext context);

        Task BeforeNodeInitializationAsync(IActivityNodeContext context);
        Task AfterNodeInitializationAsync(IActivityNodeContext context);

        Task BeforeNodeFinalizationAsync(IActivityNodeContext context);
        Task AfterNodeFinalizationAsync(IActivityNodeContext context);

        Task BeforeNodeExecuteAsync(IActivityNodeContext context);
        Task AfterNodeExecuteAsync(IActivityNodeContext context);

        Task BeforeFlowGuardAsync(IGuardContext<Token> context);
        Task AfterFlowGuardAsync(IGuardContext<Token> context, bool guardResult);

        Task BeforeFlowTransformationAsync(ITransformationContext<Token> context);
        Task AfterFlowTransformationAsync(ITransformationContext<Token> context);
    }
}
