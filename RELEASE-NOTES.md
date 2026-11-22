# Release notes

## Named payloads

The dynamically defined event API has been renamed to payloads. `StructuralEvent`
and related types, `AddEvents`, `Stateflows.Events`, and the old event JSON
definition API have no compatibility forwarders; use `Payload`, `AddPayloads`,
`Stateflows.Payloads`, and `JsonPayloadDefinition`.

Persisted Stateflows data written before the rename is not supported by the new
types. Clear or explicitly migrate persisted behavior contexts, streams, and
token queues before upgrading; old data may contain serialized names for the
removed types.

Activity initializer declarations using `ITypeMapper` alternatives now match the
mapped runtime event types, rather than only the mapper declaration type. This
corrects initialization for OneOf and UnionTypes alternatives and affects
expected-event reporting accordingly.
