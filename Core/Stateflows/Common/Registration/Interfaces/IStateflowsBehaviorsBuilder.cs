using System.Collections.Generic;

namespace Stateflows.Common.Registration.Interfaces
{
    internal interface IStateflowsBehaviorsBuilder
    {
        List<IBehaviorRegister> Registers { get; }
    }
}