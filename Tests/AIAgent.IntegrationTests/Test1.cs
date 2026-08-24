using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.IntegrationTests.Utils;
using Stateflows;
using Stateflows.Common;
using Stateflows.Common.Registration.Interfaces;
using Stateflows.MAF;
using Stateflows.MAF.AIAgents;

namespace AIAgent.IntegrationTests;

[TestClass]
[DoNotParallelize]
public sealed class AIAgentActionTests : StateflowsTestClass
{
    private static readonly AIAgentId AIAgentActionId = new("agent", "instance-1");
    
    protected IAIAgentLocator AIAgentLocator => ServiceProvider.GetRequiredService<IAIAgentLocator>();

    [TestInitialize]
    public override void Initialize()
    {
        SharedTestAIAgent.Instance.Reset();
        base.Initialize();
    }

    [TestCleanup]
    public override void Cleanup()
        => base.Cleanup();

    protected override void InitializeStateflows(IStateflowsBuilder builder)
        => builder.AddAIAgents(b => b.AddAIAgent<TestBehaviorAgent>("agent"));

    [TestMethod]
    public async Task FirstRun_DoesNotDeserialize_AndPersistsSession()
    {
        var action = LocateAIAgent();

        await action.SendInputAsync(b => b.Add("hello"));

        Assert.AreEqual(0, SharedTestAIAgent.Instance.DeserializeSessionCalls);
        Assert.AreEqual(1, SharedTestAIAgent.Instance.SerializeSessionCalls);
        Assert.IsFalse(string.IsNullOrWhiteSpace(SharedTestAIAgent.Instance.LastSerializedSessionRaw));
        Assert.AreEqual("hello", SharedTestAIAgent.Instance.LastRunMessages.Single().Text);
    }

    [TestMethod]
    public async Task SecondRun_Deserializes_PreviouslyPersistedSession()
    {
        var action = LocateAIAgent();

        await action.SendInputAsync(b => b.Add("first"));
        await action.SendInputAsync(b => b.Add("second"));

        Assert.AreEqual(1, SharedTestAIAgent.Instance.DeserializeSessionCalls);
        Assert.AreEqual(1, SharedTestAIAgent.Instance.LastDeserializedSessionVersion);
    }

    [TestMethod]
    public async Task EmptyChatMessage_DoesNotThrow_DuringLogging()
    {
        var action = LocateAIAgent();

        await action.SendInputAsync(b => b.Add(new ChatMessage { Role = ChatRole.User }));

        Assert.AreEqual(1, SharedTestAIAgent.Instance.RunCalls);
    }

    [TestMethod]
    public async Task AgenticInquiry_IsFormatted_UsingMessageText()
    {
        var action = LocateAIAgent();
        var inquiry = new AgenticChatInquiry
        {
            Message = new ChatMessage
            {
                Role = ChatRole.System,
                Contents = [ new TextContent("Approval required") ]
            },
            GuardTriggerHolder = new EventHolder<string> { Payload = "ignored" }
        };

        await action.SendInputAsync(b => b.Add(inquiry));

        var formatted = SharedTestAIAgent.Instance.LastRunMessages.Single();
        Assert.AreEqual(ChatRole.User, formatted.Role);
        Assert.AreEqual("There is the inquiry about the statement: Approval required", formatted.Text);
    }

    private IAIAgentBehavior LocateAIAgent()
    {
        var found = AIAgentLocator.TryLocateAIAgent(AIAgentActionId, out var aiAgent);
        Assert.IsTrue(found && aiAgent != null);
        return aiAgent;
    }

}

public sealed class TestBehaviorAgent : IAIAgent
{
    public string? Name => "test";
    public string? Description => null;
    public string? Instructions => null;
    public string? Arguments => null;
    public string? Template => null;
    public string? InitialPrompt => null;

    public Task<Microsoft.Agents.AI.AIAgent> BuildAgentAsync(IAIAgentContext aiAgentContext)
        => Task.FromResult<Microsoft.Agents.AI.AIAgent>(SharedTestAIAgent.Instance);
}

public sealed class SharedTestAIAgent : Microsoft.Agents.AI.AIAgent
{
    public static SharedTestAIAgent Instance { get; } = new();

    public int RunCalls { get; private set; }
    public int SerializeSessionCalls { get; private set; }
    public int DeserializeSessionCalls { get; private set; }
    public string LastSerializedSessionRaw { get; private set; } = string.Empty;
    public string FirstSerializedSessionRaw { get; private set; } = string.Empty;
    public string LastDeserializedSessionRaw { get; private set; } = string.Empty;
    public int LastDeserializedSessionVersion { get; private set; }
    public IReadOnlyList<ChatMessage> LastRunMessages { get; private set; } = [];

    public void Reset()
    {
        RunCalls = 0;
        SerializeSessionCalls = 0;
        DeserializeSessionCalls = 0;
        LastSerializedSessionRaw = string.Empty;
        FirstSerializedSessionRaw = string.Empty;
        LastDeserializedSessionRaw = string.Empty;
        LastDeserializedSessionVersion = 0;
        LastRunMessages = [];
    }

    protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken)
        => ValueTask.FromResult<AgentSession>(new TestAgentSession());

    protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? serializerOptions, CancellationToken cancellationToken)
    {
        SerializeSessionCalls++;
        var sessionData = JsonSerializer.SerializeToElement(new { Version = SerializeSessionCalls });
        LastSerializedSessionRaw = sessionData.GetRawText();
        if (string.IsNullOrWhiteSpace(FirstSerializedSessionRaw))
        {
            FirstSerializedSessionRaw = LastSerializedSessionRaw;
        }
        return ValueTask.FromResult(sessionData);
    }

    protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement sessionData, JsonSerializerOptions? serializerOptions, CancellationToken cancellationToken)
    {
        DeserializeSessionCalls++;
        LastDeserializedSessionRaw = sessionData.GetRawText();
        if (sessionData.TryGetProperty("Version", out var versionElement))
        {
            LastDeserializedSessionVersion = versionElement.GetInt32();
        }
        return ValueTask.FromResult<AgentSession>(new TestAgentSession());
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? runOptions,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        RunCalls++;
        LastRunMessages = messages.ToArray();
        await Task.CompletedTask;
        yield break;
    }

    protected override Task<AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session,
        AgentRunOptions? runOptions,
        CancellationToken cancellationToken)
    {
        RunCalls++;
        LastRunMessages = messages.ToArray();
        return Task.FromResult(new AgentResponse());
    }

    private sealed class TestAgentSession : AgentSession;
}