using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stateflows.Actions;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Common.Registration.Builders;
using Stateflows.Common.Validation;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Registration;

namespace Stateflows.StateMachines.Json;

internal sealed partial class BoundDocument(
    JObject document,
    IReadOnlyDictionary<string, Type> types,
    StateflowsBuilder services,
    ModelValidationReport report)
{
    private readonly TypedJson serializer = new(types);
    private readonly Dictionary<string, ClrBinding> bindings = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> names = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Type[]> triggers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, object?> metadata = new(StringComparer.Ordinal);
    private readonly Dictionary<string, BehaviorClass> behaviors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<Type, Type>> eventBindings = new(StringComparer.Ordinal);

    public string Name { get; private set; } = "<document>";
    public int Version => document.Value<int?>("version") ?? 1;

    private void Error(string path, string message, string rule = "JSON-003") => report.Add(rule, path, message);

    private static IEnumerable<(JToken Node, string Path)> Items(JToken parent, string path, string property)
        => parent[property] is JArray array
            ? array.Select((node, index) => (node, $"{path}/{property}/{index}"))
            : [];

    private Type? Resolve(string alias, string path)
    {
        if (types.TryGetValue(alias, out var type)) return type;
        Error(path, $"CLR alias '{alias}' is not registered.");
        return null;
    }

    private string? BindName(JToken node, string path, Type contract)
    {
        if (node.Type == JTokenType.String)
            return names[path] = node.Value<string>()!;
        var type = Resolve(node.Value<string>("clrType")!, path + "/clrType");
        if (type == null) return null;
        if (!type.IsClass || !contract.IsAssignableFrom(type))
        {
            Error(path, $"Name provision requires a class implementing '{contract}'.");
            return null;
        }
        var name = contract == typeof(IStateMachine)
            ? (string)typeof(StateMachine<>).MakeGenericType(type).GetProperty("Name")!.GetValue(null)!
            : State.GetName(type);
        if (string.IsNullOrEmpty(name)) Error(path, "The CLR name provider returned an empty name.");
        return names[path] = name;
    }

    private ClrBinding? BindClr(JToken node, string path, Type contract, bool staticCapability = false,
        bool constructorFree = false)
    {
        var type = Resolve(node.Value<string>("clrType")!, path + "/clrType");
        if (type == null) return null;
        if (!type.IsClass || !contract.IsAssignableFrom(type) ||
            (!staticCapability && type.IsAbstract))
            Error(path, $"Selected capability requires a {(staticCapability ? "" : "nonabstract ")}class implementing '{contract}'.");
        if (!staticCapability && !constructorFree && !type.GetConstructors().Any())
            Error(path, $"Instance capability '{type}' requires a public constructor.");
        PropertyInfo? configurationProperty = null;
        Type? configurationType = null;
        var configuration = node["configuration"];
        if (configuration != null)
        {
            if (staticCapability)
                Error(path + "/configuration", "Static definition and identity capabilities cannot consume instance configuration.");
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
                if (configurationProperty != null)
                    ReadData(path + "/configuration", () => serializer.Read(configuration, configurationType!));
            }
        }
        var binding = new ClrBinding(type, path, serializer, configurationProperty, configurationType, configuration);
        bindings[path] = binding;
        return binding;
    }

    private void ReadData(string path, Func<object?> read)
    {
        try { metadata[path] = read(); }
        catch (JsonException exception) { Error(path, exception.Message, "JSON-004"); }
        catch (ArgumentException exception) { Error(path, exception.Message, "JSON-004"); }
        catch (FormatException exception) { Error(path, exception.Message, "JSON-004"); }
        catch (OverflowException exception) { Error(path, exception.Message, "JSON-004"); }
    }

    private void BindMetadata(JToken parent, string path)
    {
        if (parent["metadata"] is not JObject values) return;
        foreach (var property in values.Properties())
        {
            var valuePath = path + "/metadata/" + JsonStateMachineDefinition.Escape(property.Name);
            ReadData(valuePath, () => serializer.ReadMetadata(property.Value));
        }
    }

    private Type[] Mapped(Type type)
        => services.TypeMapper.GetMappedTypes(type)?.ToArray() ?? [];

    private Type[] BindTriggers(JToken parent, string path, bool completion = false)
    {
        if (completion) return triggers[path] = [typeof(Completion)];
        var declared = new List<Type>();
        var covered = new HashSet<Type>();
        foreach (var (node, itemPath) in Items(parent, path, "triggers"))
        {
            var type = Resolve(node.Value<string>("clrType")!, itemPath + "/clrType");
            if (type == null) continue;
            var actual = Mapped(type);
            if (actual.Length == 0 || actual.Any(t => t == null || t.ContainsGenericParameters ||
                t == typeof(void) || t.IsByRef || t.IsPointer || t.IsByRefLike))
            {
                Error(itemPath, "Mapped triggers must be usable closed CLR event types.");
                continue;
            }
            if (actual.Any(t => !covered.Add(t)))
                Error(itemPath, "Trigger declarations contain duplicate or overlapping mapped CLR types.");
            declared.Add(type);
        }
        if (covered.GroupBy(Event.GetName).Any(g => g.Count() > 1))
            Error(path + "/triggers", "Different trigger types resolve to the same event name.");
        if (covered.Contains(typeof(Completion)) && covered.Count != 1)
            Error(path + "/triggers", "Completion cannot be combined with other triggers.");
        return triggers[path] = declared.ToArray();
    }

    private void BindEventClr(JToken node, string path, Type genericContract, Type[] declared, bool definition = false)
    {
        var type = Resolve(node.Value<string>("clrType")!, path + "/clrType");
        if (type == null) return;
        var actual = declared.SelectMany(Mapped).Distinct().ToArray();
        var choices = new Dictionary<Type, Type>();
        var selected = typeof(object);
        if (definition)
        {
            var candidates = type.GetInterfaces().Where(i => i.IsGenericType &&
                i.GetGenericTypeDefinition() == genericContract).Select(i => i.GenericTypeArguments[0]);
            var eventType = candidates.FirstOrDefault(candidate =>
                actual.All(t => candidate.IsAssignableFrom(t) || Mapped(candidate).Contains(t)));
            if (eventType == null)
                Error(path, "A transition definition must expose one definition contract covering the complete trigger set.");
            else
                eventBindings[path] = actual.ToDictionary(t => t, _ => eventType);
            selected = eventType ?? selected;
        }
        else
        {
            foreach (var trigger in actual)
            {
                var eventType = new[] { trigger }.Concat(declared).FirstOrDefault(candidate =>
                    genericContract.MakeGenericType(candidate).IsAssignableFrom(type) &&
                    (candidate.IsAssignableFrom(trigger) || Mapped(candidate).Contains(trigger)));
                if (eventType == null)
                    Error(path, $"'{type}' does not implement '{genericContract.MakeGenericType(trigger)}' or a compatible declared trigger contract.");
                else choices[trigger] = eventType;
            }
            eventBindings[path] = choices;
            selected = choices.Values.FirstOrDefault() ?? typeof(object);
        }
        BindClr(node, path, genericContract.MakeGenericType(selected), constructorFree: definition);
    }

    private void BindBehavior(JToken node, string path, bool hosted = false, string? vertexKind = null)
    {
        var behavior = node["behaviorClass"]!;
        var behaviorClass = new BehaviorClass(behavior.Value<string>("type")!, behavior.Value<string>("name")!);
        behaviors[path] = behaviorClass;
        if (!hosted && behaviorClass.Type is not (BehaviorType.Action or BehaviorType.Activity))
            Error(path, "Handler behaviors must be actions or activities.");
        if (hosted && vertexKind != "state" && behaviorClass.Type == BehaviorType.StateMachine)
            Error(path, "Regional states support do-actions and do-activities, not submachines.");
    }

    private void BindGuard(JToken node, string path, Type[] declared, bool deferral)
    {
        foreach (var group in new[] { "all", "any" })
            if (node[group] is JArray array)
            {
                for (var i = 0; i < array.Count; i++) BindGuard(array[i], $"{path}/{group}/{i}", declared, deferral);
                return;
            }
        if (node["behaviorClass"] != null) BindBehavior(node, path);
        else BindEventClr(node, path, deferral ? typeof(IDeferralGuard<>) : typeof(ITransitionGuard<>), declared);
    }

    public void Bind()
    {
        Name = BindName(document["name"]!, "/name", typeof(IStateMachine)) ?? "<document>";
        foreach (var (node, path) in Items(document, "", "definitions"))
            BindClr(node, path, typeof(IStateMachine), staticCapability: true);
        BindContainer(document, "");
        foreach (var (node, path) in Items(document, "", "defaultInitializers"))
            BindClr(node, path, typeof(IDefaultInitializer));
        foreach (var (node, path) in Items(document, "", "initializers"))
        {
            var declared = BindTriggers(node, path);
            foreach (var (handler, handlerPath) in Items(node, path, "handlers"))
                BindEventClr(handler, handlerPath, typeof(IInitializer<>), declared);
        }
        foreach (var (property, contract) in new (string, Type)[]
        {
            ("finalizers", typeof(IFinalizer)), ("observers", typeof(IStateMachineObserver)),
            ("interceptors", typeof(IStateMachineInterceptor)), ("exceptionHandlers", typeof(IStateMachineExceptionHandler))
        })
            foreach (var (node, path) in Items(document, "", property))
                BindClr(node, path, contract);
    }

    private void BindContainer(JToken container, string path)
    {
        if (container["initial"] is { } initial) BindName(initial, path + "/initial", typeof(IVertex));
        if (container["history"] is { } history) BindName(history, path + "/history", typeof(IHistory));
        foreach (var (node, vertexPath) in Items(container, path, "vertices"))
        {
            var kind = node.Value<string>("kind")!;
            var identity = kind switch
            {
                "state" => typeof(IState), "composite" => typeof(ICompositeState),
                "orthogonal" => typeof(IOrthogonalState), "final" => typeof(IFinalState),
                "history" => typeof(IHistory), _ => typeof(IVertex)
            };
            BindName(node["name"]!, vertexPath + "/name", identity);
            BindMetadata(node, vertexPath);
            var definition = kind switch
            {
                "state" => typeof(IStateDefinition), "composite" => typeof(ICompositeStateDefinition),
                "orthogonal" => typeof(IOrthogonalStateDefinition), _ => null
            };
            foreach (var (binding, bindingPath) in Items(node, vertexPath, "definitions"))
                BindClr(binding, bindingPath, definition!, staticCapability: true);
            foreach (var (property, contract) in new (string, Type)[]
            {
                ("onEntry", typeof(IStateEntry)), ("onExit", typeof(IStateExit)),
                ("onInitialize", typeof(ICompositeStateInitialization)), ("onFinalize", typeof(ICompositeStateFinalization))
            })
                foreach (var (handler, handlerPath) in Items(node, vertexPath, property))
                    if (handler["behaviorClass"] != null) BindBehavior(handler, handlerPath);
                    else BindClr(handler, handlerPath, contract);
            foreach (var (transition, transitionPath) in Items(node, vertexPath, "transitions"))
            {
                var declared = BindTriggers(transition, transitionPath, transition.Value<string>("kind") == "default");
                if (transition["target"] is { } target) BindName(target, transitionPath + "/target", typeof(IVertex));
                foreach (var (binding, bindingPath) in Items(transition, transitionPath, "definitions"))
                    BindEventClr(binding, bindingPath, typeof(ITransitionDefinition<>), declared, definition: true);
                if (transition["guard"] is { } guard) BindGuard(guard, transitionPath + "/guard", declared, false);
                foreach (var (effect, effectPath) in Items(transition, transitionPath, "effects"))
                    if (effect["behaviorClass"] != null) BindBehavior(effect, effectPath);
                    else BindEventClr(effect, effectPath, typeof(ITransitionEffect<>), declared);
                BindMetadata(transition, transitionPath);
            }
            foreach (var (deferral, deferralPath) in Items(node, vertexPath, "deferrals"))
            {
                var declared = BindTriggers(deferral, deferralPath);
                if (deferral["guard"] is { } guard) BindGuard(guard, deferralPath + "/guard", declared, true);
            }
            if (node["do"] is { } hosted) BindBehavior(hosted, vertexPath + "/do", true, kind);
            if (node["region"] is { } region) BindContainer(region, vertexPath + "/region");
            foreach (var (regionNode, regionPath) in Items(node, vertexPath, "regions")) BindContainer(regionNode, regionPath);
        }
    }
}
