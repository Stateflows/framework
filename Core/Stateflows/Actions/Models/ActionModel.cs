using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Common.Registration.Builders;
using Stateflows.Actions.Registration;

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
        public Func<IActionVisitor, Task> VisitingAction { get; set; }

        public System.Action<object>? ConfigurationAction { get; set; } = null;

        public List<ActionExceptionHandlerFactoryAsync> ExceptionHandlerFactories { get; set; } = [];
        
        public List<ActionInterceptorFactoryAsync> InterceptorFactories { get; set; } = [];
        
        public List<ActionObserverFactoryAsync> ObserverFactories { get; set; } = [];

        internal void Build(StateflowsBuilder stateflowsBuilder)
        {
            if (stateflowsBuilder.ResourceNames.TryGetValue(ResourceName ?? string.Empty, out var resourceName))
            {
                stateflowsBuilder.ResourcesByBehaviorClass[new BehaviorClass(BehaviorType.Action, Name)] = resourceName;
            }
            else
            {
                throw new InvalidOperationException($"Resource group {ResourceName ?? string.Empty} does not exist");
            }
        }
    }
}