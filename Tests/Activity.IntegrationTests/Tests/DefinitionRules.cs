using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Stateflows.Activities;
using Stateflows.Activities.Exceptions;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Registration.Interfaces;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests;

public sealed class RequiredPairConsumer(IInputToken<string> text, IInputToken<int> number) : IActionNode
{
    public static int Executions { get; set; }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _ = text.Token;
        _ = number.Token;
        Executions++;
        return Task.CompletedTask;
    }
}

public sealed class MultipleConstructorAction : IActionNode
{
    public MultipleConstructorAction() { }
    public MultipleConstructorAction(string unused) { }

    public Task ExecuteAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}

[TestClass]
public class DefinitionRules
{
    // Reflective mutations exercise invalid model states that the fluent API cannot construct.
    private static void BuildActivities(ActivitiesBuildAction configure)
        => new ServiceCollection().AddStateflows(builder => builder.AddActivities(configure));

    private static void BuildMutatedActivity(
        string name,
        Action<IActivityBuilder> configure,
        Action<object> mutateGraph
    )
        => new ServiceCollection().AddStateflows(stateflows => stateflows.AddActivities(activities =>
            activities.AddActivity(name, activity =>
            {
                configure(activity);
                mutateGraph(GetProperty(activity, "Graph"));
            })
        ));

    private static object GetProperty(object instance, string name)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var property = instance.GetType().GetProperty(name, flags);
        if (property is not null)
        {
            return property.GetValue(instance)!;
        }

