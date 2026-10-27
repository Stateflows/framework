using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Builders;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Activities.Registration.Interfaces.Base;
using Stateflows.Activities.Registration.Interfaces.Internal;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Validation;
using Stateflows.StateMachines.Json;

namespace Stateflows.Activities.Json;

internal sealed partial class BoundActivityDocument(
    JObject document,
    IReadOnlyDictionary<string, Type> types,
    StateflowsBuilder services,
    ModelValidationReport report)
{
    private readonly TypedJson serializer = new(types);
    private readonly Dictionary<string, ClrBinding> bindings = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Type> resolvedTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, string>> targetAliases = new(StringComparer.Ordinal);

    public string Name => document.Value<string>("name") ?? "<document>";
    public int Version => document.Value<int?>("version") ?? 1;

    private void Error(string path, string message, string rule = "JSON-003") => report.Add(rule, path, message);

    private static IEnumerable<(JToken Node, string Path)> Items(JToken parent, string path, string property)
        => parent[property] is JArray array
            ? array.Select((node, index) => (node, $"{path}/{property}/{index}"))
            : [];

    private Type? Resolve(string? alias, string path)
    {
        if (alias != null && types.TryGetValue(alias, out var type))
        {
            resolvedTypes[path] = type;
            return type;
        }
        Error(path, $"CLR alias '{alias}' is not registered.");
        return null;
    }

    private ClrBinding? BindClr(JToken node, string path, Type contract, bool staticCapability = false)
    {
        var type = Resolve(node.Value<string>("clrType"), path + "/clrType");
        if (type == null) return null;
        if (!type.IsClass || type.IsAbstract || !contract.IsAssignableFrom(type))
            Error(path, $"Selected capability requires a nonabstract class implementing '{contract}'.");
        if (!staticCapability && !type.IsAbstract && !type.GetConstructors().Any())
            Error(path, $"Instance capability '{type}' requires a public constructor.");

        PropertyInfo? configurationProperty = null;
        Type? configurationType = null;
        var configuration = node["configuration"];
        if (configuration != null)
        {
            if (staticCapability)
                Error(path + "/configuration", "Static definitions cannot consume instance configuration.");
            else
            {
                var contracts = type.GetInterfaces().Where(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IConfigurable<>)).ToArray();
                if (node.Value<string>("configurationType") is { } alias)
                {
                    configurationType = Resolve(alias, path + "/configurationType");
                    configurationProperty = contracts.FirstOrDefault(c => c.GenericTypeArguments[0] == configurationType)
                        ?.GetProperty("Configuration");
                    if (configurationType != null && configurationProperty == null)
                        Error(path + "/configurationType", $"'{type}' does not implement IConfigurable<{configurationType}>.");
                }
                else if (contracts.Length == 1)
                {
                    configurationProperty = contracts[0].GetProperty("Configuration");
                    configurationType = contracts[0].GenericTypeArguments[0];
                }
                else
                    Error(path + "/configuration", contracts.Length == 0
                        ? $"'{type}' does not implement IConfigurable<T>."
                        : "Multiple configuration contracts require an explicit configurationType alias.");

                if (configurationType != null)
                {
                    try { serializer.Read(configuration, configurationType); }
                    catch (Exception exception) when (exception is JsonException or ArgumentException or FormatException or OverflowException)
                    {
                        Error(path + "/configuration", exception.Message, "JSON-004");
                    }
                }
            }
        }
        var binding = new ClrBinding(type, path, serializer, configurationProperty, configurationType, configuration);
        bindings[path] = binding;
        return binding;
    }

    private Type? BindType(string alias, string path)
        => Resolve(alias, path);

    private static bool HasContract(Type type, Type contract)
        => contract.IsAssignableFrom(type);

    private Type? BindEventNode(JToken node, string path, string kind)
    {
        Type? eventType;
        if (kind == "acceptEvent" && node.Value<string>("eventName") is { } eventName)
        {
            try { services.Events.GetEvent(eventName); }
            catch (ArgumentException exception) { Error(path + "/eventName", exception.Message); }
            resolvedTypes[path + "/eventType"] = eventType = typeof(StructuralEvent);
        }
        else eventType = BindType(node.Value<string>("eventType")!, path + "/eventType");
        if (eventType == null) return null;
        if (services.TypeMapper.GetMappedTypes(eventType).Any() != true)
            Error(path + "/eventType", $"Event type '{eventType}' has no mapped event types.");
        var contract = kind switch
        {
            "acceptEvent" => typeof(IAcceptEventActionNode<>).MakeGenericType(eventType),
            "sendEvent" => typeof(ISendEventActionNode<>).MakeGenericType(eventType),
            "publishEvent" => typeof(IPublishEventActionNode<>).MakeGenericType(eventType),
            _ => typeof(ITimeEventActionNode)
        };
        BindClr(node["implementation"]!, path + "/implementation", contract);
        if (kind == "timeEvent" &&
            (!typeof(TimeEvent).IsAssignableFrom(eventType) ||
             eventType.GetConstructor(Type.EmptyTypes) == null))
            Error(path + "/eventType", "Time event types must derive from TimeEvent and have a public parameterless constructor.");
        return eventType;
    }

    private void BindFlow(JToken flow, string path, string sourceKind)
    {
        var kind = flow.Value<string>("kind")!;
        var target = flow.Value<string>("target")!;
        if (kind.StartsWith("else", StringComparison.Ordinal) && sourceKind != "decision")
            Error(path + "/kind", "Else flows are only supported by decision nodes.");
        if (kind is "control" or "elseControl")
        {
            foreach (var (stage, stagePath) in Items(flow, path, "pipeline"))
            {
                var stageKind = stage.Value<string>("kind");
                if (stageKind != "guard")
                {
                    Error(stagePath + "/kind", "Control-flow pipelines support guards only.");
                    continue;
                }
                var contract = typeof(IControlFlowGuard);
                BindClr(stage["handler"]!, stagePath + "/handler", contract);
            }
        }
        else
        {
            var current = BindType(flow.Value<string>("tokenType")!, path + "/tokenType");
            if (current == null) return;
            var isElse = kind == "elseObject";
            foreach (var (stage, stagePath) in Items(flow, path, "pipeline"))
            {
                if (stage.Value<string>("kind") == "guard")
                {
                    if (isElse)
                    {
                        Error(stagePath + "/kind", "Else object-flow pipelines do not support guards.");
                        continue;
                    }
                    BindClr(stage["handler"]!, stagePath + "/handler", typeof(IFlowGuard<>).MakeGenericType(current));
                }
                else if (stage.Value<string>("kind") == "transform")
                {
                    var output = BindType(stage.Value<string>("outputTokenType")!, stagePath + "/outputTokenType");
                    if (output == null) continue;
                    BindClr(stage["handler"]!, stagePath + "/handler",
                        typeof(IFlowTransformation<,>).MakeGenericType(current, output));
                    current = output;
                }
            }
        }
        if (flow.Value<int?>("weight") < 0)
            Error(path + "/weight", "Flow weights must be nonnegative.");
        _ = target;
    }

    private void BindNodes(JToken parent, string path, bool reactive)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var specialCounts = new Dictionary<string, int>(StringComparer.Ordinal);
        var aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        targetAliases[path] = aliases;
        foreach (var (node, nodePath) in Items(parent, path, "nodes"))
        {
            var kind = node.Value<string>("kind")!;
            var name = node.Value<string>("name");
            if (!reactive && kind is "acceptEvent" or "timeEvent" or "publishEvent")
                Error(nodePath + "/kind", $"Node kind '{kind}' requires a reactive structured builder.", "ACT-036");
            if (node["options"] != null &&
                kind is not ("action" or "structured" or "parallel" or "iterative"))
                Error(nodePath + "/options", $"Node kind '{kind}' does not expose configurable node options.");
            if (kind is "initial" or "input" or "output" or "final")
            {
                specialCounts[kind] = specialCounts.GetValueOrDefault(kind) + 1;
                if (name is { Length: > 0 })
                    aliases[name] = kind switch
                    {
                        "initial" => "InitialNode",
                        "input" => "InputNode",
                        "output" => OutputNode.Name,
                        "final" => FinalNode.Name,
                        _ => name
                    };
            }
            else
            {
                if (name is { Length: > 0 } && !names.Add(name))
                    Error(nodePath + "/name", $"Node name '{name}' is duplicated in this scope.");
                if (name is { Length: > 0 }) aliases[name] = name;
            }

            var allowedFlowKinds = kind switch
            {
                "initial" => new[] { "control" },
                "input" => new[] { "object" },
                "output" or "final" => Array.Empty<string>(),
                "dataStore" => new[] { "object" },
                "sendEvent" or "publishEvent" => new[] { "control" },
                "decision" when node.Value<string>("mode") == "control" => new[] { "control", "elseControl" },
                "decision" => new[] { "object", "elseObject" },
                _ => new[] { "control", "object", "elseControl", "elseObject" }
            };
            foreach (var (flow, flowPath) in Items(node, nodePath, "flows"))
            {
                if (!allowedFlowKinds.Contains(flow.Value<string>("kind"), StringComparer.Ordinal))
                    Error(flowPath + "/kind", $"Flow kind '{flow.Value<string>("kind")}' is not supported by node kind '{kind}'.");
                BindFlow(flow, flowPath, kind);
            }

            if (node["exceptionHandlers"] != null &&
                kind is not ("action" or "acceptEvent" or "timeEvent" or "structured" or "parallel" or "iterative"))
                Error(nodePath + "/exceptionHandlers", $"Node kind '{kind}' does not expose exception handlers.");

            switch (kind)
            {
                case "action":
                    BindClr(node["implementation"]!, nodePath + "/implementation", typeof(IActionNode));
                    break;
                case "acceptEvent":
                case "timeEvent":
                case "sendEvent":
                case "publishEvent":
                    BindEventNode(node, nodePath, kind);
                    break;
                case "decision":
                    if (node.Value<string>("mode") == "object")
                    {
                        var token = BindType(node.Value<string>("tokenType")!, nodePath + "/tokenType");
                        if (token != null)
                            foreach (var (flow, flowPath) in Items(node, nodePath, "flows"))
                                if (flow.Value<string>("kind") is "object" or "elseObject" &&
                                    BindType(flow.Value<string>("tokenType")!, flowPath + "/tokenType") is { } flowToken &&
                                    flowToken != token)
                                    Error(flowPath + "/tokenType", "Decision object flows must use the declared decision token type.");
                    }
                    break;
                case "structured":
                    if (node.Value<bool?>("reactive") == true && !reactive)
                        Error(nodePath + "/reactive", "A reactive structured activity requires a reactive parent builder.");
                    BindStructuredCapabilities(node, nodePath);
                    BindDefinitions(node, nodePath, typeof(IStructuredActivityNodeDefinition), typeof(IStructuredActivityBuilder));
                    BindNodes(node, nodePath, node.Value<bool?>("reactive") == true);
                    break;
                case "parallel":
                case "iterative":
                    BindType(node.Value<string>("iterationTokenType")!, nodePath + "/iterationTokenType");
                    BindStructuredCapabilities(node, nodePath);
                    BindDefinitions(node, nodePath, typeof(IStructuredActivityNodeDefinition), typeof(IStructuredActivityBuilder));
                    BindNodes(node, nodePath, false);
                    break;
            }

            foreach (var (exception, exceptionPath) in Items(node, nodePath, "exceptionHandlers"))
            {
                var exceptionType = BindType(exception.Value<string>("exceptionType")!, exceptionPath + "/exceptionType");
                if (exceptionType == null) continue;
                if (!typeof(Exception).IsAssignableFrom(exceptionType))
                    Error(exceptionPath + "/exceptionType", "Exception handler types must derive from Exception.");
                else
                    BindClr(exception["handler"]!, exceptionPath + "/handler",
                        typeof(IExceptionHandlerNode<>).MakeGenericType(exceptionType));
            }
        }
        foreach (var (kind, count) in specialCounts)
            if (count > 1) Error(path + "/nodes", $"A scope may contain at most one '{kind}' node.", "ACT-007");
    }

    private void BindStructuredCapabilities(JToken node, string path)
    {
        if (node["onInitialize"] != null)
            BindClr(node["onInitialize"]!, path + "/onInitialize", typeof(IStructuredActivityNodeInitialization));
        if (node["onFinalize"] != null)
            BindClr(node["onFinalize"]!, path + "/onFinalize", typeof(IStructuredActivityNodeFinalization));
    }

    private void BindDefinitions(JToken parent, string path, Type capability, Type builderContract)
    {
        foreach (var (definition, definitionPath) in Items(parent, path, "definitions"))
        {
            var binding = BindClr(definition, definitionPath, capability, staticCapability: true);
            if (binding != null)
            {
                var method = binding.Type.GetMethod("Build", BindingFlags.Public | BindingFlags.Static, [builderContract]);
                if (method == null || method.ReturnType != typeof(void))
                    Error(definitionPath, $"Definition type '{binding.Type}' must have static void Build({builderContract.Name}).");
            }
        }
    }

    private void BindInfrastructure()
    {
        BindDefinitions(document, "", typeof(IActivity), typeof(IActivityBuilder));
        BindNodes(document, "", true);

        foreach (var (initializer, path) in Items(document, "", "initializers"))
        {
            var eventType = BindType(initializer.Value<string>("eventType")!, path + "/eventType");
            if (eventType != null) BindClr(initializer["handler"]!, path + "/handler",
                typeof(IInitializer<>).MakeGenericType(eventType));
        }
        BindBindings(document, "", "defaultInitializers", typeof(IDefaultInitializer));
        BindBindings(document, "", "finalizers", typeof(IFinalizer));
        BindBindings(document, "", "observers", typeof(IActivityObserver));
        BindBindings(document, "", "interceptors", typeof(IActivityInterceptor));
        BindBindings(document, "", "exceptionHandlers", typeof(IActivityExceptionHandler));
        if (document["resource"] is { Type: not JTokenType.String })
            Error("/resource", "Resource must be a string.");
    }

    private void BindBindings(JToken parent, string path, string property, Type contract)
    {
        foreach (var (node, bindingPath) in Items(parent, path, property))
            BindClr(node, bindingPath, contract);
    }

    public void Bind()
    {
        BindInfrastructure();
    }

    public void Construct(IActivityBuilder builder)
    {
        foreach (var (definition, path) in Items(document, "", "definitions"))
            ConstructAt(path, () => InvokeDefinition(bindings[path], builder));

        if (document.Value<string>("resource") is { } resource)
        {
            var graph = ((IGraphBuilder)builder).Graph;
            if (graph.ResourceName != null && graph.ResourceName != resource)
            {
                Error("/resource", $"An activity definition already selected resource '{graph.ResourceName}'.", "JSON-005");
                JsonActivityDefinition.ThrowIfErrors(report, Name);
            }
            builder.SetResourceName(resource);
        }

        foreach (var (node, path) in Items(document, "", "nodes"))
            ConstructAt(path, () => ConstructNode(builder, node, path, reactive: true));

        foreach (var (node, path) in Items(document, "", "defaultInitializers"))
        {
            var binding = bindings[path];
            var callback = (Func<IActivityInitializationContext, Task<bool>>)(context =>
                InvokeDefaultInitializer(binding, context));
            builder.AddDefaultInitializer(callback);
        }
        foreach (var (node, path) in Items(document, "", "initializers"))
            ConstructInitializer(builder, node, path);
        foreach (var (node, path) in Items(document, "", "finalizers"))
            ConstructFinalizer(builder, bindings[path]);

        foreach (var (node, path) in Items(document, "", "observers"))
            ConstructAt(path, () => ConstructInfrastructure(builder, bindings[path], "observer"));
        foreach (var (node, path) in Items(document, "", "interceptors"))
            ConstructAt(path, () => ConstructInfrastructure(builder, bindings[path], "interceptor"));
        foreach (var (node, path) in Items(document, "", "exceptionHandlers"))
            ConstructAt(path, () => ConstructInfrastructure(builder, bindings[path], "exceptionHandler"));
    }

    private void ConstructAt(string path, System.Action action)
    {
        try { action(); }
        catch (JsonActivityException) { throw; }
        catch (Exception exception) when (exception is StateflowsDefinitionException or ArgumentException or InvalidOperationException)
        {
            Error(path, exception.Message, "JSON-005");
            throw new JsonActivityException(report, Name, exception);
        }
    }

    private void InvokeDefinition(ClrBinding binding, object builder)
    {
        try
        {
            var contract = builder is IActivityBuilder ? typeof(IActivityBuilder) : typeof(IStructuredActivityBuilder);
            var method = binding.Type.GetMethod("Build", BindingFlags.Public | BindingFlags.Static, [contract])!;
            method.Invoke(null, [builder]);
        }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
        }
    }

    private void ConstructNode(object parent, JToken node, string path, bool reactive)
    {
        var kind = node.Value<string>("kind")!;
        var name = node.Value<string>("name");
        var callback = new Action<object>(child => ConfigureNode(child, node, path, kind, reactive));
        switch (kind)
        {
            case "initial":
                CallGenericInterface(parent, typeof(IInitialBase<>), "AddInitial", [], [CreateCallback(typeof(InitialBuildAction), callback)]);
                break;
            case "input":
                CallGenericInterface(parent, typeof(IInputBase<>), "AddInput", [], [CreateCallback(typeof(InputBuildAction), callback)]);
                break;
            case "output":
                CallGenericInterface(parent, typeof(IOutputBase<>), "AddOutput", [], []);
                break;
            case "final":
                CallGenericInterface(parent, typeof(IFinalBase<>), "AddFinal", [], []);
                break;
            case "action":
            {
                var binding = bindings[path + "/implementation"];
                var actionCallback = CreateCallback(typeof(TypedActionBuildAction<>).MakeGenericType(binding.Type), child =>
                {
                    ConfigureElement(child, binding);
                    callback(child);
                });
                CallGenericInterface(parent, typeof(IActivityActionBase<>), "AddAction", [binding.Type],
                    [name!, actionCallback], methodParameterCount: 2);
                break;
            }
            case "acceptEvent":
            {
                var eventType = resolvedTypes[path + "/eventType"];
                var binding = bindings[path + "/implementation"];
                var actionCallback = CreateCallback(typeof(AcceptEventActionBuildAction<>).MakeGenericType(eventType), callback);
                var eventAction = CreateBoundDelegate(
                    typeof(AcceptEventActionDelegateAsync<>).MakeGenericType(eventType),
                    nameof(HandleAcceptedEvent), eventType, binding.Type, binding);
                if (node.Value<string>("eventName") is { } eventName)
                    CallGenericInterface(parent, typeof(IAcceptEventBase<>), "AddAcceptEventAction",
                        [], [eventName, name!, eventAction, actionCallback], methodParameterCount: 4);
                else
                    CallGenericInterface(parent, typeof(IAcceptEventBase<>), "AddAcceptEventAction",
                        [eventType], [name!, eventAction, actionCallback], methodParameterCount: 3);
                break;
            }
            case "timeEvent":
            {
                var eventType = resolvedTypes[path + "/eventType"];
                var binding = bindings[path + "/implementation"];
                var actionCallback = CreateCallback(typeof(TimeEventNodeBuildAction<>).MakeGenericType(binding.Type),
                    child => { ConfigureElement(child, binding); callback(child); });
                CallGenericInterface(parent, typeof(IAcceptEventBase<>), "AddTimeEventAction",
                    [eventType, binding.Type], [name!, actionCallback], methodParameterCount: 2);
                break;
            }
            case "sendEvent":
            {
                var eventType = resolvedTypes[path + "/eventType"];
                var binding = bindings[path + "/implementation"];
                var actionCallback = CreateCallback(typeof(SendEventActionBuildAction), callback);
                var generate = CreateBoundDelegate(typeof(SendEventActionDelegateAsync<>).MakeGenericType(eventType),
                    nameof(GenerateEvent), eventType, binding.Type, binding);
                var selectTarget = CreateBoundDelegate(typeof(BehaviorIdSelectorAsync), nameof(SelectEventTarget),
                    eventType, binding.Type, binding);
                CallGenericInterface(parent, typeof(ISendEventBase<>), "AddSendEventAction",
                    [eventType], [name!, generate, selectTarget, actionCallback], methodParameterCount: 4);
                break;
            }
            case "publishEvent":
            {
                var eventType = resolvedTypes[path + "/eventType"];
                var binding = bindings[path + "/implementation"];
                var actionCallback = CreateCallback(typeof(PublishEventActionBuildAction), callback);
                var generate = CreateBoundDelegate(typeof(PublishEventActionDelegateAsync<>).MakeGenericType(eventType),
                    nameof(GeneratePublishEvent), eventType, binding.Type, binding);
                CallGenericInterface(parent, typeof(IPublishEventBase<>), "AddPublishEventAction",
                    [eventType], [name!, generate, actionCallback], methodParameterCount: 3);
                break;
            }
            case "decision":
                ConstructDecision(parent, node, path, name!, callback);
                break;
            case "fork":
                CallGenericInterface(parent, typeof(IActivitySpecials<>), "AddFork", [],
                    [name!, CreateCallback(typeof(ForkBuildAction), callback)], methodParameterCount: 2);
                break;
            case "join":
                CallGenericInterface(parent, typeof(IActivitySpecials<>), "AddJoin", [],
                    [name!, CreateCallback(typeof(JoinBuildAction), callback)], methodParameterCount: 2);
                break;
            case "merge":
                CallGenericInterface(parent, typeof(IActivitySpecials<>), "AddMerge", [],
                    [name!, CreateCallback(typeof(MergeBuildAction), callback)], methodParameterCount: 2);
                break;
            case "dataStore":
                CallGenericInterface(parent, typeof(IActivitySpecials<>), "AddDataStore", [],
                    [name!, CreateCallback(typeof(DataStoreBuildAction), callback)], methodParameterCount: 2);
                break;
            case "structured":
                ConstructStructured(parent, node, path, name!, callback, reactive);
                break;
            case "parallel":
            case "iterative":
                ConstructParallelOrIterative(parent, node, path, name!, callback, kind);
                break;
            default:
                throw new InvalidOperationException($"Unsupported activity node kind '{kind}' at '{path}'.");
        }
    }

    private void ConstructDecision(object parent, JToken node, string path, string name, Action<object> callback)
    {
        if (node.Value<string>("mode") == "control")
        {
            CallGenericInterface(parent, typeof(IActivitySpecials<>), "AddControlDecision", [],
                [name, CreateCallback(typeof(DecisionBuildAction), callback)], methodParameterCount: 2);
        }
        else
        {
            var token = resolvedTypes[path + "/tokenType"];
            CallGenericInterface(parent, typeof(IActivitySpecials<>), "AddDecision", [token],
                [name, CreateCallback(typeof(DecisionBuildAction<>).MakeGenericType(token), callback)], methodParameterCount: 2);
        }
    }

    private void ConstructStructured(object parent, JToken node, string path, string name, Action<object> callback, bool parentReactive)
    {
        if (parentReactive)
        {
            CallGenericInterface(parent, typeof(IReactiveActivityBase<>), "AddStructuredActivity", [],
                [name, CreateCallback(typeof(ReactiveStructuredActivityBuildAction), callback)], methodParameterCount: 2);
        }
        else
        {
            CallGenericInterface(parent, typeof(IActivityBase<>), "AddStructuredActivity", [],
                [name, CreateCallback(typeof(StructuredActivityBuildAction), callback)], methodParameterCount: 2);
        }
    }

    private void ConstructParallelOrIterative(object parent, JToken node, string path, string name, Action<object> callback, string kind)
    {
        var token = resolvedTypes[path + "/iterationTokenType"];
        var chunk = node.Value<int?>("chunkSize") ?? 1;
        var delegateType = kind == "parallel" ? typeof(ParallelActivityBuildAction) : typeof(IterativeActivityBuildAction);
        var activityBase = HasOpenInterface(parent, typeof(IReactiveActivityBase<>))
            ? typeof(IReactiveActivityBase<>)
            : typeof(IActivityBase<>);
        CallGenericInterface(parent, activityBase, kind == "parallel" ? "AddParallelActivity" : "AddIterativeActivity",
            [token], [name, CreateCallback(delegateType, callback), chunk], methodParameterCount: 3);
    }

    private void ConfigureNode(object nodeBuilder, JToken node, string path, string kind, bool reactive)
    {
        if (kind is "structured" or "parallel" or "iterative")
            foreach (var (definition, definitionPath) in Items(node, path, "definitions"))
                InvokeDefinition(bindings[definitionPath], nodeBuilder);

        ApplyOptions(nodeBuilder, node);
        var aliases = targetAliases.TryGetValue(path[..Math.Max(0, path.LastIndexOf("/nodes/", StringComparison.Ordinal))], out var found)
            ? found
            : targetAliases.GetValueOrDefault("");
        foreach (var (flow, flowPath) in Items(node, path, "flows"))
            ConstructFlow(nodeBuilder, flow, flowPath, aliases ?? new Dictionary<string, string>(), kind);
        foreach (var (exception, exceptionPath) in Items(node, path, "exceptionHandlers"))
            ConstructExceptionHandler(nodeBuilder, exception, exceptionPath);

        if (kind is "structured" or "parallel" or "iterative")
        {
            foreach (var property in new[] { "onInitialize", "onFinalize" })
                if (node[property] != null)
                    AddStructuredLifecycle(nodeBuilder, bindings[path + "/" + property], property);

            var childReactive = kind == "structured" && node.Value<bool?>("reactive") == true;
            foreach (var (child, childPath) in Items(node, path, "nodes"))
                ConstructNode(nodeBuilder, child, childPath, childReactive);
        }
        _ = reactive;
    }

    private void ApplyOptions(object nodeBuilder, JToken node)
    {
        if (node["options"] is not JArray options) return;
        var flags = NodeOptions.None;
        foreach (var option in options.Values<string>())
            flags |= option switch
            {
                "implicitJoin" => NodeOptions.ImplicitJoin,
                "implicitFork" => NodeOptions.ImplicitFork,
                _ => NodeOptions.None
            };
        CallGenericInterface(nodeBuilder, typeof(INodeOptions<>), "SetOptions", [], [flags], methodParameterCount: 1);
    }

    private void ConstructExceptionHandler(object builder, JToken exception, string path)
    {
        var exceptionType = resolvedTypes[path + "/exceptionType"];
        var binding = bindings[path + "/handler"];
        var callback = CreateExceptionHandlerDelegate(exceptionType, binding);
        CallGenericInterface(builder, typeof(IExceptionHandlerBase<>), "AddExceptionHandler", [exceptionType],
            [callback], methodParameterCount: 1);
    }

    private Delegate CreateExceptionHandlerDelegate(Type exceptionType, ClrBinding binding)
    {
        var delegateType = typeof(ExceptionHandlerDelegateAsync<>).MakeGenericType(exceptionType);
        var invoke = delegateType.GetMethod("Invoke")!;
        var contextParameter = Expression.Parameter(invoke.GetParameters()[0].ParameterType, "context");
        var method = typeof(BoundActivityDocument).GetMethod(nameof(InvokeExceptionHandler),
            BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(exceptionType, binding.Type);
        var call = Expression.Call(method, Expression.Constant(binding), contextParameter);
        return Expression.Lambda(delegateType, call, contextParameter).Compile();
    }

    private static async Task InvokeExceptionHandler<TException, THandler>(ClrBinding binding,
        IExceptionHandlerContext<TException> context)
        where TException : Exception
        where THandler : class, IExceptionHandlerNode<TException>
    {
        var handler = await ((BaseContext)context).NodeScope.GetExceptionHandlerAsync<TException, THandler>(context);
        binding.Configure(handler);
        var previousOutput = OutputTokens.TokensHolder.Value;
        var previousContext = ActivityNodeContextAccessor.Context.Value;
        OutputTokens.TokensHolder.Value = ((ExceptionHandlerContext<TException>)context).OutputTokens;
        ActivityNodeContextAccessor.Context.Value = context;
        try { await handler.HandleAsync(context.Exception, context.CancellationToken); }
        finally
        {
            OutputTokens.TokensHolder.Value = previousOutput;
            ActivityNodeContextAccessor.Context.Value = previousContext;
        }
    }

    private void ConstructFlow(object nodeBuilder, JToken flow, string path, IReadOnlyDictionary<string, string> aliases,
        string sourceKind)
    {
        var target = flow.Value<string>("target")!;
        if (aliases.TryGetValue(target, out var resolvedTarget)) target = resolvedTarget;
        var kind = flow.Value<string>("kind")!;
        var isObject = kind is "object" or "elseObject";
        var isElse = kind.StartsWith("else", StringComparison.Ordinal);
        var weight = flow.Value<int?>("weight") ?? 1;
        if (isObject)
        {
            var token = resolvedTypes[path + "/tokenType"];
            var callbackType = (isElse ? typeof(ElseObjectFlowBuildAction<>) : typeof(ObjectFlowBuildAction<>))
                .MakeGenericType(token);
            var callback = CreateCallback(callbackType, flowBuilder => ConfigureObjectFlow(flowBuilder, flow, path, token, weight, isElse));
            if (sourceKind == "decision")
            {
                var flowInterface = isElse
                    ? typeof(IElseDecisionFlowBase<,>)
                    : typeof(IDecisionFlowBase<,>);
                CallGenericInterface(nodeBuilder, flowInterface, isElse ? "AddElseFlow" : "AddFlow", [],
                    [target, callback], methodParameterCount: 2);
            }
            else if (isElse)
                CallGenericInterface(nodeBuilder, typeof(IElseObjectFlowBase<>), "AddElseFlow", [token],
                    [target, callback], methodParameterCount: 2);
            else
                CallObjectFlow(nodeBuilder, token, target, callback);
        }
        else
        {
            var delegateType = isElse ? typeof(ElseControlFlowBuildAction) : typeof(ControlFlowBuildAction);
            var callback = CreateCallback(delegateType, flowBuilder => ConfigureControlFlow(flowBuilder, flow, path, weight));
            if (sourceKind == "decision")
            {
                var flowInterface = isElse ? typeof(IElseDecisionFlowBase<>) : typeof(IDecisionFlowBase<>);
                CallGenericInterface(nodeBuilder, flowInterface, isElse ? "AddElseFlow" : "AddFlow", [],
                    [target, callback], methodParameterCount: 2);
            }
            else if (isElse)
                CallGenericInterface(nodeBuilder, typeof(IElseControlFlowBase<>), "AddElseControlFlow", [], [target, callback], methodParameterCount: 2);
            else
                CallControlFlow(nodeBuilder, target, callback);
        }
    }

    private void CallObjectFlow(object builder, Type token, string target, Delegate callback)
    {
        if (HasOpenInterface(builder, typeof(IObjectFlowBase<>)))
            CallGenericInterface(builder, typeof(IObjectFlowBase<>), "AddFlow", [token], [target, callback], methodParameterCount: 2);
        else
            CallNonGenericInterface(builder, typeof(IObjectFlowBase), "AddFlow", [token], [target, callback], methodParameterCount: 2);
    }

    private void CallControlFlow(object builder, string target, Delegate callback)
    {
        if (HasOpenInterface(builder, typeof(IControlFlowBase<>)))
            CallGenericInterface(builder, typeof(IControlFlowBase<>), "AddControlFlow", [], [target, callback], methodParameterCount: 2);
        else
            CallNonGenericInterface(builder, typeof(IControlFlowBase), "AddControlFlow", [], [target, callback], methodParameterCount: 2);
    }

    private void ConfigureObjectFlow(object flowBuilder, JToken flow, string path, Type initialToken, int weight, bool isElse)
    {
        var currentToken = initialToken;
        foreach (var (stage, stagePath) in Items(flow, path, "pipeline"))
        {
            var binding = bindings[stagePath + "/handler"];
            if (stage.Value<string>("kind") == "guard")
            {
                var callback = CreateObjectGuard(currentToken, binding);
                CallObjectGuard(flowBuilder, currentToken, callback);
            }
            else
            {
                var output = resolvedTypes[stagePath + "/outputTokenType"];
                var callback = CreateObjectTransformation(currentToken, output, binding);
                flowBuilder = CallTransformation(flowBuilder, currentToken, output, callback, isElse);
                currentToken = output;
            }
        }
        SetWeight(flowBuilder, weight);
    }

    private void ConfigureControlFlow(object flowBuilder, JToken flow, string path, int weight)
    {
        foreach (var (stage, stagePath) in Items(flow, path, "pipeline"))
        {
            var binding = bindings[stagePath + "/handler"];
            var callback = CreateControlGuard(binding);
            CallControlGuard(flowBuilder, callback);
        }
        SetWeight(flowBuilder, weight);
    }

    private static Delegate CreateCallback(Type delegateType, Action<object> callback)
    {
        var invoke = delegateType.GetMethod("Invoke")!;
        var parameters = invoke.GetParameters().Select(parameter =>
            Expression.Parameter(parameter.ParameterType, parameter.Name)).ToArray();
        var call = Expression.Call(Expression.Constant(callback), typeof(Action<object>).GetMethod("Invoke")!,
            Expression.Convert(parameters[0], typeof(object)));
        return Expression.Lambda(delegateType, call, parameters).Compile();
    }

    private static Delegate CreateObjectGuard(Type token, ClrBinding binding)
    {
        var method = typeof(BoundActivityDocument).GetMethod(nameof(ObjectGuard), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(token, binding.Type);
        return Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(
            typeof(IGuardContext<>).MakeGenericType(token), typeof(Task<bool>)), binding, method);
    }

    private static async Task<bool> ObjectGuard<TToken, TGuard>(ClrBinding binding, IGuardContext<TToken> context)
        where TGuard : class, IFlowGuard<TToken>
    {
        var guard = await ((BaseContext)context).NodeScope.GetObjectFlowAsync<TGuard, TToken>(context);
        binding.Configure(guard);
        return await guard.GuardAsync(context.Token);
    }

    private static Delegate CreateControlGuard(ClrBinding binding)
    {
        var method = typeof(BoundActivityDocument).GetMethod(nameof(ControlGuard), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(binding.Type);
        return Delegate.CreateDelegate(typeof(Func<IGuardContext, Task<bool>>), binding, method);
    }

    private static async Task<bool> ControlGuard<TGuard>(ClrBinding binding, IGuardContext context)
        where TGuard : class, IControlFlowGuard
    {
        var guard = await ((BaseContext)context).NodeScope.GetControlFlowAsync<TGuard>(context);
        binding.Configure(guard);
        return await guard.GuardAsync();
    }

    private static Delegate CreateObjectTransformation(Type input, Type output, ClrBinding binding)
    {
        var method = typeof(BoundActivityDocument).GetMethod(nameof(ObjectTransformation),
            BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(input, output, binding.Type);
        return Delegate.CreateDelegate(typeof(TransformationDelegateAsync<,>).MakeGenericType(input, output), binding, method);
    }

    private static async Task<TOutput> ObjectTransformation<TInput, TOutput, TTransformation>(
        ClrBinding binding, ITransformationContext<TInput> context)
        where TTransformation : class, IFlowTransformation<TInput, TOutput>
    {
        var flow = await ((BaseContext)context).NodeScope.GetObjectTransformationFlowAsync<TTransformation, TInput, TOutput>(context);
        binding.Configure(flow);
        return await flow.TransformAsync(context.Token);
    }

    private static void CallObjectGuard(object flowBuilder, Type token, Delegate callback)
    {
        var interfaceType = flowBuilder.GetType().GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IObjectFlowGuardBuilderBase<,>) &&
                        i.GenericTypeArguments[0] == token);
        var method = interfaceType.GetMethods().First(m => m.Name == "AddGuard" && !m.IsGenericMethod);
        method.Invoke(flowBuilder, [CreateDelegateArray(method, callback)]);
    }

    private static object CallTransformation(object flowBuilder, Type input, Type output, Delegate callback, bool isElse)
    {
        var generic = isElse ? typeof(IElseObjectFlowBuilder<>) : typeof(IObjectFlowBuilder<>);
        var interfaceType = flowBuilder.GetType().GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == generic && i.GenericTypeArguments[0] == input);
        var method = interfaceType.GetMethods().First(m => m.Name == "AddTransformation" &&
            m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
        return method.MakeGenericMethod(output).Invoke(flowBuilder, [callback])!;
    }

    private static void CallControlGuard(object flowBuilder, Delegate callback)
    {
        var interfaceType = flowBuilder.GetType().GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IControlFlowBuilderBase<>));
        var method = interfaceType.GetMethods().First(m => m.Name == "AddGuard" && !m.IsGenericMethod);
        method.Invoke(flowBuilder, [CreateDelegateArray(method, callback)]);
    }

    private static Array CreateDelegateArray(MethodInfo method, Delegate callback)
    {
        var itemType = method.GetParameters()[0].ParameterType.GetElementType()!;
        var items = Array.CreateInstance(itemType, 1);
        items.SetValue(callback, 0);
        return items;
    }

    private static void SetWeight(object flowBuilder, int weight)
    {
        var interfaceType = flowBuilder.GetType().GetInterfaces().FirstOrDefault(i => i == typeof(IFlowWeight));
        if (interfaceType != null)
        {
            interfaceType.GetMethod("SetWeight")!.Invoke(flowBuilder, [weight]);
            return;
        }
        var generic = flowBuilder.GetType().GetInterfaces().First(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IFlowWeight<>));
        generic.GetMethod("SetWeight")!.Invoke(flowBuilder, [weight]);
    }

    private static bool HasOpenInterface(object target, Type openInterface)
        => target.GetType().GetInterfaces().Any(i => openInterface.IsGenericTypeDefinition
            ? i.IsGenericType && i.GetGenericTypeDefinition() == openInterface
            : i == openInterface);

    private static object? CallGenericInterface(object target, Type openInterface, string methodName, Type[] typeArguments,
        object?[] arguments, int? methodParameterCount = null)
    {
        var interfaceType = target.GetType().GetInterfaces()
            .First(i => openInterface.IsGenericTypeDefinition
                ? i.IsGenericType && i.GetGenericTypeDefinition() == openInterface
                : i == openInterface);
        return InvokeInterface(target, interfaceType, methodName, typeArguments, arguments, methodParameterCount);
    }

    private static object? CallNonGenericInterface(object target, Type interfaceType, string methodName, Type[] typeArguments,
        object?[] arguments, int? methodParameterCount = null)
        => InvokeInterface(target, interfaceType, methodName, typeArguments, arguments, methodParameterCount);

    private static object? InvokeInterface(object target, Type interfaceType, string methodName, Type[] typeArguments,
        object?[] arguments, int? methodParameterCount)
    {
        var method = interfaceType.GetMethods().First(m => m.Name == methodName &&
            (methodParameterCount == null || m.GetParameters().Length == methodParameterCount) &&
            m.IsGenericMethodDefinition == (typeArguments.Length > 0) &&
            (!m.IsGenericMethodDefinition || m.GetGenericArguments().Length == typeArguments.Length));
        if (method.IsGenericMethodDefinition) method = method.MakeGenericMethod(typeArguments);
        try { return method.Invoke(target, arguments); }
        catch (TargetInvocationException exception) when (exception.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static void ConfigureElement(object typedBuilder, ClrBinding binding)
    {
        var elementBuilder = typedBuilder.GetType().GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IElementBuilderBase<,>) &&
                        i.GenericTypeArguments[0] == binding.Type);
        var delegateType = typeof(Action<>).MakeGenericType(binding.Type);
        var parameter = Expression.Parameter(binding.Type, "element");
        var apply = typeof(BoundActivityDocument).GetMethod(nameof(ApplyBinding), BindingFlags.NonPublic | BindingFlags.Static)!;
        var body = Expression.Call(apply, Expression.Constant(binding), Expression.Convert(parameter, typeof(object)));
        var callback = Expression.Lambda(delegateType, body, parameter).Compile();
        elementBuilder.GetMethod("Configure")!.Invoke(typedBuilder, [callback]);
    }

    private static void ApplyBinding(ClrBinding binding, object instance) => binding.Configure(instance);

    private static Delegate CreateBoundDelegate(Type delegateType, string helperName, Type eventType, Type nodeType,
        ClrBinding binding)
    {
        var helper = typeof(BoundActivityDocument).GetMethod(helperName, BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(eventType, nodeType);
        return Delegate.CreateDelegate(delegateType, binding, helper);
    }

    private static async Task<TEvent> GenerateEvent<TEvent, TNode>(ClrBinding binding, IActionContext context)
        where TNode : class, ISendEventActionNode<TEvent>
    {
        var action = await ((BaseContext)context).NodeScope.GetSendEventActionAsync<TEvent, TNode>(context);
        binding.Configure(action);
        return await action.GenerateEventAsync();
    }

    private static async Task HandleAcceptedEvent<TEvent, TNode>(ClrBinding binding,
        IAcceptEventActionContext<TEvent> context)
        where TNode : class, IAcceptEventActionNode<TEvent>
    {
        var action = await ((BaseContext)context).NodeScope.GetAcceptEventActionAsync<TEvent, TNode>(context);
        binding.Configure(action);
        var previousInput = InputTokens.TokensHolder.Value;
        var previousOutput = OutputTokens.TokensHolder.Value;
        var previousContext = ActivityNodeContextAccessor.Context.Value;
        InputTokens.TokensHolder.Value = ((ActionContext)context).InputTokens;
        OutputTokens.TokensHolder.Value = ((ActionContext)context).OutputTokens;
        ActivityNodeContextAccessor.Context.Value = context;
        try { await action.ExecuteAsync(context.Event, context.CancellationToken); }
        finally
        {
            InputTokens.TokensHolder.Value = previousInput;
            OutputTokens.TokensHolder.Value = previousOutput;
            ActivityNodeContextAccessor.Context.Value = previousContext;
        }
    }

    private static async Task<BehaviorId> SelectEventTarget<TEvent, TNode>(ClrBinding binding, IActionContext context)
        where TNode : class, ISendEventActionNode<TEvent>
    {
        var action = await ((BaseContext)context).NodeScope.GetSendEventActionAsync<TEvent, TNode>(context);
        binding.Configure(action);
        return await action.SelectTargetAsync();
    }

    private static async Task<TEvent> GeneratePublishEvent<TEvent, TNode>(ClrBinding binding, IActionContext context)
        where TNode : class, IPublishEventActionNode<TEvent>
    {
        var action = await ((BaseContext)context).NodeScope.GetPublishEventActionAsync<TEvent, TNode>(context);
        binding.Configure(action);
        return await action.GenerateEventAsync();
    }

    private void AddStructuredLifecycle(object builder, ClrBinding binding, string property)
    {
        var methodName = property == "onInitialize" ? "AddOnInitialize" : "AddOnFinalize";
        var delegateType = typeof(Func<IActivityNodeContext, Task>);
        var parameter = Expression.Parameter(typeof(IActivityNodeContext), "context");
        var helper = typeof(BoundActivityDocument).GetMethod(nameof(InvokeStructuredLifecycle),
            BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(binding.Type);
        var call = Expression.Call(helper, Expression.Constant(binding), Expression.Constant(property), parameter);
        var callback = Expression.Lambda(delegateType, call, parameter).Compile();
        CallGenericInterface(builder, typeof(IStructuredActivityEvents<>), methodName, [], [callback], methodParameterCount: 1);
    }

    private static async Task InvokeStructuredLifecycle<TStructured>(ClrBinding binding, string property,
        IActivityNodeContext context)
        where TStructured : class, IStructuredActivityNode
    {
        var instance = await ((BaseContext)context).NodeScope.GetStructuredActivityAsync<TStructured>(context);
        binding.Configure(instance);
        var previousContext = ActivityNodeContextAccessor.Context.Value;
        ActivityNodeContextAccessor.Context.Value = context;
        try
        {
            if (property == "onInitialize")
                await ((IStructuredActivityNodeInitialization)instance).OnInitializeAsync();
            else
                await ((IStructuredActivityNodeFinalization)instance).OnFinalizeAsync();
        }
        finally { ActivityNodeContextAccessor.Context.Value = previousContext; }
    }

    private static async Task<bool> InvokeDefaultInitializer(ClrBinding binding, IActivityInitializationContext context)
    {
        var method = typeof(BoundActivityDocument).GetMethod(nameof(DefaultInitializer), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(binding.Type);
        return await (Task<bool>)method.Invoke(null, [binding, context])!;
    }

    private static async Task<bool> DefaultInitializer<TInitializer>(ClrBinding binding, IActivityInitializationContext context)
        where TInitializer : class, IDefaultInitializer
    {
        var initializer = await ((BaseContext)context).NodeScope.GetDefaultInitializerAsync<TInitializer>(context);
        binding.Configure(initializer);
        return await initializer.OnInitializeAsync();
    }

    private void ConstructInitializer(IActivityBuilder builder, JToken initializer, string path)
    {
        var eventType = resolvedTypes[path + "/eventType"];
        var binding = bindings[path + "/handler"];
        var delegateType = typeof(Func<,>).MakeGenericType(
            typeof(IActivityInitializationContext<>).MakeGenericType(eventType), typeof(Task<bool>));
        var method = typeof(BoundActivityDocument).GetMethod(nameof(Initializer), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(eventType, binding.Type);
        var callback = Delegate.CreateDelegate(delegateType, binding, method);
        CallGenericInterface(builder, typeof(IActivityEvents<>), "AddInitializer", [eventType], [callback], methodParameterCount: 1);
    }

    private static async Task<bool> Initializer<TEvent, TInitializer>(ClrBinding binding,
        IActivityInitializationContext<TEvent> context)
        where TInitializer : class, IInitializer<TEvent>
    {
        var initializer = await ((BaseContext)context).NodeScope.GetInitializerAsync<TInitializer, TEvent>(context);
        binding.Configure(initializer);
        return await initializer.OnInitializeAsync(context.InitializationEvent);
    }

    private void ConstructFinalizer(IActivityBuilder builder, ClrBinding binding)
    {
        var callback = (Func<IActivityActionContext, Task>)(context => InvokeFinalizer(binding, context));
        builder.AddFinalizer(callback);
    }

    private static async Task InvokeFinalizer(ClrBinding binding, IActivityActionContext context)
    {
        var method = typeof(BoundActivityDocument).GetMethod(nameof(Finalizer), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(binding.Type);
        await (Task)method.Invoke(null, [binding, context])!;
    }

    private static async Task Finalizer<TFinalizer>(ClrBinding binding, IActivityActionContext context)
        where TFinalizer : class, IFinalizer
    {
        var finalizer = await ((BaseContext)context).NodeScope.GetFinalizerAsync<TFinalizer>(context);
        binding.Configure(finalizer);
        await finalizer.OnFinalizeAsync();
    }

    private void ConstructInfrastructure(IActivityBuilder builder, ClrBinding binding, string kind)
    {
        var factory = new Func<IServiceProvider, Task<object>>(provider => CreateConfigured(binding, provider));
        switch (kind)
        {
            case "observer":
                builder.AddObserver(async provider => (IActivityObserver)await factory(provider));
                break;
            case "interceptor":
                builder.AddInterceptor(async provider => (IActivityInterceptor)await factory(provider));
                break;
            case "exceptionHandler":
                builder.AddExceptionHandler(async provider => (IActivityExceptionHandler)await factory(provider));
                break;
        }
    }

    private static Task<object> CreateConfigured(ClrBinding binding, IServiceProvider provider)
    {
        var instance = ActivatorUtilities.CreateInstance(provider, binding.Type);
        binding.Configure(instance);
        return Task.FromResult(instance);
    }
}
