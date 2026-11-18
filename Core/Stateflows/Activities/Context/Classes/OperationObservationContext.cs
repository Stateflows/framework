using System;
using System.Collections.Generic;
using Stateflows.Activities.Models;
using Stateflows.Common;

namespace Stateflows.Activities.Context.Classes;

internal abstract class OperationObservationContext(BaseContext context, ActivityOperationRegistration registration)
    : BaseContext(context), IActivityOperationObservationContext
{
    private readonly IActivityActionContext actionContext = (IActivityActionContext)context;
    private readonly EventHolder? eventHolder = context.Context.EventHolder;

    public string Identifier => registration.Identifier;
    public string OwnerId => registration.OwnerId;
    public int Order => registration.Order;
    public object? Event => eventHolder?.BoxedPayload;
    public string? EventName => eventHolder?.Name;
    public virtual string? TokenName => null;
    public Guid? EventId => eventHolder?.Id;
    public IDictionary<string, EventHeader>? EventHeaders => eventHolder?.Headers;
    public IBehaviorContext Behavior => ((IBehaviorActionContext)actionContext).Behavior;
    public object LockHandle => actionContext.LockHandle;
    public IReadOnlyTree<INodeContext> ActiveNodes => actionContext.ActiveNodes;
    public bool TryGetParentBehaviorContext(out IParentBehaviorContext parentBehaviorContext)
        => actionContext.TryGetParentBehaviorContext(out parentBehaviorContext);
    public bool TryGetOwnerBehaviorContext(out IOwnerBehaviorContext ownerBehaviorContext)
        => actionContext.TryGetOwnerBehaviorContext(out ownerBehaviorContext);
}

internal sealed class ActionObservationContext(BaseContext context, ActivityOperationRegistration registration)
    : OperationObservationContext(context, registration), IActivityActionObservationContext
{
    public ActivityOperationKind Operation => registration.Operation;
}

internal sealed class PredicateObservationContext(BaseContext context, ActivityOperationRegistration registration)
    : OperationObservationContext(context, registration), IActivityPredicateObservationContext
{
    private readonly FlowContext? flowContext = context as FlowContext;
    public ActivityOperationKind? Operation => flowContext == null ? registration.Operation : null;
    public string? ParentGuardId => registration.ParentGuardId;
    public ActivityGuardKind Kind => registration.GuardKind;
    public Type? TokenType => flowContext?.Operation.InputType;
    public object? Token => flowContext?.OperationToken.BoxedPayload;
    public override string? TokenName => flowContext?.OperationToken.Name;
    public Guid? TokenId => flowContext?.OperationToken.Id;
}
