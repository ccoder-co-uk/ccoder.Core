# Core Aggregate

`cCoder.Core` is the aggregation package for the cCoder platform. It composes the individual domain packages into a single package and provides the shared host wiring used by the aggregate sample applications.

## What It Provides

- Composition of the cCoder domain packages through one NuGet package.
- Aggregate web and hosted-services wiring for validating the full dependency graph.
- OData/API exposure helpers, route registration, formatters, CORS support, and SignalR hub integration.
- First-time setup and baseline embedded assets for platform bootstrap.
- Integration points for logging, eventing, security, workflow, scheduling, mail, document management, content management, and packaging.

## When Consumers Use It

Use this package when building an application that wants the platform as a composed whole. It reduces the need to manually align versions and service registrations across the individual cCoder domain packages.

