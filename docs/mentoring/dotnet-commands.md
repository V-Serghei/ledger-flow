# .NET Commands

Run these commands from the repository root:

```text
E:\Program\banking-platform\ledger-flow
```

## Check the Selected SDK

```powershell
dotnet --version
```

- `dotnet` — the .NET command-line program.
- `--version` — prints the SDK version selected for the current directory.

The `global.json` file can affect SDK selection.

## List All Installed SDKs

```powershell
dotnet --list-sdks
```

- `--list-sdks` — lists installed SDKs and their installation directories.

## Pin the SDK for the Repository

```powershell
dotnet new globaljson --sdk-version 10.0.401 --roll-forward latestPatch --force
```

- `new` — creates a file from a template.
- `globaljson` — selects the `global.json` template.
- `--sdk-version 10.0.401` — sets the baseline SDK version for the repository.
- `--roll-forward latestPatch` — allows a newer patch in the same SDK feature band, such as `10.0.402`.
- `--force` — overwrites an existing `global.json` file.

## Restore Dependencies

```powershell
dotnet restore .\Banking.sln
```

- `restore` — downloads and validates NuGet dependencies for all projects in the solution.
- `.\Banking.sln` — the solution in the current directory; `.` means the current directory.

A successful restore does not prove that the code compiles. Warnings such as `NU1902` and `NU1903` report known package vulnerabilities.

## Build the Solution

```powershell
dotnet build .\Banking.sln --configuration Release --no-restore
```

- `build` — compiles the projects.
- `.\Banking.sln` — builds the entire solution.
- `--configuration Release` — uses the Release configuration.
- `--no-restore` — skips another dependency restore because `restore` was already run separately.

## Run All Tests Without Rebuilding

```powershell
dotnet test .\Banking.sln --configuration Release --no-build
```

- `test` — discovers and runs test projects.
- `.\Banking.sln` — tests the entire solution.
- `--configuration Release` — uses the Release build output.
- `--no-build` — skips compilation and expects a successful build to already exist.

Do not use `--no-build` if the code changed after the last build. Otherwise, the command may test stale binaries.

## Verify Formatting

```powershell
dotnet format --verify-no-changes --no-restore
```

- `format` — runs the .NET formatting tool.
- `--verify-no-changes` — does not modify files and fails if formatting changes are required.
- `--no-restore` — skips another dependency restore.

## List Outdated Packages

```powershell
dotnet list .\Banking.sln package --outdated
```

- `list` — displays information about a solution or project.
- `package` — selects NuGet package information.
- `--outdated` — shows packages for which a newer version exists.

This is a read-only report. It does not update packages.

## Inspect Direct and Transitive Packages

```powershell
dotnet list .\tests\Banking.IntegrationTests\Banking.IntegrationTests.csproj package --include-transitive
```

- `list` — displays information about a project.
- The `.csproj` path limits the report to the integration test project.
- `package` — selects NuGet package information.
- `--include-transitive` — includes indirect dependencies brought in by direct package references.

A direct dependency is explicitly declared in the project file. A transitive dependency is brought in by another package. This command is read-only and is useful when the failing component is not explicitly listed in the project file.

## Find Why a Transitive Package Is Present

```powershell
dotnet nuget why .\src\Banking.Worker\Banking.Worker.csproj Microsoft.EntityFrameworkCore.Relational --framework net10.0
```

- `nuget` — selects NuGet dependency tooling.
- `why` — prints the dependency path that brings a package into a project.
- The `.csproj` path selects the project to inspect.
- `Microsoft.EntityFrameworkCore.Relational` is the package being investigated.
- `--framework net10.0` limits the graph to the .NET 10 target framework.

This command is read-only. It is more reliable than guessing which direct dependency introduced a conflicting transitive package.
