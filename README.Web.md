# cCoder.Core Web Host Setup

Use the web host when you want a complete interactive cCoder.Core environment with the first-run setup experience.

## What The Web Host Does

- starts the aggregate API surface
- applies Core and SSO EF migrations during startup
- serves the first-time setup page when the databases are empty
- creates the first tenant, admin user, app, and baseline app content

## Minimum Configuration

Update `src/Apps/Web/appsettings.json`, use `appsettings.testing.json`, or override with environment variables.

Required settings:

- `ConnectionStrings:Core`
- `ConnectionStrings:SSO`
- `Settings:DecryptionKey`

Optional but commonly needed:

- `Settings:enableExternalEventing`
- `Services:SSO`
- `Services:Workflow`
- `Services:Scheduler`
- `ConnectionStrings:ServiceBus`

## Local Run

```powershell
dotnet restore src\Apps\Web\Web.csproj --source https://api.nuget.org/v3/index.json --no-cache
dotnet run --project src\Apps\Web\Web.csproj
```

Then browse to the configured host, for example:

```text
https://localhost:7099/
```

## First-Time Setup Flow

On a brand-new environment the home page shows `Welcome to cCoder.Core platform setup`.

The setup form asks for:

- tenant name
- admin display name
- admin email
- password
- confirm password

On submit the web host:

- uses the current request host as the first app domain
- creates the first SSO tenant
- creates the first app in Core
- registers the admin user in SSO and Core
- seeds the baseline application data set
- redirects back to `/`

Once setup has completed successfully, the setup page is no longer shown.

## Notes

- Core and SSO migrations are applied automatically on startup.
- The baseline setup data ships with the web host and is copied to the publish output.
- If you are also running background services, bring up the web host first so the environment is initialized before the hosted-services app starts processing work.
