using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using StateMachine.IntegrationTests.Utils;

namespace StateMachine.IntegrationTests.Tests;

[TestClass]
public class OrthogonalBehaviors : StateflowsTestClass
{
    public class Executed;

    public class DoAction(IParentBehaviorContext parent) : IAction
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            parent.Send(new Executed());
            return Task.CompletedTask;
        }
    }

    public class DoActivity : IActivity
    {
        public static void Build(IActivityBuilder builder) => builder
            .AddInitial(b => b.AddControlFlow("execute"))
            .AddAction("execute", c =>
            {
                Assert.IsTrue(c.TryGetParentBehaviorContext(out var parent));
                parent.Send(new Executed());
                return Task.CompletedTask;
            });
    }

    public class BaseMachine : IStateMachine
    {
        public static void Build(IStateMachineBuilder builder) => builder
            .AddInitialOrthogonalState("active", b => b
                .AddRegion(r => r.AddInitialState("left")));
    }

    private readonly Dictionary<string, TaskCompletionSource> executions = [];
    private readonly Dictionary<string, TaskCompletionSource> regionsEntered = [];
    private readonly Dictionary<string, int> entries = [];

    [TestInitialize]
    public override void Initialize() => base.Initialize();

    [TestCleanup]
    public override void Cleanup() => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddStateMachines(m =>
        {
            foreach (var kind in new[] { "action", "typed-action", "activity", "typed-activity" })
            {
                foreach (var form in new[] { "initial", "ordinary", "override" })
                {
                    var name = $"{form}-{kind}";
                    executions[name] = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    regionsEntered[name] = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    entries[name] = 0;
                    m.AddStateMachine(name, b =>
                    {
                        if (form == "override")
                        {
                            b.UseStateMachine<BaseMachine>(b => b.UseOrthogonalState("active", s =>
                                Attach(s, kind)
                                    .UseRegion(0, r => r.UseState("left", s => s
                                        .AddOnEntry(_ => RegionEntered(name))))
                                    .AddRegion(r => r.AddInitialState("right", s => s
                                        .AddOnEntry(_ => RegionEntered(name))))
                                    .AddOnEntry(_ => Task.CompletedTask)
                                    .AddInternalTransition<Executed>(_ => executions[name].TrySetResult())));
                        }
                        else
                        {
                            void Configure(IOrthogonalStateBuilder s) => Attach(
                                    s.AddRegion(r => r.AddInitialState("left", s => s
                                        .AddOnEntry(_ => RegionEntered(name)))), kind)
                                .AddRegion(r => r.AddInitialState("right", s => s
                                    .AddOnEntry(_ => RegionEntered(name))))
                                .AddOnEntry(_ => Task.CompletedTask)
                                .AddInternalTransition<Executed>(_ => executions[name].TrySetResult());

                            if (form == "initial")
                            {
                                b.AddInitialOrthogonalState("active", Configure);
                            }
                            else
                            {
                                b.AddInitialState("idle", s => s.AddDefaultTransition("active"))
                                    .AddOrthogonalState("active", Configure);
                            }
                        }
                    });
                }
            }
        });
    }

    private static IBehaviorOrthogonalStateBuilder Attach(IOrthogonalStateBuilder builder, string kind)
        => kind switch
        {
            "action" => builder.AddDoAction(c =>
            {
                Assert.IsTrue(c.TryGetParentBehaviorContext(out var parent));
                parent.Send(new Executed());
                return Task.CompletedTask;
            }),
            "typed-action" => builder.AddDoAction<DoAction>(),
            "activity" => builder.AddDoActivity(DoActivity.Build),
            "typed-activity" => builder.AddDoActivity<DoActivity>(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static IBehaviorOverridenOrthogonalStateBuilder Attach(IOverridenOrthogonalStateBuilder builder, string kind)
        => kind switch
        {
            "action" => builder.AddDoAction(c =>
            {
                Assert.IsTrue(c.TryGetParentBehaviorContext(out var parent));
                parent.Send(new Executed());
                return Task.CompletedTask;
            }),
            "typed-action" => builder.AddDoAction<DoAction>(),
            "activity" => builder.AddDoActivity(DoActivity.Build),
            "typed-activity" => builder.AddDoActivity<DoActivity>(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    [TestMethod]
    [DataRow("initial-action")]
    [DataRow("initial-typed-action")]
    [DataRow("initial-activity")]
    [DataRow("initial-typed-activity")]
    [DataRow("ordinary-action")]
    [DataRow("ordinary-typed-action")]
    [DataRow("ordinary-activity")]
    [DataRow("ordinary-typed-activity")]
    [DataRow("override-action")]
    [DataRow("override-typed-action")]
    [DataRow("override-activity")]
    [DataRow("override-typed-activity")]
    public async Task DoBehaviorRunsOnOrthogonalState(string name)
    {
        Assert.IsTrue(StateMachineLocator.TryLocateStateMachine(new StateMachineId(name, "x"), out var machine));
        await machine.SendAsync(new Initialize());
        await Task.WhenAll(executions[name].Task, regionsEntered[name].Task).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.AreEqual(2, entries[name]);
    }

    private void RegionEntered(string name)
    {
        lock (entries)
        {
            if (++entries[name] == 2) regionsEntered[name].TrySetResult();
        }
    }

    [TestMethod]
    public void RegionAndConfiguredStateInterfacesDoNotExposeDoBehaviors()
    {
        var regionInterfaces = typeof(IRegionBuilder).Assembly.GetExportedTypes()
            .Where(t => t.IsInterface && t.Namespace == typeof(IRegionBuilder).Namespace &&
                t.Name.EndsWith("RegionBuilder"));

        foreach (var type in regionInterfaces.Concat(
            [typeof(IBehaviorOrthogonalStateBuilder), typeof(IBehaviorOverridenOrthogonalStateBuilder)]))
        {
            Assert.IsFalse(type.GetInterfaces().Any(i => i.IsGenericType &&
                (i.GetGenericTypeDefinition() == typeof(IStateDoActivity<>) ||
                 i.GetGenericTypeDefinition() == typeof(IStateDoAction<>))), type.Name);
        }
    }
}
