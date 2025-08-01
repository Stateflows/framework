using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Models;
using Stateflows.Activities.Models;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Context.Interfaces;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common;
using Stateflows.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Actions.Engine;

namespace Stateflows.Activities.Registration.Builders
{
    internal class FlowBuilder<TToken> :
        IObjectFlowBuilder<TToken>,
        IObjectFlowBuilderWithWeight<TToken>,
        IElseObjectFlowBuilder<TToken>,
        IElseObjectFlowBuilderWithWeight<TToken>,
        IControlFlowBuilder,
        IControlFlowBuilderWithWeight,
        IElseControlFlowBuilder
    {
        public Edge Edge { get; set; }

        public Graph Graph => Edge.Graph;

        public FlowBuilder(Edge edge)
        {
            Edge = edge;
        }

        public IObjectFlowBuilder<TToken> AddGuard(params Func<IGuardContext<TToken>, Task<bool>>[]  guardsAsync)
        {
            foreach (var guardAsync in guardsAsync)
            {
                var logic = new Logic<TokenPipelineActionAsync>(Constants.Guard);

                logic.Actions.Add(async (context, inspector) =>
                {
                    if (!(context.Token is TokenHolder<TToken> token)) return default;

                    var flowContext = new TokenFlowContext<TToken>(context, token.Payload);
                    
                    try
                    {
                        inspector.BeforeFlowGuard(flowContext);

                        var result = await guardAsync(flowContext)
                            ? token
                            : default;

                        inspector.AfterFlowGuard(flowContext, result != default);
                        
                        return result;
                    }
                    catch (Exception e)
                    {
                        if (e is StateflowsDefinitionException)
                        {
                            throw;
                        }
                        else
                        {
                            Trace.WriteLine($"⦗→s⦘ Activity '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                            if (!(Edge.Source != null && await Edge.Source.HandleExceptionAsync(e, context)))
                            {
                                inspector.OnFlowGuardException(flowContext, e);
                                throw;
                            }
                            else
                            {
                                throw new BehaviorExecutionException(e);
                            }
                        }
                    }
                });

                Edge.TokenPipeline.Actions.Add(logic);
            }

            return this;
        }

        public IObjectFlowBuilder<TTransformedToken> AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
        {
            var logic = new Logic<TokenPipelineActionAsync>(Constants.Transformation);

            logic.Actions.Add(async (context, inspector) =>
            {
                if (!(context.Token is TokenHolder<TToken> token)) return default;

                var flowContext = new TokenFlowContext<TToken>(context, token.Payload);
                
                try
                {
                    inspector.BeforeFlowTransform<TToken, TTransformedToken>(flowContext);
                        
                    var transformedToken = (await transformationAsync(flowContext))
                        .ToTokenHolder();
                    
                    inspector.AfterFlowTransform(new TokenFlowContext<TToken, TTransformedToken>(context, token.Payload, transformedToken.Payload));

                    return transformedToken;
                }
                catch (Exception e)
                {
                    if (e is StateflowsDefinitionException)
                    {
                        throw;
                    }
                    else
                    {
                        Trace.WriteLine($"⦗→s⦘ Activity '{context.Context.Id.Name}:{context.Context.Id.Instance}': exception '{e.GetType().FullName}' thrown with message '{e.Message}'");
                        if (!(Edge.Source != null && await Edge.Source.HandleExceptionAsync(e, context)))
                        {
                            inspector.OnFlowTransformationException<TToken, TTransformedToken>(flowContext, e);
                            throw;
                        }
                        else
                        {
                            throw new BehaviorExecutionException(e);
                        }
                    }
                }
            });

            Edge.TokenPipeline.Actions.Add(logic);
            Edge.TargetTokenType = typeof(TTransformedToken);

            return new FlowBuilder<TTransformedToken>(Edge);
        }

        public IObjectFlowBuilderWithWeight<TToken> SetWeight(int weight)
        {
            Edge.Weight = weight;

            return this;
        }

        IControlFlowBuilderWithWeight IControlFlowBuilderBase<IControlFlowBuilderWithWeight>.AddGuard(params Func<IGuardContext, Task<bool>>[]  guardsAsync)//GuardDelegateAsync guardAsync)
            => AddGuard(
                guardsAsync
                    .Select<Func<IGuardContext, Task<bool>>, Func<IGuardContext<TToken>, Task<bool>>>(
                        guardAsync => c => guardAsync(c as IGuardContext)
                    )
                    .ToArray()
            ) as IControlFlowBuilderWithWeight;

        IControlFlowBuilderWithWeight IFlowWeight<IControlFlowBuilderWithWeight>.SetWeight(int weight)
            => SetWeight(weight) as IControlFlowBuilderWithWeight;

        IObjectFlowBuilder<TTransformedToken> IObjectFlowBuilder<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync);

        IObjectFlowBuilderWithWeight<TToken> IObjectFlowGuardBuilderBase<TToken, IObjectFlowBuilderWithWeight<TToken>>.AddGuard(params Func<IGuardContext<TToken>, Task<bool>>[]  guardsAsync)//GuardDelegateAsync<TToken> guardAsync)
            => AddGuard(guardsAsync) as IObjectFlowBuilderWithWeight<TToken>;

        IObjectFlowBuilderWithWeight<TTransformedToken> IObjectFlowBuilderWithWeight<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IObjectFlowBuilderWithWeight<TTransformedToken>;

        IElseObjectFlowBuilderWithWeight<TToken> IFlowWeight<IElseObjectFlowBuilderWithWeight<TToken>>.SetWeight(int weight)
            => SetWeight(weight) as IElseObjectFlowBuilderWithWeight<TToken>;

        IElseObjectFlowBuilder<TTransformedToken> IElseObjectFlowBuilder<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IElseObjectFlowBuilder<TTransformedToken>;

        IElseObjectFlowBuilderWithWeight<TTransformedToken> IElseObjectFlowBuilderWithWeight<TToken>.AddTransformation<TTransformedToken>(TransformationDelegateAsync<TToken, TTransformedToken> transformationAsync)
            => AddTransformation<TTransformedToken>(transformationAsync) as IElseObjectFlowBuilderWithWeight<TTransformedToken>;

        void IFlowWeight.SetWeight(int weight)
            => SetWeight(weight);
    }
}
