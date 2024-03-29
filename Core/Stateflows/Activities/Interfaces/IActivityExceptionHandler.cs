﻿using System;
using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityExceptionHandler
    {
        Task OnActivityInitializationExceptionAsync(IActivityInitializationContext context, Exception exception);
        Task OnActivityFinalizationExceptionAsync(IActivityFinalizationContext context, Exception exception);
        Task OnNodeInitializationExceptionAsync(IActivityNodeContext context, Exception exception);
        Task OnNodeFinalizationExceptionAsync(IActivityNodeContext context, Exception exception);
        Task OnNodeExecutionExceptionAsync(IActivityNodeContext context, Exception exception);
        Task OnFlowGuardExceptionAsync(IGuardContext<Token> context, Exception exception);
        Task OnFlowTransformationExceptionAsync(ITransformationContext<Token> context, Exception exception);
    }
}
