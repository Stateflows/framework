using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;
using Stateflows.StateMachines.Context.Interfaces;

namespace Stateflows.StateMachines
{
    public struct TransitionEffectGlobalValueExpression
    {
        private readonly string ValueName;

        public TransitionEffectGlobalValueExpression(string valueName)
        {
            ValueName = valueName;
        }

        /// <summary>
        /// Provides an effect that sets the specified global value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <returns>Effect function.</returns>
        public Func<IStateMachineActionContext, Task> Set<T>(T value)
        {
            var self = this;
            return c => c.Behavior.Values.SetAsync(self.ValueName, value);
        }

        /// <summary>
        /// Provides an effect that updates the specified global value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="valueUpdater">Function to update the value.</param>
        /// <param name="defaultValue">Default value if the value is not set.</param>
        /// <returns>Effect function.</returns>
        public Func<IStateMachineActionContext, Task> Update<T>(Func<T, T> valueUpdater, T defaultValue = default)
        {
            var self = this;
            return c => c.Behavior.Values.UpdateAsync(self.ValueName, valueUpdater, defaultValue);
        }

        /// <summary>
        /// Provides an effect that removes the specified global value.
        /// </summary>
        /// <returns>Effect function.</returns>
        public Func<IStateMachineActionContext, Task> Remove
        {
            get
            {
                var self = this;
                return c => c.Behavior.Values.RemoveAsync(self.ValueName);
            }
        }
    }

    public struct TransitionEffectGlobalNamespaceExpression
    {
        private readonly string NamespaceName;

        public TransitionEffectGlobalNamespaceExpression(string namespaceName)
        {
            NamespaceName = namespaceName;
        }

        /// <summary>
        /// Provides declarative effects based on the global namespace's value with the given name.
        /// </summary>
        /// <param name="valueName">Name of the global namespace's value used in the effect.</param>
        /// <returns>Declarative effects.</returns>
        public TransitionEffectGlobalValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionEffectGlobalValueExpression($"{self.NamespaceName}.{valueName}");
        }

        /// <summary>
        /// Provides an effect that clears all values in the specified global value set.
        /// </summary>
        /// <returns>Effect function.</returns>
        public Func<IStateMachineActionContext, Task> Clear
        {
            get
            {
                var self = this;
                return c =>
                    ((ContextValuesCollection)c.Behavior.Values).RemoveMatchingAsync(
                        new Regex($"{self.NamespaceName}[.](.*)"));
            }
        }
    }

    public struct TransitionEffectStateValueExpression
    {
        private readonly string ValueName;
        private readonly bool IsSource;

        public TransitionEffectStateValueExpression(string valueName, bool isSource)
        {
            ValueName = valueName;
            IsSource = isSource;
        }

        private IContextValues GetValueSet(ITransitionContext transitionContext)
            => IsSource
                ? transitionContext.Source.Values
                : transitionContext.Target?.Values;

        /// <summary>
        /// Provides an effect that sets the specified state's value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <returns>Effect function.</returns>
        public Func<ITransitionContext, Task> Set<T>(T value)
        {
            var self = this;
            return c =>
            {
                var valueSet = self.GetValueSet(c);
                return valueSet.SetAsync(self.ValueName, value);
            };
        }

        /// <summary>
        /// Provides an effect that updates the specified state's value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="valueUpdater">Function to update the value.</param>
        /// <param name="defaultValue">Default value if the value is not set.</param>
        /// <returns>Effect function.</returns>
        public Func<ITransitionContext, Task> Update<T>(Func<T, T> valueUpdater, T defaultValue = default)
        {
            var self = this;
            return c =>
            {
                var valueSet = self.GetValueSet(c);
                return valueSet.UpdateAsync(self.ValueName, valueUpdater, defaultValue);
            };
        }

        /// <summary>
        /// Provides an effect that removes the specified state's value.
        /// </summary>
        /// <returns>Effect function.</returns>
        public Func<ITransitionContext, Task> Remove
        {
            get
            {
                var self = this;
                return c =>
                {
                    var valueSet = self.GetValueSet(c);
                    return valueSet.RemoveAsync(self.ValueName);
                };
            }
        }
    }

    public struct TransitionEffectStateNamespaceExpression
    {
        private readonly string NamespaceName;
        private readonly bool IsSource;

        public TransitionEffectStateNamespaceExpression(string namespaceName, bool isSource)
        {
            NamespaceName = namespaceName;
            IsSource = isSource;
        }

        /// <summary>
        /// Provides declarative effects based on the state namespace's value with the given name.
        /// </summary>
        /// <param name="valueName">Name of the state namespace's value used in the effect.</param>
        /// <returns>Declarative effects.</returns>
        public TransitionEffectStateValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionEffectStateValueExpression($"{self.NamespaceName}.{valueName}", self.IsSource);
        }

        /// <summary>
        /// Provides an effect that clears all values in the specified state value set.
        /// </summary>
        /// <returns>Effect function.</returns>
        public Func<ITransitionContext, Task> Clear
        {
            get
            {
                var self = this;
                return c =>
                {
                    var valueSet = self.IsSource
                        ? c.Source.Values
                        : c.Target?.Values;

                    return ((ContextValuesCollection)valueSet)!.RemoveMatchingAsync(
                        new Regex($"{self.NamespaceName}[.](.*)"));
                };
            }
        }
    }

    public struct TransitionEffectGlobalSelector
    {
        /// <summary>
        /// Provides declarative effects. based on the global namespace's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the global namespace's value used in guard.</param>
        /// <returns>Declarative effects.</returns>
        public TransitionEffectGlobalValueExpression Value(string valueName)
            => new TransitionEffectGlobalValueExpression(valueName);
        
        public TransitionEffectGlobalNamespaceExpression Namespace(string namespaceName)
            => new TransitionEffectGlobalNamespaceExpression(namespaceName);
    }

    public struct TransitionEffectStateSelector
    {
        private readonly bool IsSource;

        public TransitionEffectStateSelector(bool isSource)
        {
            IsSource = isSource;
        }

        /// <summary>
        /// Provides declarative effects. based on the state namespace's value with given name.
        /// </summary>
        /// <param name="valueName">Name of the state namespace's value used in guard.</param>
        /// <returns>Declarative effects.</returns>
        public TransitionEffectStateValueExpression Value(string valueName)
        {
            var self = this;
            return new TransitionEffectStateValueExpression(valueName, self.IsSource);
        }
        
        public TransitionEffectStateNamespaceExpression Namespace(string namespaceName)
        {
            var self = this;
            return new TransitionEffectStateNamespaceExpression(namespaceName, self.IsSource);
        }
    }
    
    public static class Effects
    {
        /// <summary>
        /// Declares an empty effect that does nothing.
        /// </summary>
        public static Task Empty<TEvent>(ITransitionContext<TEvent> _)
            => Task.CompletedTask;

        /// <summary>
        /// Provides declarative effects based on global values.
        /// </summary>
        public static TransitionEffectGlobalSelector Global
            => new TransitionEffectGlobalSelector();
        
        /// <summary>
        /// Provides declarative effects based on the source values.
        /// </summary>
        public static TransitionEffectStateSelector Source
            => new TransitionEffectStateSelector(true);
        
        /// <summary>
        /// Provides declarative effects based on the target values.
        /// </summary>
        public static TransitionEffectStateSelector Target
            => new TransitionEffectStateSelector(false);
    }
}
