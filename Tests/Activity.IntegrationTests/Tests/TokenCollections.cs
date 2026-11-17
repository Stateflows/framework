using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class OptionalPassAll(IOptionalInputOutputTokens<int> tokens) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Enumerate explicitly: LINQ uses ICollection<T>.Count, which reflects the output side.
            foreach (var _ in tokens)
            {
                TokenCollections.OptionalAvailable = true;
            }
            tokens.PassAllOn();
            tokens.Add(100);
        }
    }

    public class OptionalPassOne(IOptionalInputOutputToken<int> token) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TokenCollections.OptionalAvailable = token.IsAvailable;
            token.PassOn();
        }
    }

    public class OutputEditor(IOutputTokens<int> output) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            output.AddRange([1, 2, 3, 2]);
            TokenCollections.ContainsTwo = output.Contains(2);
            TokenCollections.ContainsFive = output.Contains(5);
            TokenCollections.Removed = output.Remove(2);
            TokenCollections.RemovedMissing = output.Remove(5);
            var array = new int[output.Count];
            output.CopyTo(array, 0);
            TokenCollections.Copied = array.ToList();
        }
    }

    public class OutputClearer(IOutputTokens<int> output, IOutputTokens<string> texts) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            output.AddRange([1, 2]);
            texts.Add("kept");
            output.Clear();
            TokenCollections.CountAfterClear = output.Count;
        }
    }

    [TestClass]
    public class TokenCollections : StateflowsTestClass
    {
        public static bool OptionalAvailable;
        public static bool ContainsTwo;
        public static bool ContainsFive;
        public static bool Removed;
        public static bool RemovedMissing;
        public static List<int> Copied = [];
        public static int CountAfterClear = -1;
        public static List<int> Received = [];
        public static List<string> ReceivedTexts = [];

        [TestInitialize]
        public override void Initialize()
        {
            OptionalAvailable = false;
            ContainsTwo = ContainsFive = Removed = RemovedMissing = false;
            Copied = [];
            CountAfterClear = -1;
            Received = [];
            ReceivedTexts = [];
            base.Initialize();
        }

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddActivities(b => b
                .AddActivity("optional-pass-all", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", async c => c.OutputRange([1, 2]), b => b.AddFlow<int, OptionalPassAll>())
                    .AddAction<OptionalPassAll>(b => b.AddFlow<int>("collect"))
                    .AddAction("collect", async c => Received.AddRange(c.GetTokensOfType<int>()))
                )
                .AddActivity("optional-pass-one", b => b
                    .AddInitial(b => b.AddControlFlow("produce"))
                    .AddAction("produce", async c => c.Output(7), b => b.AddFlow<int, OptionalPassOne>())
                    .AddAction<OptionalPassOne>(b => b.AddFlow<int>("collect"))
                    .AddAction("collect", async c => Received.AddRange(c.GetTokensOfType<int>()))
                )
                .AddActivity("output-editor", b => b
                    .AddInitial(b => b.AddControlFlow<OutputEditor>())
                    .AddAction<OutputEditor>(b => b.AddFlow<int>("collect"))
                    .AddAction("collect", async c => Received.AddRange(c.GetTokensOfType<int>()))
                )
                .AddActivity("output-clearer", b => b
                    .AddInitial(b => b.AddControlFlow<OutputClearer>())
                    .AddAction<OutputClearer>(b => b
                        .AddFlow<int>("collectInts")
                        .AddFlow<string>("collectTexts")
                    )
                    .AddAction("collectInts", async c => Received.AddRange(c.GetTokensOfType<int>()))
                    .AddAction("collectTexts", async c => ReceivedTexts.AddRange(c.GetTokensOfType<string>()))
                )
            );
        }

        [TestMethod]
        public async Task OptionalInputOutputTokensPassAllOnAndAdd()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("optional-pass-all", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.IsTrue(OptionalAvailable);
            CollectionAssert.AreEquivalent(new[] { 1, 2, 100 }, Received);
        }

        [TestMethod]
        public async Task OptionalInputOutputTokenPassOn()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("optional-pass-one", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.IsTrue(OptionalAvailable);
            CollectionAssert.AreEqual(new[] { 7 }, Received);
        }

        [TestMethod]
        public async Task OutputTokensContainsRemoveCopyTo()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("output-editor", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.IsTrue(ContainsTwo);
            Assert.IsFalse(ContainsFive);
            Assert.IsTrue(Removed);
            Assert.IsFalse(RemovedMissing);
            CollectionAssert.AreEqual(new[] { 1, 3, 2 }, Copied);
            CollectionAssert.AreEquivalent(new[] { 1, 3, 2 }, Received);
        }

        [TestMethod]
        public async Task OutputTokensClearRemovesOnlyItsType()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("output-clearer", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(0, CountAfterClear);
            Assert.AreEqual(0, Received.Count);
            CollectionAssert.AreEqual(new[] { "kept" }, ReceivedTexts);
        }
    }
}
