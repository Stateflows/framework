using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Stateflows.Common.Registration.Builders;
using Stateflows.Actions.Registration;
using Stateflows.Actions.Exceptions;
using Stateflows.Common;

namespace Stateflows.Actions.Models
{
    internal class ActionModel
    {
        internal string ResourceName { get; set; } = null;
        public string Name { get; set; }
        public string BehaviorClassType { get; set; } = BehaviorType.Action;
        public int Version { get; set; }
        
        public BehaviorClass? ParentClass { get; set; }
        public BehaviorClass? OwnerClass { get; set; }
        
        public bool IsStateless { get; set; }
        public ActionDelegateAsync Delegate { get; set; }
        public string Identifier => "Action";
        internal Type ImplementationType { get; set; }
        internal Stateflows.Common.Interfaces.ITypeMapper TypeMapper { get; set; }

        public List<System.Action<object>> ConfigurationActions { get; set; } = [];

        public List<ActionExceptionHandlerFactoryAsync> ExceptionHandlerFactories { get; set; } = [];
        
        public List<ActionInterceptorFactoryAsync> InterceptorFactories { get; set; } = [];
        
        public List<ActionObserverFactoryAsync> ObserverFactories { get; set; } = [];

        public List<Type> ConsumedEventTypes { get; } = [];
        public List<string> ConsumedEventNames { get; } = [];
        public List<Type> ProducedEventTypes { get; } = [];
        public List<Type> ConsumedTokenTypes { get; } = [];
        public List<Type> ProducedTokenTypes { get; } = [];
        internal List<Type> ConsumedBindingOrder { get; } = [];
        private Dictionary<Type, string[]> BindingNames { get; } = [];
        private Dictionary<Type, string[]> ProducedBindingNames { get; } = [];

        internal void FinalizeBindings()
        {
            foreach (var type in ConsumedBindingOrder)
                BindingNames[type] = type == typeof(StructuralEvent) && ConsumedEventNames.Count != 0
                    ? ConsumedEventNames.ToArray()
                    : TypeMapper.GetMappedTypes(type).Select(mapped => mapped.GetEventName()).Distinct().ToArray();
            foreach (var type in ProducedEventTypes.Distinct())
                ProducedBindingNames[type] = TypeMapper.GetMappedTypes(type).Select(mapped => mapped.GetEventName()).Distinct().ToArray();
        }

        internal void Build(StateflowsBuilder stateflowsBuilder)
        {
            TypeMapper = stateflowsBuilder.TypeMapper;
            FinalizeBindings();
            if (stateflowsBuilder.ResourceNames.TryGetValue(ResourceName ?? string.Empty, out var resourceName))
            {
                stateflowsBuilder.ResourcesByBehaviorClass[new BehaviorClass(BehaviorClassType, Name)] = resourceName;
            }

            else
            {
                throw new InvalidOperationException($"Resource group {ResourceName ?? string.Empty} does not exist");
            }

            foreach (var eventType in ConsumedEventTypes.Concat(ProducedEventTypes))
            {
                stateflowsBuilder.Events.RegisterClr(eventType);
            }

            foreach (var eventName in ConsumedEventNames)
            {
                try
                {
                    stateflowsBuilder.Events.GetEvent(eventName);
                }
                catch (ArgumentException e)
                {
                    throw new ActionDefinitionException($"Action '{Name}' consumes event '{eventName}' which is not registered.", new ActionClass(Name), e);
                }
            }
        }

        internal async Task VisitAsync(IActionVisitor visitor)
        {
            var hasDefault = Stateflows.Common.Initializer.BehaviorClassesInitializations.Instance.DefaultInstanceInitializationTokens
                .Any(token => token.BehaviorClass.Type == BehaviorClassType && token.BehaviorClass.Name == Name);
            await visitor.ActionAddingAsync(Identifier, Name, Version, BehaviorClassType, OwnerClass, ParentClass, hasDefault);
            if (ImplementationType != null)
                await (Task)typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.ActionTypeAddedAsync))
                    .MakeGenericMethod(ImplementationType).Invoke(visitor, BindingFlags.DoNotWrapExceptions, null, [Identifier, Name, Version], null);
            var method = typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.CustomEventAddedAsync));
            var order = 0;
            foreach (var type in ConsumedBindingOrder)
            {
                var names = BindingNames[type];
                await (Task)method.MakeGenericMethod(type).Invoke(visitor, BindingFlags.DoNotWrapExceptions, null,
                    [$"{Identifier}/event/{order++}", Name, Version, names, new[] { BehaviorStatus.Initialized }], null);
            }
            await visitor.ActionConfigurationAddedAsync(Identifier, Name, Version, IsStateless);
            order = 0;
            foreach (var type in ProducedEventTypes.Distinct())
                await (Task)typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.ProducedEventAddedAsync))
                    .MakeGenericMethod(type).Invoke(visitor, BindingFlags.DoNotWrapExceptions, null, [$"{Identifier}/produced-event/{order++}", Name, Version,
                        ProducedBindingNames[type]], null);
            order = 0;
            foreach (var type in ConsumedTokenTypes.Distinct())
                await (Task)typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.InputTokenAddedAsync))
                    .MakeGenericMethod(type).Invoke(visitor, BindingFlags.DoNotWrapExceptions, null, [$"{Identifier}/input/{order++}", Name, Version], null);
            order = 0;
            foreach (var type in ProducedTokenTypes.Distinct())
                await (Task)typeof(IActionVisitor).GetMethod(nameof(IActionVisitor.OutputTokenAddedAsync))
                    .MakeGenericMethod(type).Invoke(visitor, BindingFlags.DoNotWrapExceptions, null, [$"{Identifier}/output/{order++}", Name, Version], null);
            await visitor.ActionAddedAsync(Identifier, Name, Version);
        }
    }
}