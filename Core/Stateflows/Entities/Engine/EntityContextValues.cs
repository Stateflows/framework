using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stateflows.Common;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Models;

namespace Stateflows.Entities.Engine;

internal static class EntityContextValues
{
    private static readonly object NullProjectionValue = new();

    private static readonly MethodInfo BehaviorPublishMethod = typeof(IPublishes<IBehaviorContext>)
        .GetMethods()
        .Single(method =>
            method.Name == nameof(IBehaviorContext.Publish) &&
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1
        );
    
    private static readonly MethodInfo ParentPublishMethod = typeof(IPublishes<IParentBehaviorContext>)
        .GetMethods()
        .Single(method =>
            method.Name == nameof(IParentBehaviorContext.Publish) &&
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1
        );
    
    private static readonly MethodInfo OwnerPublishMethod = typeof(IPublishes<IOwnerBehaviorContext>)
        .GetMethods()
        .Single(method =>
            method.Name == nameof(IOwnerBehaviorContext.Publish) &&
            method.IsGenericMethodDefinition &&
            method.GetGenericArguments().Length == 1
        );

    private static string GetProjectionIdentifier(Type projectionType)
        => projectionType.AssemblyQualifiedName ?? projectionType.FullName ?? projectionType.Name;

    internal static IReadOnlyCollection<string> GetFieldDependencies(Dictionary<string, object> values, string fieldName)
        => GetDependencies(values, fieldName.GetFieldDependenciesKey());

    internal static void SetFieldDependencies(Dictionary<string, object> values, string fieldName, IEnumerable<string> dependencies)
        => SetDependencies(values, fieldName.GetFieldDependenciesKey(), dependencies);

    internal static IReadOnlyCollection<string> GetProjectionDependencies(Dictionary<string, object> values, Type projectionType)
        => GetDependencies(values, GetProjectionIdentifier(projectionType).GetProjectionDependenciesKey());

    internal static void SetProjectionDependencies(Dictionary<string, object> values, Type projectionType, IEnumerable<string> dependencies)
        => SetDependencies(values, GetProjectionIdentifier(projectionType).GetProjectionDependenciesKey(), dependencies);

    internal static string GetProjectionValueKey(Type projectionType)
        => GetProjectionIdentifier(projectionType).GetProjectionKey();

    internal static bool TryGetProjectionValue(Dictionary<string, object> values, Type projectionType, out object? projection)
    {
        if (values.TryGetValue(GetProjectionValueKey(projectionType), out var storedProjection))
        {
            projection = ReferenceEquals(storedProjection, NullProjectionValue)
                ? null
                : storedProjection;
            return true;
        }

        projection = null;
        return false;
    }

    internal static void SetProjectionValue(Dictionary<string, object> values, Type projectionType, object? projection)
        => values[GetProjectionValueKey(projectionType)] = projection ?? NullProjectionValue;

    internal static IReadOnlyCollection<string> StabilizeComputedFields(EntityModel model, Dictionary<string, object> values, IEnumerable<string> changedFieldNames)
    {
        var computedFields = model.Fields.Values
            .Where(field => field.IsComputed)
            .ToDictionary(field => field.Name, StringComparer.Ordinal);

        return DependencyStabilizer.Stabilize(
            changedFieldNames,
            computedFields.Keys.ToArray(),
            fieldName => GetFieldDependencies(values, fieldName),
            fieldName =>
            {
                var computedField = computedFields[fieldName];
                var fieldValueKey = fieldName.GetFieldKey();
                var hadOldValue = values.TryGetValue(fieldValueKey, out var oldValue);

                computedField.Compute(values);

                var hasNewValue = values.TryGetValue(fieldValueKey, out var newValue);
                return hadOldValue != hasNewValue || !Equals(oldValue, newValue);
            },
            $"Computed field dependencies for entity '{model.TemplateType.Name}'"
        );
    }

    internal static void RefreshDependentProjections(EntityModel model, Dictionary<string, object> values, BehaviorContext behaviorContext, IEnumerable<string> changedFieldNames)
    {
        var pendingPublishes = DependencyRefreshPlanner.CollectChangedNodes(
            changedFieldNames,
            model.Projections.Values,
            projection => GetProjectionDependencies(values, projection.ProjectionType),
            projection =>
            {
                var hadCachedProjection = TryGetProjectionValue(values, projection.ProjectionType, out var cachedProjection);
                var newProjection = projection.Invoke(values, behaviorContext);

                return
                    projection.PublishScope != PublishScope.None &&
                    hadCachedProjection &&
                    !Equals(cachedProjection, newProjection);
            }
        );

        foreach (var pendingPublish in pendingPublishes)
        {
            PublishProjectionValue(behaviorContext, pendingPublish.ProjectionType, TryGetProjectionValue(values, pendingPublish.ProjectionType, out var projectionValue) ? projectionValue : null, pendingPublish.PublishScope);
        }
    }

    private static void PublishProjectionValue(BehaviorContext behaviorContext, Type projectionType, object? projectionValue, PublishScope publishScope)
    {
        IDictionary<string, EventHeader> headers = new Dictionary<string, EventHeader>();

        if (publishScope.HasFlag(PublishScope.Self))
        {
            BehaviorPublishMethod
                .MakeGenericMethod(projectionType)
                .Invoke(behaviorContext, [projectionValue, headers]);
        }
        
        if (publishScope.HasFlag(PublishScope.Parent) && behaviorContext.Context.ContextParentId != null)
        {
            ParentPublishMethod
                .MakeGenericMethod(projectionType)
                .Invoke(behaviorContext, [projectionValue, headers]);
        }
        
        if (publishScope.HasFlag(PublishScope.Owner) && behaviorContext.Context.ContextOwnerId != null)
        {
            OwnerPublishMethod
                .MakeGenericMethod(projectionType)
                .Invoke(behaviorContext, [projectionValue, headers]);
        }
    }

    private static IReadOnlyCollection<string> GetDependencies(Dictionary<string, object> values, string key)
        => values.TryGetValue(key, out var dependencies)
            ? dependencies switch
            {
                string[] array => array,
                List<string> list => list,
                IEnumerable<string> enumerable => enumerable.Distinct().ToArray(),
                _ => [],
            }
            : [];

    private static void SetDependencies(Dictionary<string, object> values, string key, IEnumerable<string> dependencies)
        => values[key] = dependencies
            .Where(fieldName => !string.IsNullOrWhiteSpace(fieldName))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(fieldName => fieldName, StringComparer.Ordinal)
            .ToArray();
}

