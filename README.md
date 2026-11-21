# Introduction
Stateflows is a .NET framework for building stateful applications with executable state machines, data-flow activities, actions, and entities. Model business lifecycles, coordinate concurrent work, and process live streams through typed, testable C# APIs. Run locally or scale with Orleans, with persistence, messaging, and observability integrated into your behaviors.

# Getting Started
Go to [Stateflows homepage](https://www.stateflows.net) to learn more.

## Orleans storage and notification changes

The Orleans runtime uses ordinary grain storage, not Orleans transactions. Related entity operations
are independent calls and cannot be enlisted in the caller's state write. Existing Orleans transactional
behavior records are not automatically migrated to ordinary grain storage; migrate or recreate them
before upgrading. Pending publications in the former behavior outbox are not recovered by the new runtime;
drain them before upgrading and remove obsolete `Stateflows.Outbox` reminders. The `StageSending` API and
its in-memory staging mechanism have been removed.

Notifications are published during execution, even if the behavior later fails. Retention and
delivery retries, when configured, are handled by the subscriptions grain. `ExecutionMode.Transactional`
identifies an invocation outside a live Activity stream; it does not imply a distributed transaction.
The former `ImmediatePublish` attribute and header, publish-node option, transactional entity operations,
and transaction-specific `Aborted` status have been removed.
