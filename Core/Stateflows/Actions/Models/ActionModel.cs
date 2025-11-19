using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Actions.Registration;
using Stateflows.Common.Registration;

namespace Stateflows.Actions.Models
{
    public class ActionModel
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public bool Reentrant { get; set; }
        public ActionDelegateAsync Delegate { get; set; }
        public Func<IActionVisitor, Task> VisitingAction { get; set; }

        public List<ActionExceptionHandlerFactoryAsync> ExceptionHandlerFactories { get; set; } = [];
        
        public List<ActionInterceptorFactoryAsync> InterceptorFactories { get; set; } = [];
    }
}