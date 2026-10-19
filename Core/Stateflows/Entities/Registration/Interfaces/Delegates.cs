using System;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Entities.Registration.Interfaces
{
    public delegate void EntitiesBuildAction(IEntitiesBuilder builder);

    public delegate void EntityBuildAction<TTemplate>(IEntityBuilder<TTemplate> builder);

    public delegate void FieldBuildAction<in TTemplate, out TFieldType>(IFieldBuilder<TTemplate, TFieldType> builder);

    public delegate TFieldType FieldComputation<in TTemplate, out TFieldType>(TTemplate entity);

    public delegate void MutationAction<in TTemplate, in TMutationEvent>(IMutationContext<TTemplate, TMutationEvent> context);
    
    public delegate TProjectionTemplate ProjectionAction<in TTemplate, out TProjectionTemplate>(TTemplate entity);

    public delegate void DefaultInitializerAction<in TTemplate>(IDefaultEntityInitializationContext<TTemplate> context);

    public delegate void InitializerAction<in TTemplate, in TInitializationEvent>(IEntityInitializationContext<TTemplate, TInitializationEvent> context);

    public delegate IEntityObserver EntityObserverFactory(IServiceProvider serviceProvider, IBehaviorActionContext context);
    public delegate Task<IEntityObserver> EntityObserverFactoryAsync(IServiceProvider serviceProvider, IBehaviorActionContext context);

    public delegate IEntityInterceptor EntityInterceptorFactory(IServiceProvider serviceProvider, IBehaviorActionContext context);
    public delegate Task<IEntityInterceptor> EntityInterceptorFactoryAsync(IServiceProvider serviceProvider, IBehaviorActionContext context);

    public delegate IEntityExceptionHandler EntityExceptionHandlerFactory(IServiceProvider serviceProvider, IBehaviorActionContext context);
    public delegate Task<IEntityExceptionHandler> EntityExceptionHandlerFactoryAsync(IServiceProvider serviceProvider, IBehaviorActionContext context);
}
