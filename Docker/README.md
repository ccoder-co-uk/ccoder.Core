# cCoder.Core local Docker harness

This folder starts the latest cCoder.Core applications for local testing. It creates exactly two cCoder containers:

- `application`: Web on ports 80/443 plus loopback-only HostedServices
- `workflow`: Workflow on ports 800/4433, reachable privately as `http://workflow:800`

SQL Server and Azure Storage are deliberately not bundled. Point the harness at development services you already run.

## Set up

Download this `Docker` folder, open PowerShell in it, and run:

```powershell
./Initialize.ps1
```

Edit the generated `.env` and set:

- `CCODER_CORE_CONNECTION_STRING`: the shared Core-domain database
- `CCODER_SECURITY_CONNECTION_STRING`: the separate Security database
- `CCODER_AZURE_WEBJOBS_STORAGE`: storage used by the Workflow Functions host

The addresses must be reachable from inside a container. If SQL Server exposes port 1433 on the Docker host, use `host.docker.internal,1433`, for example:

```text
Server=host.docker.internal,1433;Database=cCoder-Core;User Id=sa;Password=your-local-password;Encrypt=True;TrustServerCertificate=True
```

Keep `.env` local; it contains credentials and is ignored by Git.

## Run

```powershell
docker compose pull
docker compose up --wait
```

Open `https://localhost`, or an application subdomain such as `https://app2.localhost`. The generated self-signed certificate covers both `localhost` and `*.localhost`; browsers will warn until it is trusted locally.

To stop the applications:

```powershell
docker compose down
```

`pull_policy: always` ensures each start checks GHCR for the current `latest` application and workflow images.
