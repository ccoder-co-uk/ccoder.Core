# Eventing

The Eventing domain connects domain actions and background processing.

## What It Provides

- Core event hub abstractions for publishing and handling application events.
- HTTP eventing support for process-to-process integration.
- Azure Service Bus support for queue-backed event dispatch.
- Shared eventing configuration used by web, hosted-services, and integration-test hosts.

## Why It Matters

Consumers can start with simple HTTP eventing and move to Azure Service Bus-backed eventing when the deployment needs durable out-of-process messaging.

