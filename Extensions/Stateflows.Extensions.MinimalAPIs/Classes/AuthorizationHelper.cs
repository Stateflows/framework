using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Stateflows.Extensions.MinimalAPIs;

public static class AuthorizationHelper
{
    public static async Task<bool> IsAuthorizedAsync(
        AuthorizeAttribute attribute,
        ClaimsPrincipal user,
        IAuthorizationService authorizationService)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }
        
        // Check Policy
        if (!string.IsNullOrEmpty(attribute.Policy))
        {
            var policyResult = await authorizationService.AuthorizeAsync(user, null, attribute.Policy);
            if (!policyResult.Succeeded)
                return false;
        }

        // Check Roles
        if (!string.IsNullOrEmpty(attribute.Roles))
        {
            var roles = attribute.Roles.Split(',').Select(r => r.Trim());
            if (!roles.Any(user.IsInRole))
                return false;
        }

        // Check AuthenticationSchemes (optional, depends on your setup)
        if (!string.IsNullOrEmpty(attribute.AuthenticationSchemes))
        {
            var schemes = attribute.AuthenticationSchemes.Split(',').Select(s => s.Trim());
            var authenticated = schemes.Any(scheme =>
                user.Identities.Any(id => id.AuthenticationType == scheme && id.IsAuthenticated));
            if (!authenticated)
                return false;
        }

        // All checks passed
        return true;
    }

    internal static async Task<(bool Success, IResult? Result)> AuthorizeAsync(
        IEnumerable<IAuthorizeData> attributes, IServiceProvider services, HttpContext context)
    {
        var policyProvider = services.GetRequiredService<IAuthorizationPolicyProvider>();
        var policy = await AuthorizationPolicy.CombineAsync(policyProvider, attributes);
        if (policy == null)
        {
            return (true, null);
        }

        var evaluator = services.GetRequiredService<IPolicyEvaluator>();
        var authentication = await evaluator.AuthenticateAsync(policy, context);
        var authorization = await evaluator.AuthorizeAsync(policy, authentication, context, context);
        if (authorization.Succeeded)
        {
            return (true, null);
        }

        var schemes = policy.AuthenticationSchemes.ToArray();
        return (false, authorization.Challenged
            ? Results.Challenge(authenticationSchemes: schemes)
            : Results.Forbid(authenticationSchemes: schemes));
    }
}