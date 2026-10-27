using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Stateflows.Common;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    /// <summary>Builds deferred predicates that inspect one machine-global context value.</summary>
    /// <remarks>Comparison predicates reject when the value cannot be read, including negative comparisons.</remarks>
    public struct TransitionGuardGlobalValueExpression
    {
        private readonly string ValueName;

        /// <summary>Selects a machine-global value for later predicate evaluation.</summary>
        /// <param name="valueName">The machine-global context value key.</param>
        public TransitionGuardGlobalValueExpression(string valueName)
        {
            ValueName = valueName;
        }

        /// <summary>
        /// Provides guard that checks if specified global value is set. 
        /// </summary>
        public Func<IStateMachineActionContext, Task<bool>> IsSet
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.IsSetAsync(self.ValueName);
            }
        }

        /// <summary>
        /// Provides guard that checks if specified global value is not set. 
        /// </summary>
        public Func<IStateMachineActionContext, Task<bool>> IsNotSet
        {
            get
            {
                var self = this;
                return async c => !await c.Behavior.Values.IsSetAsync(self.ValueName);
            }
        }

        /// <summary>
        /// Provides guard that checks if specified global value is equal to given value.
        /// </summary>
        /// <param name="value">Value to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read value equal to the supplied value.</returns>
        public Func<IStateMachineActionContext, Task<bool>> IsEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified global value is not equal to given value.
        /// </summary>
        /// <param name="value">Value to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read value unequal to the supplied value.</returns>
        public Func<IStateMachineActionContext, Task<bool>> IsNotEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && !result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified global value is equal to one of given values.
        /// </summary>
        /// <param name="values">Values to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read value present in the supplied set.</returns>
        public Func<IStateMachineActionContext, Task<bool>> IsOneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && values.Contains(result.Value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified global value is equal to none of given values.
        /// </summary>
        /// <param name="values">Values to compare global value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read value absent from the supplied set.</returns>
        public Func<IStateMachineActionContext, Task<bool>> IsNoneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && !values.Contains(result.Value);
            };
        }
        
        /// <summary>
        /// Provides guard that checks if specified global value matches given condition.
        /// </summary>
        /// <param name="condition">Condition to check global value against.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read value satisfying the supplied condition.</returns>
        public Func<IStateMachineActionContext, Task<bool>> Matches<T>(Func<T, bool> condition)
        {
            var self = this;
            return async c =>
            {
                var result = await c.Behavior.Values.TryGetAsync<T>(self.ValueName);
                return result.Success && condition(result.Value);
            };
        }
    }

    /// <summary>Builds deferred predicates over a value scoped to the transition's source or target state.</summary>
    /// <remarks>Predicates reject when the selected endpoint is absent. Comparisons also reject when its value cannot be read.</remarks>
    public struct TransitionGuardStateValueExpression
    {
        private readonly string ValueName;
        private readonly bool IsSource;

        /// <summary>Selects a state-local value for later predicate evaluation.</summary>
        /// <param name="valueName">The selected state's context value key.</param>
        /// <param name="isSource">True to use the source state; false to use the target state.</param>
        public TransitionGuardStateValueExpression(string valueName, bool isSource)
        {
            ValueName = valueName;
            IsSource = isSource;
        }

        private IContextValues GetValueSet(ITransitionContext transitionContext)
            => IsSource
                ? transitionContext.Source.Values
                : transitionContext.Target?.Values;

        /// <summary>
        /// Provides guard that checks if specified state's value is set. 
        /// </summary>
        public Func<ITransitionContext, Task<bool>> IsSet
        {
            get
            {
                var self = this;
                return c =>
                {
                    var valueSet = self.GetValueSet(c);
                    return valueSet != null
                        ? valueSet.IsSetAsync(self.ValueName)
                        : Task.FromResult(false);
                };
            }
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is not set. 
        /// </summary>
        public Func<ITransitionContext, Task<bool>> IsNotSet
        {
            get
            {
                var self = this;
                return async c =>
                {
                    var valueSet = self.GetValueSet(c);
                    return valueSet != null && !await valueSet.IsSetAsync(self.ValueName);
                };
            }
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is equal to given value.
        /// </summary>
        /// <param name="value">Value to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read state value equal to the supplied value.</returns>
        public Func<ITransitionContext, Task<bool>> IsEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is not equal to given value.
        /// </summary>
        /// <param name="value">Value to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read state value unequal to the supplied value.</returns>
        public Func<ITransitionContext, Task<bool>> IsNotEqualTo<T>(T value)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && !result.Value.Equals(value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is equal to one of given values.
        /// </summary>
        /// <param name="values">Values to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read state value present in the supplied set.</returns>
        public Func<ITransitionContext, Task<bool>> IsOneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && values.Contains(result.Value);
            };
        }

        /// <summary>
        /// Provides guard that checks if specified state's value is equal to none of given values.
        /// </summary>
        /// <param name="values">Values to compare state's value to.</param>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <returns>A predicate accepting only a successfully read state value absent from the supplied set.</returns>
        public Func<ITransitionContext, Task<bool>> IsNoneOf<T>(params T[] values)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && !values.Contains(result.Value);
            };
        }
        
        /// <summary>
        /// Provides a guard that checks a source-state or target-state value against the given condition.
        /// </summary>
        /// <param name="condition">The predicate evaluated only when the selected state value is read successfully.</param>
        /// <typeparam name="T">The stored value type.</typeparam>
        /// <returns>A predicate accepting only a successfully read state value satisfying the condition.</returns>
        public Func<ITransitionContext, Task<bool>> Matches<T>(Func<T, bool> condition)
        {
            var self = this;
            return async c =>
            {
                var valueSet = self.GetValueSet(c);
                var result = valueSet != null
                    ? await valueSet.TryGetAsync<T>(self.ValueName)
                    : (Success: false, Value: default);
                return result.Success && condition(result.Value);
            };
        }
    }

    /// <summary>Builds predicates over machine-global values under a dot-separated namespace prefix.</summary>
    public struct TransitionGuardGlobalNamespaceExpression
    {
        private readonly string NamespaceName;

        /// <summary>Selects a machine-global value namespace.</summary>
        /// <param name="namespaceName">The prefix to which a dot and value or child namespace name are appended.</param>
        public TransitionGuardGlobalNamespaceExpression(string namespaceName)
        {
            NamespaceName = namespaceName;
        }

        /// <summary>
        /// Provides declarative guards based on the global namespace's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global namespace's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardGlobalValueExpression($"{self.NamespaceName}.{valueName}");
        }

        /// <summary>
        /// Provides declarative guards based on the global namespace's sub-namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the global namespace's sub-namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new TransitionGuardGlobalNamespaceExpression($"{self.NamespaceName}.{namespaceName}");
        }
        
        /// <summary>
        /// Provides guard that checks if specified namespace has any values. 
        /// </summary>
        public Func<IStateMachineActionContext, Task<bool>> HasAnyValues
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.HasAnyPrefixedAsync($"{self.NamespaceName}.");
            }
        }
    }

    /// <summary>Builds predicates over a source-state or target-state value namespace.</summary>
    /// <remarks>Namespace existence rejects when the selected transition endpoint is absent.</remarks>
    public struct TransitionGuardStateNamespaceExpression
    {
        private readonly string NamespaceName;
        private readonly bool IsSource;

        /// <summary>Selects a namespace within one transition endpoint's state-local values.</summary>
        /// <param name="namespaceName">The dot-separated value prefix.</param>
        /// <param name="isSource">True to use source values; false to use target values.</param>
        public TransitionGuardStateNamespaceExpression(string namespaceName, bool isSource)
        {
            NamespaceName = namespaceName;
            IsSource = isSource;
        }

        /// <summary>
        /// Provides declarative guards based on the state namespace's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the state namespace's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardStateValueExpression($"{self.NamespaceName}.{valueName}", self.IsSource);
        }

        /// <summary>
        /// Provides declarative guards based on the state namespace's sub-namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the state namespace's sub-namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new TransitionGuardStateNamespaceExpression($"{self.NamespaceName}.{namespaceName}", IsSource);
        }

        private IContextValues GetValueSet(ITransitionContext transitionContext)
            => IsSource
                ? transitionContext.Source.Values
                : transitionContext.Target?.Values;
        
        /// <summary>
        /// Provides guard that checks if specified namespace has any values. 
        /// </summary>
        public Func<ITransitionContext, Task<bool>> HasAnyValues
        {
            get
            {
                var self = this;
                return c =>
                {
                    var valueSet = self.GetValueSet(c);
                    return valueSet != null
                        ? valueSet.HasAnyPrefixedAsync($"{self.NamespaceName}.")
                        : Task.FromResult(false);
                };
            }
        }
    }

    /// <summary>Selects machine-global values and namespaces for deferred transition predicates.</summary>
    public struct TransitionGuardGlobalSelector
    {
        /// <summary>
        /// Provides declarative guards based on the global value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalValueExpression Value(string valueName)
            => new TransitionGuardGlobalValueExpression(valueName);
        
        /// <summary>
        /// Provides declarative guards based on the global namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the global namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardGlobalNamespaceExpression Namespace(string namespaceName)
            => new TransitionGuardGlobalNamespaceExpression(namespaceName);
    }

    /// <summary>Selects values and namespaces belonging to one transition endpoint for guard expressions.</summary>
    public struct TransitionGuardStateSelector
    {
        private readonly bool IsSource;

        /// <summary>Selects the transition endpoint inspected by subsequent expressions.</summary>
        /// <param name="isSource">True for source-state values; false for target-state values.</param>
        public TransitionGuardStateSelector(bool isSource)
        {
            IsSource = isSource;
        }

        /// <summary>
        /// Provides declarative guards based on the state's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the state's value used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionGuardStateValueExpression(valueName, self.IsSource);
        }
        
        /// <summary>
        /// Provides declarative guards based on the state's namespace with given name.
        /// </summary>
        /// <param name="namespaceName">Name of the state's namespace used in guard.</param>
        /// <returns>Declarative guards.</returns>
        public TransitionGuardStateNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new TransitionGuardStateNamespaceExpression(namespaceName, self.IsSource);
        }
    }
    
    /// <summary>
    /// Helper static class that provides predefined, declarative-style guards for transitions of all kinds. 
    /// </summary>
    public static class Guards
    {
        /// <summary>
        /// Declares empty, positive guard that allows transition unconditionally.
        /// </summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="context">The transition context; no state or event data is inspected.</param>
        /// <returns>A completed task containing true.</returns>
        public static Task<bool> Empty<TEvent>(ITransitionContext<TEvent> context)
            => Allow(context);

        /// <summary>
        /// Declares empty, positive guard that allows transition unconditionally.
        /// </summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="_">The transition context, intentionally unused.</param>
        /// <returns>A completed task containing true.</returns>
        public static Task<bool> Allow<TEvent>(ITransitionContext<TEvent> _)
            => Task.FromResult(true);

        /// <summary>
        /// Declares negative guard that denies transition unconditionally.
        /// </summary>
        /// <typeparam name="TEvent">The trigger payload type.</typeparam>
        /// <param name="_">The transition context, intentionally unused.</param>
        /// <returns>A completed task containing false.</returns>
        public static Task<bool> Deny<TEvent>(ITransitionContext<TEvent> _)
            => Task.FromResult(false);
        
        /// <summary>
        /// Declares guard that checks if state machine is in given state.
        /// </summary>
        /// <typeparam name="TState">State to be checked against.</typeparam>
        /// <param name="context">The machine context whose active-state tree is searched.</param>
        /// <returns>Whether the typed vertex's registered name occurs anywhere in the active configuration.</returns>
        public static Task<bool> InState<TState>(IStateMachineActionContext context)
            where TState : class, IVertex
            => Task.FromResult(context.CurrentStates.GetAllNodes().Any(node => node.Value.Name == State<TState>.Name));
        
        /// <summary>
        /// Declares guard that checks if state machine is in given state.
        /// </summary>
        /// <param name="stateName">Name of the state to be checked against.</param>
        /// <returns>A predicate testing whether the exact registered name occurs anywhere in the active configuration.</returns>
        public static Func<IStateMachineActionContext, Task<bool>> InState(string stateName)
            => c => Task.FromResult(c.CurrentStates.GetAllNodes().Any(node => node.Value.Name == stateName));

        /// <summary>
        /// Provides declarative guards based on global values.
        /// </summary>
        public static TransitionGuardGlobalSelector Global
            => new TransitionGuardGlobalSelector();
        
        /// <summary>
        /// Provides declarative guards based on the source values.
        /// </summary>
        public static TransitionGuardStateSelector Source
            => new TransitionGuardStateSelector(true);
        
        /// <summary>
        /// Provides declarative guards based on the target values.
        /// </summary>
        public static TransitionGuardStateSelector Target
            => new TransitionGuardStateSelector(false);
    }
}