using System;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Context.Classes;
using Stateflows.StateMachines.Context.Interfaces;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Registration.Builders;

internal class DeferralBuilder<TEvent> : IDeferralBuilder<TEvent>
{
    public Vertex Vertex { get; }

    public Graph Graph => Vertex.Graph;
        
    public DeferralBuilder(Vertex vertex)
    {
        Vertex = vertex;
    }

    public Logic<StateMachinePredicateAsync> Logic { get; } = new(Constants.Deferral);
    
    public IDeferralBuilder<TEvent> AddGuard(params Func<IDeferralContext<TEvent>, Task<bool>>[] guardsAsync)
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
}