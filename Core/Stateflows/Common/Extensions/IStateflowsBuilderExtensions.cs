using Stateflows.Common.Registration.Interfaces;

namespace Stateflows
{
    public static class IStateflowsBuilderExtensions
    {
        public static IStateflowsBuilder UseFullNamesFor(this IStateflowsBuilder stateflowsBuilder, TypedElements typedElements)
        {
            (stateflowsBuilder as IStateflowsClientBuilder).UseFullNamesFor(typedElements);

            return stateflowsBuilder;
        }
    }
}
