﻿using System;
using Stateflows.Activities.Engine;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities.Extensions
{
    internal static class IActivityContextExtensions
    {
        public static Executor GetExecutor(this IActivityContext context)
            => (context as ActivityContext).Context.Executor;

        public static Node GetNode(this Context.Interfaces.IActionContext context)
            => (context as ActionContext).Node;

        public static RootContext GetContext(this IActivityContext context)
            => (context as BaseContext).Context;

        public static NodeScope GetNodeScope(this IActivityContext context)
            => (context as BaseContext).NodeScope;
    }
}
