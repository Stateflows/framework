# Stateflows.Tools.Dashboard

Angular SPA dashboard for Stateflows, served by the host application via `UseStateflowsDashboard()`.

## What is included

- C# extension methods:
  - `AddDashboard(...)`
  - `UseStateflowsDashboard(...)`
- Angular app scaffold under `Dashboard/`
- Runtime static-file serving at `/stateflows/dashboard`
- Manifest endpoint at `/stateflows/dashboard/manifest`

## Host registration

```csharp
builder.Services.AddStateflows(b => b
    // ... your stateflows setup
    .AddDashboard()
);

var app = builder.Build();

app.MapStateflowsMinimalAPIsEndpoints();
app.UseStateflowsDashboard();

app.Run();
```

## Build SPA

Run from `Tools/Stateflows.Tools.Dashboard/Dashboard`:

```powershell
npm install
npm run build
```

Build output is generated in:

- `Tools/Stateflows.Tools.Dashboard/Dashboard/dist/stateflows-dashboard`

## Publish SPA with host

Copy the built browser files into a `Dashboard/` folder in the host project output.

At runtime, `UseStateflowsDashboard()` serves:

- static files from `<entry-assembly-dir>/Dashboard`
- SPA fallback for client routes (`/stateflows/dashboard/{**slug}`)

## Notes

- The SPA expects the Stateflows Minimal APIs under `/stateflows`.
- If you customize API prefix in Minimal APIs, update `src/app/core/services/stateflows-api.service.ts`.
