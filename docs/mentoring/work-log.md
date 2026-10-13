# Work Log

## LF-101 — Seed User Login

Observation: the API returned `401 Unauthorized`, and the expected users were not present in the database.

Verified causes:

1. The API container did not receive `ASPNETCORE_ENVIRONMENT=Development`, so the development seed did not run.
2. The seeder and login flow used different casing for the normalized email.

Changes:

- Added the Development environment to the API service in `docker-compose.yml`.
- Changed email normalization in the seeder to lowercase.
- Added an integration test for seed-user login.

Verification: unit, architecture, and integration tests passed. The change was committed and merged into `develop` through a pull request.

## LF-113 — Migrate from .NET 8 to .NET 10

Working branch: `chore/LF-113-dotnet-10`.

Completed:

1. Installed .NET SDK `10.0.401`.
2. Created `global.json` with version `10.0.401` and `latestPatch` roll-forward policy.
3. Changed `TargetFramework` from `net8.0` to `net10.0` in all eight projects.
4. Successfully ran `dotnet restore .\Banking.sln`.
5. Successfully built the entire solution in Release configuration.

Current warnings:

- `OpenTelemetry.Api 1.9.0` — known moderate-severity vulnerability.
- `SSH.NET 2025.1.0` — two known high-severity vulnerabilities; the package is present in integration tests as a dependency and requires separate investigation.

Integration test result after the initial framework change:

- Unit tests: 22 passed.
- Architecture tests: 4 passed.
- Integration tests: 1 passed and 22 failed.
- All 22 failures repeat the same exception: `ResponseBodyPipeWriter does not implement PipeWriter.UnflushedBytes`.

Verified cause:

- The integration test project targets `net10.0` but directly references `Microsoft.AspNetCore.Mvc.Testing 8.0.11`.
- That package brings in `Microsoft.AspNetCore.TestHost 8.0.11` as a transitive dependency.
- The .NET 10 JSON output formatter expects the .NET 10 `PipeWriter` behavior, while the .NET 8 test host implements the older contract.
- `CorrelationIdMiddleware.cs:20` appears in the stack trace because the exception travels through the middleware; it is not the source of the incompatibility.

Next change:

- Update `Microsoft.AspNetCore.Mvc.Testing` from `8.0.11` to `10.0.12` in the integration test project. The package will bring in the matching TestHost dependency.

Next verification after that change:

```powershell
dotnet restore .\Banking.sln
dotnet build .\Banking.sln --configuration Release --no-restore
dotnet test .\Banking.sln --configuration Release --no-build
```

Verification result:

- Updating `Microsoft.AspNetCore.Mvc.Testing` to `10.0.12` resolved the `PipeWriter.UnflushedBytes` incompatibility.
- Restore, build, and all tests completed successfully.
- This confirms that the failures were caused by the outdated .NET 8 test host rather than application business logic.

Package update investigation:

- Updating every outdated package at once introduced unrelated major-version migrations.
- `Swashbuckle.AspNetCore 10.3.0` brought in `Microsoft.OpenApi 2.7.5`, where the old `Microsoft.OpenApi.Models` namespace is no longer available.
- `MassTransit 9.2.4-develop.192` was a prerelease package and did not match the stable `MassTransit.RabbitMQ 9.2.3` package.
- `dotnet nuget why` showed that `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3` brings `Microsoft.EntityFrameworkCore.Relational 10.0.4` into the Worker dependency graph, while Infrastructure was compiled with EF Relational `10.0.12`.
- Decision: keep LF-113 focused on the required .NET 10 platform packages and postpone unrelated library upgrades. Explicitly align EF Relational with the selected EF Core patch version.

Applied resolution:

- Restored unrelated package upgrades to their previous versions.
- Kept the required .NET 10 package updates for JWT authentication, EF Core, Npgsql, Worker hosting, and MVC integration testing.
- Added an explicit `Microsoft.EntityFrameworkCore.Relational 10.0.12` reference to keep the runtime dependency consistent across Infrastructure and Worker.
- Restore completed successfully.
- Release build completed with zero errors; only the previously identified vulnerability warnings remain.
- All 49 tests passed: 22 unit, 4 architecture, and 23 integration tests.

Container and existing-database verification:

- Docker images for API and Worker were built from .NET 10 images.
- Existing containers were initially still running .NET 8, demonstrating that `docker compose build` does not replace running containers.
- API and Worker were force-recreated without recreating dependency containers or deleting the PostgreSQL volume.
- The recreated API reported `Microsoft.AspNetCore.App 10.0.12` and `Microsoft.NETCore.App 10.0.12`.
- The recreated Worker reported `Microsoft.NETCore.App 10.0.12`.
- PostgreSQL reused the existing `ledger-flow_postgres_data` volume created before the application-container upgrade.
- EF Core reported that no migrations were required and that the database was already up to date.
- API and Worker started successfully, and `GET /health/live` returned HTTP `200` with `Healthy`.

After the tests, update platform packages, Docker images, and CI separately. Do not ignore package vulnerabilities, but do not mix their remediation with the initial diagnostic step.
