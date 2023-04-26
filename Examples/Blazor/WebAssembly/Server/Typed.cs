using Stateflows.StateMachines;
using Examples.Common;
using System.Diagnostics;
using Stateflows.StateMachines.Context.Interfaces;
using Microsoft.Extensions.Logging;

namespace Blazor.WebAssembly.Server
{
    public class SomeOtherClass
    {
        private int counter = -1;
        public SomeOtherClass(int counter)
        {
            this.counter = counter;
        }

        public bool Guard(ITransitionContext<SomeEvent> context)
        {
            return counter > 0;
        }
    }

    public class Typed : StateMachine
    {
        private ILogger<Typed> Logger;

        private int counter = -1;

        private SomeOtherClass internalDependency = new SomeOtherClass(0);

        public bool Guard(IGuardContext<SomeEvent> context)
        {
            return false;
        }

        public Typed(ILogger<Typed> logger)
        {
            Logger = logger;
            counter = Random.Shared.Next();
            internalDependency = new SomeOtherClass(counter);
        }

        public override void Build(ITypedStateMachineInitialBuilder builder)
        {
            builder
                .AddInitialState("initial", b => b
                    .AddTransition<SomeEvent>("next", b => b
                        .AddGuard(c => true)
                        .AddGuard(async c => true)
                        .AddGuard(TransitionGuard.Empty)
                        .AddGuard(Guard)
                        .AddGuard(async c => counter > 0)
                        .AddGuard(internalDependency.Guard) // this will break on startup
                    )
                )
                .AddState("next", b => b
                    .AddTransition<SomeEvent>("initial")
                )
                ;
        }
    }
}
