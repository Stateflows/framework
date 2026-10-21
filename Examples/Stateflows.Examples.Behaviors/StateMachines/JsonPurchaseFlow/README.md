# JSON purchase flow

`model.json` is embedded in `Stateflows.Examples.Behaviors` and registered by
`Stateflows.Examples.Orleans` as the `JsonPurchaseFlow` state machine. Its CLR event,
entry/exit handler, and effect types are bound explicitly in `JsonPurchaseFlow.cs`.

Start the Orleans example and use the Minimal APIs at `/sf`:

```http
GET  /sf/stateMachines/JsonPurchaseFlow/demo/status
POST /sf/stateMachines/JsonPurchaseFlow/demo/purchaseSubmitted
Content-Type: application/json

{ "event": { "orderId": "order-123" } }
```

The submission moves the instance from `AwaitingPurchase` to the final `Completed` state.
To test the other path, use a new instance:

```http
POST /sf/stateMachines/JsonPurchaseFlow/cancel-demo/purchaseCancelled
Content-Type: application/json

{ "event": { "orderId": "order-456" } }
```

Check each instance with `GET /sf/stateMachines/JsonPurchaseFlow/{instance}/status`.
The handler and effect messages are emitted through the application logger. Instances are
independent; once an instance reaches a final state, reset it or choose another instance name
before sending another path.
