using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Models;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Validation;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Interfaces;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Models
{
    internal partial class Graph
    {
        internal string? ResourceName { get; set; } = null;

        internal readonly List<Func<IStateMachineVisitor, Task>> VisitingTasks = new List<Func<IStateMachineVisitor, Task>>();

        internal readonly StateflowsBuilder StateflowsBuilder;

        public Dictionary<string, int> InitCounter = new Dictionary<string, int>();

        public IEnumerable<VertexType> StateVertexTypes = new List<VertexType>()
        {
            VertexType.State,
            VertexType.InitialState,
            VertexType.CompositeState,
            VertexType.InitialCompositeState,
            VertexType.OrthogonalState,
            VertexType.InitialOrthogonalState,
        };

        public IEnumerable<VertexType> ForkTargetableVertexTypes = new List<VertexType>()
        {
            VertexType.History,
            VertexType.State,
            VertexType.InitialState,
            VertexType.CompositeState,
            VertexType.InitialCompositeState,
            VertexType.OrthogonalState,
            VertexType.InitialOrthogonalState,
        };

        public Graph(string name, int version, StateflowsBuilder stateflowsBuilder, BehaviorClass? ownerClass, BehaviorClass? parentClass)
        {
            Name = name;
            Version = version;
            StateflowsBuilder = stateflowsBuilder;
            Class = new StateMachineClass(Name);
            OwnerClass = ownerClass;
            ParentClass = parentClass;
        }

        public StateMachineClass Class { get; }
        public BehaviorClass? OwnerClass { get; }
        public BehaviorClass? ParentClass { get; }
        public string BaseStateMachineName { get; set; } = null;
        public string Name { get; }
        public int Version { get; }
        public Type StateMachineType { get; set; }
        public bool PolymorphicTriggers { get; set; }
        public string InitialVertexName { get; set; }
        public Vertex InitialVertex { get; set; }
        public Dictionary<string, Vertex> Vertices { get; } = new Dictionary<string, Vertex>();
        public Dictionary<string, Vertex> AllVertices { get; } = new Dictionary<string, Vertex>();
        public List<Edge> AllEdges { get; } = new List<Edge>();

        public readonly Dictionary<string, Logic<StateMachinePredicateAsync>> Initializers = new Dictionary<string, Logic<StateMachinePredicateAsync>>();
        public readonly List<Type> InitializerTypes = new List<Type>();
        public Logic<StateMachinePredicateAsync> DefaultInitializer;

        public Logic<StateMachineActionAsync> Finalize { get; } =
            new Logic<StateMachineActionAsync>(Constants.Finalize);

        public readonly List<StateMachineExceptionHandlerFactoryAsync> ExceptionHandlerFactories = new();

        public readonly List<StateMachineInterceptorFactoryAsync> InterceptorFactories = new();

        public readonly List<StateMachineObserverFactoryAsync> ObserverFactories = new();

        public List<BehaviorClass> RequiredBehaviors = new();

        public void Validate(IEnumerable<BehaviorClass> behaviorClasses)
        {
            var path = $"StateMachine[{Name}]";
            var report = new ModelValidationReport();
            foreach (var diagnostic in ValidationReport.Diagnostics.Where(d => d.RuleId != "SM-041" || d.ElementPath != path))
                report.Add(diagnostic.RuleId, diagnostic.ElementPath, diagnostic.Message, diagnostic.Severity);
            ValidationReport = report;
            var registered = behaviorClasses.ToHashSet();
            var required = RequiredBehaviors.Concat(AllVertices.Values
                .Where(vertex => vertex != null && !string.IsNullOrEmpty(vertex.BehaviorName) && !string.IsNullOrEmpty(vertex.BehaviorType))
                .Select(vertex => new BehaviorClass(vertex.BehaviorType, vertex.BehaviorName)));
            foreach (var missing in required.Distinct().Where(bc => !registered.Contains(bc)))
            {
                var message = $"{missing.Type} '{missing.Name}' required by state machine '{Name}' is not registered.";
                if (!ValidationReport.Diagnostics.Any(d => d.RuleId == "SM-041" && d.Message == message))
                    ValidationReport.Add("SM-041", path, message);
            }
            if (ValidationReport.HasErrors)
                throw new StateMachineValidationException(ValidationReport, Class);
        }

        public void Build()
        {
            ValidateDefinition();

            if (!StateflowsBuilder.ResourceNames.TryGetValue(ResourceName ?? string.Empty, out var resourceName))
            {
                ValidationReport.Add("SM-045", $"StateMachine[{Name}]", $"Resource group '{ResourceName}' does not exist.");
                // Preserve the existing resource-registration exception contract.
                throw new InvalidOperationException($"Resource group {ResourceName ?? string.Empty} does not exist");
            }

            InitialVertex = Vertices[InitialVertexName];
            foreach (var vertex in AllVertices.Values)
            {
                foreach (var region in vertex.Regions)
                    region.InitialVertex = string.IsNullOrEmpty(region.InitialVertexName) ? null : region.Vertices[region.InitialVertexName];

                vertex.BehaviorEventTypes.Clear();
                if (string.IsNullOrEmpty(vertex.BehaviorName)) continue;
                switch (vertex.BehaviorType)
                {
                    case BehaviorType.Activity:
                        var activityVisitor = new DoActivityVisitor(StateflowsBuilder.TypeMapper);
                        StateflowsBuilder.EnsureActivitiesServices().VisitActivityAsync(vertex.BehaviorName, 1, activityVisitor).GetAwaiter().GetResult();
                        vertex.BehaviorEventTypes.AddRange(activityVisitor.EventTypes);
                        break;

                    case BehaviorType.StateMachine:
                        var machineVisitor = new SubmachineVisitor(StateflowsBuilder.TypeMapper);
                        StateflowsBuilder.EnsureStateMachinesServices().VisitStateMachineAsync(vertex.BehaviorName, 1, machineVisitor).GetAwaiter().GetResult();
                        vertex.BehaviorEventTypes.AddRange(machineVisitor.EventTypes);
                        break;

                    default:
                        var actionVisitor = new DoActionVisitor(StateflowsBuilder.TypeMapper);
                        StateflowsBuilder.EnsureActionsServices().VisitActionAsync(vertex.BehaviorName, 1, actionVisitor).GetAwaiter().GetResult();
                        vertex.BehaviorEventTypes.AddRange(actionVisitor.EventTypes);
                        break;
                }
            }

            foreach (var edge in AllEdges)
            {
                edge.Target = edge.TargetName == Constants.DefaultTransitionTarget ? null : AllVertices[edge.TargetName];
                edge.TimeTriggerTypes = edge.ActualTriggerTypes.Where(type => type.IsSubclassOf(typeof(TimeEvent))).ToHashSet();
                edge.RecurringTypes = edge.ActualTriggerTypes.Where(type => type.IsSubclassOf(typeof(RecurringEvent))).ToHashSet();
                edge.Signature = $"{edge.Source.Identifier}-{edge.Trigger}->";
                var triggerDescriptor = edge.IsElse ? $"{edge.Trigger}|else" : edge.Trigger;
                edge.Identifier = edge.Target != null
                    ? $"{edge.Source.Identifier}-{triggerDescriptor}->{edge.Target.Identifier}"
                    : $"{edge.Source.Identifier}-{triggerDescriptor}";
            }
            StateflowsBuilder.ResourcesByBehaviorClass[Class] = resourceName;
        }
    }
}
