using Activity.IntegrationTests.Classes.Tokens;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    [TestClass]
    public class OutputTypeVariance : StateflowsTestClass
    {
        private bool contextSelectionExecuted;
        private List<string> runtimeSelectedDogNames = [];
        private List<string> runtimeSelectedIAnimalNames = [];
        private List<string> watchedDogNames = [];
        private List<string> watchedIAnimalNames = [];

        [TestInitialize]
        public override void Initialize()
        {
            contextSelectionExecuted = false;
            runtimeSelectedDogNames = [];
            runtimeSelectedIAnimalNames = [];
            watchedDogNames = [];
            watchedIAnimalNames = [];
            base.Initialize();
        }

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddActivities(activities => activities
                    .AddActivity("runtime-context-selection", activity => activity
                        .AddInitial(initial => initial
                            .AddControlFlow("produce")
                        )
                        .AddAction(
                            "produce",
                            async c =>
                            {
                                Animal dog = new Dog { Name = "Rex", Breed = "Labrador" };
                                Animal cat = new Cat { Name = "Mittens", Color = "Black" };

                                c.Output(dog);
                                c.Output(cat);
                                c.Output("hello");
                            },
                            flow => flow
                                .AddFlow<Animal>("consume")
                                .AddFlow<string>("string-sink")
                        )
                        .AddAction(
                            "consume",
                            async c =>
                            {
                                contextSelectionExecuted = true;
                                runtimeSelectedDogNames = c.GetTokensOfType<Dog>().Select(dog => dog.Name).ToList();
                                runtimeSelectedIAnimalNames = c.GetTokensOfType<IAnimal>().Select(animal => animal.Name).OrderBy(name => name).ToList();
                            }
                        )
                        .AddAction("string-sink", async _ => { })
                    )
                    .AddActivity("runtime-output-variance", activity => activity
                        .AddInitial(initial => initial
                            .AddControlFlow("main")
                        )
                        .AddAction(
                            "main",
                            async c =>
                            {
                                Animal dog = new Dog { Name = "Rex", Breed = "Labrador" };
                                Animal cat = new Cat { Name = "Mittens", Color = "Black" };

                                c.Output(dog);
                                c.Output(cat);
                                c.Output("hello");
                            },
                            flow => flow
                                .AddFlow<Animal, OutputPin<IAnimal>>()
                                .AddFlow<Dog, OutputPin<Dog>>()
                                .AddFlow<string, OutputPin<string>>()
                                .AddControlFlow<FinalNode>()
                        )
                        .AddOutputPin<IAnimal>(pin => pin.SetTimeToLive(300))
                        .AddOutputPin<Dog>(pin => pin.SetTimeToLive(300))
                        .AddOutputPin<string>(pin => pin.SetTimeToLive(300))
                        .AddFinal()
                    )
                )
                ;
        }

        [TestMethod]
        public async Task ContextGetTokensOfTypeUsesRuntimePayloadType()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("runtime-context-selection", "x"), out var activity))
            {
                await activity.SendAsync(new Initialize());
            }

            Assert.IsTrue(contextSelectionExecuted, "Consumer action should execute.");
            CollectionAssert.AreEqual(new[] { "Rex" }, runtimeSelectedDogNames);
            CollectionAssert.AreEqual(new[] { "Mittens", "Rex" }, runtimeSelectedIAnimalNames);
        }

        [TestMethod]
        public async Task GetOutputAsyncQueriesDeclaredNotificationTypes()
        {
            IEnumerable<EventHolder>? outputResult = null;
            IEnumerable<Dog>? dogOutputResult = null;
            IEnumerable<IAnimal>? iAnimalOutputResult = null;

            if (ActivityLocator.TryLocateActivity(new ActivityId("runtime-output-variance", "x"), out var activity))
            {
                await activity.SendAsync(new Initialize());
                outputResult = await activity.GetOutputAsync([Event<Dog>.Name], DateTime.MinValue);
                dogOutputResult = await activity.GetOutputAsync<Dog>(DateTime.MinValue);
                iAnimalOutputResult = await activity.GetOutputAsync<IAnimal>(DateTime.MinValue);
            }

            Assert.IsNotNull(outputResult);
            Assert.IsNotNull(dogOutputResult);
            Assert.IsNotNull(iAnimalOutputResult);

            CollectionAssert.AreEqual(
                new[] { "Rex" },
                outputResult.Cast<EventHolder<Dog>>().Select(holder => holder.Payload.Name).ToArray()
            );
            CollectionAssert.AreEqual(
                new[] { "Rex" },
                dogOutputResult.Select(dog => dog.Name).ToArray()
            );
            CollectionAssert.AreEqual(
                new[] { "Mittens", "Rex" },
                iAnimalOutputResult.Select(animal => animal.Name).OrderBy(name => name).ToArray()
            );
        }

        [TestMethod]
        public async Task WatchOutputAsyncReceivesIndividualNotificationPayloads()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("runtime-output-variance", "x"), out var activity))
            {
                await using var dogs = await activity.WatchOutputAsync<Dog>(dog =>
                {
                    watchedDogNames.Add(dog.Name);
                });

                await using var animals = await activity.WatchOutputAsync<IAnimal>(animal =>
                {
                    watchedIAnimalNames.Add(animal.Name);
                });

                await activity.SendAsync(new Initialize());
                await dogs.WaitForDeliveryAsync();
                await animals.WaitForDeliveryAsync();
            }

            CollectionAssert.AreEqual(new[] { "Rex" }, watchedDogNames);
            CollectionAssert.AreEquivalent(new[] { "Mittens", "Rex" }, watchedIAnimalNames);
        }
    }
}
