using Stateflows.Common;

namespace Stateflows.MAF.AIAgents
{
    public interface IAIAgentInterceptor
    {
        bool BeforeProcessEvent<TEvent>(Actions.Context.Interfaces.IEventContext<TEvent> context);

        void AfterProcessEvent<TEvent>(Actions.Context.Interfaces.IEventContext<TEvent> context, EventStatus eventStatus);

    }
}
