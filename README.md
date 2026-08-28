# cCoder.Core

`cCoder.Core` is the aggregate package for the cCoder platform. It composes the domain packages published from the separate `cCoder.*` repositories and is the package used by the aggregate sample applications in this repository.

[View the latest code coverage report](https://ccoder-co-uk.github.io/ccoder.Core/)

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

## Run Locally

Each executable binds the complete configuration root from `appsettings.json`,
the environment-specific appsettings file, and environment variables into its
own `AppConfiguration`. The Web and HostedServices roots extend
`CoreConfiguration`; the Workflow app has the smaller root required by that
process.

Core is the deliberate aggregate-composition exception: its composite API
registers every configured child domain recursively. Persistence is still
owned by the side-by-side Data domains. `CoreData` owns the shared platform
database and `SecurityData` owns the SSO database; business-domain sections do
not contain connection strings or register Data themselves. Values left empty
in appsettings are secrets that must be defined as user-level or machine-level
environment variables.

For a normal local SQL Server setup, define:

```text
CoreData__ConnectionString
CoreData__AdminConnectionString
SecurityData__ConnectionString
SecurityData__AdminConnectionString
Security__DecryptionKey
```

The two `AdminConnectionString` values are optional migration-only overrides.
When configured, startup migrations use the admin connection while normal
runtime operations continue using the regular connection. When omitted,
migrations use the regular connection.

Optional provider credentials, such as Mail or Azure Service Bus credentials,
and AI provider API keys use the same `Section__Property` naming shown by the
matching appsettings section.

Domain sections are opt-in. When a child-domain section is absent, Core does
not register that domain's services or hosted services and does not advertise
its Swagger document, API context, OData route, metadata, or migrations. To
compose a smaller application, remove the complete section rather than leaving
an empty section behind. A section that is present but cannot be bound to its
typed configuration fails during application composition.

The Microsoft Graph integration path requires:

```text
Mail__Providers__MicrosoftGraph__TenantId
Mail__Providers__MicrosoftGraph__ClientId
Mail__Providers__MicrosoftGraph__ClientSecret
CoreIntegrationTests__MailSendUser
CoreIntegrationTests__MailReceiveUser
```

After setting those variables, restart Visual Studio so it receives the updated
environment, select the Web, HostedServices, and Workflow startup projects,
and press F5.
There is no configuration conversion step and no local secrets file to
generate.

## Optional local Docker harness

Contributors who prefer containers can run the latest Web, HostedServices, and
Workflow reference applications against their existing development SQL Server.
Docker remains optional and uses the same environment-variable configuration
contract described above.

See [the self-contained Docker harness](Docker/README.md) for setup and startup
instructions. Docker image-build and CI support files are also isolated beneath
that folder.

## CI application artifacts

The CI publishing helper creates versioned application artifacts and a `latest`
copy from the same successfully tested files:

```powershell
./.github/workflows/Publish-Applications.ps1 -Version 2026.8.4.1530
```

Output is written beneath the workflow's `artifacts/applications` directory and
is not committed to the repository. The image workflow builds the Application
and Workflow images from the identical `latest` artifact. These are CI
implementation details; local Docker users only need the root `Docker` folder.

## Build And Test

```powershell
dotnet restore src\cCoder.Core.slnx --source https://api.nuget.org/v3/index.json --no-cache
dotnet build src\cCoder.Core.slnx -c Release --no-restore
dotnet test src\cCoder.Core.slnx -c Release --no-build --settings src\cCoder.Core.runsettings
```

The acceptance and integration tests use the same
`CoreData__ConnectionString`, `SecurityData__ConnectionString`, and
`Security__DecryptionKey` variables as the applications. A single shared test
configuration source appends `-acceptance-{guid}` to both database names,
resets those isolated databases before running, and drops them during cleanup.
The integration suite defaults to HTTP eventing. Set
`Eventing__ProviderType=ServiceBus` and
`Eventing__ServiceBus__ConnectionString` to exercise Azure Service Bus
eventing.

When validating local changes across unpublished Security/AppSecurity repositories, the integration fixture can build against sibling local repositories:

```powershell
$env:CoreIntegrationTests__LocalSecurityAssemblyVersion = "2026.4.29.2038"
dotnet test src\Apps\cCoder.IntegrationTests\cCoder.IntegrationTests.csproj /p:UseLocalSecurity=true /p:UseLocalAppSecurity=true
```

The local assembly version override is only needed while downstream packages still reference the currently published Security assembly version. Once the package chain has been republished, consume the published package versions and run without the override.

The publish workflow runs on a self-hosted runner and always restores, builds, and tests `src/cCoder.Core.slnx` before packing.

## Platform Functionality

`cCoder.Core` gives consumers the composed platform package rather than asking each application to assemble the individual cCoder domain packages by hand. It brings together the shared data model, security, application permissions, content, documents, mail, scheduling, workflow, logging, eventing, and package import/export capabilities used by the aggregate hosts.

Account registration, invitation, password reset, and SSO lifecycle ownership lives in `cCoder.Security`. `cCoder.Core` consumes typed Security account events only to resolve the app from the request domain and queue app-template emails.

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