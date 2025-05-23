using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Stateflows;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// In order to interact with Stateflows behaviors from remote process (in this example - in WebAssembly app),
// Stateflows client capabilities must be added to the app.
builder.Services.AddStateflowsClient(b => b
        
    // As client services are registered, transport layer must be configured - in this example, HTTP transport is used.
    // As a result, there are two processes communicating:
    // 1. Stateflows.Examples.Blazor, which hosts Stateflows behaviors
    // 2. Stateflows.Examples.Blazor.Client, which interacts with behaviors registered on the first one. 
    .AddHttpTransport(_ => new Uri(builder.HostEnvironment.BaseAddress))
);

await builder.Build().RunAsync();