namespace Stateflows.Activities
{
    internal interface IActivityPlugin : IActivityInterceptor, IActivityObserver, IActivityExceptionHandler
    { }
}
