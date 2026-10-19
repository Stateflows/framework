using Stateflows.Common;
using Stateflows.Common.Context;
using Stateflows.Common.Utilities;

namespace StateMachine.IntegrationTests.Tests
{
    [TestClass]
    public class TimeEventTriggerTimes
    {
        private static readonly DateTime UtcStart = new(2026, 1, 15, 10, 7, 30, DateTimeKind.Utc);

        public sealed class TenSecondDelay : DelayEvent
        {
            protected override TimeSpan Delay => TimeSpan.FromSeconds(10);
        }

        public sealed class LocalClockEvent : TimeEvent
        {
            protected override DateTime GetTriggerTime(DateTime startedAt)
                => startedAt.ToLocalTime().AddSeconds(5);
        }

        [TestMethod]
        public void DelayIsAppliedOnce()
        {
            var timeEvent = new TenSecondDelay();
            timeEvent.SetTriggerTime(UtcStart);

            Assert.AreEqual(UtcStart.AddSeconds(10), timeEvent.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, timeEvent.TriggerTime.Kind);
        }

        [TestMethod]
        public void BuiltInDelayTruncatesAndAppliesDelayOnceInUtc()
        {
            var timeEvent = new AfterTenMinutes();
            timeEvent.SetTriggerTime(UtcStart);

            Assert.AreEqual(new DateTime(2026, 1, 15, 10, 10, 0, DateTimeKind.Utc), timeEvent.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, timeEvent.TriggerTime.Kind);
        }

        [TestMethod]
        public void BuiltInIntervalIsCalculatedInUtc()
        {
            var timeEvent = new EveryFifteenMinutes();
            timeEvent.SetTriggerTime(UtcStart);

            Assert.AreEqual(new DateTime(2026, 1, 15, 10, 15, 0, DateTimeKind.Utc), timeEvent.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, timeEvent.TriggerTime.Kind);
        }

        [TestMethod]
        public void LocalStartIsConvertedToUtcBeforeCalculation()
        {
            var timeEvent = new TenSecondDelay();
            timeEvent.SetTriggerTime(UtcStart.ToLocalTime());

            Assert.AreEqual(UtcStart.AddSeconds(10), timeEvent.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, timeEvent.TriggerTime.Kind);
        }

        [TestMethod]
        public void LocalTriggerTimeIsConvertedToUtc()
        {
            var timeEvent = new LocalClockEvent();
            timeEvent.SetTriggerTime(UtcStart);

            Assert.AreEqual(UtcStart.AddSeconds(5), timeEvent.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, timeEvent.TriggerTime.Kind);
        }

        [TestMethod]
        public void UnspecifiedTriggerTimeIsTreatedAsUtc()
        {
            var unspecified = new DateTime(2026, 1, 15, 10, 7, 30, DateTimeKind.Unspecified);
            var timeEvent = new TenSecondDelay { TriggerTime = unspecified };
            var context = new StateflowsContext { TriggerTime = unspecified };

            Assert.AreEqual(UtcStart, timeEvent.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, timeEvent.TriggerTime.Kind);
            Assert.AreEqual(UtcStart, context.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, context.TriggerTime!.Value.Kind);
        }

        [TestMethod]
        public void ContextTriggerTimeIsConvertedToUtc()
        {
            var context = new StateflowsContext { TriggerTime = UtcStart.ToLocalTime() };

            Assert.AreEqual(UtcStart, context.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, context.TriggerTime!.Value.Kind);
        }

        [TestMethod]
        public void TriggerTimesSurviveSerializationAsUtc()
        {
            var timeEvent = new TenSecondDelay();
            timeEvent.SetTriggerTime(UtcStart);
            var context = new StateflowsContext(new BehaviorId("StateMachine", "timed", "utc")) { TriggerTime = timeEvent.TriggerTime };
            context.PendingTimeEvents.Add(timeEvent.Id, timeEvent);

            var restored = StateflowsJsonConverter.DeserializeObject<StateflowsContext>(StateflowsJsonConverter.SerializePolymorphicObject(context))!;

            Assert.AreEqual(UtcStart.AddSeconds(10), restored.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, restored.TriggerTime!.Value.Kind);
            var restoredEvent = restored.PendingTimeEvents[timeEvent.Id];
            Assert.AreEqual(UtcStart.AddSeconds(10), restoredEvent.TriggerTime);
            Assert.AreEqual(DateTimeKind.Utc, restoredEvent.TriggerTime.Kind);
        }
    }
}
