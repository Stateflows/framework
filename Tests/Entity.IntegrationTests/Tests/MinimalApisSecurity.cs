using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Extensions.MinimalAPIs;

namespace Entity.IntegrationTests.Tests;

[Authorize]
public record AuthorizedMutation(int Amount);

[Authorize]
public record AuthorizedInitialization(int Amount);

[Authorize(Policy = "writer")]
[Authorize(Roles = "admin")]
public record PolicyMutation(int Amount);

[Authorize(AuthenticationSchemes = "secondary", Roles = "admin")]
public record SchemeMutation(int Amount);

[Authorize]
public class SecureProjection
{
    public int Count { get; set; }
}

public class ValidatedAddress
{
    [Required]
    public string Code { get; set; } = "";
}

public class ValidatedProfile
{
    [Required]
    public string Owner { get; set; } = "";
    public List<ValidatedAddress> Addresses { get; set; } = [];
}

public interface IValidatedApiBase
{
    [Field(FieldAccess.Get | FieldAccess.Set), Required, StringLength(8)]
    string Name { get; set; }
}

public interface ISecurityApiTemplate : IValidatedApiBase
{
    [Field(FieldAccess.Get | FieldAccess.Set), Range(0, 10)]
    int Count { get; set; }

    [Field(FieldAccess.Get | FieldAccess.Set), Required]
    ValidatedProfile Profile { get; set; }

    [Mutation]
    void Increment(AuthorizedMutation mutation) { Count += mutation.Amount; }

    [Mutation]
    void IncrementWithPolicy(PolicyMutation mutation) { Count += mutation.Amount; }

    [Mutation]
    void IncrementWithScheme(SchemeMutation mutation) { Count += mutation.Amount; }

    [Projection(PublishScope.Self)]
    SecureProjection View => new() { Count = Count };
}

public interface IAccessorAuthorizationBase
{
    [Field(FieldAccess.Get | FieldAccess.Set)]
    int ReadProtected
    {
        [Authorize]
        get;
        set;
    }
}

public interface IAccessorAuthorizationTemplate : IAccessorAuthorizationBase
{
    [Field(FieldAccess.Get | FieldAccess.Set)]
    int WriteProtected
    {
        get;
        [Authorize(Policy = "writer")]
        set;
    }

    [Field(FieldAccess.Get | FieldAccess.Set)]
    int BothProtected
    {
        [Authorize, Authorize(Roles = "admin")]
        get;
        [Authorize(Policy = "writer"), Authorize(Roles = "admin")]
        set;
    }

    [Field(FieldAccess.Get)]
    int Computed
    {
        [Authorize]
        get => ReadProtected + WriteProtected;
    }

    [Field(FieldAccess.Get | FieldAccess.Set)]
    int SchemeProtected
    {
        [Authorize(AuthenticationSchemes = "secondary", Roles = "admin")]
        get;
        [Authorize(AuthenticationSchemes = "secondary", Roles = "admin")]
        set;
    }
}

public record MethodMutation(int Amount);
public record MethodPolicyMutation(int Amount);
public record MethodSchemeMutation(int Amount);
public class MethodProjection
{
    public int Count { get; set; }
}

public interface IMethodAuthorizationBase
{
    [Field]
    int Count { get; set; }

    [Mutation, Authorize]
    void Increment(MethodMutation mutation) { Count += mutation.Amount; }

    [Projection(PublishScope.Self)]
    MethodProjection View
    {
        [Authorize]
        get => new() { Count = Count };
    }
}

public interface IMethodAuthorizationTemplate : IMethodAuthorizationBase
{
    [Mutation, Authorize(Policy = "writer"), Authorize(Roles = "admin")]
    void IncrementWithPolicy(MethodPolicyMutation mutation) { Count += mutation.Amount; }

    [Mutation, Authorize(Policy = "writer")]
    void IncrementWithTypeAuthorization(AuthorizedMutation mutation) { Count += mutation.Amount; }

