using System.Threading.Tasks;
using Stateflows.Activities.Context.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivityObserver
    {
        Task BeforeActivityInitializeAsync(IActivityInitializationContext context);
        Task AfterActivityInitializeAsync(IActivityInitializationContext context);

        Task BeforeNodeInitializeAsync(IActivityNodeContext context);
        Task AfterNodeInitializeAsync(IActivityNodeContext context);

        Task BeforeNodeFinalizeAsync(IActivityNodeContext context);
        Task AfterNodeFinalizeAsync(IActivityNodeContext context);

        Task BeforeNodeExecuteAsync(IActivityNodeContext context);
        Task AfterNodeExecuteAsync(IActivityNodeContext context);

        Task BeforeFlowGuardAsync(IGuardContext<Token> context);
        Task AfterFlowGuardAsync(IGuardContext<Token> context, bool guardResult);

        Task BeforeFlowTransformationAsync(ITransformationContext<Token> context);
        Task AfterFlowTransformationAsync(ITransformationContext<Token> context);
    }
}
