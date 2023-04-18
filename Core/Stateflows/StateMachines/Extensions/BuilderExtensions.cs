using Stateflows.Common.Extensions;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces.Base;

namespace Stateflows.StateMachines.Extensions
{
    internal static class BuilderExtensions
    {
        public static void AddStateEvents<TState, TReturn>(this IStateEventsBuilderBase<TReturn> builder, Graph graph)
            where TState : State
        {
            if (typeof(State).GetMethod(Constants.OnEntryAsync).IsOverridenIn<TState>())
            {
                builder.AddOnEntry(c => (c as BaseContext).Context.Executor.ServiceProvider.GetState<TState>(c)?.OnEntryAsync());
            }

            if (typeof(State).GetMethod(Constants.OnExitAsync).IsOverridenIn<TState>())
            {
                builder.AddOnExit(c => (c as BaseContext).Context.Executor.ServiceProvider.GetState<TState>(c)?.OnExitAsync());
            }
        }

        public static void AddCompositeStateEvents<TCompositeState, TReturn>(this ICompositeStateEventsBuilderBase<TReturn> builder, Graph graph)
            where TCompositeState : CompositeState
        {
            if (typeof(State).GetMethod(Constants.OnInitializeAsync).IsOverridenIn<TCompositeState>())
            {
                builder.AddOnInitialize(c => (c as BaseContext).Context.Executor.ServiceProvider.GetState<TCompositeState>(c)?.OnEntryAsync());
            }
        }
    }
}
