﻿using System.Threading.Tasks;
using Stateflows.Activities;
using Stateflows.Common;

namespace Stateflows.Activities
{
    public interface IActivityVisitor
    {
        Task ActivityAddedAsync(string activityName, int activityVersion);

        Task ActivityTypeAddedAsync<TActivity>(string activityName, int activityVersion)
            where TActivity : class, IActivity;

        Task InitializerAddedAsync<TInitializationEvent>(string activityName, int activityVersion);

        Task DefaultInitializerAddedAsync(string activityName, int activityVersion);
        
        Task FinalizerAddedAsync(string activityName, int activityVersion);
        
        Task NodeAddedAsync(string activityName, int activityVersion, string nodeName, NodeType nodeType, string parentNodeName = null);
        
        Task NodeTypeAddedAsync<TNode>(string activityName, int activityVersion, string nodeName)
            where TNode : class, IActivityNode;
        
        Task AcceptEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName, string parentNodeName = null);
        
        Task AcceptEventNodeTypeAddedAsync<TEvent, TAcceptEventAction>(string activityName, int activityVersion, string nodeName)
            where TAcceptEventAction : class, IAcceptEventActionNode<TEvent>;
        
        Task SendEventNodeAddedAsync<TEvent>(string activityName, int activityVersion, string nodeName, string parentNodeName = null);
        
        Task SendEventNodeTypeAddedAsync<TEvent, TSendEventAction>(string activityName, int activityVersion, string nodeName)
            where TSendEventAction : class, ISendEventActionNode<TEvent>;

        Task ControlFlowAddedAsync(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false);

        Task ControlFlowTypeAddedAsync<TControlFlow>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            where TControlFlow : class, IControlFlow;
        
        Task FlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false);

        Task FlowTypeAddedAsync<TToken, TFlow>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            where TFlow : class, IFlow<TToken>;
        
        Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false);

        Task TransformationFlowTypeAddedAsync<TToken, TTransformedToken, TFlowTransformation>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName, bool isElse = false)
            where TFlowTransformation : class, IFlowTransformation<TToken, TTransformedToken>;
        
        Task CustomEventAddedAsync<TEvent>(string activityName, int activityVersion, BehaviorStatus[] supportedStatuses);
        
        // Task TransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName);
        //
        // Task ElseFlowAddedAsync<TToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName);
        //
        // Task ElseTransformationFlowAddedAsync<TToken, TTransformedToken>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName);
        //
        // Task ControlFlowGuardTypeAddedAsync<TGuard>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName = null)
        //     where TGuard : class, IControlFlowGuard;
        //
        // Task FlowGuardTypeAddedAsync<TToken, TGuard>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName = null)
        //     where TGuard : class, IFlowGuard<TToken>;
        //
        // Task FlowTransformationTypeAddedAsync<TToken, TTransformedToken, TTransformation>(string activityName, int activityVersion, string sourceNodeName, string targetNodeName = null)
        //     where TTransformation : class, IFlowTransformation<TToken, TTransformedToken>;
    }
}
