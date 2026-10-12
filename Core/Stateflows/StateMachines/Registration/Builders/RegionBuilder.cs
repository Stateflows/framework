using System;
using System.Diagnostics;
using Stateflows.Actions;
using Stateflows.Actions.Registration.Interfaces;
using Stateflows.Activities;
using Stateflows.Activities.Registration.Interfaces;
using Stateflows.Common.Registration;
using Stateflows.StateMachines.Models;
using Stateflows.StateMachines.Exceptions;
using Stateflows.StateMachines.Registration.Interfaces;
using Stateflows.StateMachines.Registration.Interfaces.Base;
using Stateflows.StateMachines.Registration.Interfaces.Internal;
using ActionBuildAction = Stateflows.Actions.Registration.Interfaces.ActionBuildAction;
using ActionDelegateAsync = Stateflows.Actions.Registration.ActionDelegateAsync;
using ForkBuildAction = Stateflows.StateMachines.Registration.Interfaces.ForkBuildAction;
using JoinBuildAction = Stateflows.StateMachines.Registration.Interfaces.JoinBuildAction;
using OverridenForkBuildAction = Stateflows.StateMachines.Registration.Interfaces.OverridenForkBuildAction;
using OverridenJoinBuildAction = Stateflows.StateMachines.Registration.Interfaces.OverridenJoinBuildAction;

namespace Stateflows.StateMachines.Registration.Builders
{
    internal class RegionBuilder :
        IRegionBuilder,
        IInitializedRegionBuilder,
        IFinalizedRegionBuilder,
        IFinalizedOverridenRegionBuilder,
        IOverridenRegionBuilder,
        IBehaviorInitializedRegionBuilder,
        IBehaviorFinalizedRegionBuilder,
        IBehaviorRegionBuilder,
        IBehaviorFinalizedOverridenRegionBuilder,
        IBehaviorOverridenRegionBuilder,
        IBehaviorBuilder,
        IGraphBuilder
    {
        public Region Region { get; }

        public Graph Graph => Region.Graph;

        BehaviorClass IBehaviorBuilder.BehaviorClass => new BehaviorClass(Constants.StateMachine, Region.Graph.Name);

        int IBehaviorBuilder.BehaviorVersion => Region.Graph.Version;

        public RegionBuilder(Region region)
        {
            Region = region;
            Builder = new StateBuilder(Region.ParentVertex);
        }

        private StateBuilder Builder { get; set; }

        [DebuggerHidden]
        private RegionBuilder AddVertex(string stateName, VertexType type, System.Action<Vertex> vertexBuildAction = null)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                throw new StateDefinitionException(stateName, $"State name cannot be empty", Region.Graph.Class);
            }

            if (Region.Vertices.ContainsKey(stateName))
            {
                throw new StateDefinitionException(stateName, $"State '{stateName}' is already registered", Region.Graph.Class);
            }

            var vertex = new Vertex()
            {
                Name = stateName,
                Type = type,
                ParentRegion = Region,
                Graph = Region.Graph,
            };

            vertexBuildAction?.Invoke(vertex);

            Region.Vertices.Add(vertex.Name, vertex);
            Region.Graph.AllVertices.Add(vertex.Identifier, vertex);
            
            Region.Graph.VisitingTasks.Add(visitor => visitor.VertexAddedAsync(Region.Graph.Name, Region.Graph.Version, vertex.Name, vertex.Type));

