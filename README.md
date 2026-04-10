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

## Build And Test

```powershell
dotnet restore src\cCoder.Core\cCoder.Core.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet restore src\cCoder.Core.Tests\cCoder.Core.Tests.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet restore src\Apps\Web\Web.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet restore src\Apps\HostedServices\HostedServices.csproj --source https://api.nuget.org/v3/index.json --no-cache

dotnet build src\cCoder.Core\cCoder.Core.csproj -v minimal --no-restore
dotnet build src\cCoder.Core.Tests\cCoder.Core.Tests.csproj -v minimal --no-restore
dotnet test src\cCoder.Core.Tests\cCoder.Core.Tests.csproj -v minimal --no-build
dotnet build src\Apps\Web\Web.csproj -v minimal --no-restore
dotnet build src\Apps\HostedServices\HostedServices.csproj -v minimal --no-restore
```

`src/Apps/Web.AcceptanceTests` remains in the repository for full integration verification, but it is not part of the package publish workflow because it requires SQL Server-backed acceptance infrastructure.

## Publishing

The repository uses GitHub Actions trusted publishing to push the `cCoder.Core` NuGet package from `.github/workflows/publish.yml`.

## License

This repository is licensed under The Standard Software License Version 1.0. See `LICENSE.txt` for details.
