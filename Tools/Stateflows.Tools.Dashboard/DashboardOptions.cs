namespace Stateflows.Tools.Dashboard
{
    public class DashboardOptions
    {
        /// <summary>
        /// Base route prefix for the dashboard. Defaults to <c>/stateflows/dashboard</c>.
        /// </summary>
        public string RoutePrefix { get; set; } = "/stateflows/dashboard";

        /// <summary>
        /// Optional CORS policy name to apply to dashboard endpoints.
        /// When null, the manifest endpoint defaults to allowing any origin.
        /// </summary>
        public string? CorsPolicyName { get; set; }

        /// <summary>
        /// Optional authorization policy name to protect the dashboard.
        /// When null, no authorization is applied.
        /// </summary>
        public string? AuthorizationPolicyName { get; set; }
    }
}