            return this;
        }

        #region AddState
        [DebuggerHidden]
        private RegionBuilder AddState(string stateName, StateBuildAction stateBuildAction = null)
            => AddVertex(stateName, VertexType.State, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        private RegionBuilder AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddVertex(junctionName, VertexType.Junction, vertex => junctionBuildAction?.Invoke(new StateBuilder(vertex)));

        [DebuggerHidden]
        private RegionBuilder AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddVertex(choiceName, VertexType.Choice, vertex => choiceBuildAction?.Invoke(new StateBuilder(vertex)));

        IBehaviorOverridenRegionBuilder IStateMachineElements<IBehaviorOverridenRegionBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineElements<IBehaviorInitializedRegionBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineElements<IBehaviorOverridenRegionBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineElements<IBehaviorOverridenRegionBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineElements<IBehaviorOverridenRegionBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineElements<IBehaviorOverridenRegionBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineElements<IBehaviorInitializedRegionBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineElements<IBehaviorInitializedRegionBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineElements<IBehaviorInitializedRegionBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IInitializedRegionBuilder IStateMachineElements<IInitializedRegionBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        IInitializedRegionBuilder IStateMachineElements<IInitializedRegionBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IInitializedRegionBuilder IStateMachineElements<IInitializedRegionBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        IOverridenRegionBuilder IStateMachineElements<IOverridenRegionBuilder>.AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddFork(forkName, forkBuildAction);

        IOverridenRegionBuilder IStateMachineElements<IOverridenRegionBuilder>.AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddJoin(joinName, joinBuildAction);

        private RegionBuilder AddFork(string forkName, ForkBuildAction forkBuildAction)
            => AddVertex(forkName, VertexType.Fork, vertex => forkBuildAction?.Invoke(new StateBuilder(vertex)));

        private RegionBuilder AddJoin(string joinName, JoinBuildAction joinBuildAction)
            => AddVertex(joinName, VertexType.Join, vertex => joinBuildAction?.Invoke(new StateBuilder(vertex)));

        IInitializedRegionBuilder IStateMachineElements<IInitializedRegionBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineElements<IBehaviorOverridenRegionBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineElements<IBehaviorOverridenRegionBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineElements<IBehaviorInitializedRegionBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineElements<IBehaviorInitializedRegionBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineElements<IBehaviorInitializedRegionBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        IInitializedRegionBuilder IStateMachineElements<IInitializedRegionBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        IInitializedRegionBuilder IStateMachineElements<IInitializedRegionBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachineElements<IOverridenRegionBuilder>.AddCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddCompositeState(compositeStateName, compositeStateBuildAction);

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachineElements<IOverridenRegionBuilder>.AddOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachineElements<IOverridenRegionBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        IInitializedRegionBuilder IStateMachineElements<IInitializedRegionBuilder>.AddJunction(string junctionName, JunctionBuildAction junctionBuildAction)
            => AddJunction(junctionName, junctionBuildAction);

        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachineElements<IOverridenRegionBuilder>.AddChoice(string choiceName, ChoiceBuildAction choiceBuildAction)
            => AddChoice(choiceName, choiceBuildAction);

        #endregion

        #region AddFinalState
        [DebuggerHidden]
        private RegionBuilder AddFinalState(string finalStateName = null)
            => AddVertex(finalStateName ?? State<FinalState>.Name, VertexType.FinalState);
        #endregion

        #region AddCompositeState
        [DebuggerHidden]
        IOverridenRegionBuilder IStateMachineElements<IOverridenRegionBuilder>.AddState(string stateName, StateBuildAction stateBuildAction)
            => AddState(stateName, stateBuildAction);

        [DebuggerHidden]
        private RegionBuilder AddCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
            => AddVertex(compositeStateName, VertexType.CompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));

        [DebuggerHidden]
        private RegionBuilder AddOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddVertex(orthogonalStateName, VertexType.OrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));

        [DebuggerHidden]
        private RegionBuilder AddInitialState(string stateName, StateBuildAction stateBuildAction = null)
        {
            stateName ??= $"{Region.ParentVertex.Name}:{Region.ParentVertex.Regions.IndexOf(Region)}:{InitialState.Name}";
            Region.InitialVertexName = stateName;
            return AddVertex(stateName, VertexType.InitialState, vertex => stateBuildAction?.Invoke(new StateBuilder(vertex)));
        }

        [DebuggerHidden]
        private RegionBuilder AddInitialCompositeState(string compositeStateName, CompositeStateBuildAction compositeStateBuildAction)
        {
            Region.InitialVertexName = compositeStateName;
            return AddVertex(compositeStateName, VertexType.InitialCompositeState, vertex => compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion)));
        }

        [DebuggerHidden]
        private RegionBuilder AddInitialOrthogonalState(string orthogonalStateName, OrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            Region.InitialVertexName = orthogonalStateName;
            return AddVertex(orthogonalStateName, VertexType.InitialOrthogonalState, vertex => orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex)));
        }
        #endregion

        private RegionBuilder UseState(string stateName, OverridenStateBuildAction stateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(stateName, out var vertex) ||
                (
                    vertex.Type != VertexType.State && 
                    vertex.Type != VertexType.InitialState
                ) || 
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"State '{stateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            stateBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        private RegionBuilder UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(compositeStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.CompositeState &&
                    vertex.Type != VertexType.InitialCompositeState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Composite state '{compositeStateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }

            compositeStateBuildAction?.Invoke(new CompositeStateBuilder(vertex.DefaultRegion));

            return this;
        }

        private RegionBuilder UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
        {
            if (
                !Region.Vertices.TryGetValue(orthogonalStateName, out var vertex) ||
                (
                    vertex.Type != VertexType.OrthogonalState &&
                    vertex.Type != VertexType.InitialOrthogonalState
                ) ||
                vertex.OriginStateMachineName == null
            )
            {
                throw new StateMachineOverrideException($"Orthogonal state '{orthogonalStateName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }

            orthogonalStateBuildAction?.Invoke(new OrthogonalStateBuilder(vertex));

            return this;
        }

        private RegionBuilder UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
        {
            if (!Region.Vertices.TryGetValue(junctionName, out var vertex) || vertex.Type != VertexType.Junction || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Junction '{junctionName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            junctionBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        private RegionBuilder UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
        {
            if (!Region.Vertices.TryGetValue(choiceName, out var vertex) || vertex.Type != VertexType.Choice || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Choice '{choiceName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            choiceBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        private RegionBuilder UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
        {
            if (!Region.Vertices.TryGetValue(forkName, out var vertex) || vertex.Type != VertexType.Fork || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Fork '{forkName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            forkBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        private RegionBuilder UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
        {
            if (!Region.Vertices.TryGetValue(joinName, out var vertex) || vertex.Type != VertexType.Join || vertex.OriginStateMachineName == null)
            {
                throw new StateMachineOverrideException($"Join '{joinName}' not found in overriden composite state '{Region.ParentVertex.Name}'", Region.Graph.Class);
            }
            
            joinBuildAction?.Invoke(new StateBuilder(vertex));

            return this;
        }

        IBehaviorOverridenRegionBuilder IStateMachineOverrides<IBehaviorOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineOverrides<IBehaviorOverridenRegionBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineOverrides<IBehaviorOverridenRegionBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineOverrides<IBehaviorOverridenRegionBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineOverrides<IBehaviorOverridenRegionBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>.UseJunction(string junctionName,
            OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseFork(string forkName, OverridenForkBuildAction forkBuildAction)
            => UseFork(forkName, forkBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseJoin(string joinName, OverridenJoinBuildAction joinBuildAction)
            => UseJoin(joinName, joinBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineFinal<IFinalizedOverridenRegionBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineOverrides<IBehaviorOverridenRegionBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorOverridenRegionBuilder IStateMachineOverrides<IBehaviorOverridenRegionBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineOverrides<IBehaviorFinalizedOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName,
            OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseCompositeState(string compositeStateName,
            OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IOverridenRegionBuilder IStateMachineOverrides<IOverridenRegionBuilder>.UseState(string stateName, OverridenStateBuildAction stateBuildAction)
            => UseState(stateName, stateBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseCompositeState(string compositeStateName, OverridenCompositeStateBuildAction compositeStateBuildAction)
            => UseCompositeState(compositeStateName, compositeStateBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseOrthogonalState(string orthogonalStateName, OverridenOrthogonalStateBuildAction orthogonalStateBuildAction)
            => UseOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseJunction(string junctionName, OverridenJunctionBuildAction junctionBuildAction)
            => UseJunction(junctionName, junctionBuildAction);

        IFinalizedOverridenRegionBuilder IStateMachineOverrides<IFinalizedOverridenRegionBuilder>.UseChoice(string choiceName, OverridenChoiceBuildAction choiceBuildAction)
            => UseChoice(choiceName, choiceBuildAction);

        private RegionBuilder AddHistory(string historyEntrypointName, HistoryBuildAction buildAction = null)
        {
            historyEntrypointName ??= State<History>.Name;
            AddVertex(historyEntrypointName, VertexType.History);
            var historyVertex = Region.Vertices[historyEntrypointName];
            historyVertex.HistoricalRegion = Region;
            Region.History ??= historyVertex;
            
            buildAction?.Invoke(new StateBuilder(historyVertex));

            return this;
        }

        IOverridenRegionBuilder IStateHistory<IOverridenRegionBuilder>.AddHistory(string historyEntrypointName,
            HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IRegionBuilder IStateHistory<IRegionBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);
        
        private RegionBuilder AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
            where TActivity : class, IActivity
        {
            Builder.AddDoActivity<TActivity>(buildAction);
            return this;
        }

        private RegionBuilder AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
        {
            Builder.AddDoActivity(activityBuildAction);
            return this;
        }

        IBehaviorRegionBuilder IStateDoActivity<IBehaviorRegionBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
            => AddDoActivity<TActivity>(buildAction);

        IBehaviorOverridenRegionBuilder IStateDoActivity<IBehaviorOverridenRegionBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
            => AddDoActivity(activityBuildAction);

        IBehaviorOverridenRegionBuilder IStateDoActivity<IBehaviorOverridenRegionBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
            => AddDoActivity<TActivity>(buildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateDoActivity<IBehaviorFinalizedOverridenRegionBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
            => AddDoActivity(activityBuildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateDoActivity<IBehaviorFinalizedOverridenRegionBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
            => AddDoActivity<TActivity>(buildAction);

        IBehaviorFinalizedRegionBuilder IStateDoActivity<IBehaviorFinalizedRegionBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
            => AddDoActivity(activityBuildAction);

        IBehaviorFinalizedRegionBuilder IStateDoActivity<IBehaviorFinalizedRegionBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
            => AddDoActivity<TActivity>(buildAction);

        IBehaviorInitializedRegionBuilder IStateDoActivity<IBehaviorInitializedRegionBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
            => AddDoActivity(activityBuildAction);

        IBehaviorInitializedRegionBuilder IStateDoActivity<IBehaviorInitializedRegionBuilder>.AddDoActivity<TActivity>(ActivityUtilsBuildAction buildAction)
            => AddDoActivity<TActivity>(buildAction);

        IBehaviorRegionBuilder IStateDoActivity<IBehaviorRegionBuilder>.AddDoActivity(ReactiveActivityBuildAction activityBuildAction)
            => AddDoActivity(activityBuildAction);
        
        private RegionBuilder AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
            where TAction : class, IAction
        {
            Builder.AddDoAction<TAction>(buildAction);
            return this;
        }

        private RegionBuilder AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
        {
            Builder.AddDoAction(actionDelegate, buildAction);
            return this;
        }

        IBehaviorRegionBuilder IStateDoAction<IBehaviorRegionBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
            => AddDoAction<TAction>(buildAction);

        IBehaviorOverridenRegionBuilder IStateDoAction<IBehaviorOverridenRegionBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
            => AddDoAction(actionDelegate, buildAction);

        IBehaviorOverridenRegionBuilder IStateDoAction<IBehaviorOverridenRegionBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
            => AddDoAction<TAction>(buildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateDoAction<IBehaviorFinalizedOverridenRegionBuilder>.AddDoAction(ActionDelegateAsync actionDelegate,
            ActionBuildAction buildAction)
            => AddDoAction(actionDelegate, buildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateDoAction<IBehaviorFinalizedOverridenRegionBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
            => AddDoAction<TAction>(buildAction);

        IBehaviorFinalizedRegionBuilder IStateDoAction<IBehaviorFinalizedRegionBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
            => AddDoAction(actionDelegate, buildAction);

        IBehaviorFinalizedRegionBuilder IStateDoAction<IBehaviorFinalizedRegionBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
            => AddDoAction<TAction>(buildAction);

        IBehaviorInitializedRegionBuilder IStateDoAction<IBehaviorInitializedRegionBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
            => AddDoAction(actionDelegate, buildAction);

        IBehaviorInitializedRegionBuilder IStateDoAction<IBehaviorInitializedRegionBuilder>.AddDoAction<TAction>(ActionBuildAction<TAction> buildAction)
            => AddDoAction<TAction>(buildAction);

        IBehaviorRegionBuilder IStateDoAction<IBehaviorRegionBuilder>.AddDoAction(ActionDelegateAsync actionDelegate, ActionBuildAction buildAction)
            => AddDoAction(actionDelegate, buildAction);

        IInitializedRegionBuilder IStateMachineInitial<IInitializedRegionBuilder>.AddInitialState(string stateName, StateBuildAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineInitial<IBehaviorInitializedRegionBuilder>.AddInitialState(string stateName, StateBuildAction stateBuildAction)
            => AddInitialState(stateName, stateBuildAction);

        IInitializedRegionBuilder IStateMachineInitial<IInitializedRegionBuilder>.AddInitialCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddInitialCompositeState(compositeStateName, compositeStateBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineInitial<IBehaviorInitializedRegionBuilder>.AddInitialOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddInitialOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IBehaviorInitializedRegionBuilder IStateMachineInitial<IBehaviorInitializedRegionBuilder>.AddInitialCompositeState(string compositeStateName,
            CompositeStateBuildAction compositeStateBuildAction)
            => AddInitialCompositeState(compositeStateName, compositeStateBuildAction);

        IInitializedRegionBuilder IStateMachineInitial<IInitializedRegionBuilder>.AddInitialOrthogonalState(string orthogonalStateName,
            OrthogonalStateBuildAction orthogonalStateBuildAction)
            => AddInitialOrthogonalState(orthogonalStateName, orthogonalStateBuildAction);

        IInitializedRegionBuilder IStateHistory<IInitializedRegionBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);
        
        IFinalizedRegionBuilder IStateMachineFinal<IFinalizedRegionBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        IBehaviorInitializedRegionBuilder IStateHistory<IBehaviorInitializedRegionBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IBehaviorFinalizedRegionBuilder IStateMachineFinal<IBehaviorFinalizedRegionBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);

        IBehaviorRegionBuilder IStateHistory<IBehaviorRegionBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IBehaviorOverridenRegionBuilder IStateHistory<IBehaviorOverridenRegionBuilder>.AddHistory(string historyEntrypointName, HistoryBuildAction buildAction)
            => AddHistory(historyEntrypointName, buildAction);

        IBehaviorFinalizedOverridenRegionBuilder IStateMachineFinal<IBehaviorFinalizedOverridenRegionBuilder>.AddFinalState(string finalStateName)
            => AddFinalState(finalStateName);
    }
}
