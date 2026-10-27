using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Initializes a behavior in response to a typed initialization event.</summary>
    /// <typeparam name="TInitializationEvent">The initialization event payload type.</typeparam>
    public interface IInitializer<in TInitializationEvent>
    {
        /// <summary>Initializes the behavior using the supplied event.</summary>
        /// <param name="initializationEvent">The initialization event payload.</param>
        /// <returns><see langword="true"/> to accept initialization; <see langword="false"/> to leave the behavior uninitialized.</returns>
        Task<bool> OnInitializeAsync(TInitializationEvent initializationEvent);
    }

    /// <summary>Initializes a behavior without requiring a typed initialization payload.</summary>
    public interface IDefaultInitializer
    {
        /// <summary>Performs default initialization.</summary>
        /// <returns><see langword="true"/> to accept initialization; <see langword="false"/> to leave the behavior uninitialized.</returns>
        Task<bool> OnInitializeAsync();
    }
}
