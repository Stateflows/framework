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
                                .AddFlow<Animal, OutputNode>()
                                .AddFlow<string, OutputNode>()
                                .AddControlFlow<FinalNode>()
                        )
                        .AddOutput()
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
        public async Task GetOutputAsyncUsesRuntimePayloadType()
        {
            RequestResult<TokensOutput>? outputResult = null;
            RequestResult<TokensOutput<Dog>>? dogOutputResult = null;
            RequestResult<TokensOutput<IAnimal>>? iAnimalOutputResult = null;

            if (ActivityLocator.TryLocateActivity(new ActivityId("runtime-output-variance", "x"), out var activity))
            {
                await activity.SendAsync(new Initialize());
                outputResult = await activity.GetOutputAsync();
                dogOutputResult = await activity.GetOutputAsync<Dog>();
                iAnimalOutputResult = await activity.GetOutputAsync<IAnimal>();
            }

            Assert.IsNotNull(outputResult);
            Assert.IsNotNull(dogOutputResult);
            Assert.IsNotNull(iAnimalOutputResult);

            CollectionAssert.AreEqual(
                new[] { "Rex" },
                outputResult.Response.GetOfType<Dog>().Select(dog => dog.Name).ToArray()
            );
            CollectionAssert.AreEqual(
                new[] { "Mittens", "Rex" },
                outputResult.Response.GetOfType<IAnimal>().Select(animal => animal.Name).OrderBy(name => name).ToArray()
            );
            CollectionAssert.AreEqual(
                new[] { "Rex" },
                dogOutputResult.Response.GetAll().Select(dog => dog.Name).ToArray()
            );
            CollectionAssert.AreEqual(
                new[] { "Mittens", "Rex" },
                iAnimalOutputResult.Response.GetAll().Select(animal => animal.Name).OrderBy(name => name).ToArray()
            );
        }

        [TestMethod]
        public async Task WatchOutputAsyncUsesRuntimePayloadType()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("runtime-output-variance", "x"), out var activity))
            {
                await activity.WatchOutputAsync<Dog>(dogs =>
                {
                    watchedDogNames = dogs.Select(dog => dog.Name).ToList();
                });

                await activity.WatchOutputAsync<IAnimal>(animals =>
                {
                    watchedIAnimalNames = animals.Select(animal => animal.Name).OrderBy(name => name).ToList();
                });

                await activity.SendAsync(new Initialize());
            }

            CollectionAssert.AreEqual(new[] { "Rex" }, watchedDogNames);
            CollectionAssert.AreEqual(new[] { "Mittens", "Rex" }, watchedIAnimalNames);
        }
    }
}


