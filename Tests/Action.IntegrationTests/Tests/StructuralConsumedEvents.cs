using Microsoft.Extensions.DependencyInjection;
using Action.IntegrationTests.Classes.Events;
using StateMachine.IntegrationTests.Utils;
using Stateflows.Actions;
using Stateflows.Actions.Exceptions;
using Stateflows.Common;
using Stateflows.Events;

namespace Action.IntegrationTests.Tests
{
    internal class StructuralActionVisitor : ActionVisitor
    {
        public readonly List<(string ActionName, string EventName)> NamedEvents = [];
        public readonly List<(string ActionName, Type EventType)> TypedEvents = [];

        public override Task CustomEventAddedAsync(EventDefinition definition, string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
        {
            NamedEvents.Add((actionName, definition.Name));
            return Task.CompletedTask;
        }

        public override Task CustomEventAddedAsync<TEvent>(string actionName, int actionVersion, BehaviorStatus[] supportedStatuses)
        {
            TypedEvents.Add((actionName, typeof(TEvent)));
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class StructuralConsumedEvents : StateflowsTestClass
    {
        private static List<string> ReceivedNames = [];
        private static int ReceivedId;

        [TestInitialize]
        public override void Initialize()
        {
            ReceivedNames = [];
            ReceivedId = 0;
            base.Initialize();
        }

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddEvents(e => e
                    .AddEvent("order", b => b.AddField("id", FieldType.Int32))
                    .AddEvent("cancel", b => b.AddField("reason", FieldType.String))
                )
                .AddActions(b => b
                    .AddAction("structural", async c =>
                    {
                        var events = c.GetTokensOfType<StructuralEvent>().ToArray();
                        ReceivedNames.AddRange(events.Select(e => e.Name));
                        ReceivedId = events.FirstOrDefault(e => e.Name == "order")?.Get<int>("id") ?? 0;
                    }, a => a
                        .AddConsumedEvent("order")
                        .AddConsumedEvent("cancel")
                        .AddConsumedEvent("order")
                        .AddConsumedEvent<SomeEvent>()
                    )
                );
        }

        [TestMethod]
        public async Task NamedConsumedEventsAreVisitedWithDefinitions()
        {
            var visitor = new StructuralActionVisitor();
            await ServiceProvider.GetRequiredService<IActionsRegister>().VisitActionsAsync(visitor);

            CollectionAssert.AreEqual(
                new[] { "order", "cancel" },
                visitor.NamedEvents.Where(e => e.ActionName == "structural").Select(e => e.EventName).ToArray());
            CollectionAssert.AreEqual(
                new[] { typeof(SomeEvent) },
                visitor.TypedEvents.Where(e => e.ActionName == "structural").Select(e => e.EventType).ToArray());
        }

        [TestMethod]
        public async Task StructuralEventIsDeliveredAsToken()
        {
            var factory = ServiceProvider.GetRequiredService<IEventFactory>();
            Assert.IsTrue(ActionLocator.TryLocateAction(new ActionId("structural", "x"), out var action));

            var result = await action.SendAsync(factory.Create("order", b => b.Set("id", 42)));

            Assert.AreEqual(EventStatus.Consumed, result.Status);
            CollectionAssert.AreEqual(new[] { "order" }, ReceivedNames);
            Assert.AreEqual(42, ReceivedId);
        }

        [TestMethod]
        public void UnregisteredConsumedEventNameFailsAtBuild()
        {
            var services = new ServiceCollection();

            Assert.ThrowsExactly<ActionDefinitionException>(() => services.AddStateflows(b => b
                .AddActions(a => a.AddAction("missing", async c => { }, x => x.AddConsumedEvent("notRegistered")))
            ));
        }
    }
}
