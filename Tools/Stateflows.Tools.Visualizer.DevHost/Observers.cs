using Stateflows.Activities;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Context.Interfaces;
using NodeContext = Stateflows.Activities.IActivityNodeContext;
using MachineContext = Stateflows.StateMachines.IStateActionContext;

namespace Stateflows.Tools.Visualizer.DevHost;

public sealed class MachineObserver(Capture capture) : StateMachineObserver
{
    public override Task OnStateEntryAsync(MachineContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.State.Identifier, "state.entry", next, "entered");
    public override Task OnStateExitAsync(MachineContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.State.Identifier, "state.exit", next, "exited");
    public override Task OnStateInitializeAsync(MachineContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.State.Identifier, "state.initialize", next);
    public override Task OnStateFinalizeAsync(MachineContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.State.Identifier, "state.finalize", next);
    public override Task OnHostedBehaviorInitializeAsync(IHostedBehaviorObservationContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.State.Identifier, "state.do", next);
    public override Task OnHostedBehaviorFinalizeAsync(IHostedBehaviorObservationContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.State.Identifier, "state.do.finalize", next);
    public override Task OnHostedBehaviorEventForwardAsync(IHostedBehaviorObservationContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.State.Identifier, "state.do.forward", next);
    public override async Task OnDeferralGuardAsync(IDeferralObservationContext context, Func<Task<bool>> next)
        => await capture.ObserveAsync(context, context.State.Identifier, $"state.defer:{context.EventName}", next,
            deferred => deferred ? "waiting" : "executed");
    public override async Task OnTransitionGuardAsync<TEvent>(
        ITransitionContext<TEvent> context, Func<Task<bool>> next)
        => await capture.ObserveAsync(context, context.Identifier, "transition.guard", next,
            result => result ? "executed" : "guard-rejected");
    public override Task OnTransitionEffectAsync<TEvent>(
        ITransitionContext<TEvent> context, Func<Task> next)
        => capture.ObserveAsync(context, context.Identifier, "transition.effect", next, "taken");
}

public sealed class NetworkObserver(Capture capture) : ActivityObserver
{
    public override Task OnNodeInitializeAsync(NodeContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.Node.Identifier, "node.initialize", next);
    public override Task OnNodeFinalizeAsync(NodeContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.Node.Identifier, "node.finalize", next);
    public override Task OnNodeActivateAsync(NodeContext context, bool activated, Func<Task> next)
        => capture.ObserveAsync(context, context.Node.Identifier, "node.activation", next, activated ? "activated" : "waiting");
    public override Task OnNodeExecuteAsync(NodeContext context, Func<Task> next)
        => capture.ObserveAsync(context, context.Node.Identifier, "node.execute", next);
    public override async Task OnFlowActivateAsync(
        IActivityBeforeFlowContext context, Func<Task<IActivityAfterFlowContext>> next)
        => await capture.ObserveAsync(context, context.Identifier, "flow.activation", next,
            result => result.Activated ? "taken" : "waiting", result => result.TargetTokenCount, context.Weight,
            payloadTokens: () => Capture.SnapshotPayloadTokens(context.PayloadTokens));
    public override async Task OnFlowGuardAsync<TToken>(IGuardContext<TToken> context, Func<Task<bool>> next)
        => await capture.ObserveAsync(context, context.Identifier, "flow.guard", next,
            result => result ? "executed" : "guard-rejected",
            payloadTokens: () => Capture.SnapshotPayloadTokens(context.Token));
    public override async Task OnFlowTransformAsync<TToken, TTransformedToken>(
        ITransformationContext<TToken> context, Func<Task<TTransformedToken>> next)
        => await capture.ObserveAsync(context, context.Identifier, "flow.transform", next, _ => "executed",
            payloadTokens: () => Capture.SnapshotPayloadTokens(context.Token),
            completedPayloadTokens: result => Capture.SnapshotPayloadTokens(result));
}
