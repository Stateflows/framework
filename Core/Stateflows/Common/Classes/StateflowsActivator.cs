using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Attributes;
using Stateflows.Common.Context.Classes;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines;
using Stateflows.StateMachines.Attributes;
using Stateflows.StateMachines.Context.Classes;

namespace Stateflows.Common.Classes
{
    /// <summary>
    /// Toolset that instantiates class types used in Stateflows models
    /// </summary>
    public static class StateflowsActivator
    {
        /// <summary>
        /// Creates uninitialized instance of given type. Constructor of type won't be executed!
        /// </summary>
        /// <typeparam name="T">Type of class to be instantiated</typeparam>
        /// <returns>Instance of given class</returns>
        [DebuggerHidden]
        public static T CreateUninitializedInstance<T>()
            => (T)CreateUninitializedInstance(typeof(T));

        /// <summary>
        /// Creates uninitialized instance of given type. Constructor of type won't be executed!
        /// </summary>
        /// <param name="serviceType">Type of class to be instantiated</param>
        /// <returns>Instance of given class</returns>
        [DebuggerHidden]
        public static object CreateUninitializedInstance(Type serviceType)
            => FormatterServices.GetUninitializedObject(serviceType); 
        /// <summary>
        /// Creates instance of class
        /// </summary>
        /// <param name="serviceProvider">Service provider used to resolve dependencies of given type</param>
        /// <typeparam name="T">Type of class to be instantiated</typeparam>
        /// <returns>Instance of given class</returns>
        [DebuggerHidden]
        public static T CreateClassInstance<T>(IServiceProvider serviceProvider)
            => ActivatorUtilities.CreateInstance<T>(serviceProvider);
        
        /// <summary>
        /// Creates instance of class
        /// </summary>
        /// <param name="serviceProvider">Service provider used to resolve dependencies of given type</param>
        /// <param name="serviceType">Type of class to be instantiated</param>
        /// <returns>Instance of given class</returns>
        [DebuggerHidden]
        public static object CreateClassInstance(IServiceProvider serviceProvider, Type serviceType)
            => ActivatorUtilities.CreateInstance(serviceProvider, serviceType);
        
        /// <summary>
        /// Creates instance of Stateflows model element class  
        /// </summary>
        /// <param name="serviceProvider">Service provider used to resolve dependencies of given type</param>
        /// <param name="serviceKind">Name of class kind - for better exceptions ;-)</param>
        /// <typeparam name="T">Type of class to be instantiated</typeparam>
        /// <returns>Instance of given class</returns>
        /// <exception cref="StateflowsDefinitionException">Thrown in case of missing parameter attributes</exception>
        /// <exception cref="StateflowsRuntimeException">Thrown in case of missing required context values</exception>
        [DebuggerHidden]
        public static async Task<T> CreateModelElementInstanceAsync<T>(IServiceProvider serviceProvider, string serviceKind = null)
            => (T)await CreateModelElementInstanceAsync(serviceProvider, typeof(T), serviceKind);
        
