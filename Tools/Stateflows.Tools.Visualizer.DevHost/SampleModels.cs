using Stateflows.Activities;
using Stateflows.Actions;
using Stateflows.Common;
using Stateflows.StateMachines;

namespace Stateflows.Tools.Visualizer.DevHost;

public sealed record Approve(bool Allowed);
public sealed record Finish;
public sealed record Cancel;
public sealed record Refresh;
public sealed record Synchronize;
public sealed record Resume;

public sealed class ProcessingState : ICompositeState, IStateEntry, IStateExit,
    ICompositeStateInitialization, ICompositeStateFinalization
{
    public Task OnInitializeAsync() => Task.CompletedTask;
    public Task OnFinalizeAsync() => Task.CompletedTask;
    public Task OnEntryAsync() => Task.CompletedTask;
    public Task OnExitAsync() => Task.CompletedTask;
}

public sealed class ReviewState : IStateEntry, IStateExit
{
    public async Task OnEntryAsync() => await Task.Delay(150);
    public Task OnExitAsync() => Task.CompletedTask;
}

public sealed class ReviewAudit : IStateEntry, ICompositeStateInitialization, ICompositeStateFinalization,
    ITransitionEffect<Approve>, ITransitionEffect<Refresh>
{
    public Task OnInitializeAsync() => Task.CompletedTask;
    public Task OnFinalizeAsync() => Task.CompletedTask;
    public Task OnEntryAsync() => Task.CompletedTask;
    public Task EffectAsync(Approve @event) => Task.CompletedTask;
    public Task EffectAsync(Refresh @event) => Task.CompletedTask;
}

public sealed class ApprovalAllowed : ITransitionGuard<Approve>
{
    public Task<bool> GuardAsync(Approve @event) => Task.FromResult(@event.Allowed);
}

public sealed class ReviewPreparation : IActivity
{
    public static void Build(IActivityBuilder activity) => activity
        .AddInitial(initial => initial.AddControlFlow("Prepare review"))
        .AddAction("Prepare review", _ => Task.CompletedTask, action => action.AddControlFlow("Ready"))
        .AddFinal("Ready");
}

public sealed class CompletionAudit : IAction
{
    public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class RejectionHandling : IStateMachine
{
    public static void Build(IStateMachineBuilder machine) => machine
        .AddInitialState("Record rejection", state => state.AddDefaultTransition("Rejection recorded"))
        .AddState("Rejection recorded");
}

public static class SampleModels
{
    public static void BuildMachine(IStateMachineBuilder machine) => machine
        .AddInitialCompositeState<ProcessingState>("Processing", processing => processing
            .AddOnInitialize<ReviewAudit>()
            .AddOnFinalize<ReviewAudit>()
            .AddHistory("Resume processing")
            .AddTransition<Cancel>("Cancelled")
            .AddInitialState<ReviewState>("Review", state => state
                .AddDoActivity<ReviewPreparation>()
                .AddOnEntry<ReviewAudit>()
                .AddInternalTransition<Refresh>(transition => transition.AddEffect<ReviewAudit>())
                .AddTransition<Approve>("Rejected junction", transition => transition.AddNegatedGuard<ApprovalAllowed>())
                .AddTransition<Approve>("Dispatch", transition => transition.AddGuard<ApprovalAllowed>().AddEffect<ReviewAudit>())
                .AddDeferredEvent<Finish>(b => b.AddGuard<Deny>())
            )
            .AddJunction("Rejected junction", junction => junction.AddTransition("Rejected"))
            .AddFork("Dispatch", fork => fork.AddTransition("PaymentPending").AddTransition("StockPending"))
            .AddOrthogonalState("Checking", state => state
                .AddDoActivity<ReviewPreparation>()
                .AddTransition<Finish>("Decision")
                .AddRegion(region => region
                    .AddInitialState("PaymentPending", pending => pending.AddTransition<Approve>("PaymentReady").AddTransition<Synchronize>("PaymentComplete"))
                    .AddState("PaymentReady", ready => ready.AddTransition<Synchronize>("PaymentComplete"))
                    .AddState("PaymentComplete", complete => complete.AddDefaultTransition("Joined")))
                .AddRegion(region => region
                    .AddInitialState("StockPending", pending => pending.AddTransition<Approve>("StockReady").AddTransition<Synchronize>("StockComplete"))
                    .AddState("StockReady", ready => ready
                        .AddOnEntry(_ => throw new InvalidOperationException("StockReady entry failed (visualizer laboratory test)."))
                        .AddTransition<Synchronize>("StockComplete"))
                    .AddState("StockComplete", complete => complete.AddDefaultTransition("Joined"))))
            .AddJoin("Joined", join => join.AddTransition("Decision"))
            .AddState("Rejected", state => state.AddSubmachine<RejectionHandling>()))
        .AddChoice("Decision", choice => choice
            .AddTransition("Done", transition => transition.AddGuard(context => context.ExecutionTrigger is Finish or Synchronize))
            .AddElseTransition("Cancelled"))
        .AddState("Done", state => state.AddDoAction<CompletionAudit>())
        .AddState("Cancelled", state => state.AddTransition<Resume>("Resume processing"));

    public static void BuildActivity(IActivityBuilder activity) => activity
        .AddInitial(initial => initial.AddControlFlow("Prepare"))
        .AddAction("Prepare", async context =>
        {
            await Task.Delay(150, context.CancellationToken);
            context.Output(7);
        }, node => node.AddFlow<int>("Nested"))
        .AddStructuredActivity("Nested", nested => nested
            .AddInputPin<int>(pin => pin.AddFlow("Double"))
            .AddAction("Double", async context =>
            {
                await Task.Delay(250, context.CancellationToken);
                foreach (var value in context.GetTokensOfType<int>()) context.Output(value * 2);
            }, node => node.AddFlow<int, OutputPin<int>>())
            .AddOutputPin<int>()
            .AddFlow<int>("Store"))
        .AddDataStore("Store", store => store.AddFlow<int>("Join"))
        .AddAction("Join", context =>
        {
            context.PassTokensOfTypeOn<int>();
            return Task.CompletedTask;
        }, node => node.AddFlow<int>("Result"))
        .AddOutputPin<int>("Result", _ => { });
}
