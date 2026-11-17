using System;
using System.Linq;
using System.Collections.Generic;

namespace Stateflows.Common.Models
{
    internal class Logic<TDelegate>
        where TDelegate : Delegate
    {
        public Logic(string name)
        {
             Name = name;
        }
        
        public string? OriginStateMachineName { get; set; }

        public List<TDelegate> Actions { get; set; } = [];
        internal Dictionary<int, Stateflows.StateMachines.Models.OperationRegistration> Registrations { get; } = [];

        public bool Any => Actions.Any();

        public string Name { get; }
    }
}