        /// <summary>
        /// Creates instance of Stateflows model element class  
        /// </summary>
        /// <param name="serviceProvider">Service provider used to resolve dependencies of given type</param>
        /// <param name="serviceType">Type of class to be instantiated</param>
        /// <param name="serviceKind">Name of class kind - for better exceptions ;-)</param>
        /// <returns>Instance of given class</returns>
        /// <exception cref="StateflowsDefinitionException">Thrown in case of missing parameter attributes</exception>
        /// <exception cref="StateflowsRuntimeException">Thrown in case of missing required context values</exception>
        [DebuggerHidden]
        public static async Task<object> CreateModelElementInstanceAsync(IServiceProvider serviceProvider, Type serviceType, string serviceKind = null)
        {
            serviceKind ??= "service";
            var constructor = serviceType.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault();
            if (constructor == null)
            {
                return null;
            }
            
            var parameters = constructor.GetParameters();
            var parameterValues = new object[parameters.Length];
        
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (parameter.ParameterType.IsImplementerOfRawGeneric(typeof(IValueSet)))
                {
                    parameterValues[i] = await CreateModelElementInstanceAsync(serviceProvider, parameter.ParameterType, "values set");
                    
                    continue;
                }

                if (parameter.ParameterType.IsSubclassOfRawGeneric(typeof(BaseValueAccessor<>)))
                {
                    var customAttribute = parameter.GetCustomAttribute<ValueNameAttribute>();

                    if (customAttribute != null)
                    {
                        // Resolve service by name
                        parameterValues[i] = Activator.CreateInstance(parameter.ParameterType, customAttribute.Name);
                    }
                    else
                    {
                        throw new InvalidOperationException($"ValueNameAttribute not found for parameter {parameter.Name} in constructor of {serviceKind} {serviceType.Name}");
                    }
                    
                    continue;
                }

                if (parameter.ParameterType.IsSubclassOf(typeof(BaseNamespaceAccessor)))
                {
                    var customAttribute = parameter.GetCustomAttribute<ValueSetNameAttribute>();

                    if (customAttribute != null)
                    {
                        // Resolve service by name
                        parameterValues[i] = Activator.CreateInstance(parameter.ParameterType, customAttribute.Name);
                    }
                    else
                    {
                        throw new InvalidOperationException($"ValueSetNameAttribute not found for parameter {parameter.Name} in constructor {constructor.Name} of service {serviceType.Name}");
                    }
                    
                    continue;
                }

                if (parameter.ParameterType == typeof(INamespace))
                {
                    var globalNamespaceAttribute = parameter.GetCustomAttribute<GlobalNamespaceAttribute>();
                    if (globalNamespaceAttribute != null)
                    {
                        var namespaceName = globalNamespaceAttribute.Name ?? parameter.Name;
                        parameterValues[i] = new Namespace(
                            namespaceName,
                            () => ContextValues.AreGlobalValuesAvailable
                                ? ContextValues.GlobalValues
                                : null,
                            nameof(ContextValues.GlobalValues)
                        );
                        continue;
                    }
                    
                    var stateNamespaceAttribute = parameter.GetCustomAttribute<StateNamespaceAttribute>();
                    if (stateNamespaceAttribute != null)
                    {
                        var namespaceName = stateNamespaceAttribute.Name ?? parameter.Name;
                        parameterValues[i] = new Namespace(
                            namespaceName,
                            () => ContextValues.AreStateValuesAvailable
                                ? ContextValues.StateValues
                                : null,
                            nameof(ContextValues.StateValues)
                        );
                        continue;
                    }
                    
                    var sourceStateNamespaceAttribute = parameter.GetCustomAttribute<SourceStateNamespaceAttribute>();
                    if (sourceStateNamespaceAttribute != null)
                    {
                        var namespaceName = sourceStateNamespaceAttribute.Name ?? parameter.Name;
                        parameterValues[i] = new Namespace(
                            namespaceName,
                            () => ContextValues.AreSourceStateValuesAvailable
                                ? ContextValues.SourceStateValues
                                : null,
                            nameof(ContextValues.SourceStateValues)
                        );
                        continue;
                    }
                    
                    var targetStateNamespaceAttribute = parameter.GetCustomAttribute<TargetStateNamespaceAttribute>();
                    if (targetStateNamespaceAttribute != null)
                    {
                        var namespaceName = targetStateNamespaceAttribute.Name ?? parameter.Name;
                        parameterValues[i] = new Namespace(
                            namespaceName,
                            () => ContextValues.AreTargetStateValuesAvailable
                                ? ContextValues.TargetStateValues
                                : null,
                            nameof(ContextValues.TargetStateValues)
                        );
                        continue;
                    }
                    
                    continue;
                }
                
                var globalAttribute = parameter.GetCustomAttribute<GlobalValueAttribute>();
                if (globalAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(
                        parameter,
                        globalAttribute,
                        () => ContextValues.AreGlobalValuesAvailable
                            ? ContextValues.GlobalValues
                            : null,
                        nameof(ContextValues.GlobalValues),
                        serviceType,
                        serviceKind
                    );
                    continue;
                }
                
                var stateAttribute = parameter.GetCustomAttribute<StateValueAttribute>();
                if (stateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(
                        parameter,
                        stateAttribute,
                        () => ContextValues.AreStateValuesAvailable
                            ? ContextValues.StateValues
                            : null,
                        nameof(ContextValues.StateValues),
                        serviceType,
                        serviceKind
                    );
                    continue;
                }
                
                var parentStateAttribute = parameter.GetCustomAttribute<ParentStateValueAttribute>();
                if (parentStateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(
                        parameter,
                        parentStateAttribute,
                        () => ContextValues.AreParentStateValuesAvailable
                            ? ContextValues.ParentStateValues
                            : null,
                        nameof(ContextValues.ParentStateValues),
                        serviceType,
                        serviceKind
                    );
                    continue;
                }
                
                var sourceStateAttribute = parameter.GetCustomAttribute<SourceStateValueAttribute>();
                if (sourceStateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(
                        parameter,
                        sourceStateAttribute,
                        () => ContextValues.AreSourceStateValuesAvailable
                            ? ContextValues.SourceStateValues
                            : null,
                        nameof(ContextValues.SourceStateValues),
                        serviceType,
                        serviceKind
                    );
                    continue;
                }
                
                var targetStateAttribute = parameter.GetCustomAttribute<TargetStateValueAttribute>();
                if (targetStateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(
                        parameter,
                        targetStateAttribute,
                        () => ContextValues.AreTargetStateValuesAvailable
                            ? ContextValues.TargetStateValues
                            : null,
                        nameof(ContextValues.TargetStateValues),
                        serviceType,
                        serviceKind
                    );
                    continue;
                }
                
                parameterValues[i] = serviceProvider.GetRequiredService(parameter.ParameterType);
            }
        