        return instance.GetType().GetField(name, flags)!.GetValue(instance)!;
    }

    private static void SetProperty(object instance, string name, object? value)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var property = instance.GetType().GetProperty(name, flags);
        if (property is not null)
        {
            property.SetValue(instance, value);
            return;
        }

        instance.GetType().GetField(name, flags)!.SetValue(instance, value);
    }

    private static object GetNode(object graph, string name)
    {
        var nodes = (IDictionary)GetProperty(graph, "AllNamedNodes");
        if (nodes[name] is object node)
        {
            return node;
        }

        return nodes.Values.Cast<object>().First(candidate =>
            Equals(GetProperty(candidate, "Name"), name) ||
            Equals(GetProperty(candidate, "OwnName"), name)
        );
    }

    private static object GetFirstEdge(object graph)
        => ((IEnumerable)GetProperty(graph, "AllEdgesList")).Cast<object>().First();

    private static object GetNodeOfType(object graph, string typeName)
        => ((IDictionary)GetProperty(graph, "AllNodes")).Values.Cast<object>()
            .First(node => GetProperty(node, "Type").ToString() == typeName);

    private static void InvokeBuild(object graph)
        => graph.GetType().GetMethod("Build", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .Invoke(graph, null);

    private static void AddIncomingAction(IActivityBuilder activity)
        => activity
            .AddInitial(initial => initial.AddControlFlow("action"))
            .AddAction("action", async _ => { });

    [TestMethod]
    public void UnsupportedNodeKindsAreRejected()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "unsupported-node-kind",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("accept"))
                .AddAcceptEventAction<TestEvent>("accept", _ => Task.CompletedTask),
            graph => SetProperty(GetNode(graph, "accept"), "Type",
                Enum.ToObject(GetProperty(GetNode(graph, "accept"), "Type").GetType(), 1000))
        ));
    }

    [TestMethod]
    public void OneOfEventMappingsMustExpandToAllAcceptedTypes()
    {
        object? graph = null;
        new ServiceCollection().AddStateflows(stateflows => stateflows
            .AddOneOf()
            .AddActivities(activities => activities.AddActivity("oneof-event-mappings", activity =>
            {
                activity.AddAcceptEventAction<OneOf<TestEvent, OtherTestEvent>>(
                    "accept",
                    _ => Task.CompletedTask
                );
                graph = GetProperty(activity, "Graph");
            }))
        );

        Assert.IsNotNull(graph);
        var mappedTypes = (IEnumerable)GetProperty(GetNode(graph, "accept"), "ActualEventTypes");
        CollectionAssert.AreEquivalent(
            new[] { typeof(TestEvent), typeof(OtherTestEvent) },
            mappedTypes.Cast<Type>().ToArray()
        );
    }

    [TestMethod]
    public void NonContainerNodesCannotOwnChildNodes()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "action-with-child",
            activity =>
            {
                AddIncomingAction(activity);
                activity.AddAcceptEventAction<TestEvent>("accept", _ => Task.CompletedTask);
            },
            graph =>
            {
                var parent = GetNode(graph, "action");
                var child = GetNode(graph, "accept");
                ((IDictionary)GetProperty(parent, "Nodes")).Add(GetProperty(child, "Identifier"), child);
                SetProperty(child, "Parent", parent);
                SetProperty(child, "Level", 2);
            }
        ));
    }

    [TestMethod]
    public void ParallelActivityBodiesCannotContainReactiveEventNodes()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "reactive-node-in-parallel-body",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("parallel"))
                .AddParallelActivity<int>("parallel", parallel => parallel
                    .AddInitial(initial => initial.AddControlFlow("child"))
                    .AddAction("child", async _ => { })
                ),
            graph =>
            {
                var node = GetNode(graph, "parallel.child");
                var nodes = (IDictionary)GetProperty(graph, "AllNodes");
                var scopeNodes = (IDictionary)GetProperty(GetNode(graph, "parallel"), "Nodes");
                var oldIdentifier = GetProperty(node, "Identifier").ToString();
                SetProperty(node, "Type", Enum.Parse(GetProperty(node, "Type").GetType(), "AcceptEventAction"));
                SetProperty(node, "EventType", typeof(TestEvent));
                SetProperty(node, "ActualEventTypes", new[] { typeof(TestEvent) });
                SetProperty(node, "Identifier", $"AcceptEventAction:{GetProperty(GetNode(graph, "parallel"), "Name")}:parallel.child");
                nodes.Remove(oldIdentifier);
                scopeNodes.Remove(oldIdentifier);
                nodes.Add(GetProperty(node, "Identifier"), node);
                scopeNodes.Add(GetProperty(node, "Identifier"), node);
                SetProperty(graph, "Interactive", true);
            }
        ));
    }

    [TestMethod]
    public void NodesMustRetainTheirParent()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "missing-parent",
            AddIncomingAction,
            graph => SetProperty(GetNode(graph, "action"), "Parent", null)
        ));
    }

    [TestMethod]
    public void NodesMustRetainTheirGraphReference()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "missing-graph-reference",
            AddIncomingAction,
            graph => SetProperty(GetNode(graph, "action"), "Graph", null)
        ));
    }

    [TestMethod]
    public void ContainmentMustBeAcyclic()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "containment-cycle",
            AddIncomingAction,
            graph =>
            {
                var node = GetNode(graph, "action");
                ((IDictionary)GetProperty(node, "Nodes")).Add(GetProperty(node, "Identifier"), node);
            }
        ));
    }

    [TestMethod]
    public void NamedNodesMustHaveNonEmptyEffectiveNames()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "empty-effective-name",
            AddIncomingAction,
            graph => SetProperty(GetNode(graph, "action"), "Name", string.Empty)
        ));
    }

    [TestMethod]
    public void NodeLevelsMustMatchTheirContainment()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "incorrect-node-level",
            AddIncomingAction,
            graph => SetProperty(GetNode(graph, "action"), "Level", 42)
        ));
    }

    [TestMethod]
    public void NodeIdentifiersMustMatchGraphIndexes()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "node-identifier-mismatch",
            AddIncomingAction,
            graph => SetProperty(GetNode(graph, "action"), "Identifier", "not-the-index-key")
        ));
    }

    [TestMethod]
    public void FlowSourceNameMustMatchItsSourceReference()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "incorrect-flow-source-name",
            AddIncomingAction,
            graph => SetProperty(GetFirstEdge(graph), "SourceName", "not-the-source")
        ));
    }

    [TestMethod]
    public void FlowsMustBelongToTheirSourceOutgoingCollection()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "missing-source-edge-membership",
            AddIncomingAction,
            graph =>
            {
                var edge = GetFirstEdge(graph);
                var source = GetProperty(edge, "Source");
                ((IList)GetProperty(source, "Edges")).Remove(edge);
            }
        ));
    }

    [TestMethod]
    public void FlowsMustBelongToTheGraphEdgeCollection()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "missing-global-edge-membership",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("accept"))
                .AddAcceptEventAction<TestEvent>("accept", _ => Task.CompletedTask),
            graph => ((IList)GetProperty(graph, "AllEdgesList")).Clear()
        ));
    }

    [TestMethod]
    public void InitialFlowsMustRetainControlTokenCapability()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "object-flow-from-initial",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("action"))
                .AddAction("action", async _ => { }),
            graph =>
            {
                var edge = GetFirstEdge(graph);
                SetProperty(edge, "TokenType", typeof(string));
                SetProperty(edge, "TargetTokenType", typeof(string));
            }
        ));
    }

    [TestMethod]
    public void NodeOptionsMustUseSupportedFlags()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "invalid-node-options",
            AddIncomingAction,
            graph => SetProperty(GetNode(graph, "action"), "Options",
                Enum.ToObject(GetProperty(GetNode(graph, "action"), "Options").GetType(), int.MaxValue))
        ));
    }

    [TestMethod]
    public void BuildValidationAccumulatesIndependentErrors()
    {
        object? graph = null;

        Assert.Throws<ActivityDefinitionException>(() => new ServiceCollection().AddStateflows(stateflows =>
            stateflows.AddActivities(activities => activities.AddActivity("accumulated-validation-errors", activity =>
            {
                activity
                    .SetResourceName("missing-resource-group")
                    .AddInitial(initial => initial.AddControlFlow("producer"))
                    .AddAction("producer", async _ => { }, producer => producer
                        .AddControlFlow("first", flow => flow.SetWeight(-1))
                        .AddControlFlow("second", flow => flow.SetWeight(-2))
                    )
                    .AddAction("first", async _ => { })
                    .AddAction("second", async _ => { });
                graph = GetProperty(activity, "Graph");
            }))
        ));

        Assert.IsNotNull(graph);
        var report = GetProperty(graph, "ValidationReport");
        var diagnostics = ((IEnumerable)GetProperty(report, "Diagnostics")).Cast<object>();
        var ruleIds = diagnostics.Select(diagnostic => GetProperty(diagnostic, "RuleId").ToString()).ToArray();
        CollectionAssert.AreEquivalent(new[] { "ACT-049", "ACT-054", "ACT-054" }, ruleIds);
    }

    [TestMethod]
    public void InteractiveEventTypeAndGraphFlagsMustAgree()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "accept-event-type-missing",
            activity => activity.AddAcceptEventAction<TestEvent>("accept", _ => Task.CompletedTask),
            graph => SetProperty(GetNode(graph, "accept"), "EventType", null)
        ));

        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "accept-event-mappings-missing",
            activity => activity.AddAcceptEventAction<TestEvent>("accept", _ => Task.CompletedTask),
            graph => SetProperty(GetNode(graph, "accept"), "ActualEventTypes", Array.Empty<Type>())
        ));

        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "interactive-graph-flag-missing",
            activity => activity.AddAcceptEventAction<TestEvent>("accept", _ => Task.CompletedTask),
            graph => SetProperty(graph, "Interactive", false)
        ));
    }

    [TestMethod]
    public void EventActionNodeKindsMustRetainTheirEventCapability()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "action-masquerades-as-send-event",
            AddIncomingAction,
            graph => SetProperty(GetNode(graph, "action"), "Type",
                Enum.Parse(GetProperty(GetNode(graph, "action"), "Type").GetType(), "SendEventAction"))
        ));
    }

    [TestMethod]
    public void ExceptionHandlersRequireExceptionTypes()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "invalid-exception-handler-type",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("action"))
                .AddAction("action", async _ => { }, action => action
                    .AddExceptionHandler<Exception>(async _ => { })
                ),
            graph => SetProperty(GetNodeOfType(graph, "ExceptionHandler"), "ExceptionType", typeof(string))
        ));
    }

    [TestMethod]
    public void ExceptionRoutingMustUseExceptionAndNodeReferenceTokens()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "invalid-exception-routing-token",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("action"))
                .AddAction("action", async _ => { }, action => action
                    .AddExceptionHandler<Exception>(async _ => { })
                ),
            graph =>
            {
                var edges = (IEnumerable)GetProperty(GetNode(graph, "action"), "Edges");
                var exceptionEdge = edges.Cast<object>()
                    .First(edge => Equals(GetProperty(edge, "TokenType"), typeof(Exception)));
                SetProperty(exceptionEdge, "TokenType", typeof(string));
            }
        ));

        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "invalid-exception-handler-reference-token",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("action"))
                .AddAction("action", async _ => { }, action => action
                    .AddExceptionHandler<Exception>(async _ => { })
                ),
            graph =>
            {
                var edges = (IEnumerable)GetProperty(GetNode(graph, "action"), "Edges");
                var handlerReferenceEdge = edges.Cast<object>()
                    .First(edge => GetProperty(edge, "TokenType").ToString()!.EndsWith("NodeReferenceToken", StringComparison.Ordinal));
                SetProperty(handlerReferenceEdge, "TokenType", typeof(string));
            }
        ));
    }

    [TestMethod]
    public void GlobalNodeIndexesMustContainTheSameNodesAsTheirScopes()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "missing-global-node-index",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("action"))
                .AddAction("action", async _ => { }),
            graph => ((IDictionary)GetProperty(graph, "AllNodes")).Remove(GetProperty(GetNode(graph, "action"), "Identifier"))
        ));

        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "missing-global-name-index",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("action"))
                .AddAction("action", async _ => { }),
            graph => ((IDictionary)GetProperty(graph, "AllNamedNodes")).Remove(GetProperty(GetNode(graph, "action"), "Name"))
        ));

        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "missing-scope-node-index",
            activity => activity
                .AddInitial(initial => initial.AddControlFlow("action"))
                .AddAction("action", async _ => { }),
            graph => ((IDictionary)GetProperty(graph, "Nodes")).Remove(GetProperty(GetNode(graph, "action"), "Identifier"))
        ));
    }

    [TestMethod]
    public void InheritedActivitiesAreValidatedAfterExpansion()
    {
        BuildActivities(activities => activities.AddActivity("valid-expanded-override", activity =>
            activity.UseActivity<ActivityWithJoinFork>(_ => { })
        ));

        Assert.Throws<ActivityDefinitionException>(() => BuildMutatedActivity(
            "invalid-expanded-override",
            activity => activity.UseActivity<ActivityWithJoinFork>(_ => { }),
            graph => ((IList)GetProperty(GetNodeOfType(graph, "Join"), "Edges")).Clear()
        ));
    }

    [TestMethod]
    public void JoinRequiresExactlyOneOutgoingFlow()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("empty-join-output", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("join"))
                .AddJoin("join", _ => { })
            )
        ));
    }

    [TestMethod]
    public void JoinRejectsMultipleOutgoingFlows()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("multiple-join-outputs", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("join"))
                .AddJoin("join", join =>
                {
                    join.AddFlow<int>("first");
                    join.AddFlow<int>("second");
                })
                .AddAction("first", async _ => { })
                .AddAction("second", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void FlowsCannotTargetInitialNodesOrCrossStructuredScopes()
    {
        Assert.Throws<FlowDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("initial-is-not-a-target", activity => activity
                .AddInitial(initial => initial.AddControlFlow("InitialNode"))
            )
        ));

        Assert.Throws<FlowDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("cross-scope-flow", activity => activity
                .AddInitial(initial => initial.AddControlFlow("outer"))
                .AddStructuredActivity("outer", scope => scope
                    .AddInitial(initial => initial.AddControlFlow("child"))
                    .AddAction("child", async _ => { })
                )
                .AddAction("outside", async _ => { }, action => action.AddControlFlow("outer.child"))
            )
        ));
    }

    [TestMethod]
    public void MergeRequiresExactlyOneOutgoingFlow()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("empty-merge-output", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("merge"))
                .AddMerge("merge", _ => { })
            )
        ));
    }

    [TestMethod]
    public void MergeRejectsMultipleOutgoingFlows()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("multiple-merge-outputs", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("merge"))
                .AddMerge("merge", merge =>
                {
                    merge.AddFlow<int>("first");
                    merge.AddFlow<int>("second");
                })
                .AddAction("first", async _ => { })
                .AddAction("second", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void OneInputJoinAndOneOutputForkRemainValid()
    {
        BuildActivities(activities =>
            activities.AddActivity("single-input-join", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("join"))
                .AddJoin("join", join => join.AddFlow<int>("consume"))
                .AddAction("consume", async _ => { })
            )
        );

        BuildActivities(activities =>
            activities.AddActivity("single-output-fork", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("fork"))
                .AddFork("fork", fork => fork.AddFlow<int>("consume"))
                .AddAction("consume", async _ => { })
            )
        );
    }

    [TestMethod]
    public void DecisionMayOmitElseFlow()
    {
        BuildActivities(activities =>
            activities.AddActivity("decision-without-else", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("decision"))
                .AddDecision<int>("decision", decision => decision
                    .AddFlow("unreachable", flow => flow.AddGuard(context => Task.FromResult(context.Token > 1)))
                )
                .AddAction("unreachable", async _ => { })
            )
        );
    }

    [TestMethod]
    public void NegativeControlFlowWeightIsRejected()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("negative-flow-weight", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async _ => { }, action => action
                    .AddControlFlow("consume", flow => flow.SetWeight(-1))
                )
                .AddAction("consume", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void NegativeObjectFlowWeightIsRejected()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("negative-object-flow-weight", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(42), action => action
                    .AddFlow<int>("consume", flow => flow.SetWeight(-1))
                )
                .AddAction("consume", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void NegativeTransformedFlowWeightIsRejected()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("negative-transformed-flow-weight", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output(42), action => action
                    .AddFlow<int>("consume", flow => flow
                        .AddTransformation<string>(context => Task.FromResult(context.Token.ToString()))
                        .SetWeight(-1)
                    )
                )
                .AddAction("consume", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void ZeroFlowWeightRemainsValid()
    {
        BuildActivities(activities => activities
            .AddActivity("zero-flow-weight", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async _ => { }, action => action
                    .AddControlFlow("consume", flow => flow.SetWeight(0))
                )
                .AddAction("consume", async _ => { })
            )
        );
    }

    [TestMethod]
    public void ParallelChunkSizeMustBePositive()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("zero-parallel-chunk", activity => activity
                .AddInitial(initial => initial.AddControlFlow("parallel"))
                .AddParallelActivity<int>("parallel", _ => { }, 0)
            )
        ));
    }

    [TestMethod]
    public void IterativeChunkSizeMustBePositive()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("negative-iterative-chunk", activity => activity
                .AddInitial(initial => initial.AddControlFlow("iterative"))
                .AddIterativeActivity<int>("iterative", _ => { }, -1)
            )
        ));
    }

    [TestMethod]
    public void PositiveParallelChunkSizeRemainsValid()
    {
        BuildActivities(activities =>
            activities.AddActivity("positive-parallel-chunk", activity => activity
                .AddInitial(initial => initial.AddControlFlow("parallel"))
                .AddParallelActivity<int>("parallel", _ => { }, 1)
            )
        );
    }

    [TestMethod]
    public void MissingTargetsAndDanglingNodesAreRejected()
    {
        Assert.Throws<FlowDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("missing-target", activity => activity
                .AddInitial(initial => initial.AddControlFlow("not-registered"))
            )
        ));

        Assert.Throws<NodeDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("dangling-action", activity => activity
                .AddAction("orphan", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void EmptyActivitiesAndReactiveNodesWithoutIncomingFlowsRemainValid()
    {
        BuildActivities(activities => activities
            .AddActivity("empty-activity", _ => { })
            .AddActivity("empty-structured-scope", activity => activity
                .AddInitial(initial => initial.AddControlFlow("scope"))
                .AddStructuredActivity("scope", _ => { })
            )
            .AddActivity("reactive-entry", activity => activity
                .AddAcceptEventAction<TestEvent>("accept", _ => Task.CompletedTask)
            )
        );
    }

    [TestMethod]
    public void DuplicateSpecialEntryNodesAreRejected()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("duplicate-initial", activity => activity
                .AddInitial(_ => { })
                .AddInitial(_ => { })
            )
        ));
    }

    [TestMethod]
    public void LocalNamesMayRepeatInDistinctStructuredScopes()
    {
        BuildActivities(activities => activities.AddActivity("repeated-local-names", activity => activity
            .AddInitial(initial => initial
                .AddControlFlow("first")
                .AddControlFlow("second")
            )
            .AddStructuredActivity("first", scope => scope
                .AddInitial(initial => initial.AddControlFlow("child"))
                .AddAction("child", async _ => { })
            )
            .AddStructuredActivity("second", scope => scope
                .AddInitial(initial => initial.AddControlFlow("child"))
                .AddAction("child", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void TypedNodesCannotDeclareMultiplePublicConstructors()
    {
        Assert.Throws<NodeDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("multiple-node-constructors", activity => activity
                .AddInitial(initial => initial.AddControlFlow<MultipleConstructorAction>())
                .AddAction<MultipleConstructorAction>()
            )
        ));
    }

    [TestMethod]
    public void TransitiveNodesCannotEmitTypesNotPresentOnTheirIncomingFlows()
    {
        Assert.Throws<NodeDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("invalid-fork-output-type", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context => context.Output("input"), action => action.AddFlow<string>("fork"))
                .AddFork("fork", fork => fork.AddFlow<int>("sink"))
                .AddAction("sink", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void DuplicateFlowIdentityIsRejectedIndependentlyOfGuardConfiguration()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("duplicate-flow", activity => activity
                .AddInitial(initial => initial
                    .AddControlFlow("sink", flow => flow.AddGuard(_ => Task.FromResult(true)))
                    .AddControlFlow("sink", flow => flow.AddGuard(_ => Task.FromResult(false)))
                )
                .AddAction("sink", async _ => { })
            )
        ));
    }

    [TestMethod]
    public void InvalidResourceGroupsAreReportedAsDefinitionErrors()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities =>
            activities.AddActivity("unknown-resource", activity => activity
                .SetResourceName("not-registered")
            )
        ));
    }

    [TestMethod]
    public void DuplicateActivityRegistrationsAreReportedAsDefinitionErrors()
    {
        Assert.Throws<ActivityDefinitionException>(() => BuildActivities(activities => activities
            .AddActivity("duplicate-registration", _ => { })
            .AddActivity("duplicate-registration", _ => { })
        ));
    }

    [TestMethod]
    public void FailedBuildMustNotLeaveGraphMarkedBuilt()
    {
        object? graph = null;
        Assert.Throws<FlowDefinitionException>(() => new ServiceCollection().AddStateflows(stateflows =>
            stateflows.AddActivities(activities => activities.AddActivity("retry-after-failed-build", activity =>
            {
                activity
                    .AddInitial(initial => initial.AddControlFlow("sink"))
                    .AddAction("sink", async _ => { });
                graph = GetProperty(activity, "Graph");
                SetProperty(GetFirstEdge(graph), "TargetName", "missing");
            }))
        ));

        Assert.IsNotNull(graph);
        var edge = GetFirstEdge(graph);
        SetProperty(edge, "TargetName", "sink");
        InvokeBuild(graph);

        Assert.AreEqual(1, ((IDictionary)GetProperty(graph, "AllEdges")).Count);
        Assert.AreEqual(1, ((IDictionary)GetProperty(graph, "AllNodes")).Values.Cast<object>()
            .Sum(node => ((ICollection)GetProperty(node, "IncomingEdges")).Count));
    }

    [TestMethod]
    public void RepeatedSuccessfulBuildsMustRemainIdempotent()
    {
        object? graph = null;
        BuildActivities(activities => activities.AddActivity("repeated-successful-build", activity =>
        {
            activity
                .AddInitial(initial => initial.AddControlFlow("sink"))
                .AddAction("sink", async _ => { });
            graph = GetProperty(activity, "Graph");
        }));

        Assert.IsNotNull(graph);
        InvokeBuild(graph);

        Assert.AreEqual(1, ((IDictionary)GetProperty(graph, "AllEdges")).Count);
        Assert.AreEqual(1, ((IDictionary)GetProperty(graph, "AllNodes")).Values.Cast<object>()
            .Sum(node => ((ICollection)GetProperty(node, "IncomingEdges")).Count));
    }

    private sealed class TestEvent { }
    private sealed class OtherTestEvent { }
}

