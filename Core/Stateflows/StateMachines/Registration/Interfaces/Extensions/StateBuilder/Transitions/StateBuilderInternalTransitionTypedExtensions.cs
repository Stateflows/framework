using System.Diagnostics;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines
{
    public static class StateBuilderInternalTransitionTypedExtensions
    {
        ///// <summary>
        ///// Adds internal transition triggered by <see cref="TEvent"/> coming from current state.<br/>
        ///// <a href="https://github.com/Stateflows/framework/wiki/Internal-Transition">Internal transitions</a> are triggered by events sent to State Machine and are not changing its state.
        ///// <list type="number">
        ///// <item>
        ///// <term>Trigger</term>
        ///// <description>Event that triggers transition - <b>first type parameter</b>.</description>
        ///// </item>
        ///// <item>
        ///// <term>Definition</term>
        ///// <description>Class that defines transition actions (guard and/or effect) - <b>second type parameter</b>.</description>
        ///// </item>
        ///// <item>
        ///// <term>Guard/Effect</term>
        ///// <description>Transition actions can be defined using build action - <b>first parameter</b>.</description>
        ///// </item>
        ///// </list>
        ///// </summary>
        ///// <typeparam name="TEvent">Event class</typeparam>
        ///// <typeparam name="TTransition">Transition class; must implement at least one of the following interfaces:
        ///// <list type="bullet">
        ///// <item><see cref="ITransitionGuard&lt;TEvent&gt;"/></item>
        ///// <item><see cref="ITransitionEffect&lt;TEvent&gt;"/></item>
        ///// </list>
        ///// </typeparam>
        ///// <param name="transitionBuildAction">Transition build action</param>
        //[DebuggerHidden]
        //public static IStateBuilder AddInternalTransition<TEvent, TTransition>(this IStateBuilder builder, InternalTransitionBuildAction<TEvent> transitionBuildAction = null)
        //    where TTransition : class, ITransition<TEvent>
        //    => builder.AddTransition<TEvent, TTransition>(Constants.DefaultTransitionTarget, b => transitionBuildAction?.Invoke(b as IInternalTransitionBuilder<TEvent>));
    }
}