            return Activator.CreateInstance(serviceType, parameterValues);
        }
        
        [DebuggerHidden]
        private static async Task<object> BuildParameterValueAsync(ParameterInfo parameter, ValueAttribute valueAttribute, Func<IContextValues> valueSetSelector, string collectionName, Type serviceType, string serviceKind)
        {
            var valueName = valueAttribute.Name ?? parameter.Name;
            if (parameter.ParameterType.IsSubclassOfRawGeneric(typeof(IValue<>)))
            {
                var accessorType = typeof(Value<>).MakeGenericType(parameter.ParameterType.GetGenericArguments().First());
                return Activator.CreateInstance(
                    accessorType,
                    BindingFlags.Default,
                    null,
                    new object[]
                    {
                        valueName,
                        valueSetSelector,
                        collectionName
                    },
                    null
                );
            }

            var observabilityServiceKinds = new string[]
            {
                "interceptor",
                "exception handler",
                "observer"
            };
            
            if (observabilityServiceKinds.Contains(serviceKind))
            {
                var message = valueName == parameter.Name
                    ? $"Context value '{valueName}' cannot be read - reading values directly is not supported for {serviceKind} (required by {serviceKind} '{serviceType.Name}')"
                    : $"Context value '{valueName}' cannot be read - reading values directly is not supported for {serviceKind} (required by constructor parameter '{parameter.Name}' of {serviceKind} '{serviceType.Name}')";

                throw new InvalidOperationException(message);
            }

            var valueSet = valueSetSelector();
            if (valueSet == null)
            {
                var message = valueName == parameter.Name
                    ? $"Context value '{valueName}' cannot be read - no value set found (required by {serviceKind} '{serviceType.Name}')"
                    : $"Context value '{valueName}' cannot be read - no value set found (required by constructor parameter '{parameter.Name}' of {serviceKind} '{serviceType.Name}')";

                throw new InvalidOperationException(message);
            }
            
            var tryGetMethod = typeof(IContextValues).GetMethod(nameof(IContextValues.TryGetAsync));
            var task = (Task)tryGetMethod!
                .MakeGenericMethod(parameter.ParameterType)
                .Invoke(
                    valueSet,
                    BindingFlags.Default,
                    null,
                    new object[] { valueName },
                    null!
                );

            await task;
            var result = task.GetType().GetProperty(nameof(Task<object>.Result))!.GetValue(task, null);
            var success = (bool)result.GetType().GetField("Item1").GetValue(result);
            if (valueAttribute.Required && !parameter.IsOptional && !success)
            {
                var message = valueName == parameter.Name
                    ? $"Context value '{valueName}' not found (required by {serviceKind} '{serviceType.Name}')"
                    : $"Context value '{valueName}' not found (required by constructor parameter '{parameter.Name}' of {serviceKind} '{serviceType.Name}')";

                throw new InvalidOperationException(message);
            }

            return success
                ? result.GetType().GetField("Item2").GetValue(result)
                : Activator.CreateInstance(parameter.ParameterType);
        }
    }
}
