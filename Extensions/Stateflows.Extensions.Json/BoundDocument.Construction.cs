using System.Reflection;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json.Linq;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Models;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;
using Stateflows.StateMachines.Registration.Builders;
using Stateflows.StateMachines.Registration.Interfaces;

namespace Stateflows.StateMachines.Json;

internal sealed partial class BoundDocument
{
    internal static object? Invoke(string method, Type[] arguments, params object?[] parameters)
    {
        try
        {
            return typeof(JsonExecution).GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic)!
                .MakeGenericMethod(arguments).Invoke(null, parameters);
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private void Expand(ClrBinding binding, Graph graph, System.Action build)
    {
        try { build(); }
        catch (Exception exception) when (exception is StateflowsDefinitionException or ArgumentException or InvalidOperationException)
        {
            Error(binding.Path, exception.Message, "JSON-005");
            throw new JsonStateMachineException(report, Name, exception);
        }
        finally
        {
            foreach (var vertex in graph.AllVertices.Values)
            {
                vertex.SourcePath ??= binding.Path;
                foreach (var region in vertex.Regions) region.SourcePath ??= binding.Path;
            }
            foreach (var edge in graph.AllEdges) edge.SourcePath ??= binding.Path;
        }
    }

    public void Construct(StateMachineBuilder builder)
    {
        var graph = builder.Graph;
        graph.SourcePath = "/";
        foreach (var (_, path) in Items(document, "", "definitions"))
        {
            var binding = bindings[path];
            Expand(binding, graph, () => Invoke("BuildMachine", [binding.Type], builder));
        }
        if (document.Value<string>("resource") is { } resource)
        {
            if (graph.ResourceName != null && graph.ResourceName != resource)
                Error("/resource", $"A definition already selected resource '{graph.ResourceName}'.", "JSON-005");
            else builder.SetResourceName(resource);
        }
        ConstructContainer(document, "", graph, null);
        foreach (var (_, path) in Items(document, "", "defaultInitializers"))
        {
            var binding = bindings[path];
            Invoke("AddDefaultInitializer", [binding.Type], binding, builder);
        }
        foreach (var (node, path) in Items(document, "", "initializers"))
            foreach (var (_, handlerPath) in Items(node, path, "handlers"))
                foreach (var trigger in triggers[path].SelectMany(Mapped).Distinct())
                {
                    var binding = bindings[handlerPath];
                    Invoke("AddInitializer", [binding.Type, eventBindings[handlerPath][trigger], trigger], binding, builder);
                }
        foreach (var (_, path) in Items(document, "", "finalizers"))
        {
            var binding = bindings[path];
            Invoke("AddFinalizer", [binding.Type], binding, builder);
        }
        foreach (var property in new[] { "observers", "interceptors", "exceptionHandlers" })
            foreach (var (_, path) in Items(document, "", property))
                JsonExecution.AddInfrastructure(bindings[path], property, builder);
        ValidateBehaviorReferences();
        graph.RequiredBehaviors.AddRange(behaviors.Values.Distinct());
        graph.ValidateDefinition();
        JsonStateMachineDefinition.ThrowIfErrors(report, Name);
    }

    private void ValidateBehaviorReferences()
    {
        foreach (var (path, behavior) in behaviors)
        {
            var key = $"{behavior.Name}.1";
            var found = behavior.Type switch
            {
                BehaviorType.StateMachine => ((StateMachinesRegister)services.EnsureStateMachinesServices()).StateMachines.ContainsKey(key),
                BehaviorType.Activity => services.EnsureActivitiesServices().Activities.ContainsKey(key),
                _ => services.EnsureActionsServices().Actions.TryGetValue(key, out var action) &&
                    action.BehaviorClassType == behavior.Type
            };
            if (!found) Error(path + "/behaviorClass", $"Behavior '{behavior.Type}:{behavior.Name}' version 1 is not registered.", "SM-041");
        }
    }

