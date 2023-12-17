using Stateflows;
using Stateflows.Common.Registration.Interfaces;

namespace Examples.Storage
{
    public static class DependencyInjection
    {
        public static IStateflowsBuilder AddStorage(this IStateflowsBuilder builder)
        {
            return builder
                .AddEntityFrameworkCoreStorage<StateflowsDbContext>()
                ;
        }
    }
}
