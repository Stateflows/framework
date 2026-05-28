using Activity.IntegrationTests.Classes.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Stateflows.Activities.Exceptions;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    // ---------- typed action nodes used across scenarios ----------

    /// <summary>
    /// Accepts Animal tokens; used to verify that a Dog arriving on an Animal-typed edge is received.
    /// </summary>
    public class AnimalConsumer(IInputTokens<Animal> animals) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TypeVariance.ConsumedAnimals.AddRange(animals);
        }
    }

    /// <summary>
    /// Accepts IAnimal tokens; verifies interface-typed input works with derived concrete types.
    /// </summary>
    public class IAnimalConsumer(IInputTokens<IAnimal> animals) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TypeVariance.ConsumedIAnimals.AddRange(animals.Select(a => a.Name));
        }
    }

    /// <summary>
    /// Accepts an optional single Animal; verifies IOptionalInputToken with subtype.
    /// </summary>
    public class OptionalAnimalConsumer(IOptionalInputToken<Animal> animal) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TypeVariance.OptionalAnimalAvailable = animal.IsAvailable;
            TypeVariance.OptionalAnimalName = animal.TokenOrDefault?.Name;
        }
    }

    /// <summary>
    /// Accepts an optional single Dog; verifies IOptionalInputToken&lt;Dog&gt; receives a Dog
    /// that arrived via an Animal-typed edge.
    /// </summary>
    public class OptionalDogConsumer(IOptionalInputToken<Dog> dog) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            TypeVariance.OptionalDogAvailable = dog.IsAvailable;
            TypeVariance.OptionalDogBreed = dog.TokenOrDefault?.Breed;
            TypeVariance.OptionalDogName = dog.TokenOrDefault?.Name;
        }
    }

    /// <summary>
    /// Class-based producer that declares Dog output via IOutputTokens&lt;Dog&gt;.
    /// </summary>
    public class DogProducer(IOutputTokens<Dog> dogs) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            dogs.Add(new Dog { Name = "Bruno", Breed = "Shepherd" });
        }
    }

    /// <summary>
    /// Requires Dog tokens as mandatory input — used only to verify build-time rejection of mismatched edges.
    /// </summary>
    public class RequiredDogConsumer(IInputTokens<Dog> dogs) : IActionNode
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken) { }
    }

    // ---------- test class ----------

    [TestClass]
    public class TypeVariance : StateflowsTestClass
    {
        // Results collected by typed action nodes above
        public static List<Animal> ConsumedAnimals = [];
        public static List<string> ConsumedIAnimals = [];
        public static bool OptionalAnimalAvailable = false;
        public static string? OptionalAnimalName = null;
        public static bool OptionalDogAvailable = false;
        public static string? OptionalDogBreed = null;
        public static string? OptionalDogName = null;

        [TestInitialize]
        public override void Initialize()
        {
            ConsumedAnimals = [];
            ConsumedIAnimals = [];
            OptionalAnimalAvailable = false;
            OptionalAnimalName = null;
            OptionalDogAvailable = false;
            OptionalDogBreed = null;
            OptionalDogName = null;
            base.Initialize();
        }

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder.AddActivities(b => b

                // Scenario 1: output Dog on an Animal-typed edge, consume as Animal
                .AddActivity("dog-flows-on-animal-edge", b => b
                    .AddInitial(b => b
                        .AddControlFlow("produce")
                    )
                    .AddAction(
                        "produce",
                        async c => c.Output(new Dog { Name = "Rex", Breed = "Labrador" }),
                        b => b.AddFlow<Animal, AnimalConsumer>()
                    )
                    .AddAction<AnimalConsumer>()
                )

                // Scenario 2: output multiple derived types on Animal-typed edges
                .AddActivity("mixed-derived-types", b => b
                    .AddInitial(b => b
                        .AddControlFlow("produce")
                    )
                    .AddAction(
                        "produce",
                        async c =>
                        {
                            c.Output(new Dog { Name = "Rex", Breed = "Labrador" });
                            c.Output(new Cat { Name = "Whiskers", Color = "Orange" });
                        },
                        b => b.AddFlow<Animal, AnimalConsumer>()
                    )
                    .AddAction<AnimalConsumer>()
                )

                // Scenario 3: edge typed as interface IAnimal, tokens are concrete Dog
                .AddActivity("dog-flows-on-iAnimal-edge", b => b
                    .AddInitial(b => b
                        .AddControlFlow("produce")
                    )
                    .AddAction(
                        "produce",
                        async c => c.Output(new Dog { Name = "Buddy", Breed = "Poodle" }),
                        b => b.AddFlow<IAnimal, IAnimalConsumer>()
                    )
                    .AddAction<IAnimalConsumer>()
                )

                // Scenario 4: IOptionalInputToken<Animal> receives a Dog token
                .AddActivity("optional-animal-receives-dog", b => b
                    .AddInitial(b => b
                        .AddControlFlow("produce")
                    )
                    .AddAction(
                        "produce",
                        async c => c.Output(new Dog { Name = "Max", Breed = "Beagle" }),
                        b => b.AddFlow<Animal, OptionalAnimalConsumer>()
                    )
                    .AddAction<OptionalAnimalConsumer>()
                )

                // Scenario 5: exact-type edge still works — Animal output on Animal edge
                .AddActivity("exact-type-still-works", b => b
                    .AddInitial(b => b
                        .AddControlFlow("produce")
                    )
                    .AddAction(
                        "produce",
                        async c => c.Output(new Animal { Name = "Generic" }),
                        b => b.AddFlow<Animal, AnimalConsumer>()
                    )
                    .AddAction<AnimalConsumer>()
                )

                // Scenario 6: token does NOT flow on an unrelated edge (Dog must not reach string consumer)
                .AddActivity("unrelated-type-does-not-flow", b => b
                    .AddInitial(b => b
                        .AddControlFlow("produce")
                    )
                    .AddAction(
                        "produce",
                        async c =>
                        {
                            c.Output(new Dog { Name = "Ghost" });
                            c.Output("hello");
                        },
                        b => b
                            .AddFlow<Animal, AnimalConsumer>()
                            .AddFlow<string>("string-sink")
                    )
                    .AddAction<AnimalConsumer>()
                    .AddAction("string-sink", async c => { })
                )

                // Scenario 7: Dog produced, flows on Animal edge, consumer accepts IOptionalInputToken<Dog>
                .AddActivity("dog-via-animal-edge-optional-dog-consumer", b => b
                    .AddInitial(b => b
                        .AddControlFlow("produce")
                    )
                    .AddAction(
                        "produce",
                        async c => c.Output(new Dog { Name = "Daisy", Breed = "Husky" }),
                        b => b.AddFlow<Animal, OptionalDogConsumer>()
                    )
                    .AddAction<OptionalDogConsumer>()
                )

                // Scenario 8: class-based DogProducer → Animal-typed edge → AnimalConsumer
                .AddActivity("class-producer-dog-on-animal-edge", b => b
                    .AddInitial(b => b
                        .AddControlFlow<DogProducer>()
                    )
                    .AddAction<DogProducer>(b => b
                        .AddFlow<Animal, AnimalConsumer>()
                    )
                    .AddAction<AnimalConsumer>()
                )

                // Scenario 9: class-based DogProducer → Dog-typed edge → OptionalDogConsumer
                .AddActivity("class-producer-dog-on-dog-edge", b => b
                    .AddInitial(b => b
                        .AddControlFlow<DogProducer>()
                    )
                    .AddAction<DogProducer>(b => b
                        .AddFlow<Dog, OptionalDogConsumer>()
                    )
                    .AddAction<OptionalDogConsumer>()
                )

                // Scenario 10: class-based DogProducer → IAnimal-typed edge → IAnimalConsumer
                .AddActivity("class-producer-dog-on-iAnimal-edge", b => b
                    .AddInitial(b => b
                        .AddControlFlow<DogProducer>()
                    )
                    .AddAction<DogProducer>(b => b
                        .AddFlow<IAnimal, IAnimalConsumer>()
                    )
                    .AddAction<IAnimalConsumer>()
                )

                // Scenario 11: DogProducer → Animal-typed edge → IAnimalConsumer
                // Tests that a Dog flowing on an Animal edge is received by an IAnimal consumer.
                .AddActivity("class-producer-dog-on-animal-edge-to-iAnimal-consumer", b => b
                    .AddInitial(b => b
                        .AddControlFlow<DogProducer>()
                    )
                    .AddAction<DogProducer>(b => b
                        .AddFlow<Animal, IAnimalConsumer>()
                    )
                    .AddAction<IAnimalConsumer>()
                )
            );
        }

        // ---- test methods ----

        [TestMethod]
        public async Task DogFlowsOnAnimalTypedEdge()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("dog-flows-on-animal-edge", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(1, ConsumedAnimals.Count, "Exactly one Animal-typed token should arrive");
            Assert.IsInstanceOfType<Dog>(ConsumedAnimals[0]);
            Assert.AreEqual("Rex", ConsumedAnimals[0].Name);
        }

        [TestMethod]
        public async Task MixedDerivedTypesAllFlowOnAnimalEdge()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("mixed-derived-types", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(2, ConsumedAnimals.Count, "Both Dog and Cat should arrive as Animal tokens");
            CollectionAssert.AreEquivalent(
                new[] { "Rex", "Whiskers" },
                ConsumedAnimals.Select(animal => animal.Name).ToArray()
            );
        }

        [TestMethod]
        public async Task DogFlowsOnInterfaceTypedEdge()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("dog-flows-on-iAnimal-edge", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(1, ConsumedIAnimals.Count, "Dog should arrive via IAnimal-typed edge");
            Assert.AreEqual("Buddy", ConsumedIAnimals[0]);
        }

        [TestMethod]
        public async Task OptionalInputReceivesDerivedToken()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("optional-animal-receives-dog", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.IsTrue(OptionalAnimalAvailable, "IOptionalInputToken<Animal>.IsAvailable should be true when a Dog was sent");
            Assert.AreEqual("Max", OptionalAnimalName);
        }

        [TestMethod]
        public async Task ExactTypeEdgeStillWorks()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("exact-type-still-works", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(1, ConsumedAnimals.Count);
            Assert.AreEqual("Generic", ConsumedAnimals[0].Name);
        }

        [TestMethod]
        public async Task UnrelatedTypeDoesNotFlowOnAnimalEdge()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("unrelated-type-does-not-flow", "x"), out var a))
                await a.SendAsync(new Initialize());

            // Only the Dog should reach AnimalConsumer; the string must not be received there
            Assert.AreEqual(1, ConsumedAnimals.Count, "Only Dog should arrive at AnimalConsumer, not the string");
            Assert.AreEqual("Ghost", ConsumedAnimals[0].Name);
        }

        [TestMethod]
        public async Task DogViaAnimalEdgeReceivedAsOptionalDog()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("dog-via-animal-edge-optional-dog-consumer", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.IsTrue(OptionalDogAvailable, "IOptionalInputToken<Dog>.IsAvailable should be true when a Dog was sent via Animal-typed edge");
            Assert.AreEqual("Daisy", OptionalDogName);
            Assert.AreEqual("Husky", OptionalDogBreed);
        }

        [TestMethod]
        public async Task ClassProducerDogFlowsOnAnimalEdge()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("class-producer-dog-on-animal-edge", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(1, ConsumedAnimals.Count, "DogProducer (class) should deliver Dog to AnimalConsumer via Animal-typed edge");
            Assert.IsInstanceOfType<Dog>(ConsumedAnimals[0]);
            Assert.AreEqual("Bruno", ConsumedAnimals[0].Name);
        }

        [TestMethod]
        public async Task ClassProducerDogFlowsOnDogEdgeToOptionalConsumer()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("class-producer-dog-on-dog-edge", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.IsTrue(OptionalDogAvailable, "DogProducer (class) should deliver Dog to OptionalDogConsumer via Dog-typed edge");
            Assert.AreEqual("Bruno", OptionalDogName);
            Assert.AreEqual("Shepherd", OptionalDogBreed);
        }

        [TestMethod]
        public async Task ClassProducerDogFlowsOnIAnimalEdge()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("class-producer-dog-on-iAnimal-edge", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(1, ConsumedIAnimals.Count, "Dog should arrive via IAnimal-typed edge");
            Assert.AreEqual("Bruno", ConsumedIAnimals[0]);
        }

        [TestMethod]
        public async Task ClassProducerDogOnAnimalEdgeReceivedByIAnimalConsumer()
        {
            // Producer declares Dog output, edge is Animal-typed, consumer accepts IAnimal.
            // Verifies the full covariant chain: Dog ⊆ Animal ⊆ IAnimal.
            if (ActivityLocator.TryLocateActivity(new ActivityId("class-producer-dog-on-animal-edge-to-iAnimal-consumer", "x"), out var a))
                await a.SendAsync(new Initialize());

            Assert.AreEqual(1, ConsumedIAnimals.Count, "Dog should flow on Animal edge and be received by IAnimalConsumer");
            Assert.AreEqual("Bruno", ConsumedIAnimals[0]);
        }
    }

    // ---------- build-time validation tests ----------

    [TestClass]
    public class TypeVarianceBuildValidation
    {
        /// <summary>
        /// DogProducer declares IOutputTokens&lt;Dog&gt; but is connected to a string-typed edge.
        /// Dog and string share no inheritance relationship → build-time exception expected.
        /// </summary>
        [TestMethod]
        public void ProducerOutputTypeMismatch_ThrowsAtBuildTime()
        {
            Assert.ThrowsException<NodeDefinitionException>(() =>
                new ServiceCollection().AddStateflows(b => b.AddActivities(ab => ab
                    .AddActivity("invalid-producer-edge", b => b
                        .AddInitial(b => b.AddControlFlow<DogProducer>())
                        .AddAction<DogProducer>(b => b.AddFlow<string>("sink"))
                        .AddAction("sink", async c => { })
                    )
                ))
            );
        }

        /// <summary>
        /// An anonymous action outputs a Cat and connects via a Cat-typed edge to RequiredDogConsumer
        /// which declares IInputTokens&lt;Dog&gt;. Cat and Dog are siblings (neither is assignable to the other)
        /// → build-time exception expected.
        /// </summary>
        [TestMethod]
        public void IncomingTokenTypeMismatch_ThrowsAtBuildTime()
        {
            Assert.ThrowsException<NodeDefinitionException>(() =>
                new ServiceCollection().AddStateflows(b => b.AddActivities(ab => ab
                    .AddActivity("invalid-consumer-edge", b => b
                        .AddInitial(b => b.AddControlFlow("produce"))
                        .AddAction(
                            "produce",
                            async c => c.Output(new Cat { Name = "Felix", Color = "Black" }),
                            b => b.AddFlow<Cat, RequiredDogConsumer>()
                        )
                        .AddAction<RequiredDogConsumer>()
                    )
                ))
            );
        }

        /// <summary>
        /// An anonymous action outputs a Cat and connects via a Animal-typed edge to RequiredDogConsumer
        /// which declares IInputTokens&lt;Dog&gt;. Cat and Dog are siblings (neither is assignable to the other)
        /// → build-time exception expected.
        /// </summary>
        [TestMethod]
        public void IncomingTokenTypeMismatch2_ThrowsAtBuildTime()
        {
            Assert.ThrowsException<NodeDefinitionException>(() =>
                new ServiceCollection().AddStateflows(b => b.AddActivities(ab => ab
                    .AddActivity("invalid-consumer-edge-2", b => b
                        .AddInitial(b => b.AddControlFlow("produce"))
                        .AddAction(
                            "produce",
                            async c => c.Output(new Cat { Name = "Felix", Color = "Black" }),
                            b => b.AddFlow<Animal, RequiredDogConsumer>()
                        )
                        .AddAction<RequiredDogConsumer>()
                    )
                ))
            );
        }
    }
}













