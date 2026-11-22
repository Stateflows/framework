using System;
using System.Threading;

namespace Stateflows.Common.Engine;

/// <summary>Infrastructure for associating framework handles with the current source execution.</summary>
internal static class BehaviorDispatchAmbient
{
    private sealed class Suppression { public volatile bool Active = true; }
    private static readonly AsyncLocal<IBehaviorDispatchObserver> observer = new();
    private static readonly AsyncLocal<Suppression> suppression = new();

    /// <summary>Gets the active execution's dispatch bridge, unless an enclosing handle already observes this call.</summary>
    internal static IBehaviorDispatchObserver Current => suppression.Value is { Active: true } ? null : observer.Value;

    /// <summary>Enters a source execution, restoring the preceding execution on disposal.</summary>
    internal static IDisposable Enter(IBehaviorDispatchObserver value)
    {
        var old = observer.Value;
        var oldSuppression = suppression.Value;
        observer.Value = value;
        suppression.Value = null;
        return new Scope(() => { observer.Value = old; suppression.Value = oldSuppression; });
    }

    /// <summary>Suppresses duplicate instrumentation while an outer handle starts the same dispatch.</summary>
    internal static IDisposable Suppress()
    {
        var old = suppression.Value;
        var marker = new Suppression();
        suppression.Value = marker;
        return new Scope(() => { marker.Active = false; suppression.Value = old; });
    }

    private sealed class Scope(System.Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }
}
