using System.Threading.Tasks;

namespace Stateflows.Testing.Activities.Cradle
{
    public interface ITestCradle
    {
        public Task<ITestResults> SwingAsync();
    }
}
