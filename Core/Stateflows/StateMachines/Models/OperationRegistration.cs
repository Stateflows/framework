using System;
using System.Collections.Generic;
using Stateflows.Common;
using System.Threading.Tasks;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.StateMachines.Models;

internal sealed class OperationRegistration
{
    internal string Identifier { get; set; } = null!;
    internal string OwnerId { get; set; } = null!;
    internal string? ParentGuardId { get; set; }
    internal StateMachineOperation? Operation { get; set; }
    public Type? ImplementationType { get; set; }
    public BehaviorClass? Behavior { get; set; }
    public GuardKind Kind { get; set; } = GuardKind.Predicate;
    public List<OperationRegistration> Children { get; } = [];

    internal Task<bool> ObserveAsync(RootContext context, Func<Task<bool>> evaluate)
        => context.Executor.Inspector.OnPredicateAsync(new PredicateObservationContext(context, this),
            Kind == GuardKind.Not
                ? async () => !await Children[0].ObserveAsync(context, async () => !await evaluate())
                : evaluate);

    public static OperationRegistration Leaf(Type? type = null, bool negated = false)
    {
        var leaf = new OperationRegistration { ImplementationType = type };
        if (!negated) return leaf;
        var not = new OperationRegistration { Kind = GuardKind.Not };
        not.Children.Add(leaf);
        return not;
    }
}
