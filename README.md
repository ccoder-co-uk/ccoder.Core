# cCoder.Core

`cCoder.Core` is the aggregate package for the cCoder platform. It composes the published `cCoder.*` domain packages and includes sample hosts that demonstrate how to run the platform as a web application or as background hosted services.

## Quick Start

- Web host setup: [README.Web.md](README.Web.md)
- Hosted services setup: [README.HostedServices.md](README.HostedServices.md)

If you are starting from scratch, begin with the web host. On first run it now:

- applies Core and SSO EF migrations during startup
- detects an empty environment
- shows a first-time setup screen
- creates the first tenant, first admin user, and first app
- seeds the baseline application data set

After setup completes you are redirected back to `/` and can sign in to start administering pages, components, resources, and other platform data.

## Repo Layout

- `src/cCoder.Core`
  Aggregate NuGet package.
- `src/cCoder.Core.Tests`
  Unit tests for aggregate package orchestration and policies.
- `src/Apps/Web`
  Web host for the platform and first-time setup experience.
- `src/Apps/Web.Tests`
  Focused unit tests for web-host setup behavior.
- `src/Apps/HostedServices`
  Background-processing host for non-web workloads.
- `src/Apps/Web.AcceptanceTests`
  SQL Server-backed acceptance coverage for the web host.

## Build And Test

```powershell
dotnet restore src\cCoder.Core\cCoder.Core.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet restore src\cCoder.Core.Tests\cCoder.Core.Tests.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet restore src\Apps\Web\Web.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet restore src\Apps\Web.Tests\Web.Tests.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet restore src\Apps\HostedServices\HostedServices.csproj --source https://api.nuget.org/v3/index.json --no-cache

dotnet build src\cCoder.Core\cCoder.Core.csproj -v minimal --no-restore
dotnet build src\cCoder.Core.Tests\cCoder.Core.Tests.csproj -v minimal --no-restore
dotnet test src\cCoder.Core.Tests\cCoder.Core.Tests.csproj -v minimal --no-build
dotnet build src\Apps\Web\Web.csproj -v minimal --no-restore
dotnet test src\Apps\Web.Tests\Web.Tests.csproj -v minimal
dotnet build src\Apps\HostedServices\HostedServices.csproj -v minimal --no-restore
```

`src/Apps/Web.AcceptanceTests` remains outside the regular package publish workflow because it depends on SQL Server-backed acceptance infrastructure.

## Publishing

The repository uses GitHub Actions trusted publishing to push the `cCoder.Core` NuGet package from `.github/workflows/publish.yml`.

## License

This repository is licensed under The Standard Software License Version 1.0. See `LICENSE.txt` for details.
