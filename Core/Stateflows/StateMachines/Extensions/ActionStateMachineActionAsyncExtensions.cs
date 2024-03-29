﻿using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines
{
    internal static class ActionStateMachineActionAsyncExtensions
    {
        public static Task WhenAll(this Logic<StateMachineActionAsync> action, RootContext context)
            => Task.WhenAll(action.Actions.Select(a => a(context)));
    }
}
