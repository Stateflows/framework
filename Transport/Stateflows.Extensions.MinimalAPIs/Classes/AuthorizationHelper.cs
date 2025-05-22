using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Stateflows.Extensions.MinimalAPIs;

public static class AuthorizationHelper
{
    public static async Task<bool> IsAuthorizedAsync(
        AuthorizeAttribute attribute,
        ClaimsPrincipal user,
        IAuthorizationService authorizationService)
    {
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
}