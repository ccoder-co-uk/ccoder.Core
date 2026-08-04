# Docker Compose development environment

This optional environment runs the cCoder.Core reference applications in Linux containers:

- Web
- HostedServices
- Workflow
- SQL Server 2025 Developer Edition
- Azurite storage for the local Azure Functions host

It is intended for local development, evaluation, and integration testing. Docker is not required by the normal Visual Studio development flow or by production deployments.

## Prerequisites

- Docker Desktop or another Docker Engine with Linux containers
- Docker Compose v2
- At least 4 GB of memory available to the containers

Starting the SQL Server container accepts Microsoft's SQL Server container licence and uses the Developer edition. Do not use this Compose file as an undocumented production deployment.

## Start

From the repository root:

```powershell
./deploy/compose/Initialize.ps1
docker compose --env-file deploy/compose/.env --file deploy/compose/compose.yml up --build --wait
./deploy/compose/Smoke-Test.ps1
```

Open:

- Web: `http://localhost:5099`
- HostedServices: `http://localhost:5100`
- Workflow: `http://localhost:7071`
- SQL Server: `localhost,1433`

`Initialize.ps1` creates an ignored `.env` file containing random development-only SQL and encryption secrets. No real credentials or secrets are committed to the repository. The application configuration continues to use the same `Section__Property` environment-variable names documented in the main repository README.

Optional provider settings such as Microsoft Graph, Azure Service Bus, and AI credentials are deliberately omitted. The Compose environment uses local HTTP eventing.

## Stop and retain data

```powershell
docker compose --env-file deploy/compose/.env --file deploy/compose/compose.yml down
```

## Reset all local Compose data

The following command permanently removes the Compose SQL Server databases and Azurite data:

```powershell
docker compose --env-file deploy/compose/.env --file deploy/compose/compose.yml down --volumes
```

Run `Initialize.ps1 -Force` only when you also intend to replace the local development secrets. Data encrypted using the previous key may no longer be readable.

## Use a different host port

Edit only the port values in the ignored `.env` file. Container-to-container configuration uses Compose service names and does not depend on host ports.

## Diagnostic commands

```powershell
docker compose --env-file deploy/compose/.env --file deploy/compose/compose.yml ps
docker compose --env-file deploy/compose/.env --file deploy/compose/compose.yml logs --follow
```
