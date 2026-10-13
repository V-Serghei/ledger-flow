# Docker and PowerShell

## Build and Start All Services

```powershell
docker compose up --build -d
```

- `docker` — the Docker command-line client.
- `compose` — manages the services defined in `docker-compose.yml`.
- `up` — creates and starts the services.
- `--build` — rebuilds the custom API and Worker images before starting them.
- `-d` — runs the containers in the background and returns control to the terminal.

## Build Selected Service Images Without Starting Containers

```powershell
docker compose build api worker
```

- `docker compose` — uses the services defined in `docker-compose.yml`.
- `build` — builds images but does not create or start containers.
- `api worker` — limits the operation to the two application services that have Dockerfiles.

This is useful after changing Docker base images. A successful build proves that the application can be restored and published inside the selected SDK image, but it does not prove that the containers start correctly.

## Recreate Only the Application Containers

```powershell
docker compose up -d --no-deps --force-recreate api worker
```

- `up` — creates or starts the selected services.
- `-d` — runs the containers in the background.
- `--no-deps` — leaves PostgreSQL, Redis, and RabbitMQ containers unchanged.
- `--force-recreate` — replaces existing API and Worker containers even when Compose configuration did not change.
- `api worker` — limits the operation to the application containers.

This command preserves the PostgreSQL named volume. It is useful after building new application images when old containers are still running the previous runtime.

## Check Service Status

```powershell
docker compose ps
```

- `ps` — shows Compose containers, their status, and published ports.
- `Up` means the container is running.
- `healthy` means the configured health check passed.

## Read Service Logs

```powershell
docker compose logs api
docker compose logs worker
```

- `logs` — prints container logs.
- `api` or `worker` — the service name from `docker-compose.yml`.

Add `--follow` to watch new log entries in real time. Press `Ctrl+C` to stop following the output.

## Execute a Command Inside a Container

```powershell
docker compose exec api printenv ASPNETCORE_ENVIRONMENT
```

- `exec` — runs a command inside an already running container.
- `api` — the service in which the command runs.
- `printenv` — a Linux command that prints an environment variable.
- `ASPNETCORE_ENVIRONMENT` — the exact ASP.NET Core environment variable name.

Empty output usually means that the variable is not set. Every letter matters: the earlier name `ASPNETCOCRE_ENVIROMENT` contained two spelling errors.

## Stop the Services

```powershell
docker compose down
```

- `down` — stops and removes this Compose project's containers and network.
- Named volumes remain intact unless `--volumes` is added.

## Read a File in PowerShell

```powershell
Get-Content global.json
```

- `Get-Content` — reads the contents of a file.
- `global.json` — the path of the file to read.

## Set a Temporary Environment Variable in PowerShell

```powershell
$env:API_PORT='15000'
```

- `$env:` — accesses environment variables in the current PowerShell process.
- `API_PORT` — the variable name.
- `'15000'` — the string value.

The variable exists only in the current PowerShell window and is inherited by processes launched from that window.
