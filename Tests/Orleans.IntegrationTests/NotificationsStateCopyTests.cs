using Stateflows;

namespace Orleans.IntegrationTests;

[TestClass]
public sealed class NotificationsStateCopyTests
{
    [TestMethod]
    [DataRow((int)NotificationsState.Collections.PublicationWatermarks)]
    [DataRow((int)(NotificationsState.Collections.History | NotificationsState.Collections.Pending))]
    [DataRow((int)(NotificationsState.Collections.History | NotificationsState.Collections.Pending |
        NotificationsState.Collections.LatestRetained))]
    [DataRow((int)(NotificationsState.Collections.History | NotificationsState.Collections.Pending |
        NotificationsState.Collections.LatestRetained | NotificationsState.Collections.PublicationWatermarks))]
    [DataRow((int)(NotificationsState.Collections.Subscriptions | NotificationsState.Collections.Pending))]
    [DataRow((int)NotificationsState.Collections.Pending)]
    public void CandidateCopiesOnlySelectedCollections(int selectedCollections)
    {
        var collections = (NotificationsState.Collections)selectedCollections;
        var state = new NotificationsState
        {
            LastSequence = 42,
            LegacyMigrated = true,
            History = [new StoredNotification { Sequence = 42 }],
            Subscriptions = new() { ["subscriber"] = ["event"] },
            Pending = new() { ["subscriber"] = [new PendingDelivery { Sequence = 42 }] },
            LatestRetained = new() { ["event"] = 42 },
            PublicationWatermarks = new() { ["source"] = 7 },
        };

        var next = state.Clone(collections);

        Assert.AreNotSame(state, next);
        Assert.AreEqual(state.Version, next.Version);
        Assert.AreEqual(state.LastSequence, next.LastSequence);
        Assert.AreEqual(state.LegacyMigrated, next.LegacyMigrated);
        Assert.AreEqual(!collections.HasFlag(NotificationsState.Collections.History),
            ReferenceEquals(state.History, next.History));
        Assert.AreEqual(!collections.HasFlag(NotificationsState.Collections.Subscriptions),
            ReferenceEquals(state.Subscriptions, next.Subscriptions));
        Assert.AreEqual(!collections.HasFlag(NotificationsState.Collections.Pending),
            ReferenceEquals(state.Pending, next.Pending));
        Assert.AreEqual(!collections.HasFlag(NotificationsState.Collections.LatestRetained),
            ReferenceEquals(state.LatestRetained, next.LatestRetained));
        Assert.AreEqual(!collections.HasFlag(NotificationsState.Collections.PublicationWatermarks),
            ReferenceEquals(state.PublicationWatermarks, next.PublicationWatermarks));
        Assert.AreSame(state.History[0], next.History[0]);
        Assert.AreSame(state.Pending["subscriber"][0], next.Pending["subscriber"][0]);

        if (collections.HasFlag(NotificationsState.Collections.History))
            next.History.Clear();
        if (collections.HasFlag(NotificationsState.Collections.Subscriptions))
            next.Subscriptions["subscriber"].Clear();
        if (collections.HasFlag(NotificationsState.Collections.Pending))
            next.Pending["subscriber"].Clear();
        if (collections.HasFlag(NotificationsState.Collections.LatestRetained))
            next.LatestRetained.Clear();
        if (collections.HasFlag(NotificationsState.Collections.PublicationWatermarks))
            next.PublicationWatermarks["source"] = 8;
        next.LastSequence++;

        Assert.AreEqual(1, state.History.Count);
        Assert.AreEqual(1, state.Subscriptions["subscriber"].Count);
        Assert.AreEqual(1, state.Pending["subscriber"].Count);
        Assert.AreEqual(42L, state.LatestRetained["event"]);
        Assert.AreEqual(7L, state.PublicationWatermarks["source"]);
        Assert.AreEqual(42L, state.LastSequence);
    }

    [TestMethod]
    public void SubscriberCandidateCopiesOnlyAffectedNestedCollections()
    {
        var state = new NotificationsState
        {
            Subscriptions = new() { ["first"] = ["event"], ["second"] = ["event"] },
            Pending = new()
            {
                ["first"] = [new PendingDelivery()],
                ["second"] = [new PendingDelivery()],
            },
        };

        var next = state.Clone(
            NotificationsState.Collections.Subscriptions | NotificationsState.Collections.Pending, "first");
        Assert.AreNotSame(state.Subscriptions["first"], next.Subscriptions["first"]);
        Assert.AreNotSame(state.Pending["first"], next.Pending["first"]);
        Assert.AreSame(state.Subscriptions["second"], next.Subscriptions["second"]);
        Assert.AreSame(state.Pending["second"], next.Pending["second"]);

        next.Subscriptions["first"].Clear();
        next.Pending["first"].Clear();
        next.Subscriptions.Remove("second");
        next.Pending.Remove("second");
        Assert.AreEqual(1, state.Subscriptions["first"].Count);
        Assert.AreEqual(1, state.Pending["first"].Count);
        Assert.IsTrue(state.Subscriptions.ContainsKey("second"));
        Assert.IsTrue(state.Pending.ContainsKey("second"));
    }
}
