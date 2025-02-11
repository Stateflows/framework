using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common.Attributes;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Extensions;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Attributes;

namespace Stateflows.Common.Classes
{
    public class StateflowsActivator
    {
        [DebuggerHidden]
        public static T CreateUninitializedInstance<T>()
            => (T)CreateUninitializedInstance(typeof(T));

        [DebuggerHidden]
        public static object CreateUninitializedInstance(Type serviceType)
            => FormatterServices.GetUninitializedObject(serviceType); 

        [DebuggerHidden]
        public static async Task<T> CreateInstanceAsync<T>(IServiceProvider serviceProvider)
            => (T)await CreateInstanceAsync(serviceProvider, typeof(T));
        
        [DebuggerHidden]
        public static async Task<object> CreateInstanceAsync(IServiceProvider serviceProvider, Type serviceType)
        {
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
                        throw new StateflowsDefinitionException(
                            $"ValueNameAttribute not found for parameter {parameter.Name} in constructor {constructor.Name} of service {serviceType.Name}");
                    }
                    
                    continue;
                }

                if (parameter.ParameterType.IsSubclassOf(typeof(BaseValueSetAccessor)))
                {
                    var customAttribute = parameter.GetCustomAttribute<ValueSetNameAttribute>();

                    if (customAttribute != null)
                    {
                        // Resolve service by name
                        parameterValues[i] = Activator.CreateInstance(parameter.ParameterType, customAttribute.Name);
                    }
                    else
                    {
                        throw new StateflowsDefinitionException(
                            $"ValueSetNameAttribute not found for parameter {parameter.Name} in constructor {constructor.Name} of service {serviceType.Name}");
                    }
                    
                    continue;
                }
                
                var globalAttribute = parameter.GetCustomAttribute<GlobalValueAttribute>();
                if (globalAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(parameter, globalAttribute, () => ContextValues.GlobalValues, nameof(ContextValues.GlobalValues));
                    continue;
                }
                
                var stateAttribute = parameter.GetCustomAttribute<StateValueAttribute>();
                if (stateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(parameter, stateAttribute, () => ContextValues.StateValues, nameof(ContextValues.StateValues));
                    continue;
                }
                
                var parentStateAttribute = parameter.GetCustomAttribute<ParentStateValueAttribute>();
                if (parentStateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(parameter, parentStateAttribute, () => ContextValues.ParentStateValues, nameof(ContextValues.ParentStateValues));
                    continue;
                }
                
                var sourceStateAttribute = parameter.GetCustomAttribute<SourceStateValueAttribute>();
                if (sourceStateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(parameter, sourceStateAttribute, () => ContextValues.SourceStateValues, nameof(ContextValues.SourceStateValues));
                    continue;
                }
                
                var targetStateAttribute = parameter.GetCustomAttribute<TargetStateValueAttribute>();
                if (targetStateAttribute != null)
                {
                    parameterValues[i] = await BuildParameterValueAsync(parameter, targetStateAttribute, () => ContextValues.TargetStateValues, nameof(ContextValues.TargetStateValues));
                    continue;
                }
                
                parameterValues[i] = serviceProvider.GetRequiredService(parameter.ParameterType);
            }
        
            return Activator.CreateInstance(serviceType, parameterValues);
        }

        private static async Task<object> BuildParameterValueAsync(ParameterInfo parameter, ValueAttribute valueAttribute, Func<IContextValues> valueSetSelector, string collectionName)
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

            var valueSet = valueSetSelector();
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
                throw new StateflowsRuntimeException($"Required context value {valueName} not found");
            }

            return success
                ? result.GetType().GetField("Item2").GetValue(result)
                : Activator.CreateInstance(parameter.ParameterType);
        }
    }
}
