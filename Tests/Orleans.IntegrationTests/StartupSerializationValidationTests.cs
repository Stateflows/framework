using Microsoft.Extensions.DependencyInjection;
using Stateflows;
using Stateflows.Activities;
using Stateflows.Common;
using Stateflows.Common.Utilities;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class StartupSerializationValidationTests
{
    [TestMethod]
    public void VerificationIsNotRegisteredUnlessEnabled()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(_ => { });
        using var provider = services.BuildServiceProvider();
        Assert.IsNull(provider.GetService<StartupSerializationValidator>());
    }

    [TestMethod]
    public async Task ManualEventAndTokenDeclarationsRejectDefectsWithoutRegisteringEvents()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(builder => builder.AddStartupSerializationValidation(validation => validation
            .AddEvent<EventSerializationRegressionTests.AmbiguousFailure>()
            .AddToken(typeof(EventSerializationRegressionTests.UnmatchedFailure))));
        using var provider = services.BuildServiceProvider();
        var failure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => provider.GetRequiredService<StartupSerializationValidator>().ValidateAsync());
        StringAssert.Contains(failure.Message, "manual event");
        StringAssert.Contains(failure.Message, "manual token");
        Assert.AreEqual(0, provider.GetRequiredService<IEventsRegister>().Events.Count);
    }

    [TestMethod]
    public async Task ActivityVisitorDiscoversDefectivePinToken()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(builder => builder
            .AddStartupSerializationValidation()
            .AddActivities(activities => activities.AddActivity("token-validation", activity => activity
                .AddInputPin<EventSerializationRegressionTests.UnmatchedFailure>(_ => { }))));
        using var provider = services.BuildServiceProvider();
        var failure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => provider.GetRequiredService<StartupSerializationValidator>().ValidateAsync());
        StringAssert.Contains(failure.Message, "token-validation");
        StringAssert.Contains(failure.Message, "SER003");
    }

    [TestMethod]
    public async Task SamplesRunOnlyDuringEnabledValidationAndDetectChangedValues()
    {
        var calls = 0;
        var services = new ServiceCollection().AddLogging();
        services.AddStateflows(builder => builder.AddStartupSerializationValidation(validation =>
            validation.AddSample(() =>
            {
                calls++;
                return new OverwrittenSample(42);
            })));
        Assert.AreEqual(0, calls);
        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<StartupSerializationValidator>();
        var failure = await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => validator.ValidateAsync());
        StringAssert.Contains(failure.Message, "SER004");
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => validator.ValidateAsync());
        Assert.AreEqual(1, calls);
    }

    [TestMethod]
    public void OpaqueContractsWarnButMatchingImmutableContractsHaveNoErrors()
    {
        Assert.IsTrue(SerializationContractValidator.Validate(typeof(object)).Any(diagnostic => !diagnostic.IsError));
        Assert.IsFalse(SerializationContractValidator.Validate(
            typeof(EventSerializationRegressionTests.MatchingFailure)).Any(diagnostic => diagnostic.IsError));
    }

    public sealed class OverwrittenSample
    {
        [Newtonsoft.Json.JsonConstructor]
        public OverwrittenSample(int value) => Value = value + 1;
        public int Value { get; }
    }
}
