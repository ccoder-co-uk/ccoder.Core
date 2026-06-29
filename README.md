# cCoder.Core

`cCoder.Core` is the aggregate package for the cCoder platform. It composes the domain packages published from the separate `cCoder.*` repositories and is the package used by the aggregate sample applications in this repository.

## What This Repo Contains

- `src/cCoder.Core`
  The aggregate NuGet package.
- `src/cCoder.Core.Tests`
  Unit tests for the aggregate package.
- `src/Apps/Web`
  The aggregate web host used to validate the full package graph.
- `src/Apps/HostedServices`
  The aggregate hosted-services app used to validate non-web runtime wiring.
- `src/Apps/Web.AcceptanceTests`
  Acceptance coverage for the aggregate web host.
- `src/Apps/HostedServices.AcceptanceTests`
  Acceptance coverage for the aggregate hosted-services app.
- `src/Apps/cCoder.IntegrationTests`
  Full-process integration coverage across the aggregate hosts.

## Build And Test

```powershell
dotnet restore src\cCoder.Core.sln --source https://api.nuget.org/v3/index.json --no-cache
dotnet build src\cCoder.Core.sln -c Release --no-restore
dotnet test src\cCoder.Core.sln -c Release --no-build --settings src\cCoder.Core.runsettings
```

The full solution test suite requires the acceptance infrastructure connection strings:

```powershell
$env:CCODER_ACCEPTANCE_CORE_CONNECTION_STRING = "Server=...;Database=Core;..."
$env:CCODER_ACCEPTANCE_SSO_CONNECTION_STRING = "Server=...;Database=Security;..."
```

The acceptance and integration tests append suite-specific suffixes to the configured database names, reset those databases before running, and drop them during cleanup. The integration suite defaults to HTTP eventing; set `CCODER_INTEGRATION_EVENT_PROVIDER=ServiceBus` plus `CCODER_INTEGRATION_SERVICE_BUS_CONNECTION_STRING` to exercise Azure Service Bus eventing.

The publish workflow runs on a self-hosted runner and always restores, builds, and tests `src/cCoder.Core.sln` before packing.

## Platform Functionality

`cCoder.Core` gives consumers the composed platform package rather than asking each application to assemble the individual cCoder domain packages by hand. It brings together the shared data model, security, application permissions, content, documents, mail, scheduling, workflow, logging, eventing, and package import/export capabilities used by the aggregate hosts.

| Domain piece | What consumers get | Details |
| --- | --- | --- |
| Core aggregate | Composition, setup assets, OData/API exposure, CORS support, SignalR hubs, and host wiring for the combined platform. | [Core aggregate](docs/domains/core-aggregate.md) |
| Data | Shared EF Core data access, context factories, entity mappings, and database model support used by the domain packages. | [Data](docs/domains/data.md) |
| Security | SSO/security data model support, tenant/user/role/privilege services, and SQL Server security persistence. | [Security](docs/domains/security.md) |
| App Security | Application-level app, role, privilege, and user-role orchestration on top of the shared security model. | [App Security](docs/domains/app-security.md) |
| Content Management | Content/resource/component/script management used to deliver configurable platform UI and metadata-driven content. | [Content Management](docs/domains/content-management.md) |
| Document Management | File, folder, file-content, folder-role, and WebDAV-style document management capabilities. | [Document Management](docs/domains/document-management.md) |
| Eventing | In-process, HTTP, and Azure Service Bus eventing abstractions used to connect domain workflows and hosted services. | [Eventing](docs/domains/eventing.md) |
| Logging | Structured platform log storage and streaming support for web and hosted-service diagnostics. | [Logging](docs/domains/logging.md) |
| Mail | Mail server, queued email, sent email, and email workflow support. | [Mail](docs/domains/mail.md) |
| Packaging | Package and package-item import/export orchestration for moving platform configuration and baseline assets. | [Packaging](docs/domains/packaging.md) |
| Scheduling | Calendar and calendar-event scheduling support used by workflow and application features. | [Scheduling](docs/domains/scheduling.md) |
| Workflow | Workflow definitions, runtime orchestration, workflow activities, and hosted-service execution support. | [Workflow](docs/domains/workflow.md) |

## License

This repository is licensed under The Standard Software License Version 1.0. See `LICENSE.txt` for details.
