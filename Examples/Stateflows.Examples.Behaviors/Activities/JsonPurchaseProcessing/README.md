# JSON purchase processing

`model.json` is embedded in `Stateflows.Examples.Behaviors` and registered by
`Stateflows.Examples.Orleans` as the `JsonPurchaseProcessing` activity. Its event, token,
accept-event handler, transformation, and action types are explicitly bound in
`JsonPurchaseProcessing.cs`.

Start the Orleans example and use the Minimal APIs at `/sf`:

```http
POST /sf/activities/JsonPurchaseProcessing/demo/processPurchase
Content-Type: application/json

{ "event": { "orderId": "order-123", "amount": 42.50 } }
```

The request implicitly initializes the activity. `ReceivePurchase` emits the request as an
object token, the flow transforms it into a receipt with reference `JSON-order-123`, and
`RecordReceipt` logs the order ID, reference, and amount before the activity finalizes.
The event endpoint is discovered through the same builder visitors used by fluent activities.

Check the instance with:

```http
GET /sf/activities/JsonPurchaseProcessing/demo/status
```

The resulting behavior status is `Finalized`. Use a new instance name for each purchase,
or reset the existing instance before submitting another request:

```http
DELETE /sf/activities/JsonPurchaseProcessing/demo
```
