using System;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;

namespace Stateflows.StateMachines.Registration.Builders;

internal class DeferralBuilder<TEvent> :
    IDeferralBuilder<TEvent>,
    IOverridenDeferralBuilder<TEvent>,
    IDeferralBuilder
{
    public Type EventType => typeof(TEvent);
    public Vertex Vertex { get; }
    public Logic<StateMachinePredicateAsync> Guards => Logic;

    public Graph Graph => Vertex.Graph;
        
    public DeferralBuilder(Vertex vertex, Logic<StateMachinePredicateAsync>? logic = null)
    {
        Vertex = vertex;
        Logic = logic ?? new Logic<StateMachinePredicateAsync>(Constants.Deferral);
    }

    public readonly Logic<StateMachinePredicateAsync> Logic;
    
    public IDeferralBuilder<TEvent> AddGuards(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
    {
        foreach (var guardAsync in guardsAsync)
        {
            Logic.Actions.Add(c =>
            {
                var context = new DeferralContext<TEvent>(c, Vertex);
                return guardAsync(context);
            });
        }

        return this;
    }

    IOverridenDeferralBuilder<TEvent> IBaseDeferralGuard<TEvent, IOverridenDeferralBuilder<TEvent>>.AddGuards(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
        => AddGuards(guardsAsync) as IOverridenDeferralBuilder<TEvent>;
}