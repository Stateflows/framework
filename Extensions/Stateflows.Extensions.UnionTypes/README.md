# C# union type mapping

`Stateflows.Extensions.UnionTypes` targets .NET 11 and maps C# union trigger
declarations to their case types, like the OneOf extension does for OneOf types.
Register it before defining behaviors:

```csharp
using Stateflows;

public sealed record Approved;
public sealed record Rejected;
public union Decision(Approved, Rejected);

// Inside the host's service configuration:
services.AddStateflows(stateflows => stateflows
    .AddUnionTypes()
    .AddStateMachines(machines => machines
        .AddStateMachine("review", machine => machine
            .AddInitialState("pending", state => state
                .AddTransition<Decision>("decided"))
            .AddState("decided"))));
```

Send an `Approved` or `Rejected` event to enable the transition. Mapping expands
the trigger declaration; it does not unwrap union instances sent as payloads,
construct union values, or change event serialization. Existing projects can
continue targeting .NET 10; only applications referencing this extension need
.NET 11.

The mapper recognizes `System.Runtime.CompilerServices.UnionAttribute`, including
compiler-generated `union` declarations and manually implemented union patterns.
Cases come from public single-parameter constructors, or from public static
`Create` methods returning the union on its nested `IUnionMembers` provider
(including inherited provider members). Closed generic unions and `in` creation
parameters are supported. Nullable value-type cases remain `Nullable<T>`; nested
union cases are not recursively flattened, matching the existing type-mapper
pipeline.

Implementation follows the [C# union specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-15.0/unions).
An invalid attributed union without eligible creation members raises an error
during mapping rather than silently registering an unrelated trigger.