    private void ConstructContainer(JToken container, string path, Graph graph, Region? region)
    {
        var members = region?.Vertices ?? graph.Vertices;
        foreach (var (node, vertexPath) in Items(container, path, "vertices"))
        {
            var name = names[vertexPath + "/name"];
            if (graph.AllVertices.ContainsKey(name))
            {
                Error(vertexPath + "/name", $"Vertex '{name}' is already declared. JSON composition is additive.", "SM-002");
                continue;
            }
            var kind = node.Value<string>("kind")!;
            var vertex = new Vertex
            {
                Name = name, Graph = graph, ParentRegion = region!, SourcePath = vertexPath,
                Type = kind switch
                {
                    "state" => VertexType.State, "composite" => VertexType.CompositeState,
                    "orthogonal" => VertexType.OrthogonalState, "final" => VertexType.FinalState,
                    "choice" => VertexType.Choice, "junction" => VertexType.Junction,
                    "fork" => VertexType.Fork, "join" => VertexType.Join, _ => VertexType.History
                }
            };
            members.Add(name, vertex);
            graph.AllVertices.Add(name, vertex);
            if (vertex.Type == VertexType.History)
            {
                vertex.HistoricalRegion = region!;
                if (region != null) region.History ??= vertex;
            }
            foreach (var (_, bindingPath) in Items(node, vertexPath, "definitions"))
            {
                var binding = bindings[bindingPath];
                Expand(binding, graph, () => Invoke(kind switch
                {
                    "state" => "BuildState", "composite" => "BuildComposite", _ => "BuildOrthogonal"
                }, [binding.Type], vertex));
            }
            ApplyMetadata(node, vertexPath, vertex.Metadata);
            foreach (var property in new[] { "onEntry", "onExit", "onInitialize", "onFinalize" })
                foreach (var (_, handlerPath) in Items(node, vertexPath, property))
                    if (behaviors.TryGetValue(handlerPath, out var behavior))
                        JsonExecution.AddStateBehavior(vertex, property, behavior);
                    else
                        Invoke("AddStateHandler", [bindings[handlerPath].Type], bindings[handlerPath], vertex, property);
            foreach (var (transition, transitionPath) in Items(node, vertexPath, "transitions"))
                ConstructTransition(transition, transitionPath, vertex);
            foreach (var (deferral, deferralPath) in Items(node, vertexPath, "deferrals"))
                ConstructDeferral(deferral, deferralPath, vertex);
            if (node["do"] != null)
            {
                if (vertex.BehaviorName != null || vertex.BehaviorType != null)
                    Error(vertexPath + "/do", "A definition already attached a hosted behavior to this state.", "SM-040");
                else
                {
                    var behavior = behaviors[vertexPath + "/do"];
                    vertex.BehaviorType = behavior.Type;
                    vertex.BehaviorName = behavior.Name;
                    graph.VisitingTasks.Add(visitor => visitor.DoBehaviorAddedAsync(Name, Version,
                        vertex.Name, behavior.Name, behavior.Type, vertex.BehaviorEventTypes));
                }
            }
            if (node["region"] is { } child)
                ConstructContainer(child, vertexPath + "/region", graph, vertex.DefaultRegion);
            foreach (var (childRegion, regionPath) in Items(node, vertexPath, "regions"))
            {
                var created = new Region { Graph = graph, ParentVertex = vertex, SourcePath = regionPath };
                vertex.Regions.Add(created);
                ConstructContainer(childRegion, regionPath, graph, created);
            }
            graph.VisitingTasks.Add(visitor => visitor.VertexAddedAsync(Name, Version, vertex.Name, vertex.Type,
                vertex.ParentRegion?.ParentVertex.Name));
        }
        if (region != null) region.SourcePath ??= path;
        if (container["initial"] != null)
        {
            var initial = names[path + "/initial"];
            var existing = region?.InitialVertexName ?? (region == null ? graph.InitialVertexName : null);
            if (!string.IsNullOrEmpty(existing) && existing != initial)
                Error(path + "/initial", $"A definition already selected initial state '{existing}'.", "SM-013");
            else if (members.TryGetValue(initial, out var selected))
            {
                selected.Type = selected.Type switch
                {
                    VertexType.State => VertexType.InitialState,
                    VertexType.CompositeState => VertexType.InitialCompositeState,
                    VertexType.OrthogonalState => VertexType.InitialOrthogonalState,
                    _ => selected.Type
                };
                if (region == null) graph.InitialVertexName = initial;
                else region.InitialVertexName = initial;
            }
            else Error(path + "/initial", $"Initial vertex '{initial}' is not directly contained.", region == null ? "SM-009" : "SM-012");
        }
        if (region != null && container["history"] != null)
        {
            var history = names[path + "/history"];
            if (members.TryGetValue(history, out var selected)) region.History = selected;
            else Error(path + "/history", $"History vertex '{history}' is not directly contained.", "SM-029");
        }
    }