    [Mutation, Authorize(AuthenticationSchemes = "secondary", Roles = "admin")]
    void IncrementWithScheme(MethodSchemeMutation mutation) { Count += mutation.Amount; }

    [Projection(PublishScope.Self)]
    SecureProjection CombinedView
    {
        [Authorize(Policy = "writer")]
        get => new() { Count = Count };
    }
}

[TestClass]
public class MinimalApisSecurity
{
    private WebApplication app = null!;
    private HttpClient http = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var builder = WebApplication.CreateBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Error);
        builder.WebHost.UseUrls($"http://127.0.0.1:{port}");
        builder.Services.AddAuthentication("test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("test", _ => { })
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("secondary", _ => { });
        builder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().RequireClaim("mfa").Build();
            options.AddPolicy("writer", policy => policy.RequireClaim("writer"));
            options.AddPolicy("admin", policy => policy.RequireRole("admin"));
        });
        builder.Services.AddStateflows(b => b.AddLegacyRuntime().AddValidator<FieldValueValidator>().AddEntities(entities => entities
            .AddEntity<ISecurityApiTemplate>("security", entity => entity.AddDefaultInitializer(c =>
            {
                c.Entity.Name = "initial";
                c.Entity.Count = 1;
                c.Entity.Profile = new ValidatedProfile { Owner = "initial" };
            }))
            .AddEntity<IAccessorAuthorizationTemplate>("accessors")
            .AddEntity<IAccessorAuthorizationTemplate>("securedAccessors")
            .AddEntity<IMethodAuthorizationTemplate>("methods")
            .AddEntity<IMethodAuthorizationTemplate>("securedMethods")
            .AddEntity<ISecurityApiTemplate>("securedRoutes")
            .AddEntity<ISecurityApiTemplate>("secureInitializer", entity =>
                entity.AddInitializer<AuthorizedInitialization>(c => c.Entity.Count = c.InitializationEvent.Amount))));
        app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStateflowsMinimalAPIsEndpoints(api => api.ConfigureEntities(entities =>
            entities
                .ConfigureEntity("securedRoutes", entity => entity.ConfigureAllEndpoints(endpoint =>
                    endpoint.ConfigureHandler(handler => handler.RequireAuthorization("admin"))))
                .ConfigureEntity("securedAccessors", entity => entity.ConfigureAllEndpoints(endpoint =>
                    endpoint.ConfigureHandler(handler => handler.RequireAuthorization("admin"))))
                .ConfigureEntity("securedMethods", entity => entity.ConfigureAllEndpoints(endpoint =>
                    endpoint.ConfigureHandler(handler => handler.RequireAuthorization("admin"))))));
        await app.StartAsync();
        http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        http?.Dispose();
        if (app != null)
        {
            await app.StopAsync();
            await app.DisposeAsync();
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? value = null,
        string? user = null, bool body = true)
    {
        using var request = new HttpRequestMessage(method, "/stateflows/entities/" + path);
        if (user != null)
            request.Headers.Add("X-Test-User", user);
        if (body && method != HttpMethod.Get && method != HttpMethod.Delete)
            request.Content = new StringContent(JsonSerializer.Serialize(new { @event = value }), Encoding.UTF8, "application/json");
        return await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    [TestMethod]
    public async Task TemplateMethodsAndInheritedProjectionGettersEnforceDefaultPolicyIncludingStreams()
    {
        foreach (var suffix in new[] { "", "?stream=true" })
        {
            using var anonymousMutation = await SendAsync(HttpMethod.Post, "methods/a/methodMutation" + suffix, new { amount = 1 });
            Assert.AreEqual(HttpStatusCode.Unauthorized, anonymousMutation.StatusCode);
            using var deniedMutation = await SendAsync(HttpMethod.Post, "methods/a/methodMutation" + suffix, new { amount = 1 }, "basic");
            Assert.AreEqual(HttpStatusCode.Forbidden, deniedMutation.StatusCode);
            using var anonymousProjection = await SendAsync(HttpMethod.Get, "methods/a/methodProjection" + suffix);
            Assert.AreEqual(HttpStatusCode.Unauthorized, anonymousProjection.StatusCode);
            using var deniedProjection = await SendAsync(HttpMethod.Get, "methods/a/methodProjection" + suffix, user: "basic");
            Assert.AreEqual(HttpStatusCode.Forbidden, deniedProjection.StatusCode);
        }

        using var mutation = await SendAsync(HttpMethod.Post, "methods/a/methodMutation", new { amount = 3 }, "mfa");
        Assert.AreEqual(HttpStatusCode.OK, mutation.StatusCode);
        using var projection = await SendAsync(HttpMethod.Get, "methods/a/methodProjection", user: "mfa");
        Assert.AreEqual(HttpStatusCode.OK, projection.StatusCode);
        using var stream = await SendAsync(HttpMethod.Get, "methods/a/methodProjection?stream=true", user: "mfa");
        Assert.AreEqual(HttpStatusCode.OK, stream.StatusCode);
        Assert.AreEqual("text/event-stream", stream.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    public async Task MethodPoliciesCombineWithTypeAndConfiguredEndpointAuthorization()
    {
        foreach (var user in new[] { "basic", "writer", "admin" })
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Post, "methods/a/methodPolicyMutation", new { amount = 1 }, user)).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Post, "methods/a/methodPolicyMutation", new { amount = 1 }, "writer,admin")).StatusCode);

        foreach (var user in new[] { "mfa", "writer" })
        {
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Post, "methods/a/authorizedMutation", new { amount = 1 }, user)).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Get, "methods/a/secureProjection?stream=true", user: user)).StatusCode);
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Post, "methods/a/authorizedMutation", new { amount = 1 }, "mfa,writer")).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Get, "methods/a/secureProjection", user: "mfa,writer")).StatusCode);

        Assert.AreEqual(HttpStatusCode.Forbidden,
            (await SendAsync(HttpMethod.Post, "securedMethods/a/methodMutation", new { amount = 1 }, "mfa")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden,
            (await SendAsync(HttpMethod.Get, "securedMethods/a/methodProjection?stream=true", user: "mfa")).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Post, "securedMethods/a/methodMutation", new { amount = 1 }, "mfa,admin")).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Get, "securedMethods/a/methodProjection", user: "mfa,admin")).StatusCode);
    }

    [TestMethod]
    public async Task MutationMethodAuthenticationSchemeIsUsed()
    {
        Assert.AreEqual(HttpStatusCode.Unauthorized,
            (await SendAsync(HttpMethod.Post, "methods/a/methodSchemeMutation", new { amount = 1 }, "admin")).StatusCode);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/stateflows/entities/methods/a/methodSchemeMutation");
        request.Headers.Add("X-Secondary-User", "admin");
        request.Content = new StringContent("""{"event":{"amount":1}}""", Encoding.UTF8, "application/json");
        using var response = await http.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task DefaultPolicyIsEnforcedForMutationsInitializersAndProjectionsIncludingStreams()
    {
        foreach (var path in new[]
                 {
                     "security/a/authorizedMutation", "security/a/authorizedMutation?stream=true",
                     "secureInitializer/a/authorizedInitialization"
                 })
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, (await SendAsync(HttpMethod.Post, path, new { amount = 1 })).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await SendAsync(HttpMethod.Post, path, new { amount = 1 }, "basic")).StatusCode);
        }

        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Post, "security/a/authorizedMutation", new { amount = 1 }, "mfa")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Created,
            (await SendAsync(HttpMethod.Post, "secureInitializer/a/authorizedInitialization", new { amount = 1 }, "mfa")).StatusCode);
        foreach (var suffix in new[] { "", "?stream=true" })
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized,
                (await SendAsync(HttpMethod.Get, "security/a/secureProjection" + suffix)).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Get, "security/a/secureProjection" + suffix, user: "basic")).StatusCode);
        }

        using var projection = await SendAsync(HttpMethod.Get, "security/a/secureProjection", user: "mfa");
        Assert.AreEqual(HttpStatusCode.OK, projection.StatusCode);
        using var stream = await SendAsync(HttpMethod.Get, "security/a/secureProjection?stream=true", user: "mfa");
        Assert.AreEqual(HttpStatusCode.OK, stream.StatusCode);
        Assert.AreEqual("text/event-stream", stream.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    public async Task MultipleAttributesRequireBothRoleAndNamedPolicy()
    {
        foreach (var user in new[] { "basic", "admin", "writer" })
        {
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Post, "security/a/policyMutation", new { amount = 1 }, user)).StatusCode);
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Post, "security/a/policyMutation", new { amount = 1 }, "admin,writer")).StatusCode);
    }

    [TestMethod]
    public async Task PrincipalWithoutIdentityIsNeverAuthorized()
    {
        await using var scope = app.Services.CreateAsyncScope();
        Assert.IsFalse(await AuthorizationHelper.IsAuthorizedAsync(new AuthorizeAttribute(), new ClaimsPrincipal(),
            scope.ServiceProvider.GetRequiredService<IAuthorizationService>()));
    }

    [TestMethod]
    public async Task AccessorAuthorizationIsIndependentAndSupportsInheritedAndComputedGetters()
    {
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Put, "accessors/a/readProtected", 3)).StatusCode,
            "An authorized getter must not restrict a public setter.");
        foreach (var field in new[] { "readProtected", "computed" })
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized,
                (await SendAsync(HttpMethod.Get, "accessors/a/" + field)).StatusCode);
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Get, "accessors/a/" + field, user: "basic")).StatusCode);
            using var allowed = await SendAsync(HttpMethod.Get, "accessors/a/" + field, user: "mfa");
            Assert.AreEqual(HttpStatusCode.OK, allowed.StatusCode);
            using var json = JsonDocument.Parse(await allowed.Content.ReadAsStringAsync());
            Assert.AreEqual(3, json.RootElement.GetProperty("response").GetProperty("value").GetInt32());
        }

        Assert.AreEqual(HttpStatusCode.Unauthorized,
            (await SendAsync(HttpMethod.Put, "accessors/a/writeProtected", 9)).StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden,
            (await SendAsync(HttpMethod.Put, "accessors/a/writeProtected", 9, "basic")).StatusCode);
        using (var unchanged = await SendAsync(HttpMethod.Get, "accessors/a/writeProtected"))
        {
            Assert.AreEqual(HttpStatusCode.OK, unchanged.StatusCode,
                "An authorized setter must not restrict a public getter.");
            using var json = JsonDocument.Parse(await unchanged.Content.ReadAsStringAsync());
            Assert.AreEqual(0, json.RootElement.GetProperty("response").GetProperty("value").GetInt32());
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Put, "accessors/a/writeProtected", 7, "writer")).StatusCode);

        Assert.AreEqual(HttpStatusCode.Unauthorized,
            (await SendAsync(HttpMethod.Put, "accessors/never/writeProtected", 1)).StatusCode);
        using var status = await SendAsync(HttpMethod.Get, "accessors/never/status");
        StringAssert.Contains(await status.Content.ReadAsStringAsync(), "NotInitialized");
    }

    [TestMethod]
    public async Task AccessorAttributesCombineWithEachOtherAndEndpointPolicies()
    {
        foreach (var user in new[] { "mfa", "admin", "basic" })
        {
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Get, "accessors/a/bothProtected?implicitInitialization=true", user: user)).StatusCode);
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Get, "accessors/a/bothProtected?implicitInitialization=true", user: "mfa,admin")).StatusCode);

        foreach (var user in new[] { "writer", "admin", "basic" })
        {
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Put, "accessors/a/bothProtected", 5, user)).StatusCode);
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Put, "accessors/a/bothProtected", 5, "writer,admin")).StatusCode);

        foreach (var user in new[] { "mfa", "admin" })
        {
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(HttpMethod.Get, "securedAccessors/a/readProtected?implicitInitialization=true", user: user)).StatusCode);
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Get, "securedAccessors/a/readProtected?implicitInitialization=true", user: "mfa,admin")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden,
            (await SendAsync(HttpMethod.Put, "securedAccessors/a/writeProtected", 3, "admin")).StatusCode);
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Put, "securedAccessors/a/writeProtected", 3, "writer,admin")).StatusCode);
    }

    [TestMethod]
    public async Task AccessorAuthorizationAuthenticatesOnlySelectedSchemes()
    {
        foreach (var method in new[] { HttpMethod.Get, HttpMethod.Put })
        {
            using var denied = await SendAsync(method, "accessors/a/schemeProtected?implicitInitialization=true", 2, "admin");
            Assert.AreEqual(HttpStatusCode.Unauthorized, denied.StatusCode);

            using var request = new HttpRequestMessage(method,
                "/stateflows/entities/accessors/a/schemeProtected?implicitInitialization=true");
            request.Headers.Add("X-Secondary-User", "admin");
            if (method == HttpMethod.Put)
                request.Content = new StringContent("""{"event":2}""", Encoding.UTF8, "application/json");
            using var allowed = await http.SendAsync(request);
            Assert.AreEqual(HttpStatusCode.OK, allowed.StatusCode);
        }
    }

    [TestMethod]
    public async Task SchemeRestrictedAuthorizationDoesNotUseRolesFromAnotherIdentity()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/stateflows/entities/security/a/schemeMutation");
        request.Headers.Add("X-Test-User", "admin");
        request.Headers.Add("X-Secondary-User", "basic");
        request.Content = new StringContent("""{"event":{"amount":1}}""", Encoding.UTF8, "application/json");
        using var denied = await http.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.Forbidden, denied.StatusCode);

        using var authorized = new HttpRequestMessage(HttpMethod.Post, "/stateflows/entities/security/a/schemeMutation");
        authorized.Headers.Add("X-Secondary-User", "admin");
        authorized.Content = new StringContent("""{"event":{"amount":1}}""", Encoding.UTF8, "application/json");
        using var allowed = await http.SendAsync(authorized);
        Assert.AreEqual(HttpStatusCode.OK, allowed.StatusCode);
    }

    [TestMethod]
    public async Task ConfiguredEndpointPolicyProtectsEveryGeneratedRoute()
    {
        var requests = new (HttpMethod Method, string Suffix, object? Value)[]
        {
            (HttpMethod.Get, "", null),
            (HttpMethod.Get, "/a/status", null),
            (HttpMethod.Get, "/a/status?stream=true", null),
            (HttpMethod.Get, "/a/notifications?names=SecureProjection", null),
            (HttpMethod.Get, "/a/notifications?names=SecureProjection&stream=true", null),
            (HttpMethod.Get, "/a/count", null),
            (HttpMethod.Put, "/a/count", 2),
            (HttpMethod.Delete, "/a", null),
            (HttpMethod.Post, "/a/initialize", new { }),
            (HttpMethod.Post, "/a/authorizedMutation", new { amount = 1 }),
            (HttpMethod.Get, "/a/secureProjection", null),
            (HttpMethod.Get, "/a/secureProjection?stream=true", null)
        };
        foreach (var (method, suffix, value) in requests)
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized,
                (await SendAsync(method, "securedRoutes" + suffix, value)).StatusCode, suffix);
            Assert.AreEqual(HttpStatusCode.Forbidden,
                (await SendAsync(method, "securedRoutes" + suffix, value, "basic")).StatusCode, suffix);
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Get, "securedRoutes/a/status", user: "admin")).StatusCode);
    }

    [TestMethod]
    public async Task FieldPutRejectsConstraintsNullAndNestedInvalidValuesWithoutChangingState()
    {
        using (var invalid = await SendAsync(HttpMethod.Put, "security/a/count", -1))
        {
            Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
            using var invalidJson = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync());
            Assert.IsFalse(invalidJson.RootElement.GetProperty("eventValidation").GetProperty("isValid").GetBoolean());
            Assert.IsTrue(invalidJson.RootElement.GetProperty("eventValidation").GetProperty("validationResults").GetArrayLength() > 0);
        }
        using (var status = await SendAsync(HttpMethod.Get, "security/a/status"))
        {
            StringAssert.Contains(await status.Content.ReadAsStringAsync(), "NotInitialized");
        }
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Put, "security/a/count", 4)).StatusCode);
        foreach (var (field, value) in new (string, object?)[]
                 {
                     ("count", 11), ("name", ""), ("name", "too long name"), ("name", "blocked"),
                     ("name", null), ("profile", null), ("profile", new { owner = "" }),
                     ("profile", new { owner = "valid", addresses = new[] { new { code = "" } } })
                 })
        {
            Assert.AreEqual(HttpStatusCode.BadRequest,
                (await SendAsync(HttpMethod.Put, "security/a/" + field, value)).StatusCode, field);
        }
        using var count = await SendAsync(HttpMethod.Get, "security/a/count");
        var json = JsonDocument.Parse(await count.Content.ReadAsStringAsync());
        Assert.AreEqual(4, json.RootElement.GetProperty("response").GetProperty("value").GetInt32());
        Assert.AreEqual(HttpStatusCode.OK,
            (await SendAsync(HttpMethod.Put, "security/a/profile",
                new { owner = "valid", addresses = new[] { new { code = "valid" } } })).StatusCode);
    }

    [TestMethod]
    public async Task DirectFieldWritesAreValidatedToo()
    {
        using var scope = app.Services.CreateScope();
        var locator = scope.ServiceProvider.GetRequiredService<IEntityLocator>();
        Assert.IsTrue(locator.TryLocateEntity(new EntityClass("security").ToId("direct"), out var entity));
        using (entity)
        {
            var invalid = await entity.SendAsync(new FieldState<int> { Name = "Count", Value = -1 });
            Assert.AreEqual(EventStatus.Invalid, invalid.Status);
            Assert.IsFalse(invalid.Validation.IsValid);
            Assert.IsTrue(invalid.Validation.ValidationResults.Any());
            Assert.IsFalse(await entity.TrySetAsync("Count", -1));
            Assert.IsFalse(await entity.TrySetAsync("Name", ""));
            Assert.IsFalse(await entity.TrySetAsync("Name", "blocked"));
            Assert.IsFalse(await entity.TrySetAsync("Profile", new ValidatedProfile { Owner = "" }));
            Assert.IsTrue(await entity.TrySetAsync("Count", 3));
            var (found, count) = await entity.TryGetAsync<int>("Count");
            Assert.IsTrue(found);
            Assert.AreEqual(3, count);
        }
    }

    private sealed class FieldValueValidator : IStateflowsValidator
    {
        public Task<bool> ValidateAsync<TEvent>(TEvent @event, List<ValidationResult> validationResults)
        {
            if (@event is string value && value == "blocked")
            {
                validationResults.Add(new ValidationResult("This field value is blocked."));
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }

    private sealed class TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var headerName = Scheme.Name == "secondary" ? "X-Secondary-User" : "X-Test-User";
            if (!Request.Headers.TryGetValue(headerName, out var header))
                return Task.FromResult(AuthenticateResult.NoResult());
            var claims = new List<Claim> { new(ClaimTypes.Name, "test") };
            foreach (var value in header.ToString().Split(','))
            {
                claims.Add(value == "admin" ? new Claim(ClaimTypes.Role, value) : new Claim(value, "true"));
            }
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name)));
        }
    }
}
