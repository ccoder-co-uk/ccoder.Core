# Docker Compose development environment

This optional environment runs the cCoder.Core reference applications in two Linux application containers plus their data dependencies:

- Application: public Web process with HostedServices available only over container loopback
- Workflow: privately addressable from the Application container and published on its requested host ports
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
./Docker/ci/Initialize.ps1
docker compose --env-file Docker/ci/.env --file Docker/ci/compose.yml pull
docker compose --env-file Docker/ci/.env --file Docker/ci/compose.yml up --wait
./Docker/ci/Smoke-Test.ps1
```

Open:

- Web: `http://localhost:80`
- Web: `https://localhost:443`
- App subdomains: for example, `https://app2.localhost`
- Workflow: `http://localhost:800`
- Workflow: `https://localhost:4433`
- SQL Server: `localhost,1433`

`Initialize.ps1` creates an ignored `.env` file containing random development-only SQL and encryption secrets. No real credentials or secrets are committed to the repository. The application configuration continues to use the same `Section__Property` environment-variable names documented in the main repository README.

Optional provider settings such as Microsoft Graph, Azure Service Bus, and AI credentials are deliberately omitted. The Compose environment uses local HTTP eventing. The application services deliberately use the rolling `latest` images produced from the pipeline's tested `publish/latest` output.

`Initialize.ps1` creates an ignored wildcard development certificate containing SANs for `localhost`, `*.localhost`, `127.0.0.1`, and `::1`. This allows host-based cCoder apps such as `app2.localhost` to use the same container certificate. The certificate is self-signed and must be trusted locally before browsers stop displaying a warning. Production deployments must mount a trusted certificate covering the root domain and its application subdomains; certificates and private keys are never included in the images.

## Stop and retain data

```powershell
docker compose --env-file Docker/ci/.env --file Docker/ci/compose.yml down
```

## Reset all local Compose data

The following command permanently removes the Compose SQL Server databases and Azurite data:

```powershell
docker compose --env-file Docker/ci/.env --file Docker/ci/compose.yml down --volumes
```

Run `Initialize.ps1 -Force` only when you also intend to replace the local development secrets. Data encrypted using the previous key may no longer be readable.

## Use a different host port

Edit only the port values in the ignored `.env` file. Container-to-container configuration uses Compose service names and does not depend on host ports.

## Diagnostic commands

```powershell
docker compose --env-file Docker/ci/.env --file Docker/ci/compose.yml ps
docker compose --env-file Docker/ci/.env --file Docker/ci/compose.yml logs --follow
```