[TestClass]
public class MissingRequiredTokenWarning : StateflowsTestClass
{
    [TestInitialize]
    public override void Initialize()
        => base.Initialize();

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
    {
        builder.AddActivities(activities => activities
            .AddActivity("missing-required-runtime-token", activity => activity
                .AddInitial(initial => initial.AddControlFlow("produce"))
                .AddAction("produce", async context =>
                    {
                        context.Output("text");
                        context.Output(42);
                    },
                    action => action
                        .AddFlow<string>("consumer", flow => flow.AddGuard(_ => Task.FromResult(false)))
                        .AddFlow<int>("consumer")
                )
                .AddAction<RequiredPairConsumer>("consumer")
            )
        );
    }

    [TestMethod]
    public async Task MissingRequiredRuntimeTokenSkipsNodeAndReportsWarning()
    {
        RequiredPairConsumer.Executions = 0;
        var trace = new StringWriter();
        var listener = new TextWriterTraceListener(trace);
        Trace.Listeners.Add(listener);

        try
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("missing-required-runtime-token", "rules"), out var activity))
            {
                await activity.SendAsync(new Initialize());
            }

            listener.Flush();

            Assert.AreEqual(0, RequiredPairConsumer.Executions, "A node missing a required input token must not execute.");
            Assert.IsTrue(trace.ToString().Contains("warning", StringComparison.OrdinalIgnoreCase));
            StringAssert.Contains(trace.ToString(), typeof(string).FullName!);
        }
        finally
        {
            Trace.Listeners.Remove(listener);
            listener.Dispose();
            trace.Dispose();
        }
    }
}

[TestClass]
public class RuntimeDecisionWithoutElse : StateflowsTestClass
{
    public static int SinkExecutions { get; set; }

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddActivities(activities => activities
                .AddActivity("decision-without-else-runtime", activity => activity
                    .AddInitial(initial => initial.AddControlFlow("produce"))
                    .AddAction("produce", async context => context.Output(1), action => action.AddFlow<int>("decision"))
                    .AddDecision<int>("decision", decision => decision
                        .AddFlow("sink", flow => flow.AddGuard(context => Task.FromResult(context.Token > 1)))
                    )
                    .AddAction("sink", async _ => SinkExecutions++)
                )
            );
        }

        [TestMethod]
        public async Task TokenStopsWhenNoDecisionGuardPassesAndNoElseExists()
        {
            SinkExecutions = 0;

            if (ActivityLocator.TryLocateActivity(new ActivityId("decision-without-else-runtime", "rules"), out var activity))
            {
                await activity.SendAsync(new Initialize());
            }

            Assert.AreEqual(0, SinkExecutions);
    }
}
