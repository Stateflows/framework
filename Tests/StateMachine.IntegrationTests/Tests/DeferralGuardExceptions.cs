using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Attributes;
using Stateflows.Common.Exceptions;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Attributes;
using Stateflows.StateMachines.Context.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public sealed class DeferralGuardExceptions : StateflowsTestClass
{
    public sealed record Candidate(int Value);
    private sealed record Advance;
    private sealed record Release;
    public sealed record ConstructorFailure;
    public sealed record DependencyFailure;
    public sealed record RequiredValueFailure;
    public sealed record MissingValueSetFailure;
    public sealed record AccessorValue;
    public sealed record ScopedCandidate;
    public sealed record ScopedFailure;
    private sealed class CandidateHeader : EventHeader { }

    private sealed record FailureCall(
        int Handler,
        string State,
        string Rule,
        string EventName,
        object Event,
        Guid EventId,
        bool IsReevaluation,
        object? ExecutionTrigger,
        bool HasCandidateHeader,
        Exception Exception);

    private sealed record Evaluation(string Rule, Guid EventId, bool IsReevaluation, int? CandidateValue);

    private readonly InvalidOperationException syncFailure = new("synchronous deferral guard failure");
    private readonly ApplicationException asyncFailure = new("asynchronous deferral guard failure");
    private readonly InvalidOperationException observerBeforeFailure = new("observer failed before next");
    private readonly InvalidOperationException observerAfterFailure = new("observer failed after next");
    private readonly InvalidOperationException predicateBeforeFailure = new("predicate observer failed before next");
    private readonly InvalidOperationException predicateAfterFailure = new("predicate observer failed after next");
    private readonly InvalidOperationException enqueueObserverFailure = new("enqueue observer failed");
    private readonly ApplicationException handlerFailure = new("exception handler failed");
    private readonly List<FailureCall> calls = [];
    private readonly List<Evaluation> evaluations = [];
    private readonly Dictionary<Guid, int> reevaluationCounts = [];
    private readonly List<Guid> scopeIds = [];
    private readonly List<Guid> disposedScopeIds = [];
    private readonly List<bool> predicateObserverFailures = [];
    private readonly TaskCompletionSource<bool> forwardedException = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private IDictionary<string, EventHeader>? forwardedExceptionHeaders;
    private Exception? forwardedExceptionPayload;
    private bool outerMutationPersisted;
    private int releasedCandidates;
    private bool acknowledgeFailures = true;
    private static int typedGuardBodies;

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        typedGuardBodies = 0;
        acknowledgeFailures = true;
        builder.AddStateMachines(machines =>
        {
            machines.AddStateMachine("deferral-guard-errors", machine =>
            {
                machine.AddObserver((_, _) => new DeferralObserver(this));
                machine.AddInitialState("Waiting", state => state
                    .AddDeferredEvent<Candidate>(rule => rule
                        .AddGuard(context =>
                        {
                            if (context.Event.Value == 1) throw syncFailure;
                            return true;
                        })
                        .AddGuard(async context =>
                        {
                            await Task.Yield();
                            if (context.Event.Value == 2) throw asyncFailure;
                            if (context.Event.Value == 10)
                            {
                                var count = reevaluationCounts.TryGetValue(context.EventId, out var current)
                                    ? current + 1
                                    : 1;
                                reevaluationCounts[context.EventId] = count;
                                if (count > 1 && context.ExecutionTrigger is Advance) throw syncFailure;
                            }
                            return true;
                        }))
                    .AddTransition<Advance>("Waiting", transition => transition.AddEffect(async context =>
                        await context.Behavior.Values.SetAsync("outer-mutation", 42)))
                    .AddInternalTransition<Exception>(transition => transition.AddEffect(context =>
                    {
                        forwardedExceptionHeaders = context.Headers;
                        forwardedExceptionPayload = context.Event;
                        forwardedException.TrySetResult(true);
                        return Task.CompletedTask;
                    }))
                    .AddTransition<Release>("Ready"))
                .AddState("Ready", state => state
                    .AddInternalTransition<Candidate>(transition => transition.AddEffect(async context =>
                    {
                        releasedCandidates++;
                        var (success, value) = await context.Behavior.Values.TryGetAsync<int>("outer-mutation");
                        outerMutationPersisted = success && value == 42;
                    })));
                AddHandlers(machine);
            });

            machines.AddStateMachine("typed-deferral-guard-errors", machine =>
            {
                machine.AddInitialState("Waiting", state => state
                    .AddDeferredEvent<ConstructorFailure>(rule => rule.AddGuard<ThrowingConstructorGuard>())
                    .AddDeferredEvent<DependencyFailure>(rule => rule.AddGuard<UnresolvedDependencyGuard>())
                    .AddDeferredEvent<RequiredValueFailure>(rule => rule.AddGuard<RequiredValueGuard>())
                    .AddDeferredEvent<MissingValueSetFailure>(rule => rule.AddGuard<MissingValueSetGuard>())
                    .AddDeferredEvent<AccessorValue>(rule => rule.AddGuard<AccessorValueGuard>()));
                AddHandlers(machine);
            });

            machines.AddStateMachine("scoped-deferral-guard", machine =>
            {
                machine.AddInitialState("Waiting", state => state
                    .AddDeferredEvent<ScopedCandidate>(rule => rule.AddGuard<ScopedGuard>())
                    .AddDeferredEvent<ScopedFailure>(rule => rule.AddGuard<FailingScopedGuard>()));
                AddHandlers(machine);
            });
        });

        builder.ServiceCollection.AddScoped(_ => new ScopedProbe(scopeIds, disposedScopeIds));
    }

    private void AddHandlers(IStateMachineBuilder machine)
    {
        for (var index = 1; index <= 3; index++)
        {
            var handlerIndex = index;
            machine.AddExceptionHandler((_, _) => new RecordingHandler(this, handlerIndex));
        }
    }

    private IStateMachineBehavior Locate(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, "test"), out var machine));
        return machine;
    }

    private Dictionary<string, EventHeader> CandidateHeaders()
        => new() { ["candidate"] = new CandidateHeader() };

    [TestMethod]
    public async Task InitialGuardAndObserverFailuresAreReportedOnceInOrder()
    {
        var machine = Locate("deferral-guard-errors");
        var headers = CandidateHeaders();
        foreach (var value in new[] { 1, 2, 3, 4, 5, 6, 7 })
        {
            var status = (await machine.SendAsync(new Candidate(value), headers)).Status;
            Assert.AreEqual(EventStatus.Failed, status, $"candidate {value}");
        }

        Assert.AreEqual(18, calls.Count, "An enqueue observer failure is not a deferral guard failure.");
        CollectionAssert.AreEqual(
            Enumerable.Range(0, 6).SelectMany(_ => new[] { 1, 2, 3 }).ToArray(),
            calls.Select(call => call.Handler).ToArray());
        Assert.IsTrue(calls.All(call => call.State == "Waiting" && call.EventName == Event<Candidate>.Name
            && call.Event is Candidate && !call.IsReevaluation && call.HasCandidateHeader));
        Assert.IsTrue(calls.Take(3).All(call => ReferenceEquals(syncFailure, call.Exception)));
        Assert.IsTrue(calls.Skip(3).Take(3).All(call => ReferenceEquals(asyncFailure, call.Exception)));
        Assert.IsTrue(calls.Skip(6).Take(3).All(call => ReferenceEquals(observerBeforeFailure, call.Exception)));
        Assert.IsTrue(calls.Skip(9).Take(3).All(call => ReferenceEquals(observerAfterFailure, call.Exception)));
        Assert.IsTrue(calls.Skip(12).Take(3).All(call => ReferenceEquals(predicateBeforeFailure, call.Exception)));
        Assert.IsTrue(calls.Skip(15).Take(3).All(call => ReferenceEquals(predicateAfterFailure, call.Exception)));
        Assert.AreEqual(6, calls.Select(call => call.EventId).Distinct().Count());
        Assert.AreEqual(1, calls.Select(call => call.Rule).Distinct().Count());
        Assert.AreEqual(2, predicateObserverFailures.Count);
        CollectionAssert.AreEqual(new[] { false, true }, predicateObserverFailures.ToArray());
        Assert.IsTrue(evaluations.Where(evaluation => !evaluation.IsReevaluation && evaluation.CandidateValue != 5).All(evaluation =>
            calls.Any(call => call.Rule == evaluation.Rule && call.EventId == evaluation.EventId)));

        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Advance())).Status);
        Assert.AreEqual(18, calls.Count, "A failed initial evaluation must not enqueue its candidate.");
        Assert.IsFalse(forwardedException.Task.IsCompleted, "Acknowledgement suppresses exception-plugin forwarding.");
    }

    [TestMethod]
    public async Task ReevaluationFailureUsesCandidateContextAndPreservesOuterTrigger()
    {
        acknowledgeFailures = false;
        var machine = Locate("deferral-guard-errors");
        var candidateResult = await machine.SendAsync(new Candidate(10), CandidateHeaders());
        Assert.AreEqual(EventStatus.Deferred, candidateResult.Status);
        var candidateEvaluation = evaluations.Single(evaluation => !evaluation.IsReevaluation);
        Assert.IsFalse(candidateEvaluation.IsReevaluation);

        var status = (await machine.SendAsync(new Advance())).Status;
        Assert.AreEqual(EventStatus.Failed, status);
        var failureCalls = calls.TakeLast(3).ToArray();
        Assert.AreEqual(3, failureCalls.Length);
        Assert.IsTrue(failureCalls.All(call => call.IsReevaluation && call.Event is Candidate { Value: 10 }
            && call.EventId == candidateEvaluation.EventId
            && call.Rule == candidateEvaluation.Rule
            && call.ExecutionTrigger is Advance
            && call.HasCandidateHeader));
        Assert.IsTrue(failureCalls.All(call => ReferenceEquals(syncFailure, call.Exception)));
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, failureCalls.Select(call => call.Handler).ToArray());
        await forwardedException.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsNotNull(forwardedExceptionHeaders);
        Assert.AreEqual(syncFailure.GetType().Name, forwardedExceptionPayload?.GetType().Name);
        Assert.AreEqual(syncFailure.Message, forwardedExceptionPayload?.Message);
        Assert.IsTrue(forwardedExceptionHeaders.ContainsKey("candidate"),
            "The exception event must carry headers from the deferred candidate, not the triggering event.");

        Assert.AreEqual(3, calls.Count, "Follow-up processing must not duplicate the original failure report.");
        Assert.IsTrue(evaluations.Any(evaluation => evaluation.EventId == candidateEvaluation.EventId
            && evaluation.IsReevaluation && evaluation.CandidateValue == 10),
            "The forwarded exception event reevaluates the candidate retained by the failed transaction.");
        Assert.AreEqual(EventStatus.Consumed, (await machine.SendAsync(new Release())).Status);
        Assert.AreEqual(1, releasedCandidates, "The candidate remains queued after the failed reevaluation.");
        Assert.IsFalse(outerMutationPersisted, "The outer transition's partial mutation must roll back with the failed event.");
        Assert.AreEqual(3, calls.Count);
    }

    [TestMethod]
    public async Task TypedActivationAndValueInjectionFailuresReachTheDedicatedCallback()
    {
        var machine = Locate("typed-deferral-guard-errors");
        Assert.AreEqual(EventStatus.Failed, (await machine.SendAsync(new ConstructorFailure())).Status);
        Assert.AreEqual(EventStatus.Failed, (await machine.SendAsync(new DependencyFailure())).Status);
        Assert.AreEqual(EventStatus.Failed, (await machine.SendAsync(new RequiredValueFailure())).Status);
        Assert.AreEqual(EventStatus.Failed, (await machine.SendAsync(new MissingValueSetFailure())).Status);
        Assert.AreEqual(EventStatus.Failed, (await machine.SendAsync(new AccessorValue())).Status);

        var typedFailures = calls.Take(15).Where(call => call.Handler == 1).Select(call => call.Exception).ToArray();
        Assert.HasCount(5, typedFailures);
        Assert.IsInstanceOfType<TargetInvocationException>(typedFailures[0]);
        Assert.IsInstanceOfType<InvalidOperationException>(typedFailures[1]);
        Assert.IsInstanceOfType<InvalidOperationException>(typedFailures[2]);
        Assert.IsInstanceOfType<InvalidOperationException>(typedFailures[3]);
        Assert.IsInstanceOfType<TargetInvocationException>(typedFailures[4]);
        Assert.IsInstanceOfType<InvalidOperationException>(typedFailures[0].InnerException);
        Assert.IsInstanceOfType<StateflowsDefinitionException>(typedFailures[4].InnerException);
        Assert.IsTrue(typedFailures[2].Message.Contains("not found", StringComparison.Ordinal));
        Assert.IsTrue(typedFailures[3].Message.Contains("no value set found", StringComparison.Ordinal));
        Assert.AreEqual(0, typedGuardBodies, "Failed activation or value injection must not run a guard body.");
        Assert.IsTrue(calls.Take(15).All(call => call.Event is ConstructorFailure or DependencyFailure
            or RequiredValueFailure or MissingValueSetFailure or AccessorValue));
    }

    [TestMethod]
    public async Task DeferralGuardOperationScopesAreIsolatedAndDisposed()
    {
        var machine = Locate("scoped-deferral-guard");
        Assert.AreEqual(EventStatus.Failed, (await machine.SendAsync(new ScopedFailure())).Status);
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(new ScopedCandidate())).Status);
        Assert.AreEqual(EventStatus.Deferred, (await machine.SendAsync(new ScopedCandidate())).Status);

        Assert.AreEqual(3, scopeIds.Count);
        Assert.AreEqual(3, disposedScopeIds.Count);
        Assert.AreEqual(3, scopeIds.Distinct().Count());
        CollectionAssert.AreEquivalent(scopeIds, disposedScopeIds);
    }

    private sealed class DeferralObserver(DeferralGuardExceptions owner) : StateMachineObserver
    {
        public override async Task OnDeferralGuardAsync(IDeferralObservationContext context, Func<Task<bool>> next)
        {
            owner.evaluations.Add(new(
                context.Identifier,
                context.EventId,
                context.IsReevaluation,
                context.Event is Candidate candidate ? candidate.Value : null));
            if (context.Event is Candidate { Value: 3 }) throw owner.observerBeforeFailure;
            await next();
            if (context.Event is Candidate { Value: 4 }) throw owner.observerAfterFailure;
        }

        public override Task OnEventDeferredAsync(IDeferralObservationContext context, Func<Task> next)
            => context.Event is Candidate { Value: 5 }
                ? Task.FromException(owner.enqueueObserverFailure)
                : next();

        public override async Task OnPredicateAsync(IStateMachinePredicateObservationContext predicateContext, Func<Task<bool>> next)
        {
            if (predicateContext.Event is Candidate { Value: 6 })
            {
                owner.predicateObserverFailures.Add(false);
                throw owner.predicateBeforeFailure;
            }

            await next();
            if (predicateContext.Event is Candidate { Value: 7 })
            {
                owner.predicateObserverFailures.Add(true);
                throw owner.predicateAfterFailure;
            }
        }
    }

    private sealed class RecordingHandler(DeferralGuardExceptions owner, int number) : StateMachineExceptionHandler
    {
        public override Task<bool> OnDeferralGuardExceptionAsync(IDeferralObservationContext context, Exception exception)
        {
            owner.calls.Add(new(
                number,
                context.State.Name,
                context.Identifier,
                context.EventName,
                context.Event,
                context.EventId,
                context.IsReevaluation,
                context.ExecutionTrigger,
                context.EventHeaders.ContainsKey("candidate"),
                exception));

            return number == 2
                ? Task.FromException<bool>(owner.handlerFailure)
                : Task.FromResult(number == 1 && owner.acknowledgeFailures);
        }
    }

    public sealed class ThrowingConstructorGuard : IDeferralGuard<ConstructorFailure>
    {
        public ThrowingConstructorGuard() => throw new InvalidOperationException("typed guard constructor failed");
        public Task<bool> GuardAsync(ConstructorFailure @event)
        {
            typedGuardBodies++;
            return Task.FromResult(true);
        }
    }

    public sealed class MissingService { }

    public sealed class UnresolvedDependencyGuard(MissingService dependency) : IDeferralGuard<DependencyFailure>
    {
        public Task<bool> GuardAsync(DependencyFailure @event)
        {
            typedGuardBodies++;
            return Task.FromResult(true);
        }
    }

    public sealed class RequiredValueGuard([StateValue("required-but-missing")] int required)
        : IDeferralGuard<RequiredValueFailure>
    {
        public Task<bool> GuardAsync(RequiredValueFailure @event)
        {
            typedGuardBodies++;
            return Task.FromResult(true);
        }
    }

    public sealed class MissingValueSetGuard([SourceStateValue("unavailable-source-value")] int required)
        : IDeferralGuard<MissingValueSetFailure>
    {
        public Task<bool> GuardAsync(MissingValueSetFailure @event)
        {
            typedGuardBodies++;
            return Task.FromResult(true);
        }
    }

    public sealed class AccessorValueGuard([SourceStateValue("not-available-during-deferral")] IValue<int> value)
        : IDeferralGuard<AccessorValue>
    {
        public async Task<bool> GuardAsync(AccessorValue @event)
        {
            typedGuardBodies++;
            _ = await value.TryGetAsync();
            return true;
        }
    }

    public sealed class ScopedProbe(List<Guid> created, List<Guid> disposed) : IDisposable
    {
        public Guid Id { get; } = Guid.NewGuid();

        public void Track() => created.Add(Id);

        public void Dispose() => disposed.Add(Id);
    }

    public sealed class ScopedGuard(ScopedProbe probe) : IDeferralGuard<ScopedCandidate>
    {
        public Task<bool> GuardAsync(ScopedCandidate @event)
        {
            probe.Track();
            return Task.FromResult(true);
        }
    }

    public sealed class FailingScopedGuard(ScopedProbe probe) : IDeferralGuard<ScopedFailure>
    {
        public Task<bool> GuardAsync(ScopedFailure @event)
        {
            probe.Track();
            throw new InvalidOperationException("scoped guard failure");
        }
    }
}