    private void ApplyMetadata(JToken node, string path, Dictionary<string, object> target)
    {
        if (node["metadata"] is not JObject values) return;
        foreach (var property in values.Properties())
            if (!target.TryAdd(property.Name, metadata[path + "/metadata/" + JsonStateMachineDefinition.Escape(property.Name)]!))
                Error(path + "/metadata/" + JsonStateMachineDefinition.Escape(property.Name),
                    "Metadata key is already supplied by a definition; additive composition cannot replace it.", "JSON-005");
    }

    private void ConstructTransition(JToken node, string path, Vertex vertex)
    {
        var declared = triggers[path];
        if (structuralTriggers.TryGetValue(path, out var structural) && structural.Length != 0)
        {
            var clr = declared.Where(type => type != typeof(StructuralEvent)).ToArray();
            if (clr.Length != 0) ConstructTransition(node, path, vertex, clr, null);
            foreach (var name in structural)
                ConstructTransition(node, path, vertex, [typeof(StructuralEvent)], name);
        }
        else ConstructTransition(node, path, vertex, declared, null);
    }

    private void ConstructTransition(JToken node, string path, Vertex vertex, Type[] declared, string? structuralName)
    {
        var actual = declared.SelectMany(Mapped).Distinct().ToArray();
        var completion = actual.Length == 1 && actual[0] == typeof(Completion);
        if (completion && node.Value<string>("kind") == "event")
            Error(path + "/kind", "Completion-triggered targeted transitions must use kind 'default'.", "SM-019");
        var triggerType = completion ? typeof(Completion) : declared.Length == 1 ? declared[0] : typeof(object);
        var trigger = structuralName ?? (declared.Length == 1 ? Event.GetName(triggerType)
            : System.Text.Json.JsonSerializer.Serialize(actual.Select(Event.GetName).Order(StringComparer.Ordinal)));
        var target = node.Value<string>("kind") == "internal" ? "" : names[path + "/target"];
        var otherwise = node.Value<bool?>("else") == true;
        Edge edge;
        if (structuralName != null)
        {
            Edge? namedEdge = null;
            new StateBuilder(vertex).AddTransitionInternal<StructuralEvent>(target, otherwise,
                builder => namedEdge = ((TransitionBuilder<StructuralEvent>)builder).Edge, structuralName);
            edge = namedEdge!;
            edge.SourcePath = path;
            edge.DeclaredTriggerTypes = declared;
            edge.IsLocal = node.Value<bool?>("local") ?? true;
        }
        else
        {
            edge = new Edge
            {
                Graph = vertex.Graph, Source = vertex, SourceName = vertex.Name, SourcePath = path,
                TriggerType = triggerType, DeclaredTriggerTypes = declared, Trigger = trigger,
                TargetName = target, IsElse = otherwise, IsLocal = node.Value<bool?>("local") ?? true,
                Type = completion ? EdgeType.DefaultTransition : target == "" ? EdgeType.InternalTransition : EdgeType.Transition,
                Name = $"{vertex.Name}-{trigger}{(otherwise ? "|else" : "")}->{target}"
            };
            if (declared.All(t => typeof(Exception).IsAssignableFrom(t))) edge.PolymorphicTriggers = true;
            if (!vertex.Edges.TryAdd(edge.Name, edge))
            {
                Error(path, "Duplicate transition declaration.", "SM-020");
                return;
            }
            vertex.Graph.AllEdges.Add(edge);
        }
        foreach (var (_, bindingPath) in Items(node, path, "definitions"))
        {
            var binding = bindings[bindingPath];
            var eventType = eventBindings[bindingPath].Values.First();
            Expand(binding, vertex.Graph, () => Invoke("BuildTransition", [eventType], binding, edge));
        }
        if (node.Value<bool?>("local") is { } local && local != edge.IsLocal)
            Error(path + "/local", "A transition definition changed the explicitly declared local/external setting.", "JSON-005");
        if (edge.TriggerType != triggerType || edge.Trigger != trigger || edge.DeclaredTriggerTypes != declared ||
            edge.StructuralEventName != structuralName)
            Error(path + "/triggers", "A transition definition changed the explicitly declared trigger set.", "JSON-005");
        var transitionBuilder = new TransitionBuilder<object>(edge);
        if (node["guard"] is { } guard)
        {
            var evaluate = CompileGuard(guard, path + "/guard", vertex, edge);
            transitionBuilder.AddGuards(c => evaluate(((Context.Classes.BaseContext)c).Context));
        }
        foreach (var (_, effectPath) in Items(node, path, "effects"))
            if (behaviors.TryGetValue(effectPath, out var behavior))
                transitionBuilder.AddEffect(c => JsonExecution.RunEffect(c, behavior));
            else
            {
                var dispatch = EventDispatch<Func<Context.Classes.RootContext, Edge, Task>>(effectPath, "TransitionEffect");
                transitionBuilder.AddEffect(c =>
                {
                    var context = ((Context.Classes.BaseContext)c).Context;
                    return Choose(dispatch, context)(context, edge);
                });
            }
        ApplyMetadata(node, path, edge.Metadata);
        if (structuralName == null)
        {
            var visits = declared.Select(type => (Func<IStateMachineVisitor, Task>)Invoke("TransitionVisit", [type], edge)!).ToArray();
            edge.VisitingTask = async visitor =>
            {
                foreach (var visit in visits) await visit(visitor);
            };
            vertex.Graph.VisitingTasks.Add(edge.VisitingTask);
        }
    }

