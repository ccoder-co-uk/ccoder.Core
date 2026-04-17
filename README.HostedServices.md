# cCoder.Core Hosted Services Setup

Use the hosted-services app when you want background processing for an environment that has already been initialized by the web host.

## What The Hosted Services App Does

- starts the aggregate background-processing endpoints
- applies Core and SSO EF migrations during startup
- wires up external event listeners
- hosts background execution for workflow and related service processing

## Important Startup Order

Start the web host first on a new environment.

The hosted-services app does not provide the first-time setup UI. It assumes there is already at least one tenant and one app in the databases.

## Minimum Configuration

Update `src/Apps/HostedServices/appsettings.json`, use `appsettings.testing.json`, or override with environment variables.

Required settings:

- `ConnectionStrings:Core`
- `ConnectionStrings:SSO`
- `Settings:DecryptionKey`

Common additional settings:

- `ConnectionStrings:ServiceBus`
- `Services:SSO`
- `Services:Workflow`

## Local Run

```powershell
dotnet restore src\Apps\HostedServices\HostedServices.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet run --project src\Apps\HostedServices\HostedServices.csproj
```

## Notes

- Core and SSO migrations are applied automatically on startup here as well.
- If the databases are empty, the process can still create the schema, but there will be no useful work to process until the web host has completed first-time setup.
- The hosted-services app is intended to complement the web host, not replace it.
