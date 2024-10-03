using Stateflows.Activities.Typed;
using Stateflows.Common;
using StateMachine.IntegrationTests.Utils;

namespace Activity.IntegrationTests.Tests
{
    public class SomeException : Exception
    {
        public SomeException(string? message) : base(message) { }
    }

    public class OtherException : Exception
    {
        public OtherException(string? message) : base(message) { }
    }

    public class Main : IStructuredActivityNode
    {
    }

    [TestClass]
    public class Exceptions : StateflowsTestClass
    {
        private bool Executed1 = false;
        private bool Executed2 = false;
        private bool Executed3 = false;
        private bool ExceptionHandlerOutput = false;
        private static string Value1 = "boo";
        private static string Value2 = "boo";
        private static string Value3 = "boo";

        [TestInitialize]
        public override void Initialize()
            => base.Initialize();

        [TestCleanup]
        public override void Cleanup()
            => base.Cleanup();

        protected override void InitializeStateflows(IStateflowsBuilder builder)
        {
            builder
                .AddActivities(b => b
                    .AddActivity("specific", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddExceptionHandler<Exception>(async c =>
                            {
                                c.Output("generic");
                            })
                            .AddExceptionHandler<SomeException>(async c =>
                            {
                                c.Output(c.Exception.Message);
                            })
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    c.Output(42);
                                    throw new SomeException("test");
                                },
                                b => b.AddFlow<int>("action2")
                            )
                            .AddAction("action2",
                                async c =>
                                {
                                    Executed2 = true;
                                }
                            )
                            .AddFlow<string>("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed1 = true;
                            Value1 = c.GetTokensOfType<string>().FirstOrDefault() ?? "foo";
                        })
                    )
                    .AddActivity("generic", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddExceptionHandler<Exception>(async c =>
                            {
                                c.Output(c.Exception.Message);
                            })
                            .AddExceptionHandler<SomeException>(async c =>
                            {
                                c.Output("specific");
                            })
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1",
                                async c =>
                                {
                                    c.Output(42);
                                    throw new OtherException("test");
                                },
                                b => b.AddFlow<int>("action2")
                            )
                            .AddAction("action2",
                                async c =>
                                {
                                    Executed2 = true;
                                }
                            )
                            .AddFlow<string>("final")
                        )
                        .AddAction("final", async c =>
                        {
                            Executed1 = true;
                            Value1 = c.GetTokensOfType<string>().FirstOrDefault() ?? "foo";
                        })
                    )
                    .AddActivity("structured", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddExceptionHandler<Exception>(async c =>
                            {
                                Executed2 = true;
                                Value1 = c.NodeOfOrigin.NodeName;
                                Value2 = c.ProtectedNode.NodeName;
                            })
                            .AddInitial(b => b
                                .AddControlFlow("faulty")
                            )
                            .AddAction("faulty", async c =>
                            {
                                Executed1 = true;
                                throw new Exception();
                            })
                        )
                    )
                    .AddActivity("structured-typed", b => b
                        .AddInitial(b => b
                            .AddControlFlow<Main>()
                        )
                        .AddStructuredActivity<Main>(b => b
                            .AddExceptionHandler<Exception>(async c =>
                            {
                                Executed2 = true;
                                Value1 = c.NodeOfOrigin.NodeName;
                                Value2 = c.ProtectedNode.NodeName;
                            })
                            .AddInitial(b => b
                                .AddControlFlow("faulty")
                            )
                            .AddAction("faulty", async c =>
                            {
                                Executed1 = true;
                                throw new Exception();
                            })
                        )
                    )
                    .AddActivity("guard", b => b
                        .AddInitial(b => b
                            .AddControlFlow("faulty")
                        )
                        .AddAction(
                            "faulty",
                            async c => { },
                            b => b
                                .AddExceptionHandler<Exception>(async c =>
                                {
                                    Executed2 = true;
                                    Value1 = c.NodeOfOrigin.NodeName;
                                    Value2 = c.ProtectedNode.NodeName;
                                })
                                .AddControlFlow<FinalNode>(b => b
                                    .AddGuard(async c =>
                                    {
                                        Executed1 = true;
                                        throw new Exception();
                                    })
                                )
                        )
                        .AddFinal()
                    )
                    .AddActivity("plain", b => b
                        .AddInitial(b => b
                            .AddControlFlow("faulty")
                        )
                        .AddAction(
                            "faulty",
                            async c => throw new Exception(),
                            b => b
                                .AddExceptionHandler<Exception>(async c =>
                                {
                                    Executed1 = true;
                                    Value1 = c.NodeOfOrigin.NodeName;
                                    Value2 = c.ProtectedNode.NodeName;
                                })
                        )
                    )
                    .AddActivity("plain-specific", b => b
                        .AddInitial(b => b
                            .AddControlFlow("faulty")
                        )
                        .AddAction(
                            "faulty",
                            async c => throw new SomeException("foo"),
                            b => b
                                .AddExceptionHandler<Exception>(async c => Executed1 = true)
                                .AddExceptionHandler<SomeException>(async c => Executed2 = true)
                        )
                    )
                    .AddActivity("plain-generic", b => b
                        .AddInitial(b => b
                            .AddControlFlow("faulty")
                        )
                        .AddAction(
                            "faulty",
                            async c => throw new SomeException("foo"),
                            b => b
                                .AddExceptionHandler<Exception>(async c => Executed1 = true)
                        )
                    )
                    .AddActivity("structured-guard", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddExceptionHandler<Exception>(async c =>
                            {
                                Executed2 = true;
                                Value1 = c.NodeOfOrigin.NodeName;
                                Value2 = c.ProtectedNode.NodeName;
                                Value3 = c.Exception?.Message;
                            })
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction(
                                "action1",
                                async c => { },
                                b => b
                                    .AddControlFlow("action2", b => b
                                        .AddGuard(async c =>
                                        {
                                            Executed1 = true;
                                            throw new Exception("foo");
                                        })
                                    )
                            )
                            .AddAction("action2", async c =>
                            {
                                Executed3 = true;
                            })
                        )
                    )
                    .AddActivity("output", b => b
                        .AddInitial(b => b
                            .AddControlFlow("main")
                        )
                        .AddStructuredActivity("main", b => b
                            .AddExceptionHandler<Exception>(async c =>
                            {
                                c.Output(c.Exception.Message);
                            })
                            .AddInitial(b => b
                                .AddControlFlow("action1")
                            )
                            .AddAction("action1", async c =>
                            {
                                throw new Exception("test");
                            })

                            .AddFlow<string>("final")
                        )
                        .AddAction("final", async c =>
                        {
                            ExceptionHandlerOutput = c.GetTokensOfType<string>().Any();
                        })
                    )
                )
                ;
        }

        [TestMethod]
        public async Task SpecificExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("specific", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
            Assert.IsFalse(Executed2);
            Assert.AreEqual("test", Value1);
        }

        [TestMethod]
        public async Task GenericExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("generic", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
            Assert.IsFalse(Executed2);
            Assert.AreEqual("test", Value1);
        }

        [TestMethod]
        public async Task StructuredExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("structured", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
            Assert.IsTrue(Executed2);
            Assert.AreEqual("main.faulty", Value1);
            Assert.AreEqual("main", Value2);
        }

        [TestMethod]
        public async Task StructuredTypedExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("structured-typed", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
            Assert.IsTrue(Executed2);
            Assert.AreEqual("Activity.IntegrationTests.Tests.Main.faulty", Value1);
            Assert.AreEqual("Activity.IntegrationTests.Tests.Main", Value2);
        }

        [TestMethod]
        public async Task StructuredGuardExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("structured-guard", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
            Assert.IsTrue(Executed2);
            Assert.AreEqual("main.action1", Value1);
            Assert.AreEqual("main", Value2);
            Assert.AreEqual("foo", Value3);
        }

        [TestMethod]
        public async Task PlainExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("plain", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
            Assert.AreEqual("faulty", Value1);
        }

        [TestMethod]
        public async Task PlainSpecificExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("plain-specific", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed2);
        }

        [TestMethod]
        public async Task PlainGenericExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("plain-generic", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
        }

        [TestMethod]
        public async Task GuardExceptionHandled()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("guard", "x"), out var a))
            {
                await a.SendAsync(new Initialize());
            }

            Assert.IsTrue(Executed1);
            Assert.IsTrue(Executed2);
            Assert.AreEqual("faulty", Value1);
        }

        [TestMethod]
        public async Task ExceptionHandledWithOutput()
        {
            if (ActivityLocator.TryLocateActivity(new ActivityId("output", "x"), out var a))
            {
                await a.ExecuteAsync();
            }

            Assert.IsTrue(ExceptionHandlerOutput);
        }
    }
}