    private Dictionary<Type, T> EventDispatch<T>(string path, string method) where T : Delegate
        => eventBindings[path].ToDictionary(pair => pair.Key, pair => (T)Invoke(method,
            [bindings[path].Type, pair.Value], bindings[path])!);

    private static T Choose<T>(Dictionary<Type, T> dispatch, Context.Classes.RootContext context)
    {
        var type = context.EventHolder.PayloadType;
        if (dispatch.TryGetValue(type, out var selected)) return selected;
        foreach (var pair in dispatch)
            if (pair.Key.IsAssignableFrom(type)) return pair.Value;
        throw new InvalidOperationException($"No bound handler contract for event '{type}'.");
    }

    private Func<Context.Classes.RootContext, Task<bool>> CompileGuard(JToken node, string path, Vertex vertex, Edge? edge)
    {
        foreach (var group in new[] { "all", "any" })
            if (node[group] is JArray array)
            {
                var children = array.Select((child, i) => CompileGuard(child, $"{path}/{group}/{i}", vertex, edge)).ToArray();
                return async context =>
                {
                    foreach (var child in children)
                    {
                        var value = await child(context);
                        if (group == "all" && !value) return false;
                        if (group == "any" && value) return true;
                    }
                    return group == "all";
                };
            }
        if (behaviors.TryGetValue(path, out var behavior))
        {
            // A stable per-leaf key prevents two behavior-backed guards sharing delegation responses.
            var guardIndex = behaviors.Keys.Order(StringComparer.Ordinal).ToList().IndexOf(path);
            if (edge != null)
                return context => JsonExecution.RunTransitionGuard(context, edge, behavior, guardIndex);
            var declared = triggers[path[..path.IndexOf("/guard", StringComparison.Ordinal)]];
            var dispatch = declared.SelectMany(Mapped).Distinct().ToDictionary(type => type,
                type => (Func<Context.Classes.RootContext, Vertex, Task<bool>>)Invoke(
                    "DeferralBehavior", [type], behavior, guardIndex)!);
            return context => Choose(dispatch, context)(context, vertex);
        }
        if (edge != null)
        {
            var dispatch = EventDispatch<Func<Context.Classes.RootContext, Edge, Task<bool>>>(path, "TransitionGuard");
            return context => Choose(dispatch, context)(context, edge);
        }
        else
        {
            var dispatch = EventDispatch<Func<Context.Classes.RootContext, Vertex, Task<bool>>>(path, "DeferralGuard");
            return context => Choose(dispatch, context)(context, vertex);
        }
    }

    private void ConstructDeferral(JToken node, string path, Vertex vertex)
    {
        var logic = new Logic<Interfaces.StateMachinePredicateAsync>(Constants.Deferral);
        if (node["guard"] is { } guard)
        {
            var evaluate = CompileGuard(guard, path + "/guard", vertex, null);
            logic.Actions.Add(c => evaluate(c));
        }
        foreach (var type in triggers[path].SelectMany(Mapped).Distinct())
        {
            var name = Event.GetName(type);
            if (!vertex.Deferrals.TryAdd(name, logic))
                Error(path, $"Event '{name}' is already deferred.", "SM-035");
            else
            {
                vertex.DeferralTypes.Add(name, type);
                vertex.DeferralPaths.Add(name, path);
            }
        }
    }
}
