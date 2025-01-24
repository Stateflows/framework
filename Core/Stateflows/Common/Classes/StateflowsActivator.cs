using System;
using System.Linq;
using System.Reflection;
using Stateflows.Common.Attributes;
using Stateflows.Common.Exceptions;
using Stateflows.Common.Extensions;

namespace Stateflows.Common.Classes
{
    public class StateflowsActivator
    {
        public static T CreateInstance<T>(IServiceProvider serviceProvider)
            => (T)CreateInstance(serviceProvider, typeof(T));
        
        public static object CreateInstance(IServiceProvider serviceProvider, Type serviceType)
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
                if (!parameter.ParameterType.IsSubclassOfRawGeneric(typeof(BaseValueAccessor<>)))
                {
                    parameterValues[i] = serviceProvider.GetService(parameter.ParameterType);
        
                    continue;
                }
        
                var customAttribute = parameter.GetCustomAttribute<ValueNameAttribute>();
        
                if (customAttribute != null)
                {
                    // Resolve service by name
                    parameterValues[i] = Activator.CreateInstance(parameter.ParameterType, customAttribute.Name);
                }
                else
                {
                    throw new StateflowsDefinitionException($"ValueNameAttribute not found for parameter {parameter.Name} in constructor {constructor.Name} of service {serviceType.Name}");
                }
            }
        
            return Activator.CreateInstance(serviceType, parameterValues);
        }
    }
}
